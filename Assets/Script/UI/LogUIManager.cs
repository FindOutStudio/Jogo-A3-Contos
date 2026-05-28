using System.Collections;
using UnityEngine;
using TMPro; 
using System.Collections.Generic;

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

    private void Update()
    {
        if (painelLog != null && painelLog.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FecharLog();
            }
        }
    }

    // ======= AQUI ELE RECEBE AS CORES DO INSPECTOR DO COLETÁVEL =======
    public void MostrarLog(int id, string nome, Color corNome, string lore, Color corTexto)
    {
        Time.timeScale = 0f;
        PauseMenu.isPaused = true; 

        painelLog.SetActive(true);
        tituloTexto.text = "LOG #" + id.ToString("00");
        conteudoTexto.text = ""; 

        painelLog.transform.localScale = Vector3.zero;

        // O código junta o nome e a fala com as cores que você escolheu no Inspector!
        string textoPronto = MontarTextoComCores(nome, corNome, lore, corTexto);

        StopAllCoroutines(); 
        StartCoroutine(AnimacaoAparecer(textoPronto));
    }

    private string MontarTextoComCores(string nome, Color corNome, string texto, Color corTexto)
    {
        string hexNome = ColorUtility.ToHtmlStringRGBA(corNome);
        string hexTexto = ColorUtility.ToHtmlStringRGBA(corTexto);

        if (string.IsNullOrEmpty(nome))
        {
            return $"<color=#{hexTexto}>{texto}</color>";
        }
        else
        {
            return $"<color=#{hexNome}>{nome}: </color><color=#{hexTexto}>{texto}</color>";
        }
    }

    public void FecharLog()
    {
        painelLog.SetActive(false);
        Time.timeScale = 1f; 
        PauseMenu.isPaused = false;
    }

    private IEnumerator AnimacaoAparecer(string textoFinal)
    {
        while (painelLog.transform.localScale.x < 0.99f)
        {
            painelLog.transform.localScale = Vector3.Lerp(painelLog.transform.localScale, Vector3.one, Time.unscaledDeltaTime * velocidadeEscala);
            yield return null; 
        }
        painelLog.transform.localScale = Vector3.one;

        StartCoroutine(EfeitoTextoMinecraft(textoFinal));
    }

    // O Analisador Inteligente: Faz o Glitch funcionar sem quebrar as cores!
    private IEnumerator EfeitoTextoMinecraft(string textoReal)
    {
        List<string> parsedText = new List<string>();
        List<bool> isTag = new List<bool>();
        int totalVisibleChars = 0;

        int idx = 0;
        while (idx < textoReal.Length)
        {
            if (textoReal[idx] == '<')
            {
                int closingIdx = textoReal.IndexOf('>', idx);
                if (closingIdx != -1)
                {
                    parsedText.Add(textoReal.Substring(idx, closingIdx - idx + 1));
                    isTag.Add(true);
                    idx = closingIdx + 1;
                    continue;
                }
            }
            parsedText.Add(textoReal[idx].ToString());
            isTag.Add(false);
            totalVisibleChars++;
            idx++;
        }

        for (int i = 0; i <= totalVisibleChars + tamanhoDoRastro; i++)
        {
            for (int j = 0; j < vezesParaEmbaralhar; j++)
            {
                string textoAtual = "";
                int visibleCount = 0;

                for (int k = 0; k < parsedText.Count; k++)
                {
                    if (isTag[k])
                    {
                        textoAtual += parsedText[k];
                    }
                    else
                    {
                        if (visibleCount < i - tamanhoDoRastro)
                        {
                            textoAtual += parsedText[k];
                        }
                        else if (visibleCount < i)
                        {
                            if (string.IsNullOrWhiteSpace(parsedText[k])) 
                                textoAtual += parsedText[k];
                            else 
                                textoAtual += caracteresRandom[Random.Range(0, caracteresRandom.Length)].ToString();
                        }
                        else
                        {
                            break; 
                        }
                        visibleCount++;
                    }
                }

                conteudoTexto.text = textoAtual;
                yield return new WaitForSecondsRealtime(tempoPorLetra); 
            }
        }

        conteudoTexto.text = textoReal;
    }
}