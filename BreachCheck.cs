using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace PassDefend
{
    public sealed partial class BreachCheck
    {
        public static string hashForTransfer(string password)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                return hash;
            }
        }

        public static async Task<bool> checkPassword(string password)
        {
            string hashStart = hashForTransfer(password);
            string sendableHash = hashStart.Remove(5);
            string identifiableHash = hashStart.Remove(0, 5);

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            string header = "PassDefend";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "PassDefend/1.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0) PassDefend/1.0";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("https://api.pwnedpasswords.com/range/" + sendableHash);

            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            if (httpResponseBody.Contains(identifiableHash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}