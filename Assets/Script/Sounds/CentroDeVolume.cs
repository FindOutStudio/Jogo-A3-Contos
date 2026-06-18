using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Biblioteca essencial para detectar o clique do mouse na UI

public class ControleDeVolume : MonoBehaviour
{
    [Header("Sliders de Volume")]
    public Slider sliderMaster;
    public Slider sliderSFX;
    public Slider sliderMusica; 

    [Header("Feedback Sonoro")]
    [Tooltip("Se deixar vazio, ele puxa o som 'uiSelecao' automático do seu SoundManager!")]
    public AudioClip somDeTesteSFX;

    private void Start()
    {
        float volMaster = PlayerPrefs.GetFloat("VolumeMaster", 1f);
        float volSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);
        float volMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        if (sliderMaster != null)
        {
            sliderMaster.value = volMaster;
            sliderMaster.onValueChanged.AddListener(MudarVolumeMaster);
        }
        if (sliderSFX != null)
        {
            sliderSFX.value = volSFX;
            sliderSFX.onValueChanged.AddListener(MudarVolumeSFX);
            
            // A MÁGICA AQUI: Ensina a Unity a disparar o som SOMENTE quando você soltar o clique!
            AdicionarVigiaDeMouse(sliderSFX);
        }
        if (sliderMusica != null)
        {
            sliderMusica.value = volMusica;
            sliderMusica.onValueChanged.AddListener(MudarVolumeMusica);
        }

        // Aplica os volumes iniciais
        AudioListener.volume = volMaster;
        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(volSFX);
        if (MusicManager.instance != null) MusicManager.instance.SetVolumeEmTempoReal(volMusica);
    }

    // ====== FUNÇÃO QUE CRIA O DETECTOR DE SOLTAR ======
    private void AdicionarVigiaDeMouse(Slider slider)
    {
        // Pega ou adiciona o componente EventTrigger no Slider
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

        // Cria a regra: "EventTriggerType.PointerUp" (Quando o dedo/mouse levanta)
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { TocarFeedbackAoSoltar(); });
        
        trigger.triggers.Add(entry);
    }

    public void MudarVolumeMaster(float valor)
    {
        AudioListener.volume = valor; 
        PlayerPrefs.SetFloat("VolumeMaster", valor);
        PlayerPrefs.Save();
    }

    public void MudarVolumeSFX(float valor)
    {
        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(valor);
        PlayerPrefs.SetFloat("VolumeSFX", valor);
        PlayerPrefs.Save();
        
        // Removi o som daqui! Ele não vai mais tocar loucamente enquanto você arrasta.
    }

    public void MudarVolumeMusica(float valor)
    {
        if (MusicManager.instance != null) MusicManager.instance.SetVolumeEmTempoReal(valor); 
        PlayerPrefs.SetFloat("VolumeMusica", valor);
        PlayerPrefs.Save();
    }

    // ====== FUNÇÃO QUE TOCA O SOM ======
    private void TocarFeedbackAoSoltar()
    {
        if (SoundManager.instance != null && SoundManager.instance.sfxSource != null) 
        {
            // Se você esqueceu de arrastar o som no Inspector, ele puxa o do SoundManager para salvar a pátria
            AudioClip clipeParaTocar = somDeTesteSFX != null ? somDeTesteSFX : SoundManager.instance.uiSelecao;

            if (clipeParaTocar != null)
            {
                // Força tocar mesmo com o jogo pausado (Time.timeScale = 0)
                SoundManager.instance.sfxSource.ignoreListenerPause = true; 
                
                // Toca usando o volume ATUAL do slider para você saber exatamente a altura em que ficou
                float volumeAtual = sliderSFX != null ? sliderSFX.value : SoundManager.instance.volumeGlobalSFX;
                
                // Só toca se não estiver mutado (volume 0), se não é inútil!
                if (volumeAtual > 0.01f)
                {
                    SoundManager.instance.sfxSource.PlayOneShot(clipeParaTocar, volumeAtual);
                }
            }
            else
            {
                Debug.LogWarning("Chefe, o áudio de feedback tá vazio tanto aqui quanto no SoundManager!");
            }
        }
    }
}