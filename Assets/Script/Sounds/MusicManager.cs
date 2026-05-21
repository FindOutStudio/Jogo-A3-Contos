using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Configurações de Cena")]
    public string nomeCenaMenu = "MainMenu";
    public string nomeCenaCreditos = "Credits";

    [Header("Faixas de Áudio e Volumes")]
    public AudioClip musicaMenu;
    [Range(0f, 1f)] public float volumeMusicaMenu = 1f;
    
    public AudioClip musicaGameplay1;
    [Range(0f, 1f)] public float volumeGameplay1 = 1f;
    
    public AudioClip musicaGameplay2;
    [Range(0f, 1f)] public float volumeGameplay2 = 1f;
    
    public AudioClip musicaAlgoz;
    [Range(0f, 1f)] public float volumeAlgoz = 1f;

    [Header("Efeito de Pause")]
    public float frequenciaNormal = 22000f;
    public float frequenciaAbafada = 800f;
    public float velocidadeTransicao = 10f;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPass;
    private bool isGameplay = false;
    private bool tocandoAlgoz = false;
    private int musicaAtualGameplay = 1;
    
    // Variável invisível que guarda a preferência do jogador
    private float volumeGlobalMusica = 1f; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            lowPass = GetComponent<AudioLowPassFilter>();
            
            lowPass.cutoffFrequency = frequenciaNormal;
            
            AtualizarVolume(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ======= FUNÇÃO ATUALIZADA (MATEMÁTICA DA MIXAGEM) =======
    public void AtualizarVolume()
    {
        // 1. Puxa o volume salvo lá no menu de config (Multiplicador do Jogador)
        volumeGlobalMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        // 2. Atualiza o volume da música que está tocando agora na mesma hora
        if (audioSource != null && audioSource.clip != null)
        {
            if (audioSource.clip == musicaMenu) audioSource.volume = volumeMusicaMenu * volumeGlobalMusica;
            else if (audioSource.clip == musicaGameplay1) audioSource.volume = volumeGameplay1 * volumeGlobalMusica;
            else if (audioSource.clip == musicaGameplay2) audioSource.volume = volumeGameplay2 * volumeGlobalMusica;
            else if (audioSource.clip == musicaAlgoz) audioSource.volume = volumeAlgoz * volumeGlobalMusica;
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
            audioSource.volume = volumeMusicaMenu * volumeGlobalMusica; // Aplica o multiplicador
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void IniciarMusicaGameplay()
    {
        if (isGameplay && !tocandoAlgoz) return; 

        isGameplay = true;
        tocandoAlgoz = false;
        audioSource.loop = false;
        musicaAtualGameplay = Random.Range(1, 3); 

        TocarFaixaAtual();
    }

    private void TocarFaixaAtual()
    {
        if (musicaAtualGameplay == 1)
        {
            audioSource.clip = musicaGameplay1;
            audioSource.volume = volumeGameplay1 * volumeGlobalMusica;
        }
        else
        {
            audioSource.clip = musicaGameplay2;
            audioSource.volume = volumeGameplay2 * volumeGlobalMusica;
        }
        
        audioSource.Play();
    }

    private void Update()
    {
        float frequenciaAlvo = PauseMenu.isPaused ? frequenciaAbafada : frequenciaNormal;
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, frequenciaAlvo, Time.unscaledDeltaTime * velocidadeTransicao);

        if (isGameplay && !tocandoAlgoz && !audioSource.isPlaying && !PauseMenu.isPaused)
        {
            musicaAtualGameplay = (musicaAtualGameplay == 1) ? 2 : 1;
            TocarFaixaAtual();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!tocandoAlgoz)
            {
                tocandoAlgoz = true;
                audioSource.clip = musicaAlgoz;
                audioSource.volume = volumeAlgoz * volumeGlobalMusica; // Aplica o multiplicador do Algoz
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                tocandoAlgoz = false;
                string cenaNome = SceneManager.GetActiveScene().name;

                if (cenaNome == nomeCenaMenu || cenaNome == nomeCenaCreditos)
                {
                    TocarMusicaMenu();
                }
                else
                {
                    isGameplay = false; 
                    IniciarMusicaGameplay();
                }
            }
        }
    }
}