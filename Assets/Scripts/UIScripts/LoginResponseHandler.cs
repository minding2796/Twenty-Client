using NetworkScripts;

namespace UIScripts
{
    public class LoginResponseHandler : ResponseHandler
    {
        public override void HandleResponse(WebViewObject webViewObject, string response)
        {
            webViewObject.SetVisibility(false);
            API.Log(response).Build();
        }
    }
}
