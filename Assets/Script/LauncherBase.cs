using UnityEngine;

public class LauncherBase : MonoBehaviour
{
    [Header("Configurações do Lançador")]
    public float powerMultiplier = 5f;
    public float maxDragDistance = 3f;

    [Header("Configurações da Trajetória")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30; // Quantos pontos formam a curva

    private Vector2 startPoint;
    private Vector2 endPoint;
    
    private bool isPlayerReady = false;
    private bool isDragging = false;
    private Rigidbody2D playerRb;
    private float originalGravity;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerRb = collision.GetComponent<Rigidbody2D>();
            
            originalGravity = playerRb.gravityScale;
            playerRb.gravityScale = 0f;
            
            playerRb.linearVelocity = Vector2.zero;
            playerRb.transform.position = transform.position; 
            
            isPlayerReady = true;
        }
    }

    private void OnMouseDown()
    {
        if (isPlayerReady)
        {
            isDragging = true;
            startPoint = transform.position; 
            trajectoryLine.enabled = true; // Liga a linha quando começa a mirar
        }
    }

    void Update()
    {
        if (isDragging)
        {
            DrawTrajectory(); // Atualiza o desenho da linha todo frame

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                trajectoryLine.enabled = false; // Esconde a linha ao soltar
                endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Shoot();
            }
        }
    }

    void Shoot()
    {
        if (playerRb == null) return;

        Vector2 direction = (startPoint - endPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        playerRb.gravityScale = originalGravity;

        playerRb.AddForce(direction * distance * powerMultiplier, ForceMode2D.Impulse);
        
        isPlayerReady = false;
        playerRb = null;
    }

    void DrawTrajectory()
    {
        // 1. Calcula para onde o jogador está mirando agora
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (startPoint - currentMousePos).normalized;
        float distance = Vector2.Distance(startPoint, currentMousePos);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        // 2. Calcula a velocidade inicial (Força / Massa)
        Vector2 initialVelocity = (direction * distance * powerMultiplier) / playerRb.mass;

        // 3. Prepara a linha
        trajectoryLine.positionCount = trajectoryResolution;
        Vector2 startPos = transform.position;

        // 4. Desenha os pontos baseados na fórmula da física
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i / (float)trajectoryResolution * 3f; // Simula 3 segundos no futuro
            
            Vector2 calculatedPosition = startPos + initialVelocity * t + 0.5f * Physics2D.gravity * originalGravity * (t * t);
            
            trajectoryLine.SetPosition(i, calculatedPosition);
        }
    }
}