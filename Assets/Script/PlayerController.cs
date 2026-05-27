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
    [SerializeField] private float tempoDeQuiqueMorte = 0.15f;
    [SerializeField] private int sortingOrderMorte = 50;

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

    [Header("Efeito de Câmera e Mira")]
    public CinemachineVirtualCamera camVirtual; 
    public float zoomMultiplicador = 0.85f; 
    public float zoomMultiplicadorLancador = 0.95f;
    public float zoomVelocidade = 8f;
    
    // ======= NOVAS VARIÁVEIS DA MIRA =======
    [Tooltip("O máximo que a câmera vai olhar para frente (em unidades)")]
    public float maxCameraLookAhead = 4f; 
    [Tooltip("Velocidade que a câmera arrasta para olhar a mira")]
    public float cameraLookAheadSpeed = 5f;
    private CinemachineFramingTransposer framingTransposer;
    private Vector3 originalCameraOffset = Vector3.zero;
    
    // ======= VARIÁVEL DO SCREEN SHAKE =======
    private CinemachineImpulseSource impulseSource;

    private float tamanhoCameraOriginal;

    [Header("Efeitos Visuais (Game Feel)")]
    public TrailRenderer rastroArremesso;
    public Transform visualTransform;
    public Animator playerAnimator; 
    private SpriteRenderer playerSprite; 
    
    [Header("Tremor de Tensão")]
    public float forcaDoTremor = 0.05f;
    public float distanciaParaTremer = 2.5f;
    private Vector3 posicaoVisualOriginal; 

    [Header("Orientação do Desenho")]
    public SpriteOrientation spriteOriginalOlhaPara = SpriteOrientation.PointsUp;
    public float rotacaoNoLancador = 0f;

    [Header("Efeito Elástico (Celeste)")]
    public float velocidademinimaParaEsticar = 5f;
    public float velocidadeMaximaParaEsticar = 25f;
    [SerializeField] private float esticamentoBicoMaximo = 1.5f;
    [SerializeField] private float esmagamentoLadosMaximo = 0.7f;
    [SerializeField] private float tempoDoEfeitoElastico = 0.3f;
    private float timerElastico = 0f; 

    [Header("Lançamento Aéreo")]
    public int maxMidAirLaunches = 1;
    private int midAirLaunchesLeft;
    public bool puloDuploDesbloqueado = true; 
    
    [HideInInspector] public bool tutorialTempoInfinito = false; 

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
        impulseSource = GetComponent<CinemachineImpulseSource>(); // Pega o tremor automaticamente!
        originalGravity = rb.gravityScale;
        
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (camVirtual != null) 
        {
            tamanhoCameraOriginal = camVirtual.m_Lens.OrthographicSize;
            
            // ======= INICIA O FRAMING TRANSPOSER =======
            framingTransposer = camVirtual.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                originalCameraOffset = framingTransposer.m_TrackedObjectOffset;
            }
        }
        
        if (rastroArremesso != null) { rastroArremesso.emitting = false; rastroArremesso.Clear(); }
        
        if (visualTransform != null) 
        {
            posicaoVisualOriginal = visualTransform.localPosition;
            playerSprite = visualTransform.GetComponent<SpriteRenderer>(); 
        }
    }

    private void FixedUpdate()
    {
        if (rb != null) lastFrameVelocity = rb.linearVelocity;
    }

    private void Update()
    {
        if (timerImunidadeLancador > 0f) timerImunidadeLancador -= Time.unscaledDeltaTime;

        // ======= MÁGICA DA CÂMERA (ZOOM E MIRA) =======
        if (camVirtual != null)
        {
            float zoomAlvo = tamanhoCameraOriginal;
            Vector3 offsetAlvo = originalCameraOffset;

            if (isDragging && !isDead)
            {
                zoomAlvo = tamanhoCameraOriginal * (isReadyToLaunch ? zoomMultiplicadorLancador : zoomMultiplicador);

                // Calcula a direção em que o jogador quer lançar o Nano
                Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (startPoint - currentMousePos).normalized;
                float distance = Vector2.Distance(startPoint, currentMousePos);
                distance = Mathf.Clamp(distance, 0, maxDragDistance);

                // Empurra o centro da câmera na direção do lançamento, limitado pelo máximo que definimos!
                float proporcaoForca = distance / maxDragDistance;
                offsetAlvo = originalCameraOffset + (Vector3)(direction * proporcaoForca * maxCameraLookAhead);
            }

            // Aplica o Zoom
            camVirtual.m_Lens.OrthographicSize = Mathf.Lerp(camVirtual.m_Lens.OrthographicSize, zoomAlvo, Time.unscaledDeltaTime * zoomVelocidade);
            
            // Aplica a deslocação da Mira (suavemente)
            if (framingTransposer != null)
            {
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(framingTransposer.m_TrackedObjectOffset, offsetAlvo, Time.unscaledDeltaTime * cameraLookAheadSpeed);
            }
        }

        if (isDead || PauseMenu.isPaused) return;

        if (isReadyToLaunch && currentLauncher != null) transform.position = currentLauncher.position;

        if (isDragging && !isReadyToLaunch)
        {
            if (!tutorialTempoInfinito)
            {
                contadorMira += Time.unscaledDeltaTime;
                if (contadorMira >= tempoMaximoMira)
                {
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02f;
                    rb.gravityScale = originalGravity;
                }
            }
        }

        if (rastroArremesso != null && rb.linearVelocity.magnitude < 0.5f && !isDragging) rastroArremesso.emitting = false;

        AtualizarVisualCeleste();

        if (playerAnimator != null && !isDragging && !isReadyToLaunch && rb.linearVelocity.magnitude > 1f)
            playerAnimator.SetBool("IsFlying", true);
        else if (playerAnimator != null)
            playerAnimator.SetBool("IsFlying", false);

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (isReadyToLaunch && currentLauncher != null)
            {
                IniciarArraste(currentLauncher.position);
            }
            else if (midAirLaunchesLeft > 0 && puloDuploDesbloqueado) 
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

            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float currentDragDistance = Vector2.Distance(startPoint, currentMousePos);
            
            if (currentDragDistance >= distanciaParaTremer && visualTransform != null)
            {
                float tremorX = Random.Range(-forcaDoTremor, forcaDoTremor);
                float tremorY = Random.Range(-forcaDoTremor, forcaDoTremor);
                visualTransform.localPosition = posicaoVisualOriginal + new Vector3(tremorX, tremorY, 0f);
            }
            else if (visualTransform != null)
            {
                visualTransform.localPosition = posicaoVisualOriginal;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                trajectoryLine.enabled = false;
                
                if (visualTransform != null) visualTransform.localPosition = posicaoVisualOriginal; 
                
                endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Shoot();
            }
        }
    }

    private void AtualizarVisualCeleste()
    {
        if (visualTransform == null) return;

        if (timerElastico > 0f) timerElastico -= Time.deltaTime;

        if (isReadyToLaunch || isDead || rb.linearVelocity.magnitude < 0.1f)
        {
            visualTransform.rotation = Quaternion.Euler(0, 0, rotacaoNoLancador);
            visualTransform.localScale = Vector3.one;
            return;
        }

        float velocidade = rb.linearVelocity.magnitude;

        if (velocidade > 0.1f && !isDragging)
        {
            float anguloBase = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            float compensacao = (spriteOriginalOlhaPara == SpriteOrientation.PointsUp) ? -90f : 0f;
            visualTransform.rotation = Quaternion.Euler(0, 0, anguloBase + compensacao);

            if (velocidade > velocidademinimaParaEsticar && timerElastico > 0f)
            {
                float fatorVelocidade = Mathf.InverseLerp(velocidademinimaParaEsticar, velocidadeMaximaParaEsticar, velocidade);
                float fatorTempo = Mathf.Clamp01(timerElastico / tempoDoEfeitoElastico);
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
        timerElastico = 0f; 
        
        if (visualTransform != null)
        {
            visualTransform.rotation = Quaternion.Euler(0, 0, rotacaoNoLancador);
            visualTransform.localScale = Vector3.one;
        }

        midAirLaunchesLeft = maxMidAirLaunches;
        if (rastroArremesso != null) rastroArremesso.emitting = false;
        
        if (playerAnimator != null)
        {
            int carinhaSorteada = Random.Range(1, 7);
            playerAnimator.SetInteger("RostoIdle", carinhaSorteada);
            playerAnimator.SetTrigger("NovaExpressao");
        }
    }

   private void IniciarArraste(Vector2 pontoDeInicio)
    {
        if(SoundManager.instance != null) SoundManager.instance.TocarPuxar();

        if (playerAnimator != null)
        {
            int sorteio = Random.Range(0, 2); 
            if (sorteio == 0) playerAnimator.SetTrigger("Pull_A");
            else playerAnimator.SetTrigger("Pull_B");
        }

        isDragging = true;
        startPoint = pontoDeInicio;
        telaPosicaoInicialMouse = Input.mousePosition; 
        trajectoryLine.enabled = true;
        contadorMira = 0f; 

        if (!isReadyToLaunch)
        {
            if (tutorialTempoInfinito)
            {
                Time.timeScale = 0f;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero; 
            }
            else
            {
                if(SoundManager.instance != null) SoundManager.instance.TocarSlowMotion();
                rb.gravityScale = originalGravity * slowMotionGravityMultiplier; 
                Time.timeScale = slowMotionTimeScale;
            }
            
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void Shoot()
    {
        if(SoundManager.instance != null) SoundManager.instance.PararPuxar();
        if (SoundManager.instance != null) SoundManager.instance.TocarSoltar();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        Vector2 direction = (startPoint - endPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);
        distance = Mathf.Clamp(distance, 0, maxDragDistance);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(direction * distance * powerMultiplier, ForceMode2D.Impulse);
        
        timerElastico = tempoDoEfeitoElastico;
        timerImunidadeLancador = imunidadeMesmoLancador;
        isReadyToLaunch = false;
        currentLauncher = null;

        if (rastroArremesso != null) rastroArremesso.emitting = true;
        if (playerAnimator != null) playerAnimator.SetBool("IsFlying", true);
    }

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
        if(SoundManager.instance != null) SoundManager.instance.TocarMorte();
        if (SoundManager.instance != null) SoundManager.instance.PararPuxar();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isDead = true;
        col.enabled = false;
        
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(); 
        }
        
        if (playerSprite != null) playerSprite.sortingOrder = sortingOrderMorte;

        if (visualTransform != null)
        {
            visualTransform.localScale = Vector3.one;
            visualTransform.rotation = Quaternion.identity;
        }

        rb.gravityScale = 0f; 

        if (rastroArremesso != null) rastroArremesso.emitting = false;
        if (playerAnimator != null) playerAnimator.SetBool("IsFlying", false);
        if (playerAnimator != null) playerAnimator.SetTrigger("Morte");

        float tempoDecorrido = 0f;
        Vector2 velocidadeInicial = direction * knockbackSpeed;

        while (tempoDecorrido < tempoDeQuiqueMorte)
        {
            if (this == null || rb == null) return; 

            rb.linearVelocity = Vector2.Lerp(velocidadeInicial, Vector2.zero, tempoDecorrido / tempoDeQuiqueMorte);
            tempoDecorrido += Time.deltaTime;
            
            await Task.Yield(); 
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (playerAnimator != null)
        {
            await Task.Yield();

            while (playerAnimator != null && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                if (this == null) return;
                await Task.Yield();
            }
        }

        if (this == null) return;

        // ======= A MUDANÇA ACONTECE AQUI =======
        if (rastroArremesso != null) { rastroArremesso.emitting = false; rastroArremesso.Clear(); }
        
        // Em vez do corte seco do SceneManager, chamamos a transição linda de morte!
        GerenciadorTransicoes.instance.TrocarCena(SceneManager.GetActiveScene().name);
    }
}