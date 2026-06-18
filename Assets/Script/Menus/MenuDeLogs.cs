using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Necessário para a Corotina da animação
using System.Collections.Generic; 

[System.Serializable]
public class LogLore
{
    public int logID;
    public Sprite spriteDesbloqueado; 
    public Sprite spriteBloqueado;    
    
    [Header("Bate-Papo (Textos e Cores)")]
    public List<LinhaDeDialogo> batePapo;
}

public class MenuDeLogs : MonoBehaviour
{
    [Header("Navegação de Telas")]
    public GameObject telaMenuPrincipal;
    public GameObject telaGradeLogs; 
    public GameObject telaLeituraLog; 

    [Header("Textos da Tela de Leitura")]
    public TextMeshProUGUI tituloLeitura;
    public TextMeshProUGUI conteudoLeitura;

    [Header("Gerador da Grade")]
    public Transform containerDeBotoes;
    public GameObject botaoLogPrefab; 

    [Header("Banco de Dados")]
    public LogLore[] todosOsLogs;

    [Header("=== Sistema do Log Secreto ===")]
    public int idLogSecreto = 7;
    public int quantidadeLogsParaDesbloquear = 6;

    private void Start()
    {
        if (telaGradeLogs != null) telaGradeLogs.SetActive(false);
        if (telaLeituraLog != null) telaLeituraLog.SetActive(false);
    }

    public void AbrirMenuLogs()
    {
        telaMenuPrincipal.SetActive(false);
        telaGradeLogs.SetActive(true);
        GerarBotoesDeLog(); 
    }

    public void FecharMenuLogs() 
    {
        telaGradeLogs.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }

    public void GerarBotoesDeLog()
    {
        // Garante que o Layout Group (A grade) esteja ligado para organizar os botões novos
        LayoutGroup layout = containerDeBotoes.GetComponent<LayoutGroup>();
        if (layout != null) layout.enabled = true;

        foreach (Transform child in containerDeBotoes) Destroy(child.gameObject);

        // Retorna TRUE se a gente acabou de desbloquear o log 7 E ainda não viu o meme
        bool deveTocarMeme = ChecarDesbloqueioDoLogSecreto();

        RectTransform rtLog6 = null;
        RectTransform rtLog7 = null;

        foreach (LogLore log in todosOsLogs)
        {
            GameObject novoBotao = Instantiate(botaoLogPrefab, containerDeBotoes);
            Button componenteBotao = novoBotao.GetComponent<Button>();
            Image imagemDoBotao = novoBotao.GetComponent<Image>(); 

            bool foiColetado = PlayerPrefs.GetInt("LogColetado_" + log.logID, 0) == 1;

            if (foiColetado)
            {
                imagemDoBotao.sprite = log.spriteDesbloqueado; 
                componenteBotao.interactable = true; 
                
                string textoPassado = MontarConversa(log.batePapo);
                componenteBotao.onClick.AddListener(() => AbrirLeitura(log.logID, textoPassado));
            }
            else
            {
                imagemDoBotao.sprite = log.spriteBloqueado; 
                componenteBotao.interactable = false; 
            }

            // Pega secretamente a referência dos botões 6 e 7 para podermos animar
            if (log.logID == 6) rtLog6 = novoBotao.GetComponent<RectTransform>();
            if (log.logID == 7) rtLog7 = novoBotao.GetComponent<RectTransform>();
        }

        // Se estiver na hora do show e os botões existirem, roda a animação!
        if (deveTocarMeme && rtLog6 != null && rtLog7 != null)
        {
            StartCoroutine(AnimarMemeSixSeven(rtLog6, rtLog7, layout));
        }
    }

    private bool ChecarDesbloqueioDoLogSecreto()
    {
        int logsColetados = 0;

        for (int i = 1; i <= quantidadeLogsParaDesbloquear; i++)
        {
            if (PlayerPrefs.GetInt("LogColetado_" + i, 0) == 1)
            {
                logsColetados++;
            }
        }

        if (logsColetados >= quantidadeLogsParaDesbloquear)
        {
            // Checa se é a primeira vez que estamos desbloqueando o log 7
            if (PlayerPrefs.GetInt("LogColetado_" + idLogSecreto, 0) == 0)
            {
                PlayerPrefs.SetInt("LogColetado_" + idLogSecreto, 1);
                PlayerPrefs.SetInt("MemeSixSevenVisto", 0); // Deixa o meme engatilhado
                PlayerPrefs.Save();
            }

            // Se o meme está engatilhado, avisa para tocar e depois marca como visto
            if (PlayerPrefs.GetInt("MemeSixSevenVisto", 0) == 0)
            {
                PlayerPrefs.SetInt("MemeSixSevenVisto", 1);
                PlayerPrefs.Save();
                return true; 
            }
        }
        
        return false;
    }

    // ====== A COROTINA DO EASTER EGG ======
    private IEnumerator AnimarMemeSixSeven(RectTransform rt6, RectTransform rt7, LayoutGroup layout)
    {
        // Espera a Unity desenhar o frame atual para a grade organizar eles na posição certa
        yield return new WaitForEndOfFrame();
        
        // Desliga a grade temporariamente para ela não impedir a gente de mover os botões
        if (layout != null) layout.enabled = false;

        float tempo = 0f;
        float duracaoAnimacao = 5f; // Eles vão dançar por 5 segundos
        
        Vector2 posOriginal6 = rt6.anchoredPosition;
        Vector2 posOriginal7 = rt7.anchoredPosition;
        
        float velocidadeOscilacao = 25f; // O quão rápido eles sobem e descem
        float alturaPulo = 15f; // Quantos pixels de altura eles vão bater

        while (tempo < duracaoAnimacao)
        {
            // Se o jogador fechar o menu no meio da dança, a gente cancela para não dar erro
            if (rt6 == null || rt7 == null) yield break; 

            tempo += Time.deltaTime;
            
            // O Log 6 usa o Seno (Vai pra cima), o Log 7 usa o -Seno (Vai pra baixo), criando a gangorra perfeita!
            rt6.anchoredPosition = posOriginal6 + new Vector2(0, Mathf.Sin(tempo * velocidadeOscilacao) * alturaPulo);
            rt7.anchoredPosition = posOriginal7 + new Vector2(0, -Mathf.Sin(tempo * velocidadeOscilacao) * alturaPulo);

            yield return null; // Espera o próximo frame
        }

        // Quando a música acaba, a gente crava eles de volta na posição original
        if (rt6 != null) rt6.anchoredPosition = posOriginal6;
        if (rt7 != null) rt7.anchoredPosition = posOriginal7;

        // Liga a grade de volta pra garantir que não quebrou nada na UI
        if (layout != null) layout.enabled = true;
    }

    private string MontarConversa(List<LinhaDeDialogo> batePapoLog)
    {
        if (batePapoLog == null || batePapoLog.Count == 0) return "";

        string textoFinal = "";

        for (int i = 0; i < batePapoLog.Count; i++)
        {
            string corHex = ColorUtility.ToHtmlStringRGB(batePapoLog[i].corDaFala);
            textoFinal += $"<color=#{corHex}>{batePapoLog[i].fala}</color>";

            if (i < batePapoLog.Count - 1)
            {
                textoFinal += "\n\n";
            }
        }

        return textoFinal;
    }

    private void AbrirLeitura(int id, string lore)
    {
        telaGradeLogs.SetActive(false);
        telaLeituraLog.SetActive(true);

        if (tituloLeitura != null) tituloLeitura.text = "LOG " + id.ToString("00");
        if (conteudoLeitura != null) conteudoLeitura.text = lore;
    }

    public void FecharLeitura()
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }

    public void VoltarParaGrade()
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }
}