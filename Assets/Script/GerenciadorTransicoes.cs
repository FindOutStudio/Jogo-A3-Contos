using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections;

public enum TipoTransicao { Laser, PCB, Popups, Serra, Morte }

[System.Serializable]
public class BloquinhoDeTransicao
{
    public string cenaDeOrigem;
    public string cenaDeDestino;
    public string idDaCinematica;

    [Header("Opções de Transição (O sistema sorteia daqui)")]
    public TipoTransicao[] transicoesPossiveis;
}

public class GerenciadorTransicoes : MonoBehaviour
{
    public static GerenciadorTransicoes instance;

    [Header("Configurações Visuais")]
    public Animator animatorTransicao;
    public Image imagemDaTransicao;
    
    [Header("Configurações de Tempo")]
    public float tempoParaCobrirTela = 1.2f;
    public float tempoParaRevelarTela = 1.2f;

    [Header("=== SUAS REGRAS DE TRANSIÇÃO ===")]
    public BloquinhoDeTransicao[] bloquinhos;

    private AsyncOperation preCarregamento;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if (imagemDaTransicao != null) imagemDaTransicao.enabled = false;
    }

    public void PreCarregarCena(string nomeCena)
    {
        if (preCarregamento == null)
        {
            preCarregamento = SceneManager.LoadSceneAsync(nomeCena);
            preCarregamento.allowSceneActivation = false;
        }
    }

    public void TrocarCena(string nomeProximaCena)
    {
        StartCoroutine(RotinaTrocarCena(nomeProximaCena));
    }

    private IEnumerator RotinaTrocarCena(string nomeProximaCena)
    {
        string cenaAtual = SceneManager.GetActiveScene().name;
        string idDaProximaCinematica = PlayerPrefs.GetString("CinematicaPendente", "");
        
        string triggerFechar = "Start_Laser"; 
        string triggerAbrir = "End_Laser";
        bool achouRegra = false;

        if (cenaAtual == nomeProximaCena && cenaAtual != "Cinematicas") 
        {
            triggerFechar = "Start_Morte"; 
            triggerAbrir = "End_Morte";
            achouRegra = true;
        }
        else
        {
            foreach (var bloco in bloquinhos)
            {
                bool cenaBate = (cenaAtual == bloco.cenaDeOrigem && nomeProximaCena == bloco.cenaDeDestino);
                bool cinematicaBate = string.IsNullOrEmpty(bloco.idDaCinematica) || idDaProximaCinematica == bloco.idDaCinematica;

                if (cenaBate && cinematicaBate)
                {
                    if (bloco.transicoesPossiveis.Length > 0)
                    {
                        int sorteioLocal = Random.Range(0, bloco.transicoesPossiveis.Length);
                        TipoTransicao t = bloco.transicoesPossiveis[sorteioLocal];
                        triggerFechar = "Start_" + t.ToString();
                        triggerAbrir = "End_" + t.ToString();
                    }
                    achouRegra = true; break; 
                }
            }
        }

        if (!achouRegra)
        {
            int sorteio = Random.Range(0, 3);
            if (sorteio == 0) { triggerFechar = "Start_Laser"; triggerAbrir = "End_Laser"; }
            else if (sorteio == 1) { triggerFechar = "Start_PCB"; triggerAbrir = "End_PCB"; }
            else { triggerFechar = "Start_Serra"; triggerAbrir = "End_Serra"; }
        }

        if (imagemDaTransicao != null) { imagemDaTransicao.color = Color.white; imagemDaTransicao.enabled = true; }
        if (animatorTransicao != null) animatorTransicao.SetTrigger(triggerFechar);

        yield return new WaitForSecondsRealtime(tempoParaCobrirTela);

        AsyncOperation operacao = preCarregamento;
        preCarregamento = null; 

        if (operacao != null)
        {
            operacao.allowSceneActivation = true;
        }
        else
        {
            operacao = SceneManager.LoadSceneAsync(nomeProximaCena);
            operacao.allowSceneActivation = true;
        }

        while (!operacao.isDone)
        {
            yield return null;
        }

        // ======= LAG RESOLVIDO: O GC.Collect e UnloadUnusedAssets foram removidos daqui! =======

        yield return new WaitForSecondsRealtime(0.5f);
        if (animatorTransicao != null) animatorTransicao.SetTrigger(triggerAbrir);
        yield return new WaitForSecondsRealtime(tempoParaRevelarTela);
        if (imagemDaTransicao != null) imagemDaTransicao.enabled = false;
    }
}