using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool jogoPausado = false;
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (jogoPausado)
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
        Time.timeScale = 1f;
        jogoPausado = false;
    }

    private void Pausar()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;
    }

    public void AbrirConfig()
    {
        Debug.Log("Menu de Config");
    }

    public void SairParaMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
