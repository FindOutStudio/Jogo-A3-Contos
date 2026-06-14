using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScript : MonoBehaviour
{
    [Header("Movimento")]
    public float scrollSpeed = 40f;
    
    [Header("Aceleração (Fast Forward)")]
    [Tooltip("Quantas vezes mais rápido os créditos rodam ao segurar a tela/mouse")]
    public float multiplicadorAceleracao = 4f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        
        // Garante que o tempo comece no normal ao carregar a cena
        Time.timeScale = 1f;
    }

    void Update()
    {       
        // ======== A MÁGICA DA ACELERAÇÃO ========
        // Funciona para o clique esquerdo do PC e também lê o dedo segurando a tela no Mobile!
        if (Input.GetMouseButton(0))
        {
            // Acelera o universo inteiro do jogo (Textos, Fotos, Dublê e Animações!)
            Time.timeScale = multiplicadorAceleracao;
        }
        else
        {
            // Soltou o dedo/mouse, volta ao normal na mesma hora
            Time.timeScale = 1f;
        }

        // O Time.deltaTime vai ser multiplicado sozinho pela Unity graças ao Time.timeScale
        rectTransform.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);
    }

    public void StartCredits()
    {
        rectTransform.anchoredPosition = startPosition;
    }

    public void MainMenu()
    {
        // MUITO IMPORTANTE: Resetar o tempo pro normal antes de sair da cena
        // para o seu Main Menu não carregar acelerado!
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Trava de segurança extra: Se a cena for fechada/pulada de outra forma, o tempo volta ao normal
        Time.timeScale = 1f;
    }
}