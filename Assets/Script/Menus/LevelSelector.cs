using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public FaseConfig[] fases;

    void Start()
    {
        if (telaSelecaoLevel != null) telaSelecaoLevel.SetActive(false);
        GerarBotoesDeFase();
    }

    // Tornamos pública para o sistema de cheat também poder atualizar a tela
    public void GerarBotoesDeFase()
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
            
            Button componenteBotao = novoBotao.GetComponent<Button>();
            Image imagemDoBotao = novoBotao.GetComponent<Image>();

            // LÓGICA DE DESBLOQUEIO:
            bool estaDesbloqueada = (i == 0) || (PlayerPrefs.GetInt("FaseLiberada_" + i, 0) == 1);

            if (estaDesbloqueada)
            {
                imagemDoBotao.sprite = faseAtual.spriteDesbloqueado;
                componenteBotao.interactable = true; 
                
                string cenaParaCarregar = faseAtual.nomeDaCena;
                componenteBotao.onClick.AddListener(() => CarregarFase(cenaParaCarregar));
            }
            else
            {
                imagemDoBotao.sprite = faseAtual.spriteBloqueado;
                componenteBotao.interactable = false; 
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
        
        // CORREÇÃO AQUI: Força o menu a ler o PlayerPrefs e recriar os botões toda vez que abrir!
        // Evita que o painel mostre informações desatualizadas.
        GerarBotoesDeFase(); 
    }

    public void FecharSelecao() 
    {
        telaSelecaoLevel.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }
}