using UnityEngine;
using UnityEngine.UI;

public class ControleDeVolume : MonoBehaviour
{
    [Header("Sliders de Volume")]
    public Slider sliderMaster;
    public Slider sliderSFX;
    public Slider sliderMusica; 

    [Header("Feedback Sonoro")]
    [Tooltip("Impede o som de tocar como metralhadora enquanto arrasta (ex: 0.15 segundos)")]
    public float cooldownSomFeedback = 0.15f;
    private float tempoUltimoSom = 0f;

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

        // Garante que o volume Master inicial seja aplicado
        AudioListener.volume = volMaster;
    }

    public void MudarVolumeMaster(float valor)
    {
        AudioListener.volume = valor; 
        PlayerPrefs.SetFloat("VolumeMaster", valor);
        PlayerPrefs.Save();

        // Toca o bip para o jogador testar a nova altura global
        TocarFeedbackSonoro();
    }

    public void MudarVolumeSFX(float valor)
    {
        if (SoundManager.instance != null) SoundManager.instance.AtualizarVolumeGlobalSFX(valor);
        PlayerPrefs.SetFloat("VolumeSFX", valor);
        PlayerPrefs.Save();

        // Toca o bip para o jogador testar a altura dos efeitos
        TocarFeedbackSonoro();
    }

    public void MudarVolumeMusica(float valor)
    {
        // A música geralmente não precisa de bip porque o feedback é a própria música tocando
        PlayerPrefs.SetFloat("VolumeMusica", valor);
        PlayerPrefs.Save();
    }

    private void TocarFeedbackSonoro()
    {
        // Só toca o som se já passou o tempo do cooldown
        // Usamos unscaledTime porque as configurações abrem com o jogo pausado (Time.timeScale = 0)
        if (Time.unscaledTime - tempoUltimoSom > cooldownSomFeedback)
        {
            if (SoundManager.instance != null) 
            {
                // Reutilizamos o som de clique padrão da sua UI
                SoundManager.instance.TocarMaozinha(); 
            }
            tempoUltimoSom = Time.unscaledTime;
        }
    }
}