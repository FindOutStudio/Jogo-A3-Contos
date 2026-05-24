using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class GerenciadorMoedas : MonoBehaviour
{
    public static GerenciadorMoedas instance;

    [Header("Configuração da HUD")]
    [Tooltip("Arraste as 3 imagens da HUD aqui, na ordem (Esquerda, Meio, Direita)")]
    public Image[] iconesMoedas; 
    public Sprite iconeVazio; // A bolinha preta
    public Sprite iconeCheio; // A bolinha amarela

    [Header("Progresso do Jogo")]
    [Tooltip("Qual é o número desta fase? (0 para Level 0, 1 para Level 1...)")]
    public int idDestaFase = 0;

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
        // Passa por cada um dos 3 ícones do ecrã
        for (int i = 0; i < iconesMoedas.Length; i++)
        {
            if (i < moedasColetadas)
            {
                iconesMoedas[i].sprite = iconeCheio;
            }
            else
            {
                iconesMoedas[i].sprite = iconeVazio;
            }
        }
    }

    private void FaseConcluida()
    {
        // === A MÁGICA DO DESBLOQUEIO AQUI ===
        // Liberta automaticamente a próxima fase no menu guardando no disco
        int proximaFaseID = idDestaFase + 1;
        PlayerPrefs.SetInt("FaseLiberada_" + proximaFaseID, 1);
        PlayerPrefs.Save();
        
        Debug.Log("Fase " + proximaFaseID + " desbloqueada com sucesso!");

        // Carrega a próxima cena
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