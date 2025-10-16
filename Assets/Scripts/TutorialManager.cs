using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public Transform tutorial;
    public int tutorialIndex;
    public LineRenderer priceLineRenderer;
    private readonly LinkedList<float> _priceHistory = new();

    private void Start()
    {
        for (var i = 0; i < tutorial.childCount; i++)
        {
            var child = tutorial.GetChild(i);
            child.gameObject.SetActive(false);
        }
        tutorial.GetChild(0).gameObject.SetActive(true);
        InitializePriceHistory();
    }

    public void PriceUpdate(float price)
    {
        _priceHistory.AddFirst(price);
        UpdateLineRenderer();
    }

    private void InitializePriceHistory()
    {
        for (var i = 0; i < 49; i++)
        {
            _priceHistory.AddFirst(Random.Range(18, 23));
        }
        _priceHistory.AddFirst(20);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        var positions = new Vector3[_priceHistory.Count];
        var idx = 0;
        foreach (var i in _priceHistory)
        {
            positions[idx++] = new Vector3(idx * -0.1f, (i - 10) / 20f, 0);
        }
        priceLineRenderer.positionCount = positions.Length;
        priceLineRenderer.SetPositions(positions);
    }
    
    public void Next()
    {
        var child = tutorial.GetChild(tutorialIndex++);
        child.gameObject.SetActive(false);
        if (tutorialIndex >= tutorial.childCount)
        {
            OnTutorialEnd();
            return;
        }
        tutorial.GetChild(tutorialIndex).gameObject.SetActive(true);
    }

    private static void OnTutorialEnd()
    {
        SceneManager.LoadScene("MainGame");
    }
}
