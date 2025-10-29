using NetworkScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts
{
    public class LoginResponseHandler : ResponseHandler
    {
        [SerializeField] private WebViewObject webViewObject;
        public override void HandleResponse(string response)
        {
            Networking.AccessToken = response;
            Networking.Instance.webSocketClient.ConnectOn();
            SceneManager.LoadScene("Tutorial");
        }
    }
}
