using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScript : MonoBehaviour
{
    public float scrollSpeed = 40f;
    
    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {       
        rectTransform.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);
    }

    public void StartCredits()
    {
        rectTransform.anchoredPosition = startPosition;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}