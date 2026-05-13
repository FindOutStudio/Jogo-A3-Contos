using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LogColetavel : MonoBehaviour
{
    [Header("Dados do Log")]
    [Tooltip("O número de identificação único desse log (Ex: 1 para o Log #1)")]
    public int logID;
    
    [Tooltip("O texto da história que vai aparecer no pop-up")]
    [TextArea(5, 10)] 
    public string textoDoLog;

    // A função Start() foi removida daqui! Agora ele sempre vai nascer com a fase.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        // 1. Salva no dispositivo
        PlayerPrefs.SetInt("LogColetado_" + logID, 1);
        PlayerPrefs.Save();

        // 2. CHAMA A UI PARA APARECER NA TELA E PAUSAR!
        if (LogUIManager.instance != null)
        {
            LogUIManager.instance.MostrarLog(logID, textoDoLog);
        }

        // 3. Destrói o coletável da fase
        Destroy(gameObject);
    }
}