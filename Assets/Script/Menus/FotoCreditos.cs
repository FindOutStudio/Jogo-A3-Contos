using UnityEngine;

public class FotoCreditos : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("Deve ser a mesma velocidade dos créditos para subirem juntos")]
    public float scrollSpeed = 40f;
    
    [Tooltip("A posição Y onde a foto deve parar (0 é o centro da tela)")]
    public float stopY = 0f;

    private RectTransform rectTransform;
    private bool chegouNoMeio = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Se a foto ainda não chegou na posição de parada...
        if (!chegouNoMeio)
        {
            // Move ela para cima
            rectTransform.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);

            // Se ela passou ou igualou o ponto de parada, crava ela lá!
            if (rectTransform.anchoredPosition.y >= stopY)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, stopY);
                chegouNoMeio = true;
            }
        }
    }

    // Função caso você queira resetar os créditos
    public void ResetarFoto(float posicaoInicialY)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posicaoInicialY);
        chegouNoMeio = false;
    }
}