using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; 

[System.Serializable]
public class LogLore
{
    public int logID;
    public Sprite spriteDesbloqueado; 
    public Sprite spriteBloqueado;    
    
    [Header("Bate-Papo (Textos e Cores)")]
    [Tooltip("Adicione aqui as mesmas falas e cores que estão no LogColetavel da fase!")]
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

    // ======= AQUI FICA O SISTEMA DO LOG SECRETO =======
    [Header("=== Sistema do Log Secreto ===")]
    [Tooltip("O ID do Log que só desbloqueia pegando todos os outros (ex: 7)")]
    public int idLogSecreto = 7;
    [Tooltip("Quantos logs normais o jogador precisa pegar na fase para abrir este? (ex: 6)")]
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
        // Limpa os botões antigos
        foreach (Transform child in containerDeBotoes) Destroy(child.gameObject);

        // ==== O PULO DO GATO: Checa se você já merece o Log Secreto antes de gerar a tela! ====
        ChecarDesbloqueioDoLogSecreto();

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
        }
    }

    // ====== FUNÇÃO QUE VIGIA SE VOCÊ PEGOU TODOS OS ANTERIORES ======
    private void ChecarDesbloqueioDoLogSecreto()
    {
        int logsColetados = 0;

        // Conta quantos logs (de 1 até a quantidade necessária) o jogador já tem
        for (int i = 1; i <= quantidadeLogsParaDesbloquear; i++)
        {
            if (PlayerPrefs.GetInt("LogColetado_" + i, 0) == 1)
            {
                logsColetados++;
            }
        }

        // Se ele pegou todos os requeridos, o jogo injeta o salvamento do secreto automaticamente!
        if (logsColetados >= quantidadeLogsParaDesbloquear)
        {
            PlayerPrefs.SetInt("LogColetado_" + idLogSecreto, 1);
            PlayerPrefs.Save();
        }
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

    public void VoltarParaGrade()
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }
}