using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Criamos essa caixinha para organizar as opções lá no Inspector
[System.Serializable]
public struct FaseConfig
{
    [Tooltip("Nome exato da cena (Ex: Level 0)")]
    public string nomeDaCena;
    
    [Tooltip("A imagem ROSA (Nível liberado)")]
    public Sprite spriteDesbloqueado;
    
    [Tooltip("A imagem VERMELHA escuro (Nível bloqueado)")]
    public Sprite spriteBloqueado;
}

public class LevelSelector : MonoBehaviour
{
    [Header("Telas do Menu")]
    public GameObject telaMenuPrincipal;
    public GameObject telaSelecaoLevel;

    [Header("Gerador de Fases")]
    [Tooltip("Arraste o prefab do botão de fase aqui")]
    public GameObject botaoLevelPrefab; 
    public Transform containerDeBotoes; 

    [Header("Lista de Fases (Game Designer, mexa aqui!)")]
    // Mudamos de uma lista de Textos para uma lista de Configurações
    public FaseConfig[] fases;

    void Start()
    {
        if (telaSelecaoLevel != null) telaSelecaoLevel.SetActive(false);
        GerarBotoesDeFase();
    }

    void GerarBotoesDeFase()
    {
        // 1. Limpa qualquer botão antigo
        foreach (Transform child in containerDeBotoes)
        {
            Destroy(child.gameObject);
        }

        // 2. Cria os botões com as imagens certas
        for (int i = 0; i < fases.Length; i++)
        {
            FaseConfig faseAtual = fases[i];
            GameObject novoBotao = Instantiate(botaoLevelPrefab, containerDeBotoes);
            
            // Pega os componentes cruciais do botão
            Button componenteBotao = novoBotao.GetComponent<Button>();
            Image imagemDoBotao = novoBotao.GetComponent<Image>();

            // LÓGICA DE DESBLOQUEIO:
            // A primeira fase (i = 0) sempre está liberada. 
            // As outras dependem de ter um "1" salvo no PlayerPrefs.
            bool estaDesbloqueada = (i == 0) || (PlayerPrefs.GetInt("FaseLiberada_" + i, 0) == 1);

            if (estaDesbloqueada)
            {
                imagemDoBotao.sprite = faseAtual.spriteDesbloqueado;
                componenteBotao.interactable = true; // Permite clicar
                
                // Variável local para não bugar o Listener dentro do Loop
                string cenaParaCarregar = faseAtual.nomeDaCena;
                componenteBotao.onClick.AddListener(() => CarregarFase(cenaParaCarregar));
            }
            else
            {
                imagemDoBotao.sprite = faseAtual.spriteBloqueado;
                componenteBotao.interactable = false; // Impede o clique
            }
        }
    }

    private void CarregarFase(string nomeCena)
    {
        SceneManager.LoadScene(nomeCena);
    }

    public void AbrirSelecao()
    {
        telaMenuPrincipal.SetActive(false);
        telaSelecaoLevel.SetActive(true);
    }

    public void FecharSelecao() 
    {
        telaSelecaoLevel.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }
}