using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Essa estruturazinha cria um "banco de dados" bonitinho no Inspector
[System.Serializable]
public class LogLore
{
    public int logID;
    
    [Header("Imagens do Botão")]
    public Sprite spriteDesbloqueado; // A imagem ROSA
    public Sprite spriteBloqueado;    // A imagem VERMELHA

    [TextArea(3, 10)]
    public string textoDoLog;
}

public class MenuDeLogs : MonoBehaviour
{
    [Header("Navegação de Telas")]
    public GameObject telaMenuPrincipal;
    public GameObject telaGradeLogs; // A tela com os quadradinhos
    public GameObject telaLeituraLog; // A tela pra ler o texto grande

    [Header("Textos da Tela de Leitura")]
    public TextMeshProUGUI tituloLeitura;
    public TextMeshProUGUI conteudoLeitura;

    [Header("Gerador da Grade")]
    public Transform containerDeBotoes;
    public GameObject botaoLogPrefab; // Prefab do botão

    [Header("Banco de Dados (Game Designer)")]
    [Tooltip("Cadastre todos os logs do jogo aqui.")]
    public LogLore[] todosOsLogs;

    private void Start()
    {
        // Garante que tudo comece escondido
        if (telaGradeLogs != null) telaGradeLogs.SetActive(false);
        if (telaLeituraLog != null) telaLeituraLog.SetActive(false);
    }

    // ======= FUNÇÕES PARA OS BOTÕES DE VOLTAR/ABRIR =======

    public void AbrirMenuLogs()
    {
        telaMenuPrincipal.SetActive(false);
        telaGradeLogs.SetActive(true);
        GerarBotoesDeLog(); // Gera a grade atualizada toda vez que abre
    }

    public void FecharMenuLogs() // Voltar da grade pro Menu Principal
    {
        telaGradeLogs.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }

    public void FecharLeitura() // Voltar da tela de leitura para a Grade
    {
        telaLeituraLog.SetActive(false);
        telaGradeLogs.SetActive(true);
    }

    // ======= LÓGICA DE GERAR E LER =======

    private void GerarBotoesDeLog()
    {
        // 1. Limpa os botões antigos pra não duplicar
        foreach (Transform child in containerDeBotoes)
        {
            Destroy(child.gameObject);
        }

        // 2. Cria um botão para cada Log cadastrado no Inspector
        foreach (LogLore log in todosOsLogs)
        {
            GameObject novoBotao = Instantiate(botaoLogPrefab, containerDeBotoes);
            Button componenteBotao = novoBotao.GetComponent<Button>();
            Image imagemDoBotao = novoBotao.GetComponent<Image>(); // Pegamos o componente de Imagem

            // Verifica na memória se o jogador já coletou esse ID
            bool foiColetado = PlayerPrefs.GetInt("LogColetado_" + log.logID, 0) == 1;

            if (foiColetado)
            {
                imagemDoBotao.sprite = log.spriteDesbloqueado; // Fica Rosa
                componenteBotao.interactable = true; // Botão clicável

                // Adiciona o som de clique/hover se não tiver
                if (novoBotao.GetComponent<SomBotao>() == null) novoBotao.AddComponent<SomBotao>();
                
                // Manda o botão abrir a tela de leitura com a história certa
                // Guardamos em variáveis locais para o botão não se confundir
                int idPassado = log.logID;
                string textoPassado = log.textoDoLog;
                componenteBotao.onClick.AddListener(() => AbrirLeitura(idPassado, textoPassado));
            }
            else
            {
                imagemDoBotao.sprite = log.spriteBloqueado; // Fica Vermelho
                componenteBotao.interactable = false; // Botão travado (a Unity deixa-o bloqueado)
            }
        }
    }

    private void AbrirLeitura(int id, string lore)
    {
        telaGradeLogs.SetActive(false);
        telaLeituraLog.SetActive(true);

        tituloLeitura.text = "LOG #" + id.ToString("00");
        conteudoLeitura.text = lore; 
        
        // Nota: Como é um menu de acervo, joguei o texto direto pra ele ler na hora, 
        // sem a animação do Minecraft, pra não cansar o jogador que só quer reler rápido.
    }
}