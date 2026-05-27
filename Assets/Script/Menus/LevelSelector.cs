using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct FaseConfig
{
    [Tooltip("Nome exato da cena (Ex: Level 0)")]
    public string nomeDaCena;
    
    public Sprite spriteDesbloqueado;
    public Sprite spriteBloqueado;

    [Tooltip("NOVO: Escreva a ID da cinemática aqui (ex: IntroMenu). Deixe em branco se for pular direto pra fase!")]
    public string idCinematicaAntesDaFase;
}

public class LevelSelector : MonoBehaviour
{
    [Header("Telas do Menu")]
    public GameObject telaMenuPrincipal;
    public GameObject telaSelecaoLevel;

    [Header("Gerador de Fases")]
    public GameObject botaoLevelPrefab; 
    public Transform containerDeBotoes; 

    [Header("Lista de Fases")]
    public FaseConfig[] fases;

    void Start()
    {
        if (telaSelecaoLevel != null) telaSelecaoLevel.SetActive(false);
        GerarBotoesDeFase();
    }

    public void GerarBotoesDeFase()
    {
        foreach (Transform child in containerDeBotoes) Destroy(child.gameObject);

        for (int i = 0; i < fases.Length; i++)
        {
            FaseConfig faseAtual = fases[i];
            GameObject novoBotao = Instantiate(botaoLevelPrefab, containerDeBotoes);
            
            Button componenteBotao = novoBotao.GetComponent<Button>();
            Image imagemDoBotao = novoBotao.GetComponent<Image>();

            bool estaDesbloqueada = (i == 0) || (PlayerPrefs.GetInt("FaseLiberada_" + i, 0) == 1);

            if (estaDesbloqueada)
            {
                imagemDoBotao.sprite = faseAtual.spriteDesbloqueado;
                componenteBotao.interactable = true; 
                
                string cena = faseAtual.nomeDaCena;
                string cinematica = faseAtual.idCinematicaAntesDaFase; 
                
                componenteBotao.onClick.AddListener(() => CarregarFase(cena, cinematica));
            }
            else
            {
                imagemDoBotao.sprite = faseAtual.spriteBloqueado;
                componenteBotao.interactable = false; 
            }
        }
    }

    private void CarregarFase(string nomeCena, string cinematicaPendente)
    {
        // ======= MUDANÇA AQUI =======
        if (!string.IsNullOrEmpty(cinematicaPendente))
        {
            PlayerPrefs.SetString("CinematicaPendente", cinematicaPendente);
            GerenciadorTransicoes.instance.TrocarCena("Cinematicas");
        }
        else 
        {
            GerenciadorTransicoes.instance.TrocarCena(nomeCena);
        }
    }

    public void AbrirSelecao()
    {
        telaMenuPrincipal.SetActive(false);
        telaSelecaoLevel.SetActive(true);
        GerarBotoesDeFase(); 
    }

    public void FecharSelecao() 
    {
        telaSelecaoLevel.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }
}