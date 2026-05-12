using UnityEngine;
using UnityEngine.UI;

public class MenuConfiguracoes : MonoBehaviour
{
    [Header("Interface (Opcional para o Menu Inicial)")]
    public GameObject painelConfig;
    public GameObject painelAnterior; 

    [Header("Controle de Áudio")]
    public Slider sliderMusica;

    private void Start()
    {
        // Carrega o volume salvo ou define 1 (100%) como padrão
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeMusica", 1f);
        
        if (sliderMusica != null)
        {
            sliderMusica.value = volumeSalvo;
            // Configura o evento de mudança sem precisar ir no Inspector
            sliderMusica.onValueChanged.AddListener(MudarVolume);
        }

        // No Menu Inicial, como o painel já fica aberto, não escondemos nada
        // Só escondemos se for o caso do Painel de Pause
        if (painelConfig != null && painelAnterior != null) 
        {
            painelConfig.SetActive(false);
        }
    }

    public void AbrirConfiguracoes()
    {
        // Só executa a troca se os painéis existirem (Caso do Pause)
        if (painelAnterior != null) painelAnterior.SetActive(false);
        if (painelConfig != null) painelConfig.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        if (painelConfig != null) painelConfig.SetActive(false);
        if (painelAnterior != null) painelAnterior.SetActive(true);
    }

    public void MudarVolume(float novoVolume)
    {
        PlayerPrefs.SetFloat("VolumeMusica", novoVolume);
        PlayerPrefs.Save();

        if (MusicManager.instance != null)
        {
            MusicManager.instance.AtualizarVolume();
        }
    }
}