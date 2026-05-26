using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // === NOVO: Criamos uma instância para o Gerenciador de Moedas achar o PauseMenu
    public static PauseMenu instance; 
    
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton; 
    [SerializeField] private GameObject painelConfig; 
    
    public static bool isPaused = false;

    void Awake()
    {
        instance = this; // O script se registra ao nascer
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ======= A TRAVINHA DA VITÓRIA =======
            // Se o jogador já ganhou a fase, ele não pode mais pausar!
            if (GerenciadorMoedas.vitoriaAlcancada) return;

            // ======= A TRAVINHA DO LOG =======
            if (LogUIManager.instance != null && LogUIManager.instance.painelLog != null && LogUIManager.instance.painelLog.activeSelf)
            {
                return; 
            }
            // =================================

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

    // ======= NOVA FUNÇÃO: Esconder o botão =======
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