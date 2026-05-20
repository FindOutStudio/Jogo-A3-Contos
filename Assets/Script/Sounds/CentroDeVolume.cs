using UnityEngine;
using UnityEngine.UI;

public class ControleDeVolume : MonoBehaviour
{
    [Header("Sliders de Volume")]
    public Slider sliderMaster;
    public Slider sliderSFX;
    public Slider sliderMusica; 

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
        }
        if (sliderMusica != null)
        {
            sliderMusica.value = volMusica;
            sliderMusica.onValueChanged.AddListener(MudarVolumeMusica);
        }

        MudarVolumeMaster(volMaster);
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
        PlayerPrefs.SetFloat("VolumeMusica", valor);
        PlayerPrefs.Save();

        // Puxando exatamente a lógica que você já tinha no script antigo!
        if (MusicManager.instance != null)
        {
            MusicManager.instance.AtualizarVolume();
        }
    }
}