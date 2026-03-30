using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Efeito de Morte")]
    [SerializeField] private float knockbackSpeed = 15f;
    [SerializeField] private int reloadDelayMs = 400;

    [Header("Configurações do Lançamento")]
    public float powerMultiplier = 5f;
    public float maxDragDistance = 3f;

    [Header("Configurações da Trajetória")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public float trajectoryTime = 1f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead;

    private bool isReadyToLaunch = false;
    private bool isDragging = false;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private float originalGravity;
    private Transform currentLauncher;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        if (isDead || !isReadyToLaunch) return;

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

            foreach (Collider2D hit in hits)
            {
                if (hit.transform == currentLauncher)
                {
                    isDragging = true;
                    startPoint = currentLauncher.position;
                    trajectoryLine.enabled = true;
                    break;
                }
            }
        }

        if (isDragging)
        {
            DrawTrajectory();

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                trajectoryLine.enabled = false;
                endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Shoot();
            }
        }
    }

    public void SetReadyToLaunch(Transform launcherTransform)
    {
        if (isDead) return;
        
        currentLauncher = launcherTransform;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.position = launcherTransform.position;
        isReadyToLaunch = true;
    }

    private void Shoot()
    {
        Vector2 direction = (startPoint - endPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        rb.gravityScale = originalGravity;
        rb.AddForce(direction * distance * powerMultiplier, ForceMode2D.Impulse);
        
        isReadyToLaunch = false;
        currentLauncher = null;
    }

    private void DrawTrajectory()
    {
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (startPoint - currentMousePos).normalized;
        float distance = Vector2.Distance(startPoint, currentMousePos);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        Vector2 initialVelocity = (direction * distance * powerMultiplier) / rb.mass;

        Vector2 startPos = transform.position;
        Vector2 previousPos = startPos;
        
        trajectoryLine.positionCount = trajectoryResolution;
        trajectoryLine.SetPosition(0, startPos);

        for (int i = 1; i < trajectoryResolution; i++)
        {
            float t = i / (float)trajectoryResolution * trajectoryTime;
            Vector2 calculatedPosition = startPos + initialVelocity * t + 0.5f * Physics2D.gravity * originalGravity * (t * t);

            RaycastHit2D hit = Physics2D.Raycast(previousPos, calculatedPosition - previousPos, Vector2.Distance(previousPos, calculatedPosition));

            if (hit.collider != null && (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("Ground")))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                break;
            }

            trajectoryLine.SetPosition(i, calculatedPosition);
            previousPos = calculatedPosition;
        }
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