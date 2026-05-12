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
    private bool tocandoAlgoz = false;
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
            
            // ======= A MÁGICA NOVA AQUI =======
            AtualizarVolume(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ======= FUNÇÃO NOVA =======
    public void AtualizarVolume()
    {
        if (audioSource != null)
        {
            // Puxa o volume salvo lá no menu de config
            audioSource.volume = PlayerPrefs.GetFloat("VolumeMusica", 1f);
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
        audioSource.clip = (musicaAtualGameplay == 1) ? musicaGameplay1 : musicaGameplay2;
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