using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;

    
    public static bool isPaused = false;

    void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Continuar()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pausar()
    {
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResetarNivel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AbrirConfig()
    {
        Debug.Log("Menu de Config");
    }

    public void SairParaMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}