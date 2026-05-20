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
                Vector2 direcaoDaExplosao = (collision.transform.position - transform.position).normalized;

                // 2. O Toque de "Game Feel":
                if (direcaoDaExplosao.y < 0.2f) 
                {
                    direcaoDaExplosao.y = 0.5f;
                    direcaoDaExplosao = direcaoDaExplosao.normalized;
                }

                // 3. O Arremesso:
                rb.linearVelocity = direcaoDaExplosao * forcaDaExplosao;

                // === 4. O SOM 3D DIRETO DA CENTRAL ===
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.TocarSom3D(
                        SoundManager.instance.obstaculoBomba, 
                        transform.position, 
                        SoundManager.instance.volumeBomba
                    );
                }

                // 5. Destruição:
                if (destruirAoExplodir)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}