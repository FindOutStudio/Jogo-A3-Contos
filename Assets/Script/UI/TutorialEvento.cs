using System.Collections;
using UnityEngine;

public class TutorialEvento : MonoBehaviour
{
    public enum TipoDeTutorial { Lancador, PopUpColetavel, SlowMotionMidAir }

    [Header("Configuração do Evento")]
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

    private bool tutorialAtivo = false;
    private bool jaAtivouNestaRun = false; 
    
    private PlayerController playerScript; 

    private void Start()
    {
        if (objetoMaozinha != null) objetoMaozinha.SetActive(false);
        if (painelPopUp != null) painelPopUp.SetActive(false);
    }

    private void Update()
    {
        if (tutorialAtivo && (tipoDoEvento == TipoDeTutorial.Lancador || tipoDoEvento == TipoDeTutorial.SlowMotionMidAir))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Apenas esconde a mãozinha. Não mexemos no Time.timeScale aqui!
                // Quem vai devolver o tempo normal é o PlayerController quando soltar o dedo.
                if (objetoMaozinha != null) objetoMaozinha.SetActive(false);
                tutorialAtivo = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !jaAtivouNestaRun)
        {
            playerScript = collision.GetComponent<PlayerController>();
            AtivarTutorial();
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerScript != null)
        {
            playerScript.tutorialTempoInfinito = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !jaAtivouNestaRun && tipoDoEvento == TipoDeTutorial.Lancador)
        {
            playerScript = collision.gameObject.GetComponent<PlayerController>();
            AtivarTutorial();
        }
    }

    private void AtivarTutorial()
    {
        jaAtivouNestaRun = true;

        switch (tipoDoEvento)
        {
            case TipoDeTutorial.Lancador:
                tutorialAtivo = true;
                if (objetoMaozinha != null) objetoMaozinha.SetActive(true);
                break;

            case TipoDeTutorial.PopUpColetavel:
                tutorialAtivo = true;
                if (painelPopUp != null)
                {
                    painelPopUp.SetActive(true);
                    Time.timeScale = 0f; 
                    PauseMenu.isPaused = true;
                }
                if (SoundManager.instance != null) SoundManager.instance.TocarMaozinha();
                break;

            case TipoDeTutorial.SlowMotionMidAir:
                if (playerScript != null) playerScript.tutorialTempoInfinito = true;
                if (SoundManager.instance != null) SoundManager.instance.TocarSlowMotion();
                StartCoroutine(RotinaMatrix());
                break;
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

    public void FecharPopUp()
    {
        if (painelPopUp != null) painelPopUp.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        tutorialAtivo = false;
    }
}