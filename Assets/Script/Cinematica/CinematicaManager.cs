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
    public bool letraPorLetra; 
    
    [Tooltip("As letras vão nascendo da esquerda pra direita com glitch na ponta, mas SE REVELAM no final (Efeito Decodificador).")]
    public bool textoEmbaralhado; 
    
    [Tooltip("As letras nascem da esquerda pra direita, mas NUNCA se revelam! Ficam mudando de símbolo pra sempre (Efeito §k do Minecraft).")]
    public bool glitchPermanente;

    public Color corDoTexto = Color.white; 
}

[System.Serializable]
public class CinematicaConfig
{
    public string idCinematica; 
    public VideoClip videoClip; 
    
    [Header("Trilha Sonora Adicional (Fundo)")]
    public AudioClip musicaFundo;
    [Range(0f, 1f)] public float volumeMusica = 1f;

    [Header("Efeitos Sonoros Únicos")]
    [Tooltip("Toca no exato momento em que a cinemática começa")]
    public AudioClip sfxNoComeco;
    [Tooltip("Toca no exato momento em que a cinemática acaba (ou é pulada)")]
    public AudioClip sfxNoFim;

    public DialogoCinematica[] dialogos; 
    
    [Header("Para Onde Vai Depois?")]
    public string idCinematicaSeguinte;
    public string proximaCena; 
}

public class CinematicaManager : MonoBehaviour
{
    [Header("=== MODO DE TESTE (DEBUG) ===")]
    public bool testarCinematica = false;
    public string idParaTeste;

    [Header("Componentes da Tela")]
    public VideoPlayer videoPlayer; 
    public TextMeshProUGUI textoDialogo; 
    public Button botaoAvancar; 
    public Button botaoPular; 

    [Header("Configurações de Texto e Pulo")]
    public KeyCode teclaParaPular = KeyCode.Space;
    public float velocidademain = 0.05f;
    public float velocidadeDoGlitch = 0.02f;

    public CinematicaConfig[] todasAsCinematicas;

    private CinematicaConfig cinematicaAtual;
    private int indiceDialogoAtual = 0;
    private bool digitando = false;
    private string textoCompletoAtual;
    private bool pulando = false;

    void Start()
    {
        string idParaTocar = (testarCinematica && !string.IsNullOrEmpty(idParaTeste)) 
                             ? idParaTeste 
                             : PlayerPrefs.GetString("CinematicaPendente", "IntroMenu");

        foreach (var c in todasAsCinematicas)
        {
            if (c.idCinematica == idParaTocar)
            {
                cinematicaAtual = c;
                break;
            }
        }

        if (botaoPular != null) botaoPular.onClick.AddListener(PularCinematica);

        if (cinematicaAtual != null)
        {
            string destino = !string.IsNullOrEmpty(cinematicaAtual.idCinematicaSeguinte) ? "Cinematicas" : cinematicaAtual.proximaCena;
            
            if (destino != SceneManager.GetActiveScene().name)
            {
                if (GerenciadorTransicoes.instance != null) GerenciadorTransicoes.instance.PreCarregarCena(destino);
            }

            if (MusicManager.instance != null)
            {
                if (cinematicaAtual.musicaFundo != null) MusicManager.instance.TocarMusicaEspecifica(cinematicaAtual.musicaFundo, cinematicaAtual.volumeMusica);
                else MusicManager.instance.PararMusica();
            }

            // ======= TOCA O SFX NO COMEÇO =======
            if (cinematicaAtual.sfxNoComeco != null && SoundManager.instance != null)
            {
                SoundManager.instance.TocarSFX(cinematicaAtual.sfxNoComeco);
            }

            bool temDialogo = cinematicaAtual.dialogos != null && cinematicaAtual.dialogos.Length > 0;

            if(videoPlayer != null && cinematicaAtual.videoClip != null)
            {
                videoPlayer.clip = cinematicaAtual.videoClip;
                
                if (temDialogo) videoPlayer.isLooping = true; 
                else
                {
                    videoPlayer.isLooping = false; 
                    videoPlayer.loopPointReached += FimDoVideoAlcancado; 
                }
                
                videoPlayer.Play();
            }

            if (temDialogo)
            {
                if (botaoAvancar != null)
                {
                    botaoAvancar.gameObject.SetActive(true);
                    botaoAvancar.onClick.AddListener(AvancarDialogo);
                }
                MostrarDialogoAtual();
            }
            else
            {
                if (botaoAvancar != null) botaoAvancar.gameObject.SetActive(false);
                if (textoDialogo != null) textoDialogo.text = "";
            }
        }
        else
        {
            if (GerenciadorTransicoes.instance != null) GerenciadorTransicoes.instance.TrocarCena("MainMenu");
        }
    }

    void Update()
    {
        if (cinematicaAtual != null && cinematicaAtual.musicaFundo != null && MusicManager.instance != null)
        {
            MusicManager.instance.SetVolumeEmTempoReal(cinematicaAtual.volumeMusica);
        }

        if (Input.GetKeyDown(teclaParaPular) && !pulando)
        {
            PularCinematica();
        }
    }

    private void FimDoVideoAlcancado(VideoPlayer vp)
    {
        if (!pulando) FinalizarCinematica();
    }

    public void AvancarDialogo()
    {
        if (pulando) return;

        if (digitando)
        {
            StopAllCoroutines();
            DialogoCinematica d = cinematicaAtual.dialogos[indiceDialogoAtual];
            
            if (d.glitchPermanente)
            {
                string lixo = "";
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
                for (int i = 0; i < textoCompletoAtual.Length; i++) {
                    if (char.IsWhiteSpace(textoCompletoAtual[i])) lixo += textoCompletoAtual[i];
                    else lixo += chars[Random.Range(0, chars.Length)];
                }
                textoDialogo.text = lixo;
            }
            else
            {
                textoDialogo.text = textoCompletoAtual;
            }

            digitando = false;
            return;
        }

        indiceDialogoAtual++;

        if (cinematicaAtual.dialogos != null && indiceDialogoAtual < cinematicaAtual.dialogos.Length) MostrarDialogoAtual(); 
        else FinalizarCinematica();
    }

    private void MostrarDialogoAtual()
    {
        if (cinematicaAtual.dialogos == null || cinematicaAtual.dialogos.Length == 0) return;

        DialogoCinematica dialogo = cinematicaAtual.dialogos[indiceDialogoAtual];
        textoCompletoAtual = dialogo.texto;

        if (textoDialogo != null) textoDialogo.color = dialogo.corDoTexto;

        if (dialogo.letraPorLetra || dialogo.textoEmbaralhado || dialogo.glitchPermanente) 
        {
            StartCoroutine(EfeitoMaquinaDeEscrever(textoCompletoAtual, dialogo.textoEmbaralhado, dialogo.glitchPermanente));
        }
        else 
        {
            textoDialogo.text = textoCompletoAtual;
        }
    }

    private IEnumerator EfeitoMaquinaDeEscrever(string texto, bool embaralhado, bool permanente)
    {
        digitando = true;
        textoDialogo.text = "";

        if (embaralhado || permanente)
        {
            string caracteresAleatorios = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            int tamanhoDoTexto = texto.Length;
            
            int letrasLidas = 0; 
            
            float timerLetraReal = 0f;
            float timerGlitch = 0f;

            while (letrasLidas < tamanhoDoTexto)
            {
                timerLetraReal += Time.deltaTime;
                timerGlitch += Time.deltaTime;

                if (timerGlitch >= velocidadeDoGlitch)
                {
                    string textoExibido = "";
                    
                    for (int i = 0; i < letrasLidas; i++)
                    {
                        if (char.IsWhiteSpace(texto[i])) textoExibido += texto[i];
                        else if (permanente) textoExibido += caracteresAleatorios[Random.Range(0, caracteresAleatorios.Length)];
                        else textoExibido += texto[i];
                    }
                    
                    if (!permanente)
                    {
                        int letrasRestantes = tamanhoDoTexto - letrasLidas;
                        int tamanhoDoCursor = Mathf.Min(2, letrasRestantes); 

                        for (int i = 0; i < tamanhoDoCursor; i++)
                        {
                            if (char.IsWhiteSpace(texto[letrasLidas + i])) textoExibido += texto[letrasLidas + i];
                            else textoExibido += caracteresAleatorios[Random.Range(0, caracteresAleatorios.Length)];
                        }
                    }

                    textoDialogo.text = textoExibido;
                    timerGlitch = 0f;
                }

                if (timerLetraReal >= velocidademain)
                {
                    letrasLidas++;
                    timerLetraReal = 0f;
                }

                yield return null; 
            }

            if (permanente)
            {
                while (true)
                {
                    timerGlitch += Time.deltaTime;
                    if (timerGlitch >= velocidadeDoGlitch)
                    {
                        string textoExibido = "";
                        for (int i = 0; i < tamanhoDoTexto; i++)
                        {
                            if (char.IsWhiteSpace(texto[i])) textoExibido += texto[i];
                            else textoExibido += caracteresAleatorios[Random.Range(0, caracteresAleatorios.Length)];
                        }
                        textoDialogo.text = textoExibido;
                        timerGlitch = 0f;
                    }
                    yield return null;
                }
            }
        }
        else
        {
            foreach (char letra in texto.ToCharArray())
            {
                textoDialogo.text += letra;
                yield return new WaitForSeconds(velocidademain);
            }
        }

        if (!permanente) textoDialogo.text = texto; 
        digitando = false;
    }

    public void PularCinematica()
    {
        if (pulando) return; 
        pulando = true;

        StopAllCoroutines();
        digitando = false;
        
        FinalizarCinematica();
    }

    private void FinalizarCinematica()
    {
        // ======= TOCA O SFX NO FIM =======
        if (cinematicaAtual.sfxNoFim != null && SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(cinematicaAtual.sfxNoFim);
        }

        if (!string.IsNullOrEmpty(cinematicaAtual.idCinematicaSeguinte))
        {
            PlayerPrefs.SetString("CinematicaPendente", cinematicaAtual.idCinematicaSeguinte);
            PlayerPrefs.Save(); 
            if (GerenciadorTransicoes.instance != null) GerenciadorTransicoes.instance.TrocarCena("Cinematicas"); 
        }
        else
        {
            PlayerPrefs.DeleteKey("CinematicaPendente"); 
            PlayerPrefs.Save();
            if (GerenciadorTransicoes.instance != null) GerenciadorTransicoes.instance.TrocarCena(cinematicaAtual.proximaCena);
        }
    }
}