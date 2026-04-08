using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Cinemachine; 

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
    public float imunidadeMesmoLancador = 0.5f; 

    [Header("Ricochete Clássico (Espelho)")]
    public float multiplicadorRicochete = 0.9f; 
    [Tooltip("Força extra jogada para cima após o quique pra linha não 'broxar' e cair rápido")]
    public float forcaExtraParaCima = 5f; 
    public int maximoDeQuiquesNaLinha = 1; 

    [Header("Camera Lenta (Zelda)")]
    public float slowMotionTimeScale = 0.35f; 
    public float slowMotionGravityMultiplier = 0.2f;
    public float tempoMaximoMira = 2f; 
    private float contadorMira;

    [Header("Efeito de Câmera")]
    public float zoomMultiplicador = 0.85f; 
    public float zoomMultiplicadorLancador = 0.95f;
    public float zoomVelocidade = 8f;
    public CinemachineVirtualCamera camVirtual; 
    private float tamanhoCameraOriginal;

    [Header("Efeitos Visuais")]
    public TrailRenderer rastroArremesso;

    [Header("Lançamento Aéreo")]
    public int maxMidAirLaunches = 1;
    private int midAirLaunchesLeft;

    [Header("Configurações da Trajetória")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 40; 
    public float trajectoryTime = 1.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead;

    private bool isReadyToLaunch = false;
    private bool isDragging = false;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private float originalGravity;
    private Transform currentLauncher;

    private Vector2 telaPosicaoInicialMouse; 
    private Transform ultimoLancador;
    private float timerImunidadeLancador;

    private Vector2 lastFrameVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalGravity = rb.gravityScale;
        
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 

        if (camVirtual != null) tamanhoCameraOriginal = camVirtual.m_Lens.OrthographicSize;
        if (rastroArremesso != null) { rastroArremesso.emitting = false; rastroArremesso.Clear(); }
    }

    private void FixedUpdate()
    {
        if (rb != null) lastFrameVelocity = rb.linearVelocity;
    }

    private void Update()
    {
        if (timerImunidadeLancador > 0f) timerImunidadeLancador -= Time.unscaledDeltaTime;

        if (camVirtual != null)
        {
            float zoomAlvo = tamanhoCameraOriginal;
            if (isDragging) zoomAlvo = tamanhoCameraOriginal * (isReadyToLaunch ? zoomMultiplicadorLancador : zoomMultiplicador);
            camVirtual.m_Lens.OrthographicSize = Mathf.Lerp(camVirtual.m_Lens.OrthographicSize, zoomAlvo, Time.unscaledDeltaTime * zoomVelocidade);
        }

        if (isDead) return;

        if (isReadyToLaunch && currentLauncher != null) transform.position = currentLauncher.position;

        if (isDragging && !isReadyToLaunch)
        {
            contadorMira += Time.unscaledDeltaTime;
            if (contadorMira >= tempoMaximoMira)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                rb.gravityScale = originalGravity;
            }
        }

        if (rastroArremesso != null && rb.linearVelocity.magnitude < 0.5f && !isDragging) rastroArremesso.emitting = false;

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (isReadyToLaunch)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);
                foreach (Collider2D hit in hits)
                {
                    if (hit.transform == currentLauncher) { IniciarArraste(currentLauncher.position); break; }
                }
            }
            else if (midAirLaunchesLeft > 0)
            {
                IniciarArraste(transform.position);
                midAirLaunchesLeft--;
            }
        }

        if (isDragging)
        {
            if (isReadyToLaunch && currentLauncher != null) startPoint = currentLauncher.position;
            else if (!isReadyToLaunch) startPoint = Camera.main.ScreenToWorldPoint(telaPosicaoInicialMouse);

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
        if (launcherTransform == ultimoLancador && timerImunidadeLancador > 0f) return;

        currentLauncher = launcherTransform;
        ultimoLancador = launcherTransform; 

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.position = launcherTransform.position;
        isReadyToLaunch = true;
        
        midAirLaunchesLeft = maxMidAirLaunches;
        if (rastroArremesso != null) rastroArremesso.emitting = false;
    }

    private void IniciarArraste(Vector2 pontoDeInicio)
    {
        isDragging = true;
        startPoint = pontoDeInicio;
        telaPosicaoInicialMouse = Input.mousePosition; 
        trajectoryLine.enabled = true;
        contadorMira = 0f; 

        if (!isReadyToLaunch)
        {
            rb.gravityScale = originalGravity * slowMotionGravityMultiplier; 
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void Shoot()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Mira estilo Angry Birds (Puxa para um lado, atira para o outro)
        Vector2 direction = (startPoint - endPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(direction * distance * powerMultiplier, ForceMode2D.Impulse);
        
        timerImunidadeLancador = imunidadeMesmoLancador;
        isReadyToLaunch = false;
        currentLauncher = null;

        if (rastroArremesso != null) rastroArremesso.emitting = true;
    }

    private void DrawTrajectory()
    {
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Direção oposta ao mouse (Angry Birds)
        Vector2 direction = (startPoint - currentMousePos).normalized;
        float distance = Vector2.Distance(startPoint, currentMousePos);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        Vector2 currentVel = (direction * distance * powerMultiplier) / rb.mass;
        Vector2 currentPos = transform.position;

        trajectoryLine.positionCount = trajectoryResolution;
        trajectoryLine.SetPosition(0, currentPos);

        float timeStep = trajectoryTime / trajectoryResolution; 
        int bouncesCalculated = 0;

        for (int i = 1; i < trajectoryResolution; i++)
        {
            Vector2 nextPos = currentPos + currentVel * timeStep + 0.5f * Physics2D.gravity * originalGravity * (timeStep * timeStep);
            RaycastHit2D hit = Physics2D.Raycast(currentPos, nextPos - currentPos, Vector2.Distance(currentPos, nextPos));

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Bouncy") && bouncesCalculated < maximoDeQuiquesNaLinha)
                {
                    currentPos = hit.point;
                    trajectoryLine.SetPosition(i, currentPos);
                    
                    PlataformaSinuca sinuca = hit.collider.GetComponent<PlataformaSinuca>();
                    if (sinuca != null)
                    {
                        currentVel = sinuca.direcaoDoRicochete.normalized * sinuca.forcaDoRicochete;
                    }
                    else
                    {
                        // O SEGREDO PRA LINHA NÃO BROXAR: Adiciona a força extra pra cima na previsão!
                        currentVel = Vector2.Reflect(currentVel, hit.normal) * multiplicadorRicochete;
                        currentVel += Vector2.up * forcaExtraParaCima; 
                    }

                    currentPos += hit.normal * 0.05f; 
                    bouncesCalculated++;
                    continue; 
                }
                else if (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("Ground"))
                {
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPosition(i, hit.point);
                    break;
                }
            }

            currentVel += Physics2D.gravity * originalGravity * timeStep;
            currentPos = nextPos;
            trajectoryLine.SetPosition(i, currentPos);
        }
    }

    private async void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Bouncy"))
        {
            PlataformaSinuca sinuca = collision.gameObject.GetComponent<PlataformaSinuca>();
            if (sinuca != null)
            {
                rb.linearVelocity = sinuca.direcaoDoRicochete.normalized * sinuca.forcaDoRicochete;
            }
            else
            {
                // O SEGREDO PRA FÍSICA NÃO BROXAR: Adiciona a força extra pra cima no player também!
                Vector2 normal = collision.contacts[0].normal;
                Vector2 reboteDir = Vector2.Reflect(lastFrameVelocity, normal);
                rb.linearVelocity = (reboteDir * multiplicadorRicochete) + (Vector2.up * forcaExtraParaCima);
            }
            return; 
        }

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
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isDead = true;
        rb.gravityScale = 0f;
        col.enabled = false;
        rb.linearVelocity = direction * knockbackSpeed;

        if (rastroArremesso != null) rastroArremesso.emitting = false;

        await Task.Delay(reloadDelayMs);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (rastroArremesso != null) { rastroArremesso.emitting = false; rastroArremesso.Clear(); }
    }
}