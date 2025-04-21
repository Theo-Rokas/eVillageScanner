using System.Net.Http;

public class HttpClientHandlerService
{
    public HttpClient GetInsecureHttpClient()
    {
        var handler = new HttpClientHandler();

        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert.Issuer.Equals("CN=localhost"))
                return true;

            return errors == System.Net.Security.SslPolicyErrors.None;
        };

        return new HttpClient(handler);
    }
}
