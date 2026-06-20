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

    // === A TRAVA DE SEGURANÇA ===
    // Impede a Unity de zerar seus saves sozinhos quando a cena carregar!
    private bool podeSalvar = false; 

    private void Start()
    {
        // 1. Puxa os saves PRIMEIRO. Se for a primeira vez que joga, vai vir 1f (Máximo).
        float volMaster = PlayerPrefs.GetFloat("VolumeMaster", 1f);
        float volSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);
        float volMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        // 2. Coloca os sliders nas posições certas ignorando os eventos automáticos
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

        // 3. Aplica os volumes iniciais nos Managers
        AudioListener.volume = volMaster;
        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(volSFX);
        if (MusicManager.instance != null) MusicManager.instance.SetVolumeEmTempoReal(volMusica);

        // 4. Libera o sistema para salvar as mudanças que VOCÊ fizer daqui pra frente!
        podeSalvar = true;
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
        if (!podeSalvar) return; // Se a cena acabou de abrir, ignora a falsiane da Unity!

        AudioListener.volume = valor; 
        PlayerPrefs.SetFloat("VolumeMaster", valor);
        PlayerPrefs.Save();
    }

    public void MudarVolumeSFX(float valor)
    {
        if (!podeSalvar) return; // Proteção

        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(valor);
        PlayerPrefs.SetFloat("VolumeSFX", valor);
        PlayerPrefs.Save();
    }

    public void MudarVolumeMusica(float valor)
    {
        if (!podeSalvar) return; // Proteção

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
        }
    }
}