using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MinaImpulso : MonoBehaviour
{
    [Header("Configurações da Explosão")]
    [Tooltip("A força com que o player será arremessado")]
    public float forcaDaExplosao = 25f;
    
    [Tooltip("Destruir a bomba após explodir?")]
    public bool destruirAoExplodir = true;

    private void Awake()
    {
        // Garante que o colisor seja um Trigger (fantasma) para ser a área de detecção
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                // 1. A Matemática da Explosão:
                // Subtrai a posição da bomba da posição do player para achar o vetor de afastamento
                Vector2 direcaoDaExplosao = (collision.transform.position - transform.position).normalized;

                // 2. O Toque de "Game Feel":
                // Se a explosão pegar o player muito de cima ou perfeitamente de lado, 
                // forçamos o vetor a empurrar ele um pouquinho para cima pra não bater a cara no chão.
                if (direcaoDaExplosao.y < 0.2f) 
                {
                    direcaoDaExplosao.y = 0.5f;
                    direcaoDaExplosao = direcaoDaExplosao.normalized;
                }

                // 3. O Arremesso:
                // Sobrescrevemos a velocidade atual pela velocidade da explosão!
                rb.linearVelocity = direcaoDaExplosao * forcaDaExplosao;

                // 4. (Opcional) Efeitos e Destruição:
                // Aqui você pode colocar um som de explosão ou partículas depois!
                if (destruirAoExplodir)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}