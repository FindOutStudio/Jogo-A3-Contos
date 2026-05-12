using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scripts de Apoio")]
    public MenuConfiguracoes scriptConfig; // Arraste o objeto com o script de config aqui

    [Header("Áudio do Menu")]
    public AudioSource audioSourceMenu;
    public AudioClip somClique;
    public AudioClip somHover;

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

    // ======= FUNÇÃO QUE ESTAVA FALTANDO =======
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

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void Play()
    {
        SceneManager.LoadScene("Teste");
    }
}