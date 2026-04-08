using UnityEngine;

public enum DirecaoCorrente
{
    Esquerda,
    Direita,
    Cima,
    Baixo
}

[RequireComponent(typeof(Collider2D))]
public class CorrenteTile : MonoBehaviour
{
    [Header("Configuração da Corrente")]
    public DirecaoCorrente direcao;      
    
    [Tooltip("A velocidade com que a corrente te carrega pro final")]
    public float velocidadeDoVento = 15f; 
    
    [Tooltip("O quão rápido ele 'mata' seu pulo e te prende na corrente")]
    public float forcaDeCaptura = 8f; 

    private Vector2 correnteDirection;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        switch (direcao)
        {
            case DirecaoCorrente.Esquerda:
                correnteDirection = Vector2.left;
                break;
            case DirecaoCorrente.Direita:
                correnteDirection = Vector2.right;
                break;
            case DirecaoCorrente.Cima:
                correnteDirection = Vector2.up;
                break;
            case DirecaoCorrente.Baixo:
                correnteDirection = Vector2.down;
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                // A MÁGICA DA CAPTURA ACONTECE AQUI:
                
                // 1. Qual é a velocidade ideal que o vento quer impor?
                Vector2 velocidadeAlvo = correnteDirection * velocidadeDoVento;
                
                // 2. Interpolação Linear (Lerp): Mistura a velocidade maluca que o player 
                // entrou com a velocidade certinha do vento, prendendo ele na correnteza!
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, velocidadeAlvo, Time.fixedDeltaTime * forcaDeCaptura);
            }
        }
    }

    public Vector2 ObterDirecao()
    {
        return correnteDirection;
    }
}