using System;
using UnityEngine;

namespace UIScripts
{
    [RequireComponent(typeof(WebViewObject))]
    public class WebView : MonoBehaviour
    {
        [SerializeField] public string url;
        [SerializeField] private WebViewObject webViewObject;
        [SerializeField] private ResponseHandler handler;

        private void Start()
        {
            StartWebView();
        }

        private void StartWebView()
        {
            try
            {
                webViewObject.Init(s => handler.HandleResponse(s));
                
                webViewObject.LoadURL(url);
                webViewObject.SetVisibility(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"WebView Error : {e}");
            }
        }
    }

    public abstract class ResponseHandler : MonoBehaviour
    {
        public abstract void HandleResponse(string response);
    }
}
