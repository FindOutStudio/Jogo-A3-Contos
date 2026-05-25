using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class GerenciadorMoedas : MonoBehaviour
{
    public static GerenciadorMoedas instance;

    [Header("Configuração da HUD")]
    public Image[] iconesMoedas; 
    public Sprite iconeVazio; 
    public Sprite iconeCheio; 

    [Header("Progresso do Jogo")]
    public int idDestaFase = 0;

    [Header("Cinemática de Vitória")]
    [Tooltip("Nome da cinemática para tocar quando vencer (Ex: AlgozCinematica). Deixe vazio para pular direto para a próxima fase.")]
    public string idCinematicaVitoria;

    private int moedasColetadas = 0;
    private int totalMoedas = 3;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AtualizarHUD(); 
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
        // 1. Salva o progresso
        int proximaFaseID = idDestaFase + 1;
        PlayerPrefs.SetInt("FaseLiberada_" + proximaFaseID, 1);
        PlayerPrefs.Save();
        
        Debug.Log("Fase " + proximaFaseID + " desbloqueada com sucesso!");

        // 2. Toca a cinemática se você tiver digitado um nome no Inspector
        if (!string.IsNullOrEmpty(idCinematicaVitoria))
        {
            PlayerPrefs.SetString("CinematicaPendente", idCinematicaVitoria);
            SceneManager.LoadScene("Cinematicas");
        }
        else 
        {
            // 3. Se deixou vazio, ele vai direto pro próximo Level (ou Menu se zerou)
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
}