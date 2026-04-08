using UnityEngine;
using System.Threading.Tasks; // Para usar o temporizador moderno

[RequireComponent(typeof(Rigidbody2D))] // Agora a base precisa de física para poder cair
public class LauncherBase : MonoBehaviour
{
    [Header("Configurações de Queda")]
    [SerializeField] private bool caiQuandoPisa = false;
    [SerializeField] private float tempoParaCair = 0.5f; // Meio segundo de instabilidade antes de despencar

    private Rigidbody2D rb;
    private bool jaCaiu = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Garante que a base comece flutuando e ignorando a gravidade
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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

    private async void Cair()
    {
        jaCaiu = true; // Trava pra não rodar duas vezes

        // Espera o tempo configurado (converte de segundos para milissegundos)
        await Task.Delay((int)(tempoParaCair * 1000));

        // Segurança: Se você resetar a fase (morrer) antes desse tempo acabar, o objeto é destruído.
        // Isso evita que o código tente fazer cair algo que não existe mais.
        if (this == null) return; 

        // O PULO DO GATO: Muda o corpo pra Dynamic. Agora a gravidade da Unity puxa ele pra baixo!
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Opcional: Destrói a base depois de 3 segundos caindo pra não pesar a memória do jogo
        Destroy(gameObject, 3f); 
    }
}