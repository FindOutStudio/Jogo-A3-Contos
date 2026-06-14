using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Precisamos disso para usar a List<>

[System.Serializable]
public class LogLore
{
    public int logID;
    public Sprite spriteDesbloqueado; 
    public Sprite spriteBloqueado;    
    
    [Header("Bate-Papo (Textos e Cores)")]
    [Tooltip("Adicione aqui as mesmas falas e cores que estão no LogColetavel da fase!")]
    // ======= A MÁGICA AQUI =======
    // Reutilizamos a classe LinhaDeDialogo que você já tem lá no LogColetavel.cs!
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
        foreach (Transform child in containerDeBotoes) Destroy(child.gameObject);

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

                if (novoBotao.GetComponent<SomBotao>() == null) novoBotao.AddComponent<SomBotao>();
                
                int idPassado = log.logID;
                
                // Antes de enviar para a tela, nós montamos o texto com as cores HTML!
                string textoPassado = MontarConversa(log.batePapo);
                
                componenteBotao.onClick.AddListener(() => AbrirLeitura(idPassado, textoPassado));
            }
            else
            {
                imagemDoBotao.sprite = log.spriteBloqueado; 
                componenteBotao.interactable = false; 
            }
        }
    }

    // ======= A MESMA FUNÇÃO DO LOG COLETÁVEL =======
    // Pega as falas separadas e junta tudo num textão colorido para o UI Manager ler
    private string MontarConversa(List<LinhaDeDialogo> batePapoLog)
    {
        if (batePapoLog == null || batePapoLog.Count == 0) return "";

        string textoFinal = "";

        for (int i = 0; i < batePapoLog.Count; i++)
        {
            string corHex = ColorUtility.ToHtmlStringRGB(batePapoLog[i].corDaFala);
            textoFinal += $"<color=#{corHex}>{batePapoLog[i].fala}</color>";

            // Pula duas linhas entre as falas para ficar organizado (exceto na última fala)
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

        if (tituloLeitura != null) tituloLeitura.text = "LOG #" + id.ToString("00");
        if (conteudoLeitura != null) conteudoLeitura.text = lore;
    }

    public void FecharLeitura()
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }
}