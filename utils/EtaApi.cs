using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Utils
{
    public class EtaApi
    {

        private HttpClient client = HttpApi.getInstance().client();
        private readonly string loginUrl = Environment.GetEnvironmentVariable("PRODUCTION") == "TRUE" ? "https://id.eta.gov.eg/connect/token" : "https://id.preprod.eta.gov.eg/connect/token";
        private readonly string submitUrl = Environment.GetEnvironmentVariable("PRODUCTION") == "TRUE" ? "https://api.invoicing.eta.gov.eg/api/v1/documentsubmissions" : "https://api.preprod.invoicing.eta.gov.eg/api/v1/documentsubmissions";

        private readonly string clientId = Environment.GetEnvironmentVariable("CLIENT__ID")!;
        private readonly string clientSecret = Environment.GetEnvironmentVariable("CLIENT__SECRET")!;

        private string token = "";
        private DateTime tokenIssuedAt;
        private async Task login()
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", this.clientId },
                { "client_secret", this.clientSecret }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await this.client.PostAsync(this.loginUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();


            JObject? res = JsonConvert.DeserializeObject<JObject>(responseString, new JsonSerializerSettings()
            {
                FloatFormatHandling = FloatFormatHandling.String,
                FloatParseHandling = FloatParseHandling.Decimal,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.None
            });
            token = res!.SelectToken("access_token")!.ToString();
            tokenIssuedAt = DateTime.Now;
        }



        public async Task<string?> submit(JObject content)
        {

            var lastLoginDiffrencInHours = (DateTime.Now - tokenIssuedAt!).TotalHours;
            if (lastLoginDiffrencInHours > 1)
            {
                await this.login();
            }
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            this.client.DefaultRequestHeaders.Add("Contetn-Type", "application/json");
            var json = JsonConvert.SerializeObject(content);
            var c = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(this.submitUrl, c);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("this.submitUrl");
            Console.WriteLine(this.submitUrl);
            return responseString;
        }
    }
}
