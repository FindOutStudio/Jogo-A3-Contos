using UnityEngine;

public class MenuConfiguracoes : MonoBehaviour
{
    [Header("Interface")]
    public GameObject painelConfig;
    public GameObject painelAnterior; 

    public void AbrirConfiguracoes()
    {
        if (painelAnterior != null) painelAnterior.SetActive(false);
        if (painelConfig != null) painelConfig.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        if (painelConfig != null) painelConfig.SetActive(false);
        if (painelAnterior != null) painelAnterior.SetActive(true);
    }
}