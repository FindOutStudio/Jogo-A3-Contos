using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LogColetavel : MonoBehaviour
{
    [Header("Dados do Log")]
    [Tooltip("O número de identificação único desse log (Ex: 1 para o Log #1)")]
    public int logID;
    
    [Header("Estilo do Texto (Cores)")]
    [Tooltip("Nome de quem escreveu o log (Ex: SISTEMA, ALGOZ). Deixe vazio para esconder.")]
    public string nomePersonagem;
    public Color corDoNome = Color.red; 
    
    [Space(10)]
    [Tooltip("O texto da história que vai aparecer no pop-up")]
    [TextArea(5, 10)] 
    public string textoDoLog;
    public Color corDoTexto = Color.white;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        if (SoundManager.instance != null) SoundManager.instance.TocarLog();
        
        PlayerPrefs.SetInt("LogColetado_" + logID, 1);
        PlayerPrefs.Save();

        // Agora ele envia as variáveis de nome e cor mastigadinhas pra UI!
        if (LogUIManager.instance != null)
        {
            LogUIManager.instance.MostrarLog(logID, nomePersonagem, corDoNome, textoDoLog, corDoTexto);
        }

        Destroy(gameObject);
    }
}