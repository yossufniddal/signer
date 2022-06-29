using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Utils
{
    public class EtaApi
    {

        private HttpClient client = new HttpClient();
        private readonly string loginUrl = "https://id.eta.gov.eg/connect/token";
        private readonly string submitUrl = "https://api.invoicing.eta.gov.eg/api/v1/documentsubmissions";



        private async Task<string> login()
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", "92fe559b-c17e-4275-a12e-132d34189ef1" },
                { "client_secret", "1e0c3a98-b4df-489b-b366-25e3aa5e28c6" }
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
            string token = res!.SelectToken("access_token")!.ToString();
            return token;
        }



        public async Task<string?> submit(JObject content)
        {
            string token = await this.login();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // using (var request = new HttpRequestMessage(HttpMethod.Post, this.submitUrl))
            // {
            //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //     request.Headers.Add("Content-Type" , "application/json");
            //     JsonContent c = JsonContent.Create<JObject>(content);
            //     request.Content = c;
            //     var response = await this.client.PostAsJsonAsync(request);

            //     response.EnsureSuccessStatusCode();

            //     return await response.Content.ReadAsStringAsync();
            // }

            this.client.DefaultRequestHeaders.Add("Contetn-Type", "application/json");
            //  JsonContent c = JsonContent.Create<JObject>(content);
            // var response = await this.client.PostAsJsonAsync<JObject>(this.submitUrl, content);
            // var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.SerializeObject(content);
            // var content = new FormUrlEncodedContent(values);
            var c = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(this.submitUrl, c);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            return responseString;
        }
    }
}
