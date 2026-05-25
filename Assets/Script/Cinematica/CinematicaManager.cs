using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; 
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogoCinematica
{
    [TextArea(3, 10)]
    public string texto;
    
    [Tooltip("Marque para máquina de escrever. Desmarque para aparecer o texto todo de uma vez.")]
    public bool letraPorLetra; 
}

[System.Serializable]
public class CinematicaConfig
{
    [Tooltip("Ex: 'IntroMenu', 'Fase0_Para_Fase1'")]
    public string idCinematica; 
    
    public VideoClip videoClip; 
    
    public DialogoCinematica[] dialogos; 
    
    [Header("Para Onde Vai Depois?")]
    [Tooltip("Se for emendar em OUTRA cinemática, escreva a ID da próxima aqui! (Deixe em branco se for pra fase)")]
    public string idCinematicaSeguinte;

    [Tooltip("Nome da Cena da Fase (Ex: 'Level 0'). Só vai carregar se a caixa de cima estiver vazia.")]
    public string proximaCena; 
}

public class CinematicaManager : MonoBehaviour
{
    [Header("Componentes da Tela")]
    public VideoPlayer videoPlayer; 
    public TextMeshProUGUI textoDialogo; 
    public Button botaoAvancar; 

    [Header("Configurações")]
    public float velocidadeLetra = 0.05f;
    public CinematicaConfig[] todasAsCinematicas;

    private CinematicaConfig cinematicaAtual;
    private int indiceDialogoAtual = 0;
    private bool digitando = false;
    private string textoCompletoAtual;

    void Start()
    {
        string idParaTocar = PlayerPrefs.GetString("CinematicaPendente", "IntroMenu");

        foreach (var c in todasAsCinematicas)
        {
            if (c.idCinematica == idParaTocar)
            {
                cinematicaAtual = c;
                break;
            }
        }

        if (cinematicaAtual != null)
        {
            if(videoPlayer != null && cinematicaAtual.videoClip != null)
            {
                videoPlayer.clip = cinematicaAtual.videoClip;
                videoPlayer.isLooping = true; 
                videoPlayer.Play();
            }

            botaoAvancar.onClick.AddListener(AvancarDialogo);
            MostrarDialogoAtual();
        }
        else
        {
            Debug.LogError("Cinemática não encontrada! Pulando direto para o Menu...");
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void AvancarDialogo()
    {
        if (digitando)
        {
            StopAllCoroutines();
            textoDialogo.text = textoCompletoAtual;
            digitando = false;
            return;
        }

        indiceDialogoAtual++;

        // TRAVA DE SEGURANÇA: Só tenta mostrar diálogo se a lista não for nula e ainda tiver falas
        if (cinematicaAtual.dialogos != null && indiceDialogoAtual < cinematicaAtual.dialogos.Length)
        {
            MostrarDialogoAtual(); 
        }
        else
        {
            FinalizarCinematica(); // Se acabou as falas (ou se nunca teve nenhuma), avança de cena!
        }
    }

    private void MostrarDialogoAtual()
    {
        // TRAVA DE SEGURANÇA: Se a lista de diálogos estiver vazia, deixa o texto em branco e sai da função!
        if (cinematicaAtual.dialogos == null || cinematicaAtual.dialogos.Length == 0)
        {
            if (textoDialogo != null) textoDialogo.text = "";
            return;
        }

        DialogoCinematica dialogo = cinematicaAtual.dialogos[indiceDialogoAtual];
        textoCompletoAtual = dialogo.texto;

        if (dialogo.letraPorLetra)
        {
            StartCoroutine(EfeitoMaquinaDeEscrever(textoCompletoAtual));
        }
        else
        {
            textoDialogo.text = textoCompletoAtual;
        }
    }

    private IEnumerator EfeitoMaquinaDeEscrever(string texto)
    {
        digitando = true;
        textoDialogo.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadeLetra);
        }

        digitando = false;
    }

    private void FinalizarCinematica()
    {
        // Se tiver outra cinemática configurada, ele recarrega a cena passando o novo ID
        if (!string.IsNullOrEmpty(cinematicaAtual.idCinematicaSeguinte))
        {
            PlayerPrefs.SetString("CinematicaPendente", cinematicaAtual.idCinematicaSeguinte);
            SceneManager.LoadScene("Cinematicas"); 
        }
        else
        {
            // Se não, vai pro jogo normal!
            SceneManager.LoadScene(cinematicaAtual.proximaCena);
        }
    }
}