using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private UnityEngine.UI.Button buyButton;
    [SerializeField] private UnityEngine.UI.Button sellButton;
    [SerializeField] private TextMeshProUGUI fundsText;
    [SerializeField] private TextMeshProUGUI basStatusText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI colText;
    [SerializeField] private StockManager stockManager;
    [SerializeField] private UnityEngine.UI.Slider quantitySlider;
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private int maxCooldown = 3;
    [SerializeField] private int costOfLiving = 5;
    [SerializeField] private int maxColCooldown = 5;
    
    private string _basStatus = "매수";
    private int _currentFunds = 100;
    public static int Cooldown;
    public static int ColCooldown;
    public static int GoStack = 3;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ColCooldown = maxColCooldown;
        CloseModal();
        buyButton.onClick.AddListener(() => _basStatus = "매수");
        buyButton.onClick.AddListener(OpenModal);
        sellButton.onClick.AddListener(() => _basStatus = "공매도");
        sellButton.onClick.AddListener(OpenModal);
        UpdateFundsDisplay();
        stockManager.OnNextTurnAction += () =>
        {
            if (_currentFunds < 0) GoStack--;
            else GoStack = 3;
            if (GoStack <= 0) GameOver();
            if (--ColCooldown <= 0)
            {
                _currentFunds -= costOfLiving;
                UpdateFundsDisplay();
                ColCooldown = maxColCooldown;
            }
            colText.text = $"다음 생활비 지불까지: {ColCooldown}턴 ({costOfLiving}$)";
        };
    }
    
    

    // Update is called once per frame
    private void Update()
    {
        if (Cooldown > 0)
        {
            buyButton.interactable = false;
            sellButton.interactable = false;
        }
        else
        {
            buyButton.interactable = _currentFunds >= stockManager.StockPrice;
            sellButton.interactable = true;
        }
        quantitySlider.maxValue = Mathf.FloorToInt((float) _currentFunds / stockManager.StockPrice) + (_basStatus.Equals("공매도") ? 1 : 0);
        quantitySlider.minValue = 1;
        quantityText.text = $"{Mathf.RoundToInt(quantitySlider.value)}";
    }

    private void BuyStock()
    {
        var quantity = Mathf.RoundToInt(quantitySlider.value);
        if (!(_currentFunds * quantity >= stockManager.StockPrice)) return;
        var fewPrice = stockManager.StockPrice;
        stockManager.OnNextTurn();
        _currentFunds += (stockManager.StockPrice - fewPrice) * quantity;
        UpdateFundsDisplay();
        Cooldown = maxCooldown;
    }

    private void SellStock()
    {
        var quantity = Mathf.RoundToInt(quantitySlider.value);
        var fewPrice = stockManager.StockPrice;
        stockManager.OnNextTurn();
        _currentFunds -= (stockManager.StockPrice - fewPrice) * quantity;
        UpdateFundsDisplay();
        Cooldown = maxCooldown;
    }

    private void UpdateFundsDisplay()
    {
        fundsText.text = $"{_currentFunds}$";
    }

    public void ConfirmButton()
    {
        CloseModal();
        if (_basStatus.Equals("매수")) BuyStock();
        else SellStock();
    }

    private void OpenModal()
    {
        modalPanel.SetActive(true);
        basStatusText.text = $"주식 {_basStatus}";
    }

    public void CloseModal()
    {
        modalPanel.SetActive(false);
    }

    private static void GameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}
