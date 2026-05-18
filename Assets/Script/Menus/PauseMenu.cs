using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject painelConfig; // Arraste seu painel de config aqui
    
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
            // ======= A TRAVINHA DO LOG =======
            // Verifica se o Gerenciador de Log existe e se o painel dele está ativado na tela.
            // Se estiver, a gente dá um "return", que faz o código parar aqui e não pausar o jogo.
            if (LogUIManager.instance != null && LogUIManager.instance.painelLog != null && LogUIManager.instance.painelLog.activeSelf)
            {
                return; 
            }
            // =================================

            // Se apertar ESC com a config aberta, ele só volta pro pause normal
            if (painelConfig != null && painelConfig.activeSelf)
            {
                FecharConfig();
            }
            else if (isPaused)
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
        if (painelConfig != null) painelConfig.SetActive(false);
        pauseButton.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pausar()
    {
        pausePanel.SetActive(true);
        if (painelConfig != null) painelConfig.SetActive(false);
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
        pausePanel.SetActive(false);
        if (painelConfig != null) painelConfig.SetActive(true);
    }

    public void FecharConfig()
    {
        if (painelConfig != null) painelConfig.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void SairParaMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}