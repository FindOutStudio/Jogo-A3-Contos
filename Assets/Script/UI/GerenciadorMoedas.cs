using UnityEngine;
using UnityEngine.UI; // Necessário para controlar as Imagens da HUD
using UnityEngine.SceneManagement;

public class GerenciadorMoedas : MonoBehaviour
{
    public static GerenciadorMoedas instance;

    [Header("Configuração da HUD")]
    [Tooltip("Arraste as 3 imagens da HUD aqui, na ordem (Esquerda, Meio, Direita)")]
    public Image[] iconesMoedas; 
    public Sprite iconeVazio; // A bolinha preta
    public Sprite iconeCheio; // A bolinha amarela

    private int moedasColetadas = 0;
    private int totalMoedas = 3;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AtualizarHUD(); // Garante que comece com tudo preto
    }

    public void AdicionarMoeda()
    {
        moedasColetadas++;
        AtualizarHUD();

        if (moedasColetadas >= totalMoedas)
        {
            FaseConcluida();
        }
    }

    private void AtualizarHUD()
    {
        // Passa por cada um dos 3 ícones da tela
        for (int i = 0; i < iconesMoedas.Length; i++)
        {
            // Se o índice do ícone for menor que o tanto de moedas que pegamos, ele fica amarelo
            if (i < moedasColetadas)
            {
                iconesMoedas[i].sprite = iconeCheio;
            }
            // Se não, continua preto
            else
            {
                iconesMoedas[i].sprite = iconeVazio;
            }
        }
    }

    private void FaseConcluida()
    {
        // Aqui você decide o que acontece quando pega as 3! 
        // Por enquanto, vou fazer carregar a próxima cena (ou reiniciar se for a última).
        int proximaCena = SceneManager.GetActiveScene().buildIndex + 1;
        if (proximaCena < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(proximaCena);
        }
        else
        {
            Debug.Log("Fim de Jogo! Você zerou!");
            SceneManager.LoadScene("MainMenu");
        }
    }
}