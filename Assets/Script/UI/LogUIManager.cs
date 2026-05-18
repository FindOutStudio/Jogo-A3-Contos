using System.Collections;
using UnityEngine;
using TMPro; 

public class LogUIManager : MonoBehaviour
{
    public static LogUIManager instance;

    [Header("Elementos da UI")]
    public GameObject painelLog;
    public TextMeshProUGUI tituloTexto;   
    public TextMeshProUGUI conteudoTexto; 
    
    [Header("Animações")]
    public float velocidadeEscala = 10f; 
    
    [Header("Efeito Máquina Hacker com Rastro")]
    [Tooltip("Tempo em segundos que os caracteres piscam (ex: 0.02)")]
    public float tempoPorLetra = 0.02f; 
    [Tooltip("Velocidade que a máquina avança (menos vezes = avança mais rápido)")]
    public int vezesParaEmbaralhar = 2; 
    [Tooltip("Quantas letras ficam bugadas juntas na 'ponta' do texto formando o rastro?")]
    public int tamanhoDoRastro = 4; 
    private string caracteresRandom = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*?";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (painelLog != null) painelLog.SetActive(false);
    }

    // ======== ADICIONAMOS ISSO AQUI ========
    private void Update()
    {
        // Se o painel do Log estiver ligado na tela E o jogador apertar ESC
        if (painelLog != null && painelLog.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FecharLog();
            }
        }
    }
    // =======================================

    public void MostrarLog(int id, string lore)
    {
        Time.timeScale = 0f;
        PauseMenu.isPaused = true; 

        painelLog.SetActive(true);
        tituloTexto.text = "LOG #" + id.ToString("00");
        conteudoTexto.text = ""; 

        painelLog.transform.localScale = Vector3.zero;

        StopAllCoroutines(); 
        StartCoroutine(AnimacaoAparecer(lore));
    }

    public void FecharLog()
    {
        painelLog.SetActive(false);
        Time.timeScale = 1f; 
        PauseMenu.isPaused = false;
    }

    private IEnumerator AnimacaoAparecer(string lore)
    {
        while (painelLog.transform.localScale.x < 0.99f)
        {
            painelLog.transform.localScale = Vector3.Lerp(painelLog.transform.localScale, Vector3.one, Time.unscaledDeltaTime * velocidadeEscala);
            yield return null; 
        }
        painelLog.transform.localScale = Vector3.one;

        StartCoroutine(EfeitoTextoMinecraft(lore));
    }

    private IEnumerator EfeitoTextoMinecraft(string textoReal)
    {
        for (int i = 0; i <= textoReal.Length + tamanhoDoRastro; i++)
        {
            for (int j = 0; j < vezesParaEmbaralhar; j++)
            {
                string textoAtual = "";
                
                for (int k = 0; k < textoReal.Length; k++)
                {
                    if (k < i - tamanhoDoRastro)
                    {
                        textoAtual += textoReal[k];
                    }
                    else if (k < i)
                    {
                        if (textoReal[k] == ' ' || textoReal[k] == '\n') 
                            textoAtual += textoReal[k];
                        else 
                            textoAtual += caracteresRandom[Random.Range(0, caracteresRandom.Length)];
                    }
                    else
                    {
                        break; 
                    }
                }

                conteudoTexto.text = textoAtual;
                yield return new WaitForSecondsRealtime(tempoPorLetra); 
            }
        }

        conteudoTexto.text = textoReal;
    }
}