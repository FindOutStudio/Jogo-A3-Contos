using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance; 
    
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton; 
    [SerializeField] private GameObject painelConfig; 
    
    public static bool isPaused = false;

    void Awake()
    {
        instance = this; 
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GerenciadorMoedas.vitoriaAlcancada) return;

            if (LogUIManager.instance != null && LogUIManager.instance.painelLog != null && LogUIManager.instance.painelLog.activeSelf)
            {
                return; 
            }

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

    public void EsconderBotaoPauseUI()
    {
        if (pauseButton != null) pauseButton.SetActive(false);
    }

    public void Continuar()
    {
        pausePanel.SetActive(false);
        if (painelConfig != null) painelConfig.SetActive(false);
        pauseButton.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        // ======= DEVOLVE O SOM =======
        if (SoundManager.instance != null) SoundManager.instance.RetomarSonsJogo();
    }

    public void Pausar()
    {
        // ======= MÁGICA AQUI: Força o fechamento de Logs antes de pausar =======
        if (LogUIManager.instance != null && LogUIManager.instance.painelLog != null && LogUIManager.instance.painelLog.activeSelf)
        {
            LogUIManager.instance.FecharLog();
        }

        // ======= MÁGICA AQUI: Força o fechamento de Tutoriais (Pop-ups) antes de pausar =======
        TutorialEvento[] tutoriais = FindObjectsOfType<TutorialEvento>();
        foreach (TutorialEvento t in tutoriais)
        {
            if (t.painelPopUp != null && t.painelPopUp.activeSelf)
            {
                t.FecharPopUp();
            }
        }

        // Agora sim, abre o Pause de verdade!
        pausePanel.SetActive(true);
        if (painelConfig != null) painelConfig.SetActive(false);
        pauseButton.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        // ======= PARA O SOM =======
        if (SoundManager.instance != null) SoundManager.instance.PausarSonsJogo();
    }

    public void ResetarNivel()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Trava de Segurança: Garante que os sons não entrem congelados na nova cena
        if (SoundManager.instance != null) SoundManager.instance.RetomarSonsJogo();

        GerenciadorTransicoes.instance.TrocarCena(SceneManager.GetActiveScene().name);
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
        
        // Trava de Segurança: Garante que os sons não entrem congelados no Menu
        if (SoundManager.instance != null) SoundManager.instance.RetomarSonsJogo();

        GerenciadorTransicoes.instance.TrocarCena("MainMenu");
    }
}