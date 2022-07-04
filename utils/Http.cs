using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Utils
{
    public class HttpApi
    {
        private HttpClient? httpClient;
        public static HttpApi? instance;

        private string token = "";

        private static readonly object padlock = new();

        public static HttpApi getInstance()
        {
            if (instance == null)
            {
                instance = new();
                instance.httpClient = new HttpClient();

            }
            return instance;
        }

        public void setToken(string token)
        {
            this.token = token;
        }
        public HttpClient client()
        {
            return this.httpClient!;
        }
    }
}
