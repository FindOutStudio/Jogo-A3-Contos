using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class GerenciadorMoedas : MonoBehaviour
{
    public static GerenciadorMoedas instance;
    
    // === NOVO: Variável global que avisa o resto do jogo que a fase acabou
    public static bool vitoriaAlcancada = false;

    [Header("Configuração da HUD (Durante a Fase)")]
    public Image[] iconesMoedas; 
    public Sprite iconeVazio; 
    public Sprite iconeCheio; 

    [Header("Progresso do Jogo")]
    public int idDestaFase = 0;
    public string idCinematicaVitoria;

    [Header("=== TELA DE VITÓRIA ===")]
    public GameObject painelVitoria; 
    public Image imagemLetreiro; 
    public Image[] imagensRaiosVitoria; 
    [Space(10)]
    public Button botaoProximaFase;
    public Button botaoReiniciarFase; 
    public Button botaoVoltarMenu;     

    [Header("Animações das Spritesheets (Via Código)")]
    public Sprite[] framesLetreiro;
    public Sprite[] framesRaio;
    public float fpsAnimacao = 12f; 

    [Header("Efeito Flutuar (Superman)")]
    public float velocidadeFlutuar = 3f;
    public float alturaFlutuar = 15f;
    private Vector2[] posicoesOriginaisRaios;

    [Header("Áudio")]
    public AudioMixerSnapshot somAbafadoSnapshot;
    public AudioMixerSnapshot somNormalSnapshot;

    private int moedasColetadas = 0;
    private int totalMoedas = 3;
    private bool jogoPausadoVitoria = false;

    private void Awake()
    {
        instance = this;
        vitoriaAlcancada = false; // Garante que a trava do pause reseta sempre que a fase começa
    }

    private void Start()
    {
        AtualizarHUD(); 
        
        if (painelVitoria != null) painelVitoria.SetActive(false);

        if (imagensRaiosVitoria.Length > 0)
        {
            posicoesOriginaisRaios = new Vector2[imagensRaiosVitoria.Length];
            for (int i = 0; i < imagensRaiosVitoria.Length; i++)
            {
                if (imagensRaiosVitoria[i] != null)
                {
                    posicoesOriginaisRaios[i] = imagensRaiosVitoria[i].rectTransform.anchoredPosition;
                    imagensRaiosVitoria[i].gameObject.SetActive(false); 
                }
            }
        }

        if (imagemLetreiro != null) imagemLetreiro.gameObject.SetActive(false);
        
        if (botaoProximaFase != null)
        {
            botaoProximaFase.gameObject.SetActive(false);
            botaoProximaFase.onClick.AddListener(BotaoProximaFase);
        }

        if (botaoReiniciarFase != null)
        {
            botaoReiniciarFase.gameObject.SetActive(false);
            botaoReiniciarFase.onClick.AddListener(BotaoReiniciarFase);
        }

        if (botaoVoltarMenu != null)
        {
            botaoVoltarMenu.gameObject.SetActive(false);
            botaoVoltarMenu.onClick.AddListener(BotaoVoltarMenu);
        }
    }

    private void Update()
    {
        if (jogoPausadoVitoria)
        {
            AnimarSpritesheets();
            FlutuarRaios();
        }
    }

    public void AdicionarMoeda()
    {
        moedasColetadas++;
        AtualizarHUD();

        if (moedasColetadas >= totalMoedas)
        {
            StartCoroutine(SequenciaDeVitoria());
        }
    }

    private void AtualizarHUD()
    {
        for (int i = 0; i < iconesMoedas.Length; i++)
        {
            if (i < moedasColetadas) iconesMoedas[i].sprite = iconeCheio;
            else iconesMoedas[i].sprite = iconeVazio;
        }
    }

    private IEnumerator SequenciaDeVitoria()
    {
        jogoPausadoVitoria = true;
        vitoriaAlcancada = true; // Trava o botão ESC no outro script!

        // Manda o botão de Pause normal da tela sumir!
        if (PauseMenu.instance != null) PauseMenu.instance.EsconderBotaoPauseUI();

        Time.timeScale = 0f;
        if (somAbafadoSnapshot != null) somAbafadoSnapshot.TransitionTo(0.1f);

        if (painelVitoria != null) painelVitoria.SetActive(true);
        if (imagemLetreiro != null) imagemLetreiro.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < imagensRaiosVitoria.Length; i++)
        {
            if (imagensRaiosVitoria[i] != null)
            {
                imagensRaiosVitoria[i].gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f); 
            }
        }

        if (SoundManager.instance != null) 
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uxVitoria); 
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // ======= PIPOCANDO OS BOTÕES EM ORDEM =======
        if (botaoVoltarMenu != null) 
        {
            botaoVoltarMenu.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f); // Espera um pouquinho
        }

        if (botaoReiniciarFase != null) 
        {
            botaoReiniciarFase.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f); // Espera mais um pouquinho
        }

        if (botaoProximaFase != null) 
        {
            botaoProximaFase.gameObject.SetActive(true);
        }
    }

    private void AnimarSpritesheets()
    {
        int frameIndex = (int)(Time.unscaledTime * fpsAnimacao); 

        if (framesLetreiro.Length > 0 && imagemLetreiro != null)
        {
            imagemLetreiro.sprite = framesLetreiro[frameIndex % framesLetreiro.Length];
        }

        if (framesRaio.Length > 0)
        {
            Sprite frameAtualRaio = framesRaio[frameIndex % framesRaio.Length];
            foreach (var img in imagensRaiosVitoria)
            {
                if (img != null && img.gameObject.activeSelf) img.sprite = frameAtualRaio;
            }
        }
    }

    private void FlutuarRaios()
    {
        for (int i = 0; i < imagensRaiosVitoria.Length; i++)
        {
            if (imagensRaiosVitoria[i] != null && imagensRaiosVitoria[i].gameObject.activeSelf)
            {
                float variacaoY = Mathf.Sin((Time.unscaledTime * velocidadeFlutuar) + i) * alturaFlutuar;
                imagensRaiosVitoria[i].rectTransform.anchoredPosition = posicoesOriginaisRaios[i] + new Vector2(0, variacaoY);
            }
        }
    }

    // ======= METODOS DOS BOTOES =======

    public void BotaoProximaFase()
    {
        LimparEfeitosPause();

        int proximaFaseID = idDestaFase + 1;
        PlayerPrefs.SetInt("FaseLiberada_" + proximaFaseID, 1);
        PlayerPrefs.Save();
        
        Debug.Log("Fase " + proximaFaseID + " desbloqueada com sucesso!");

        if (!string.IsNullOrEmpty(idCinematicaVitoria))
        {
            PlayerPrefs.SetString("CinematicaPendente", idCinematicaVitoria);
            SceneManager.LoadScene("Cinematicas");
        }
        else 
        {
            int proximaCena = SceneManager.GetActiveScene().buildIndex + 1;
            if (proximaCena < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(proximaCena);
            else SceneManager.LoadScene("MainMenu");
        }
    }

    public void BotaoReiniciarFase()
    {
        LimparEfeitosPause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BotaoVoltarMenu()
    {
        LimparEfeitosPause();
        SceneManager.LoadScene("MainMenu");
    }

    private void LimparEfeitosPause()
    {
        Time.timeScale = 1f;
        jogoPausadoVitoria = false;
        vitoriaAlcancada = false;
        if (somNormalSnapshot != null) somNormalSnapshot.TransitionTo(0.1f);
    }
}