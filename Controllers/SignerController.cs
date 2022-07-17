using Microsoft.AspNetCore.Mvc;
using Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace signer.Controllers;

[ApiController]
[Route("[controller]")]
public class SignerController : ControllerBase
{
    private readonly ILogger<SignerController> _logger;
    public SignerController(ILogger<SignerController> logger)
    {
        _logger = logger;
    }
    [HttpPost(Name = "GetSigner")]
    public async Task<string> Sign()
    {
        Request.EnableBuffering();
        var bodyAsText = await new System.IO.StreamReader(Request.Body).ReadToEndAsync();
        Request.Body.Position = 0;
        JArray? request = JsonConvert.DeserializeObject<JArray>(bodyAsText, new JsonSerializerSettings()
        {
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Decimal,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.None
        });

        TokenSigner signer = new TokenSigner();
        JArray signedDocs = new JArray();

        for (int i = 0; i < request!.Count; i++)
        {
            var token = request![i];
            JObject obj = token.ToObject<JObject>()!;
            obj.Remove("Serial");

            String canonicalString = signer.SerializeToken(obj);
            string signature = signer.SignWithCMS(canonicalString);
            JArray signaturesArray = new JArray();
            JObject signaturesObject = new JObject(
                                       new JProperty("signatureType", "I"),
                                       new JProperty("value", signature + "ii"));
            signaturesArray.Add(signaturesObject);
            obj.Add("signatures", signaturesArray);
            signedDocs.Add(obj);
        }
        JObject documentsObject = new JObject(new JProperty("documents", signedDocs));

        EtaApi api = new EtaApi();
        Console.WriteLine(signedDocs);
        string? finalResp = await api.submit(documentsObject);
        signedDocs = new JArray();
        return finalResp.ToString();

    }
}

