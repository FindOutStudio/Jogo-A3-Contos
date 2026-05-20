using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Fontes de Áudio (Audio Sources)")]
    public AudioSource sfxSource;
    public AudioSource ambienteSource;
    public AudioSource puxaoSource; 

    // O multiplicador invisível que o Slider vai controlar
    [HideInInspector] public float volumeGlobalSFX = 1f;

    [Header("Sons da Fase (Obstáculos 3D)")]
    public AudioClip obstaculoSerra;
    [Range(0f, 1f)] public float volumeSerra = 1f;
    
    public AudioClip obstaculoLaser;
    [Range(0f, 1f)] public float volumeLaser = 1f;

    public AudioClip obstaculoBomba;
    [Range(0f, 1f)] public float volumeBomba = 1f;

    [Header("Sons de UI (Menus)")]
    public AudioClip uiHover;
    [Range(0f, 1f)] public float volumeUiHover = 0.8f;
    
    public AudioClip uiSelecao;
    [Range(0f, 1f)] public float volumeUiSelecao = 1f;

    [Header("Sons de UX (Feedback)")]
    public AudioClip uxMaozinha;
    [Range(0f, 1f)] public float volumeUxMaozinha = 0.9f;
    
    public AudioClip uxSlowMotion;
    [Range(0f, 1f)] public float volumeUxSlowMotion = 0.8f;
    
    public AudioClip uxErro;
    [Range(0f, 1f)] public float volumeUxErro = 0.7f;
    
    public AudioClip uxVitoria;
    [Range(0f, 1f)] public float volumeUxVitoria = 1f;

    [Header("Sons do Player")]
    public AudioClip playerMorte;
    [Range(0f, 1f)] public float volumePlayerMorte = 1f;
    
    public AudioClip playerPegarRaio; 
    [Range(0f, 1f)] public float volumePlayerPegarRaio = 0.9f;
    
    public AudioClip playerPegarLog;
    [Range(0f, 1f)] public float volumePlayerPegarLog = 0.9f;

    [Header("Sons do Lançador")]
    public AudioClip lancadorPuxar;
    [Range(0f, 1f)] public float volumeLancadorPuxar = 0.8f;
    
    public AudioClip lancadorSoltar;
    [Range(0f, 1f)] public float volumeLancadorSoltar = 1f;

    [Header("Ambiência Geral")]
    public AudioClip somAmbiente;
    [Range(0f, 1f)] public float volumeSomAmbiente = 0.4f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 1. Puxa o volume salvo do PC do jogador
        volumeGlobalSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);

        // 2. Aplica o volume na ambiência considerando a configuração do produtor * a configuração do jogador
        if (somAmbiente != null && ambienteSource != null)
        {
            ambienteSource.clip = somAmbiente;
            ambienteSource.loop = true;
            ambienteSource.volume = volumeSomAmbiente * volumeGlobalSFX; 
            ambienteSource.Play();
        }
    }

    // === FUNÇÕES MESTRES COM MATEMÁTICA DE VOLUME ===
    public void TocarSFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null) 
        {
            sfxSource.PlayOneShot(clip, volumeScale * volumeGlobalSFX);
        }
    }

    public void TocarSFX(AudioClip clip)
    {
        if (clip == null) return;
        float volumeDefinido = 1f;
        if (clip == uiHover) volumeDefinido = volumeUiHover;
        else if (clip == uiSelecao) volumeDefinido = volumeUiSelecao;
        TocarSFX(clip, volumeDefinido);
    }

    public void TocarSom3D(AudioClip clip, Vector3 posicao, float volume)
    {
        if (clip != null) 
        {
            AudioSource.PlayClipAtPoint(clip, posicao, volume * volumeGlobalSFX);
        }
    }

    // === ATALHOS ===
    public void TocarMorte() { TocarSFX(playerMorte, volumePlayerMorte); }
    public void TocarRaio() { TocarSFX(playerPegarRaio, volumePlayerPegarRaio); }
    public void TocarLog() { TocarSFX(playerPegarLog, volumePlayerPegarLog); }
    public void TocarSoltar() { TocarSFX(lancadorSoltar, volumeLancadorSoltar); }
    public void TocarMaozinha() { TocarSFX(uxMaozinha, volumeUxMaozinha); }
    public void TocarSlowMotion() { TocarSFX(uxSlowMotion, volumeUxSlowMotion); }
    public void TocarErro() { TocarSFX(uxErro, volumeUxErro); }
    public void TocarVitoria() { TocarSFX(uxVitoria, volumeUxVitoria); }

    public void TocarPuxar() 
    { 
        if (lancadorPuxar != null && puxaoSource != null)
        {
            puxaoSource.clip = lancadorPuxar;
            puxaoSource.volume = volumeLancadorPuxar * volumeGlobalSFX;
            puxaoSource.Play();
        }
    }

    public void PararPuxar()
    {
        if (puxaoSource != null && puxaoSource.isPlaying) puxaoSource.Stop();
    }

    // === ATUALIZAR EM TEMPO REAL PELO SLIDER ===
    public void AtualizarVolumeGlobalSFX(float novoVolume)
    {
        volumeGlobalSFX = novoVolume;
        
        if (ambienteSource != null) 
            ambienteSource.volume = volumeSomAmbiente * volumeGlobalSFX;
            
        if (puxaoSource != null && puxaoSource.isPlaying) 
            puxaoSource.volume = volumeLancadorPuxar * volumeGlobalSFX;
    }
}