using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class LogLore
{
    public int logID;
    public Sprite spriteDesbloqueado; 
    public Sprite spriteBloqueado;    
    [TextArea(3, 10)] public string textoDoLog;
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

    public void FecharLeitura() 
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }

    // MUDOU PARA PUBLIC PARA O NANO PODER ATUALIZAR
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
                string textoPassado = log.textoDoLog;
                componenteBotao.onClick.AddListener(() => AbrirLeitura(idPassado, textoPassado));
            }
            else
            {
                imagemDoBotao.sprite = log.spriteBloqueado; 
                componenteBotao.interactable = false; 
            }
        }
    }

    private void AbrirLeitura(int id, string lore)
    {
        telaGradeLogs.SetActive(false);
        telaLeituraLog.SetActive(true);

        tituloLeitura.text = "LOG #" + id.ToString("00");
        conteudoLeitura.text = lore; 
    }
}