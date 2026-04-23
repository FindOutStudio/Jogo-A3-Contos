using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    // Tornamos o LevelTimer uma "instância global" para ser fácil de achar
    public static LevelTimer instance; 

    public float tempoInicial = 60f;
    public TextMeshProUGUI textoTempo;
    private float tempoAtual;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        tempoAtual = tempoInicial;
    }

    void Update()
    {
        if (tempoAtual > 0)
        {
            tempoAtual -= Time.deltaTime;
            textoTempo.text = Mathf.CeilToInt(tempoAtual).ToString();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ======= A MÁGICA NOVA ESTÁ AQUI =======
    public void AdicionarTempo(float tempoExtra)
    {
        tempoAtual += tempoExtra;
        
        // Atualiza o texto na mesma hora pra dar um feedback rápido pro jogador
        textoTempo.text = Mathf.CeilToInt(tempoAtual).ToString();
    }
}