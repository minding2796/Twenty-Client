using System;
using System.Collections.Generic;
using NetworkScripts;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManagerScripts
{
    public class VersusGameManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private UnityEngine.UI.Button buyButton;
        [SerializeField] private UnityEngine.UI.Button sellButton;
        [SerializeField] private UnityEngine.UI.Button waitButton;
        [SerializeField] private TextMeshProUGUI fundsText;
        [SerializeField] private TextMeshProUGUI basStatusText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI colText;
        [SerializeField] private TextMeshProUGUI stockPriceText;
        [SerializeField] private TextMeshProUGUI wlText;
        [SerializeField] private UnityEngine.UI.Slider quantitySlider;
        [SerializeField] private LineRenderer priceLineRenderer;
        [SerializeField] private GameObject waitForOtherPlayer;
        [SerializeField] private GameObject basModalPanel;
        [SerializeField] private GameObject wlModalPanel;
    
        private string _basStatus = "매수";
        private PlayerStatus _p1Status;
        private PlayerStatus _p2Status;
        private StockStatus _stockStatus;

        public event Action OnNextTurnAction;
        
        public static VersusGameManager Instance;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            Instance = this;
            CloseModal();
            wlModalPanel.SetActive(false);
            buyButton.onClick.AddListener(() => _basStatus = "매수");
            buyButton.onClick.AddListener(OpenModal);
            sellButton.onClick.AddListener(() => _basStatus = "공매도");
            sellButton.onClick.AddListener(OpenModal);
            UpdateStockData(() =>
            {
                UpdateLineRenderer();
                stockPriceText.text = $"{_stockStatus.stockPrice}";
                colText.text = $"다음 생활비 지불까지: {_stockStatus.colCooldown}턴 ({_stockStatus.costOfLiving}$)";
            });
            UpdatePlayerData(UpdateFundsDisplay);
            OnNextTurnAction += () =>
            {
                UpdateStockData(() =>
                {
                    UpdateLineRenderer();
                    stockPriceText.text = $"{_stockStatus.stockPrice}";
                    colText.text = $"다음 생활비 지불까지: {_stockStatus.colCooldown}턴 ({_stockStatus.costOfLiving}$)";
                });
                UpdatePlayerData(() =>
                {
                    UpdateFundsDisplay();
                    if (_p1Status.cooldown > 0) WaitTurn();
                });
            };
            return;

            void UpdateFundsDisplay()
            {
                fundsText.text = $"{_p1Status.currentFunds}$ vs {_p2Status.currentFunds}$";
            }
        }
    
    

        // Update is called once per frame
        private void Update()
        {
            if (_p1Status == null) return;
            if (_stockStatus == null) return;
            waitForOtherPlayer.gameObject.SetActive(_p1Status.isReady);
            
            if (_p1Status.isReady)
            {
                waitButton.interactable = false;
                buyButton.interactable = false;
                sellButton.interactable = false;
            }
            else if (_p1Status.cooldown > 0)
            {
                waitButton.interactable = true;
                buyButton.interactable = false;
                sellButton.interactable = false;
            }
            else
            {
                waitButton.interactable = true;
                buyButton.interactable = _p1Status.currentFunds >= _stockStatus.stockPrice;
                sellButton.interactable = true;
            }
            quantitySlider.maxValue = Mathf.FloorToInt((float) _p1Status.currentFunds / _stockStatus.stockPrice) + (_basStatus.Equals("공매도") ? 1 : 0);
            quantitySlider.minValue = 1;
            quantityText.text = $"{Mathf.RoundToInt(quantitySlider.value)}";
        }

        private async void BuyStock()
        {
            try
            {
                _p1Status.isReady = true;
                var quantity = Mathf.RoundToInt(quantitySlider.value);
                var command = new CommandMessage("game/buy_stock", quantity.ToString());
                await Networking.Instance.webSocketClient.Message(JsonConvert.SerializeObject(command));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async void SellStock()
        {
            try
            {
                _p1Status.isReady = true;
                var quantity = Mathf.RoundToInt(quantitySlider.value);
                var command = new CommandMessage("game/sell_stock", quantity.ToString());
                await Networking.Instance.webSocketClient.Message(JsonConvert.SerializeObject(command));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void WaitTurn()
        {
            try
            {
                _p1Status.isReady = true;
                var command = new CommandMessage("game/wait_turn", "");
                await Networking.Instance.webSocketClient.Message(JsonConvert.SerializeObject(command));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void Surrender()
        {
            try
            {
                var command = new CommandMessage("game/lose", "Surrender");
                await Networking.Instance.webSocketClient.Message(JsonConvert.SerializeObject(command));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void ConfirmButton()
        {
            CloseModal();
            if (_basStatus.Equals("매수")) BuyStock();
            else SellStock();
        }

        private void OpenModal()
        {
            basModalPanel.SetActive(true);
            basStatusText.text = $"주식 {_basStatus}";
        }

        public void CloseModal()
        {
            basModalPanel.SetActive(false);
        }

        private void UpdatePlayerData(Action after = null)
        { 
            API.GetPlayerData().OnResponse(list =>
            {
                _p1Status = list[0];
                _p2Status = list[1];
                after?.Invoke();
            }).Build();
        }

        private void UpdateStockData(Action after = null)
        { 
            API.GetStockData().OnResponse(s =>
            {
                _stockStatus = s;
                after?.Invoke();
            }).Build();
        }

        private void UpdateLineRenderer()
        {
            var positions = new Vector3[_stockStatus.priceHistory.Count];
            var idx = 0;
            foreach (var i in _stockStatus.priceHistory)
            {
                positions[idx++] = new Vector3(idx * -0.1f, (i - 10) / 20f, 0);
            }
            priceLineRenderer.positionCount = positions.Length;
            priceLineRenderer.SetPositions(positions);
        }

        public void OnNextTurn()
        {
            OnNextTurnAction?.Invoke();
        }

        public void ShowWlModal(string text)
        {
            wlText.text = text;
            wlModalPanel.SetActive(true);
        }

        public void BackToMainScene()
        {
            SceneManager.LoadScene("MainGame");
        }
    }

    [Serializable]
    public class StockStatus
    {
        public int stockPrice = 20;
        public List<int> priceHistory;
        public int colCooldown;
        public int costOfLiving;
    }
}
