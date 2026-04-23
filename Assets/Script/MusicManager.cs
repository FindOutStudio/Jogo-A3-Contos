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

    [Header("Efeito de Pause")]
    public float frequenciaNormal = 22000f;
    public float frequenciaAbafada = 800f;
    public float velocidadeTransicao = 10f;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPass;
    private bool isGameplay = false;
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
        
        if (audioSource.clip != musicaMenu)
        {
            audioSource.clip = musicaMenu;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void IniciarMusicaGameplay()
    {
        if (isGameplay) return; 

        isGameplay = true;
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

        if (isGameplay && !audioSource.isPlaying && !PauseMenu.isPaused)
        {
            musicaAtualGameplay = (musicaAtualGameplay == 1) ? 2 : 1;
            TocarFaixaAtual();
        }
    }
}