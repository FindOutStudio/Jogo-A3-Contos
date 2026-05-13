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

    public void MostrarLog(int id, string lore)
    {
        Time.timeScale = 0f;
        PauseMenu.isPaused = true; 

        painelLog.SetActive(true);
        tituloTexto.text = "LOG #" + id;
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
        // A máquina vai avançar até o tamanho do texto + o rastro, para garantir que 
        // o rastro saia da tela e as últimas letras se solidifiquem
        for (int i = 0; i <= textoReal.Length + tamanhoDoRastro; i++)
        {
            // Pisca as letras do rastro X vezes antes de avançar uma casa
            for (int j = 0; j < vezesParaEmbaralhar; j++)
            {
                string textoAtual = "";
                
                for (int k = 0; k < textoReal.Length; k++)
                {
                    // 1. Letras que ficaram para trás do rastro estão "frias/sólidas"
                    if (k < i - tamanhoDoRastro)
                    {
                        textoAtual += textoReal[k];
                    }
                    // 2. Letras DENTRO do rastro ficam malucas
                    else if (k < i)
                    {
                        if (textoReal[k] == ' ' || textoReal[k] == '\n') 
                            textoAtual += textoReal[k];
                        else 
                            textoAtual += caracteresRandom[Random.Range(0, caracteresRandom.Length)];
                    }
                    // 3. Letras que o rastro ainda não alcançou ficam invisíveis
                    else
                    {
                        break; 
                    }
                }

                conteudoTexto.text = textoAtual;
                yield return new WaitForSecondsRealtime(tempoPorLetra); 
            }
        }

        // Fim da animação: crava o texto exato na tela
        conteudoTexto.text = textoReal;
    }
}