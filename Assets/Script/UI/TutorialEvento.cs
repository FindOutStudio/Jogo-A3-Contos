using System.Collections;
using UnityEngine;

public class TutorialEvento : MonoBehaviour
{
    public enum TipoDeTutorial { Lancador, PopUpColetavel, SlowMotionMidAir, LiberarPuloDuplo }

    [Header("Configuração do Evento Principal")]
    public TipoDeTutorial tipoDoEvento;
    
    [Tooltip("Se for Lançador ou MidAir, arraste o objeto da Mãozinha aqui")]
    public GameObject objetoMaozinha;
    
    [Tooltip("Se for PopUp (Coletável), arraste o Painel de UI do Tutorial aqui")]
    public GameObject painelPopUp;

    [Header("Configurações do Mid-Air (Matrix)")]
    [Tooltip("Velocidade da câmera lenta ANTES de congelar (ex: 0.2 = 20% da velocidade)")]
    public float slowMotionInicial = 0.2f;
    [Tooltip("Quantos segundos reais ele viaja em câmera lenta até parar de vez no meio")]
    public float tempoAteCongelar = 0.4f;

    [Header("Habilidades Bônus")]
    [Tooltip("Marque esta caixa se quiser que ESTE colisor libere o pulo duplo junto com o evento de cima!")]
    public bool tambemLiberaPuloDuplo = false;

    private bool tutorialAtivo = false;
    private bool jaAtivouNestaRun = false; 
    
    private PlayerController playerScript; 

    // === A MÁGICA AQUI: Guardamos a corotina para poder matá-la a qualquer momento! ===
    private Coroutine rotinaMatrixAtual;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (jaAtivouNestaRun) return;

        if (collision.CompareTag("Player"))
        {
            playerScript = collision.GetComponent<PlayerController>();
            AtivarTutorial();
        }
    }

    private void AtivarTutorial()
    {
        jaAtivouNestaRun = true;

        switch (tipoDoEvento)
        {
            case TipoDeTutorial.Lancador:
                Time.timeScale = 0f;
                if (objetoMaozinha != null) objetoMaozinha.SetActive(true);
                tutorialAtivo = true;
                break;

            case TipoDeTutorial.PopUpColetavel:
                if (painelPopUp != null) painelPopUp.SetActive(true);
                Time.timeScale = 0f;
                tutorialAtivo = true;
                break;

            case TipoDeTutorial.SlowMotionMidAir:
                if (playerScript != null) playerScript.tutorialTempoInfinito = true;
                if (SoundManager.instance != null) SoundManager.instance.TocarSlowMotion();
                
                // === Salvamos a corotina na variável ===
                rotinaMatrixAtual = StartCoroutine(RotinaMatrix());
                break;

            case TipoDeTutorial.LiberarPuloDuplo:
                if (playerScript != null) playerScript.puloDuploDesbloqueado = true;
                break;
        }

        if (tambemLiberaPuloDuplo && playerScript != null)
        {
            playerScript.puloDuploDesbloqueado = true;
        }
    }

    private IEnumerator RotinaMatrix()
    {
        Time.timeScale = slowMotionInicial;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 

        yield return new WaitForSecondsRealtime(tempoAteCongelar);

        Time.timeScale = 0f;
        if (objetoMaozinha != null) objetoMaozinha.SetActive(true);

        if (SoundManager.instance != null) SoundManager.instance.TocarMaozinha();
        
        tutorialAtivo = true; 
    }

    public void FecharTutorial()
    {
        // === CORREÇÃO: Se o jogador arrastar muito rápido, matamos a corotina 
        // ANTES de ela congelar o jogo a meio do pulo dele! ===
        if (rotinaMatrixAtual != null)
        {
            StopCoroutine(rotinaMatrixAtual);
            rotinaMatrixAtual = null;
        }

        if (tipoDoEvento == TipoDeTutorial.PopUpColetavel)
        {
            if (painelPopUp != null) painelPopUp.SetActive(false);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            tutorialAtivo = false;
        }
        else if (tipoDoEvento == TipoDeTutorial.SlowMotionMidAir || tipoDoEvento == TipoDeTutorial.Lancador)
        {
            if (objetoMaozinha != null) objetoMaozinha.SetActive(false);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            tutorialAtivo = false;
            
            // Segurança extra para garantir que ele sai do modo tutorial no script dele
            if (playerScript != null) playerScript.tutorialTempoInfinito = false;
        }
    }
    public void FecharPopUp()
    {
        if (painelPopUp != null) painelPopUp.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        tutorialAtivo = false;
    }
}