using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

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

        // O PULO DO GATO CONTRA O BUG DE ZERAR: 
        // Removemos o "ouvido" do Slider rapidinho, mudamos o valor pro seu Save, e só então ligamos o ouvido de novo!
        if (sliderMaster != null)
        {
            sliderMaster.onValueChanged.RemoveAllListeners(); 
            sliderMaster.value = volMaster;
            sliderMaster.onValueChanged.AddListener(MudarVolumeMaster);
        }
        if (sliderSFX != null)
        {
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.value = volSFX;
            sliderSFX.onValueChanged.AddListener(MudarVolumeSFX);
            
            AdicionarVigiaDeMouse(sliderSFX);
        }
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.value = volMusica;
            sliderMusica.onValueChanged.AddListener(MudarVolumeMusica);
        }

        AudioListener.volume = volMaster;
        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(volSFX);
        if (MusicManager.instance != null) MusicManager.instance.SetVolumeEmTempoReal(volMusica);
    }

    private void AdicionarVigiaDeMouse(Slider slider)
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

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
    }

    public void MudarVolumeMusica(float valor)
    {
        if (MusicManager.instance != null) MusicManager.instance.SetVolumeEmTempoReal(valor); 
        PlayerPrefs.SetFloat("VolumeMusica", valor);
        PlayerPrefs.Save();
    }

    private void TocarFeedbackAoSoltar()
    {
        if (SoundManager.instance != null && SoundManager.instance.sfxSource != null) 
        {
            AudioClip clipeParaTocar = somDeTesteSFX != null ? somDeTesteSFX : SoundManager.instance.uiSelecao;

            if (clipeParaTocar != null)
            {
                SoundManager.instance.sfxSource.ignoreListenerPause = true; 
                
                float volumeAtual = sliderSFX != null ? sliderSFX.value : SoundManager.instance.volumeGlobalSFX;
                
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