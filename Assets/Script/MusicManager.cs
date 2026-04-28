using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Configurações de Cena")]
    public string nomeCenaMenu = "MainMenu";
    public string nomeCenaCreditos = "Credits";

    [Header("Faixas de Áudio")]
    public AudioClip musicaMenu;
    public AudioClip musicaGameplay1;
    public AudioClip musicaGameplay2;
    public AudioClip musicaAlgoz;

    [Header("Efeito de Pause")]
    public float frequenciaNormal = 22000f;
    public float frequenciaAbafada = 800f;
    public float velocidadeTransicao = 10f;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPass;
    private bool isGameplay = false;
    private bool tocandoAlgoz = false; // Controle para saber se o tema do Algoz está ativo
    private int musicaAtualGameplay = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            lowPass = GetComponent<AudioLowPassFilter>();
            
            lowPass.cutoffFrequency = frequenciaNormal;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sempre que mudar de fase, o Algoz "perde" a vez por segurança
        tocandoAlgoz = false;

        if (scene.name == nomeCenaMenu || scene.name == nomeCenaCreditos) 
        {
            TocarMusicaMenu();
        }
        else 
        {
            IniciarMusicaGameplay();
        }
    }

    public void TocarMusicaMenu()
    {
        isGameplay = false;
        tocandoAlgoz = false;
        
        if (audioSource.clip != musicaMenu)
        {
            audioSource.clip = musicaMenu;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void IniciarMusicaGameplay()
    {
        // Se já estiver na gameplay normal e não estiver vindo do Algoz, não faz nada
        if (isGameplay && !tocandoAlgoz) return; 

        isGameplay = true;
        tocandoAlgoz = false;
        audioSource.loop = false;
        musicaAtualGameplay = Random.Range(1, 3); 

        TocarFaixaAtual();
    }

    private void TocarFaixaAtual()
    {
        audioSource.clip = (musicaAtualGameplay == 1) ? musicaGameplay1 : musicaGameplay2;
        audioSource.Play();
    }

    private void Update()
    {
        // ======= LÓGICA DO BOTÃO K (TOGGLE ALGOZ) =======
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!tocandoAlgoz)
            {
                // Entra no Modo Algoz: Para tudo e toca a dele
                tocandoAlgoz = true;
                audioSource.clip = musicaAlgoz;
                audioSource.loop = true; // Boss fight costuma ser loop
                audioSource.Play();
            }
            else
            {
                // Sai do Modo Algoz: Verifica onde estamos e volta a música normal
                tocandoAlgoz = false;
                string cenaNome = SceneManager.GetActiveScene().name;

                if (cenaNome == nomeCenaMenu || cenaNome == nomeCenaCreditos)
                {
                    TocarMusicaMenu();
                }
                else
                {
                    // Resetamos o isGameplay para forçar a reinicialização da playlist da fase
                    isGameplay = false; 
                    IniciarMusicaGameplay();
                }
            }
        }

        // Sistema de abafar o som (Low Pass) continua funcionando independente da música
        float frequenciaAlvo = PauseMenu.isPaused ? frequenciaAbafada : frequenciaNormal;
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, frequenciaAlvo, Time.unscaledDeltaTime * velocidadeTransicao);

        // Playlist de gameplay só alterna se NÃO estiver no modo Algoz
        if (isGameplay && !tocandoAlgoz && !audioSource.isPlaying && !PauseMenu.isPaused)
        {
            musicaAtualGameplay = (musicaAtualGameplay == 1) ? 2 : 1;
            TocarFaixaAtual();
        }
    }
}