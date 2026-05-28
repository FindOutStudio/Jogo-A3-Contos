using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Fontes de Áudio (Audio Sources)")]
    public AudioSource sfxSource;
    public AudioSource uiSource; 
    public AudioSource ambienteSource;
    public AudioSource puxaoSource; 

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
        // ======= O PASSE VIP DA UI =======
        // Garante que os cliques de botões funcionam mesmo no Pause global!
        if (uiSource != null) uiSource.ignoreListenerPause = true;

        volumeGlobalSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);

        if (somAmbiente != null && ambienteSource != null)
        {
            ambienteSource.clip = somAmbiente;
            ambienteSource.loop = true;
            ambienteSource.volume = volumeSomAmbiente * volumeGlobalSFX; 
            ambienteSource.Play();
        }
    }

    public void TocarSFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null) 
        {
            sfxSource.PlayOneShot(clip, volumeScale * volumeGlobalSFX);
        }
    }

    public void TocarSFXUI(AudioClip clip, float volumeScale)
    {
        if (clip != null && uiSource != null) 
        {
            uiSource.PlayOneShot(clip, volumeScale * volumeGlobalSFX);
        }
    }

    public void TocarSFX(AudioClip clip)
    {
        if (clip == null) return;
        
        if (clip == uiHover || clip == uiSelecao)
        {
            float volumeDefinido = (clip == uiHover) ? volumeUiHover : volumeUiSelecao;
            TocarSFXUI(clip, volumeDefinido);
            return; 
        }
        
        TocarSFX(clip, 1f);
    }

    public void TocarSom3D(AudioClip clip, Vector3 posicao, float volume)
    {
        if (clip != null) 
        {
            AudioSource.PlayClipAtPoint(clip, posicao, volume * volumeGlobalSFX);
        }
    }

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

    public void AtualizarVolumeGlobalSFX(float novoVolume)
    {
        volumeGlobalSFX = novoVolume;
        
        if (ambienteSource != null) 
            ambienteSource.volume = volumeSomAmbiente * volumeGlobalSFX;
            
        if (puxaoSource != null && puxaoSource.isPlaying) 
            puxaoSource.volume = volumeLancadorPuxar * volumeGlobalSFX;
    }

    // ======= A SOLUÇÃO ABSOLUTA =======
    public void PausarSonsJogo()
    {
        // Desliga o "ouvido" da câmara globalmente. Nada no mundo toca!
        AudioListener.pause = true;
    }

    public void RetomarSonsJogo()
    {
        // Devolve a audição ao mundo!
        AudioListener.pause = false;
    }
}