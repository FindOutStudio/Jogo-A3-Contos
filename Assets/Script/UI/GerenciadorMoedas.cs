using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class GerenciadorMoedas : MonoBehaviour
{
    public static GerenciadorMoedas instance;
    public static bool vitoriaAlcancada = false;

    [Header("=== NOVA HUD VISUAL ===")]
    [Tooltip("Arraste o OBJETO PAI da HUD aqui (o 'HUD_Raios') para ele poder sumir inteiro")]
    public GameObject painelHUD; 
    [Tooltip("Arraste as 3 imagens da HUD aqui (Ordem: Esq, Meio, Dir)")]
    public Image[] iconesRaiosHUD; 
    public Color corVazio = Color.black; 
    public Color corCheio = Color.white; 

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
        vitoriaAlcancada = false; 
    }

    private void Start()
    {
        // ======= CORREÇÃO AQUI =======
        // Força o contador a zerar toda vez que a fase inicia!
        moedasColetadas = 0; 
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

        if (painelHUD != null)
        {
            bool telaLogsAberta = (LogUIManager.instance != null && LogUIManager.instance.painelLog != null && LogUIManager.instance.painelLog.activeSelf);
            bool telaVitoriaAberta = (painelVitoria != null && painelVitoria.activeSelf);

            if (telaLogsAberta || telaVitoriaAberta)
            {
                painelHUD.SetActive(false);
            }
            else
            {
                painelHUD.SetActive(true);
            }
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
        for (int i = 0; i < iconesRaiosHUD.Length; i++)
        {
            if (iconesRaiosHUD[i] != null)
            {
                if (i < moedasColetadas) iconesRaiosHUD[i].color = corCheio; 
                else iconesRaiosHUD[i].color = corVazio; 
            }
        }
    }

    private IEnumerator SequenciaDeVitoria()
    {
        jogoPausadoVitoria = true;
        vitoriaAlcancada = true; 

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

        if (botaoVoltarMenu != null) 
        {
            botaoVoltarMenu.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f); 
        }

        if (botaoReiniciarFase != null) 
        {
            botaoReiniciarFase.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f); 
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

    public void BotaoProximaFase()
    {
        LimparEfeitosPause();
        int proximaFaseID = idDestaFase + 1;
        PlayerPrefs.SetInt("FaseLiberada_" + proximaFaseID, 1);
        PlayerPrefs.Save();
        
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
        
        // ======= CORREÇÃO AQUI TAMBÉM =======
        // Garante que ao clicar no botão, a variável interna zera na hora
        moedasColetadas = 0; 
        
        if (somNormalSnapshot != null) somNormalSnapshot.TransitionTo(0.1f);
    }
}