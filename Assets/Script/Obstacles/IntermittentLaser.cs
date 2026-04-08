using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] 
public class IntermittentLaser : MonoBehaviour
{
    [Header("Bases do Laser")]
    [SerializeField] private Transform baseA;
    [SerializeField] private Transform baseB;
    [SerializeField] private float laserThickness = 0.5f; 

    [Header("Configurações de Tempo")]
    [SerializeField] private float timeOn = 2f; 
    [SerializeField] private float timeOff = 1.5f; 
    [SerializeField] private bool startOn = true; 

    [Header("Visual do Curto-Circuito (Raio)")]
    [SerializeField] private LineRenderer arcRenderer; // Arraste o Line Renderer pra cá
    [SerializeField] private int segments = 10; // Em quantos "pedacinhos" o raio se quebra
    [SerializeField] private float arcVolatility = 0.5f; // O quão longe ele pula pros lados
    [SerializeField] private float fps = 20f; // Quantas vezes por segundo ele muda de forma

    private BoxCollider2D laserCollider;
    private float timer;
    private bool isLaserActive;
    private float arcTimer; // Cronômetro pra animação do raio

    private void Awake()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        laserCollider.isTrigger = true; 
    }

    private void Start()
    {
        SetupLaserTransform();
        isLaserActive = startOn;
        timer = isLaserActive ? timeOn : timeOff;
        UpdateLaserState();
    }

    private void Update()
    {
        // 1. O relógio principal (Liga/Desliga o perigo)
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isLaserActive = !isLaserActive;
            timer = isLaserActive ? timeOn : timeOff;
            UpdateLaserState();
        }

        // 2. A Animação Visual do Raio
        if (isLaserActive)
        {
            arcTimer -= Time.deltaTime;
            // Se passou o tempo do "frame", desenha um novo formato pro raio
            if (arcTimer <= 0f)
            {
                DrawArc();
                arcTimer = 1f / fps; // Reseta o cronômetro do raio
            }
        }
    }

    private void SetupLaserTransform()
    {
        if (baseA == null || baseB == null) return;

        // Física e Rotação do colisor (igual antes)
        transform.position = (baseA.position + baseB.position) / 2f;
        Vector2 direction = baseB.position - baseA.position;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector3(distance, laserThickness, 1f);

        // Prepara o LineRenderer com a quantidade certa de "pontos de quebra"
        if (arcRenderer != null)
        {
            arcRenderer.positionCount = segments + 1;
        }
    }

    private void UpdateLaserState()
    {
        laserCollider.enabled = isLaserActive;
        if (arcRenderer != null) arcRenderer.enabled = isLaserActive;
    }

    // A MÁGICA DA ELETRICIDADE
    private void DrawArc()
    {
        if (arcRenderer == null || baseA == null || baseB == null) return;

        Vector2 start = baseA.position;
        Vector2 end = baseB.position;
        
        // Calcula qual é o lado (perpendicular) para fazer o raio saltar
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // O primeiro ponto é sempre cravado na Base A
        arcRenderer.SetPosition(0, start); 

        // Gera os pontos do meio do caminho
        for (int i = 1; i < segments; i++)
        {
            // Acha o ponto exato na linha reta (Lerp = Linear Interpolation)
            float t = (float)i / segments;
            Vector2 basePos = Vector2.Lerp(start, end, t);

            // Joga esse ponto um pouco pro lado aleatoriamente
            float randomJitter = Random.Range(-arcVolatility, arcVolatility);
            Vector2 finalPos = basePos + (perpendicular * randomJitter);

            arcRenderer.SetPosition(i, finalPos);
        }

        // O último ponto é sempre cravado na Base B
        arcRenderer.SetPosition(segments, end); 
    }
}