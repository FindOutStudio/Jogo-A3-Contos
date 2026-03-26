using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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