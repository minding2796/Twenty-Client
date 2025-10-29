using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;
using ManagerScripts;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace NetworkScripts
{
    public class WebSocketClient : MonoBehaviour
    {
        private static WebSocket _websocket;
        private const string ServerUrl = "wss://twenty-backend.thinkinggms.com/ws";

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }

        public async void ConnectOn()
        {
            try
            {
                if (_websocket == null && Networking.AccessToken != null) await Connect(Networking.AccessToken);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async void OnApplicationPause(bool pauseStatus)
        {
            try
            {
                if (pauseStatus)
                {
                    var command = new CommandMessage("game/lose", "Quit");
                    await Message(JsonConvert.SerializeObject(command));
                    await DisconnectAsync();
                }
                else ConnectOn();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async Task Connect(string authToken = "")
        {
            if (IsConnected || IsConnecting) return;

            IsConnecting = true;

            var dictionary = new Dictionary<string, string>();
            if (!authToken.Equals("")) dictionary.Add("Authorization", $"Bearer {authToken}");

            _websocket = new WebSocket(ServerUrl, dictionary);

            _websocket.OnOpen += () =>
            {
                Debug.Log("Connection open");
                IsConnected = true;
                IsConnecting = false;
            };

            _websocket.OnError += e =>
            {
                Debug.LogError($"Error: {e}");
                IsConnected = false;
                IsConnecting = false;
            };

            _websocket.OnClose += _ =>
            {
                Debug.Log("Connection closed");
                IsConnected = false;
                IsConnecting = false;
            };

            _websocket.OnMessage += bytes =>
            {
                var message = Encoding.UTF8.GetString(bytes);
                HandleMessage(message);
            };

            await _websocket.Connect();
        }

        private async Task DisconnectAsync()
        {
            if (_websocket != null)
            {
                await _websocket.Close();
                _websocket = null;
            }
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _websocket?.DispatchMessageQueue();
#endif
        }

        private void HandleMessage(string message)
        {
            Debug.Log($"Received message: {message}");
            var command = JsonConvert.DeserializeObject<CommandMessage>(message);
            switch (command.command)
            {
                case "match/matched":
                    Debug.Log("matched");
                    SceneManager.LoadScene("VersusScene");
                    break;
                case "match/closed":
                    Debug.Log("closed");
                    GameManager.Instance.matchModalPanel.SetActive(false);
                    break;
                case "game/turn_changed":
                    Debug.Log("Next Turn");
                    VersusGameManager.Instance.OnNextTurn();
                    break;
                case "game/game_set":
                    Debug.Log($"Game Set: {command.data}");
                    VersusGameManager.Instance.ShowWlModal(command.data);
                    break;
            }
        }

        public async Task Message(string message)
        {
            Debug.Log($"Send: {message}");
            if (_websocket.State == WebSocketState.Open)
            {
                await _websocket.SendText(message);
            }
        }
    }
}
