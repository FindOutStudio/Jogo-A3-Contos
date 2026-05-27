using System.Collections;
using UnityEngine;

public class DiretorDosCreditos : MonoBehaviour
{
    public static DiretorDosCreditos instance;

    [Header("O Ator (Dublê)")]
    public Transform dubleNano;
    public Animator animatorDuble;
    public TrailRenderer rastroDuble;

    [Header("Configurações de Tempo")]
    public float tempoMinimoEspera = 2f;
    public float tempoMaximoEspera = 5f;
    
    [Header("Configurações de Velocidade")]
    public float velocidadeHorizontal = 12f; 
    public float velocidadeVertical = 4f; 

    [Header("Limites da Câmera e Posição")]
    public Vector2 limitesTela = new Vector2(12f, 7f);
    public float limiteAlturaVoo = 3.5f; 
    public float posicaoXDasColunas = 6f;

    private bool creditosRolando = true;
    private bool acaoAcontecendo = false;
    
    // ======= A ARMA CONTRA O BUG =======
    private Coroutine rotinaSorteio; 

    private void Awake()
    {
        instance = this;
        if (dubleNano != null) dubleNano.gameObject.SetActive(false);
    }

    private void Start()
    {
        creditosRolando = true;
        // Agora nós guardamos a rotina do relógio nessa variável para podermos matá-la depois!
        rotinaSorteio = StartCoroutine(RotinaDeSorteio());
    }

    public void PararBagunca()
    {
        // Esse Log vai aparecer no seu Console da Unity!
        Debug.Log("DIRETOR: CORTA! Parando o sorteio de novos Nanos na mesma hora!"); 
        
        creditosRolando = false;
        
        // Atira no sorteio para ele morrer no exato milissegundo que a foto para
        if (rotinaSorteio != null)
        {
            StopCoroutine(rotinaSorteio);
        }
    }

    private IEnumerator RotinaDeSorteio()
    {
        while (creditosRolando)
        {
            float tempoEspera = Random.Range(tempoMinimoEspera, tempoMaximoEspera);
            yield return new WaitForSeconds(tempoEspera);

            if (!creditosRolando) break; 

            if (!acaoAcontecendo)
            {
                int acaoSorteada = Random.Range(0, 4); 
                StartCoroutine(ExecutarAcao(acaoSorteada));
            }
        }
    }

    private IEnumerator ExecutarAcao(int acao)
    {
        acaoAcontecendo = true;
        dubleNano.gameObject.SetActive(true);
        if (rastroDuble != null) { rastroDuble.Clear(); rastroDuble.emitting = false; }

        Vector3 pontoInicio = Vector3.zero;
        Vector3 pontoFim = Vector3.zero;
        Vector3 rotacao = Vector3.zero;
        string triggerAnimacao = "";
        bool usarRastro = false;
        float velocidadeAtual = velocidadeHorizontal;

        float colunaEscolhida = (Random.value > 0.5f) ? posicaoXDasColunas : -posicaoXDasColunas;

        switch (acao)
        {
            case 0: 
                pontoInicio = new Vector3(-limitesTela.x, Random.Range(-limiteAlturaVoo, limiteAlturaVoo), 0f);
                pontoFim = new Vector3(limitesTela.x, pontoInicio.y, 0f);
                rotacao = new Vector3(0f, 0f, -90f); 
                triggerAnimacao = "T_Voando"; 
                usarRastro = true;
                velocidadeAtual = velocidadeHorizontal;
                break;
            
            case 1: 
                pontoInicio = new Vector3(limitesTela.x, Random.Range(-limiteAlturaVoo, limiteAlturaVoo), 0f);
                pontoFim = new Vector3(-limitesTela.x, pontoInicio.y, 0f);
                rotacao = new Vector3(0f, 0f, 90f); 
                triggerAnimacao = "T_Voando";
                usarRastro = true;
                velocidadeAtual = velocidadeHorizontal;
                break;

            case 2: 
                pontoInicio = new Vector3(colunaEscolhida, -limitesTela.y, 0f);
                pontoFim = new Vector3(pontoInicio.x, limitesTela.y, 0f);
                rotacao = Vector3.zero;
                triggerAnimacao = "T_Subir";
                usarRastro = false;
                velocidadeAtual = velocidadeVertical;
                break;

            case 3: 
                pontoInicio = new Vector3(colunaEscolhida, limitesTela.y, 0f);
                pontoFim = new Vector3(pontoInicio.x, -limitesTela.y, 0f);
                rotacao = Vector3.zero;
                triggerAnimacao = "T_Cair";
                usarRastro = false;
                velocidadeAtual = velocidadeVertical;
                break;
        }

        dubleNano.position = pontoInicio;
        dubleNano.rotation = Quaternion.Euler(rotacao);
        if (animatorDuble != null && triggerAnimacao != "") animatorDuble.SetTrigger(triggerAnimacao);
        
        yield return null; 
        
        if (rastroDuble != null) 
        {
            rastroDuble.Clear();
            rastroDuble.emitting = usarRastro; 
        }

        while (Vector3.Distance(dubleNano.position, pontoFim) > 0.5f)
        {
            dubleNano.position = Vector3.MoveTowards(dubleNano.position, pontoFim, velocidadeAtual * Time.deltaTime);
            yield return null;
        }

        dubleNano.gameObject.SetActive(false);
        if (rastroDuble != null) rastroDuble.emitting = false;
        
        acaoAcontecendo = false;
    }
}