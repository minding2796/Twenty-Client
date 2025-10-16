using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private UnityEngine.UI.Button buyButton;
    [SerializeField] private UnityEngine.UI.Button sellButton;
    [SerializeField] private TextMeshProUGUI fundsText;
    [SerializeField] private TextMeshProUGUI basStatusText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private StockManager stockManager;
    [SerializeField] private UnityEngine.UI.Slider quantitySlider;
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private int maxCooldown = 3;
    
    private string _basStatus = "매수";
    private int _currentFunds = 100;
    public static int Cooldown;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CloseModal();
        buyButton.onClick.AddListener(() => _basStatus = "매수");
        buyButton.onClick.AddListener(OpenModal);
        sellButton.onClick.AddListener(() => _basStatus = "공매도");
        sellButton.onClick.AddListener(OpenModal);
        UpdateFundsDisplay();
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
        quantitySlider.maxValue = Mathf.FloorToInt((float) _currentFunds / stockManager.StockPrice);
        quantitySlider.minValue = 1;
        quantityText.text = $"{Mathf.RoundToInt(quantitySlider.value)}";
    }

    private void BuyStock()
    {
        var quantity = Mathf.RoundToInt(quantitySlider.value);
        if (!(_currentFunds * quantity >= stockManager.StockPrice)) return;
        _currentFunds -= stockManager.StockPrice * quantity;
        stockManager.OnNextTurn();
        _currentFunds += stockManager.StockPrice * quantity;
        UpdateFundsDisplay();
        Cooldown = maxCooldown;
    }

    private void SellStock()
    {
        var quantity = Mathf.RoundToInt(quantitySlider.value);
        _currentFunds += stockManager.StockPrice * quantity;
        stockManager.OnNextTurn();
        _currentFunds -= stockManager.StockPrice * quantity;
        UpdateFundsDisplay();
        Cooldown = maxCooldown;
    }

    private void UpdateFundsDisplay()
    {
        if (_currentFunds < 20) GameOver();
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
        Debug.Log("Game Over");
    }
}
