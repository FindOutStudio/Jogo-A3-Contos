using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks; 

[RequireComponent(typeof(Rigidbody2D))] 
[RequireComponent(typeof(Collider2D))]
public class PlayerDeath : MonoBehaviour
{
    [Header("Efeito de Morte")]
    [SerializeField] private float knockbackSpeed = 15f; 
    [SerializeField] private int reloadDelayMs = 400; 

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private async void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            if (knockbackDir.y < 0.5f) knockbackDir.y = 0.5f; 
            
            await DeathSequence(knockbackDir.normalized);
        }
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            if (knockbackDir.y < 0.5f) knockbackDir.y = 0.5f;

            await DeathSequence(knockbackDir.normalized);
        }
    }

    private async Task DeathSequence(Vector2 direction)
    {
        isDead = true;


        rb.gravityScale = 0f; 
        col.enabled = false; 

        rb.linearVelocity = direction * knockbackSpeed;

        await Task.Delay(reloadDelayMs);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}