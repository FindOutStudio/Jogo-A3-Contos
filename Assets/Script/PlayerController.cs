using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Cinemachine; 

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public enum SpriteOrientation { PointsUp, PointsRight }

    [Header("Efeito de Morte")]
    [SerializeField] private float knockbackSpeed = 15f;
    [SerializeField] private int reloadDelayMs = 400;

    [Header("Configurações do Lançamento")]
    public float powerMultiplier = 5f;
    public float maxDragDistance = 3f;
    public float imunidadeMesmoLancador = 0.5f; 

    [Header("Ricochete Clássico (Espelho)")]
    public float multiplicadorRicochete = 0.9f; 
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

    [Header("Efeitos Visuais (Game Feel)")]
    public TrailRenderer rastroArremesso;
    public Transform visualTransform;
    
    [Header("Orientação do Desenho")]
    public SpriteOrientation spriteOriginalOlhaPara = SpriteOrientation.PointsUp;
    public float rotacaoNoLancador = 0f;

    [Header("Efeito Elástico (Celeste)")]
    public float velocidadeMinimaParaEsticar = 5f;
    public float velocidadeMaximaParaEsticar = 25f;
    [SerializeField] private float esticamentoBicoMaximo = 1.5f;
    [SerializeField] private float esmagamentoLadosMaximo = 0.7f;
    
    [Tooltip("Tempo em segundos que o efeito dura no começo do tiro antes de voltar ao normal (ex: 0.3)")]
    [SerializeField] private float tempoDoEfeitoElastico = 0.3f;
    private float timerElastico = 0f; // Cronômetro interno

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
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

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

        if (isDead || PauseMenu.isPaused) return;

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

        AtualizarVisualCeleste();

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (isReadyToLaunch && currentLauncher != null)
            {
                IniciarArraste(currentLauncher.position);
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

    private void AtualizarVisualCeleste()
    {
        if (visualTransform == null) return;

        // Diminui o cronômetro do elástico se ele estiver rodando
        if (timerElastico > 0f)
        {
            timerElastico -= Time.deltaTime;
        }

        if (isReadyToLaunch || isDead || rb.linearVelocity.magnitude < 0.1f)
        {
            visualTransform.rotation = Quaternion.Euler(0, 0, rotacaoNoLancador);
            visualTransform.localScale = Vector3.one;
            return;
        }

        float velocidade = rb.linearVelocity.magnitude;

        if (velocidade > 0.1f && !isDragging)
        {
            // 1. Rotação Foguete (Baseada na direção do movimento)
            float anguloBase = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            float compensacao = (spriteOriginalOlhaPara == SpriteOrientation.PointsUp) ? -90f : 0f;
            visualTransform.rotation = Quaternion.Euler(0, 0, anguloBase + compensacao);

            // 2. Squash and Stretch COM DECAIMENTO
            if (velocidade > velocidadeMinimaParaEsticar && timerElastico > 0f)
            {
                // Fator de velocidade (quão rápido ele está)
                float fatorVelocidade = Mathf.InverseLerp(velocidadeMinimaParaEsticar, velocidadeMaximaParaEsticar, velocidade);
                
                // Fator de tempo (vai de 1 no momento do tiro, caindo até 0)
                float fatorTempo = Mathf.Clamp01(timerElastico / tempoDoEfeitoElastico);
                
                // O efeito final é a mistura da velocidade com o tempo acabando
                float fatorFinal = fatorVelocidade * fatorTempo;

                float estica = Mathf.Lerp(1f, esticamentoBicoMaximo, fatorFinal);
                float esmaga = Mathf.Lerp(1f, esmagamentoLadosMaximo, fatorFinal);

                if (spriteOriginalOlhaPara == SpriteOrientation.PointsUp)
                    visualTransform.localScale = new Vector3(esmaga, estica, 1f); 
                else
                    visualTransform.localScale = new Vector3(estica, esmaga, 1f); 
            }
            else
            {
                // Se o tempo acabou, volta ao tamanho original (mas continua apontando pra direção)
                visualTransform.localScale = Vector3.one;
            }
        }
    }

    public void SetReadyToLaunch(Transform launcherTransform)
    {
        if (isDead) return;

        bool tiroFalhou = rb.linearVelocity.magnitude < 0.5f;
        if (!tiroFalhou && launcherTransform == ultimoLancador && timerImunidadeLancador > 0f) return;

        currentLauncher = launcherTransform;
        ultimoLancador = launcherTransform; 

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.position = launcherTransform.position;
        isReadyToLaunch = true;
        timerElastico = 0f; // Reseta o elástico
        
        if (visualTransform != null)
        {
            visualTransform.rotation = Quaternion.Euler(0, 0, rotacaoNoLancador);
            visualTransform.localScale = Vector3.one;
        }

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

        Vector2 direction = (startPoint - endPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(direction * distance * powerMultiplier, ForceMode2D.Impulse);
        
        // ======= MÁGICA: ATIVA O TEMPO DO EFEITO AQUI =======
        timerElastico = tempoDoEfeitoElastico;

        timerImunidadeLancador = imunidadeMesmoLancador;
        isReadyToLaunch = false;
        currentLauncher = null;

        if (rastroArremesso != null) rastroArremesso.emitting = true;
    }

    // ... (A função DrawTrajectory continua exatamente igual) ...
    private void DrawTrajectory()
    {
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
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
            // O impacto com a parede Bouncy TAMBÉM reativa o efeito elástico!
            timerElastico = tempoDoEfeitoElastico;

            PlataformaSinuca sinuca = collision.gameObject.GetComponent<PlataformaSinuca>();
            if (sinuca != null)
            {
                rb.linearVelocity = sinuca.direcaoDoRicochete.normalized * sinuca.forcaDoRicochete;
            }
            else
            {
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