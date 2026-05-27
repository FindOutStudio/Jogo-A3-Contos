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

    [Tooltip("Escolha a cor deste bloco de texto no Inspector!")]
    public Color corDoTexto = Color.white; 
}

[System.Serializable]
public class CinematicaConfig
{
    [Tooltip("Ex: 'IntroMenu', 'Fase0_Para_Fase1'")]
    public string idCinematica; 
    
    public VideoClip videoClip; 
    
    // ======= NOVAS CONFIGURAÇÕES DE MÚSICA AQUI =======
    [Header("Trilha Sonora da Cinemática")]
    [Tooltip("A música que vai tocar no fundo! (Deixe vazio para ficar em silêncio)")]
    public AudioClip musicaFundo;
    [Range(0f, 1f)] public float volumeMusica = 1f;
    // ==================================================

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
    public float velocidademain = 0.05f;
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
            // === MÁGICA DA MÚSICA ENTRANDO EM AÇÃO ===
            if (MusicManager.instance != null)
            {
                if (cinematicaAtual.musicaFundo != null)
                {
                    // Toca a música exclusiva configurada para esta cena
                    MusicManager.instance.TocarMusicaEspecifica(cinematicaAtual.musicaFundo, cinematicaAtual.volumeMusica);
                }
                else
                {
                    // Se você não colocar nenhuma música no Inspector, ele para a música anterior e fica mudo!
                    MusicManager.instance.PararMusica();
                }
            }
            // =========================================

            bool temDialogo = cinematicaAtual.dialogos != null && cinematicaAtual.dialogos.Length > 0;

            if(videoPlayer != null && cinematicaAtual.videoClip != null)
            {
                videoPlayer.clip = cinematicaAtual.videoClip;
                
                if (temDialogo)
                {
                    videoPlayer.isLooping = true; 
                }
                else
                {
                    videoPlayer.isLooping = false; 
                    videoPlayer.loopPointReached += FimDoVideoAlcancado; 
                }
                
                videoPlayer.Play();
            }

            if (temDialogo)
            {
                botaoAvancar.gameObject.SetActive(true);
                botaoAvancar.onClick.AddListener(AvancarDialogo);
                MostrarDialogoAtual();
            }
            else
            {
                botaoAvancar.gameObject.SetActive(false);
                if (textoDialogo != null) textoDialogo.text = "";
            }
        }
        else
        {
            Debug.LogError("Cinemática não encontrada! Pulando direto para o Menu...");
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void FimDoVideoAlcancado(VideoPlayer vp)
    {
        FinalizarCinematica();
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

        if (cinematicaAtual.dialogos != null && indiceDialogoAtual < cinematicaAtual.dialogos.Length)
        {
            MostrarDialogoAtual(); 
        }
        else
        {
            FinalizarCinematica();
        }
    }

    private void MostrarDialogoAtual()
    {
        if (cinematicaAtual.dialogos == null || cinematicaAtual.dialogos.Length == 0) return;

        DialogoCinematica dialogo = cinematicaAtual.dialogos[indiceDialogoAtual];
        textoCompletoAtual = dialogo.texto;

        if (textoDialogo != null)
        {
            textoDialogo.color = dialogo.corDoTexto;
        }

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
            yield return new WaitForSeconds(velocidademain);
        }

        digitando = false;
    }

    private void FinalizarCinematica()
    {
        if (!string.IsNullOrEmpty(cinematicaAtual.idCinematicaSeguinte))
        {
            PlayerPrefs.SetString("CinematicaPendente", cinematicaAtual.idCinematicaSeguinte);
            SceneManager.LoadScene("Cinematicas"); 
        }
        else
        {
            SceneManager.LoadScene(cinematicaAtual.proximaCena);
        }
    }
}