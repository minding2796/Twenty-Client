using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class StockManager : MonoBehaviour
{
    [Header("주식 가격 관리에 필요한 게임오브젝트")]
    [SerializeField]
    private TextMeshProUGUI stockPriceText;
    [SerializeField]
    private UnityEngine.UI.Button nextTurnButton;
    [SerializeField]
    private LineRenderer priceLineRenderer;
    
    [Header("주식 가격에 관여하는 변수")]
    [SerializeField]
    private int basePrice = 20;
    [SerializeField]
    private int minPrice = 10;
    [SerializeField]
    private int maxPrice = 30;
    [NonSerialized] public int StockPrice = 20;
    private readonly LinkedList<float> _priceHistory = new();
    private int _currentHistoryIndex;

    private void Start()
    {
        nextTurnButton.onClick.AddListener(OnNextTurn);
        InitializePriceHistory();
        UpdateStockPriceDisplay();
    }

    private void InitializePriceHistory()
    {
        for (var i = 0; i < 49; i++)
        {
            OnNextTurn();
            _priceHistory.AddFirst(StockPrice);
        }
        StockPrice = basePrice;
        _priceHistory.AddFirst(StockPrice);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        var positions = new Vector3[_priceHistory.Count];
        var idx = 0;
        foreach (var i in _priceHistory)
        {
            positions[idx++] = new Vector3(idx * -0.1f, (i - minPrice) / (maxPrice - minPrice), 0);
        }
        priceLineRenderer.positionCount = positions.Length;
        priceLineRenderer.SetPositions(positions);
    }

    private void UpdateStockPriceDisplay()
    {
        stockPriceText.text = StockPrice.ToString();
    }

    public void OnNextTurn()
    {
        var difference = basePrice - StockPrice;
        var maxChange = Mathf.Max(2, Mathf.Abs(difference));
        int change;

        if (Random.Range(0, 100) < 1)
        {
            change = Random.Range(-10, 11);
            StockPrice += change;
        }
        else
        {
            var probability = Mathf.Max(0.2f, 1f - Mathf.Abs(difference) / (float)(maxPrice - minPrice));

            if (Random.value < probability)
            {
                change = difference switch
                {
                    > 0 => Random.Range(-maxChange / 2, maxChange + 1),
                    < 0 => Random.Range(-maxChange, maxChange / 2 + 1),
                    _ => Random.Range(-3, 4)
                };
            }
            else change = Random.Range(-1, 2);

            StockPrice += change;
            StockPrice = Mathf.Clamp(StockPrice, minPrice, maxPrice);
        }

        _priceHistory.AddFirst(StockPrice);
        while (_priceHistory.Count > 50) _priceHistory.RemoveLast();
        UpdateLineRenderer();
        UpdateStockPriceDisplay();
        if (GameManager.Cooldown > 0) GameManager.Cooldown -= 1;
    }
}
