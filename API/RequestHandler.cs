using Kenedia.BlishHUD.API.Requests;

namespace Kenedia.BlishHUD.API
{
    public class RequestHandler
    {
        public static string HostUrl = "http://localhost:5001/";

        public void SetHostUrl(string url)
        {
            HostUrl = url;
        }

        public void SendRequest(Request request)
        {

        }
    }
}
