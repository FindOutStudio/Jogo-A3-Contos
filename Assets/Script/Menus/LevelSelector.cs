using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [Header("Telas do Menu")]
    public GameObject telaMenuPrincipal;
    public GameObject telaSelecaoLevel;

    [Header("Gerador de Fases")]
    [Tooltip("Arraste o prefab do botão de fase aqui")]
    public GameObject botaoLevelPrefab; 
    [Tooltip("O painel/objeto que tem o Grid Layout Group")]
    public Transform containerDeBotoes; 

    [Header("Lista de Fases (Game Designer, mexa aqui!)")]
    [Tooltip("Escreva o nome exato das cenas das fases. Ex: Fase1, Fase2")]
    public string[] nomeDasCenas;

    void Start()
    {
        // Garante que a tela de seleção comece escondida
        if (telaSelecaoLevel != null) telaSelecaoLevel.SetActive(false);
        
        GerarBotoesDeFase();
    }

    void GerarBotoesDeFase()
    {
        // Passo 1: Limpa qualquer botão que já esteja no container (pra não duplicar)
        foreach (Transform child in containerDeBotoes)
        {
            Destroy(child.gameObject);
        }

        // Passo 2: Cria um botão para cada fase que o designer colocou na lista
        for (int i = 0; i < nomeDasCenas.Length; i++)
        {
            int numeroDaFase = i + 1; // Para exibir "Lvl 1" em vez de "Lvl 0"
            string cenaParaCarregar = nomeDasCenas[i];

            // Instancia (fabrica) o botão dentro do Grid
            GameObject novoBotao = Instantiate(botaoLevelPrefab, containerDeBotoes);
            
            // Procura o componente de Texto dentro do botão para escrever "Lvl X"
            // (Se você usa TextMeshPro, troque 'Text' por 'TMPro.TextMeshProUGUI')
            Text textoDoBotao = novoBotao.GetComponentInChildren<Text>();
            if (textoDoBotao != null) 
            {
                textoDoBotao.text = "Lvl " + numeroDaFase;
            }

            // Programa o botão para carregar a fase correta quando clicado
            Button componenteBotao = novoBotao.GetComponent<Button>();
            componenteBotao.onClick.AddListener(() => CarregarFase(cenaParaCarregar));
        }
    }

    private void CarregarFase(string nomeCena)
    {
        SceneManager.LoadScene(nomeCena);
    }

    // ======= NAVEGAÇÃO DO MENU =======
    public void AbrirSelecao()
    {
        telaMenuPrincipal.SetActive(false);
        telaSelecaoLevel.SetActive(true);
    }

    public void FecharSelecao() // Função para o botão "Voltar"
    {
        telaSelecaoLevel.SetActive(false);
        telaMenuPrincipal.SetActive(true);
    }
}