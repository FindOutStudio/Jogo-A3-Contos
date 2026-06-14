using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis de Interface")]
    [Tooltip("Arraste aqui o painel 'Tem certeza que deseja sair?'")]
    public GameObject painelSair; 

    [Header("Scripts de Apoio")]
    public MenuConfiguracoes scriptConfig; 
    public MenuDeLogs scriptLogs; 

    [Header("Áudio do Menu")]
    public AudioSource audioSourceMenu;
    public AudioClip somClique;
    public AudioClip somHover;

    private void Start()
    {
        // Garante que o painel de sair comece desligado para não aparecer na cara do jogador
        if (painelSair != null) 
        {
            painelSair.SetActive(false);
        }
    }

    public void TocarSomClique()
    {
        if (audioSourceMenu != null && somClique != null)
        {
            audioSourceMenu.PlayOneShot(somClique);
        }
    }

    public void TocarSomHover()
    {
        if (audioSourceMenu != null && somHover != null)
        {
            audioSourceMenu.PlayOneShot(somHover);
        }
    }

    public void AbrirConfig()
    {
        if (scriptConfig != null)
        {
            scriptConfig.AbrirConfiguracoes();
        }
        else
        {
            Debug.LogError("Irmão, você esqueceu de arrastar o script de Config no Inspector do Menu!");
        }
    }

    public void AbrirLogs()
    {
        if (scriptLogs != null)
        {
            scriptLogs.AbrirMenuLogs();
        }
        else
        {
            Debug.LogError("Chefe, você esqueceu de arrastar o script MenuDeLogs no Inspector do MainMenu!");
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ==========================================
    // ======= SISTEMA DE SAÍDA DO JOGO =======
    // ==========================================

    // 1. Coloque esta função no botão "Sair" do menu principal
    public void BotaoSair() 
    {
        if (painelSair != null)
        {
            painelSair.SetActive(true);
        }
        else
        {
            Debug.LogError("Chefe, você esqueceu de arrastar o PainelSair no Inspector do MainMenu!");
        }
    }

    // 2. Coloque esta função no botão "Não" (ou fechar) dentro do painel
    public void CancelarSaida() 
    {
        if (painelSair != null)
        {
            painelSair.SetActive(false);
        }
    }

    // 3. Coloque esta função no botão "Sim" dentro do painel
    public void ConfirmarSaida() 
    {
        // Esse log serve para você saber que funcionou dentro do editor da Unity
        Debug.Log("O jogo foi fechado com sucesso!"); 
        
        // O comando real que fecha o jogo (SÓ FUNCIONA NO JOGO COMPILADO/BUILD)
        Application.Quit(); 
    }
}