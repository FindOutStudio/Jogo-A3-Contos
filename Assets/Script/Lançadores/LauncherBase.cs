using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))] 
public class LauncherBase : MonoBehaviour
{
    [Header("Configurações do Lançador")]
    [Tooltip("Tempo que o lançador ignora o player após ele sair (evita grudar de novo no mesmo tiro)")]
    [SerializeField] private float tempoCooldownReentrada = 0.1f;

    [Header("Configurações de Queda")]
    [SerializeField] private bool caiQuandoPisa = false;
    [SerializeField] private float tempoParaCair = 0.5f;

    private Rigidbody2D rb;
    private bool jaCaiu = false;
    
    // Variável para controlar o bloqueio temporário
    private float cooldownAtual = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Garante que a base comece flutuando e ignorando a gravidade
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    private void Update()
    {
        // Se o cooldown estiver ativo, vai diminuindo o tempo como um cronômetro
        if (cooldownAtual > 0f)
        {
            cooldownAtual -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // SEGREDO 1: Se o cooldown ainda estiver rodando, ignora a colisão e cancela a função!
        if (cooldownAtual > 0f) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Prende o player na base
                player.SetReadyToLaunch(transform);
                
                // Se a base for do tipo que cai, e ainda não tiver caído, inicia a contagem!
                if (caiQuandoPisa && !jaCaiu)
                {
                    Cair();
                }
            }
        }
    }

    // SEGREDO 2: O evento que detecta a SAÍDA do Nano do lançador
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Assim que o jogador é disparado e sai do colisor, ativamos a trava de segurança!
            cooldownAtual = tempoCooldownReentrada;
        }
    }

    private async void Cair()
    {
        jaCaiu = true; // Trava pra não rodar duas vezes

        // Espera o tempo configurado (converte de segundos para milissegundos)
        await Task.Delay((int)(tempoParaCair * 1000));

        if (this == null) return; 

        // Muda o corpo pra Dynamic. Agora a gravidade da Unity puxa ele pra baixo!
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}