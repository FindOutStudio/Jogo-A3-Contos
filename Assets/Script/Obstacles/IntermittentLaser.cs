using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] 
public class IntermittentLaser : MonoBehaviour
{
    [Header("Bases do Laser")]
    [SerializeField] private Transform baseA;
    [SerializeField] private Transform baseB;
    [SerializeField] private float laserThickness = 0.5f; 

    [Header("Configurações de Tempo")]
    [Tooltip("Tempo (em segundos) antes do laser começar a funcionar pela primeira vez")]
    [SerializeField] private float delayInicial = 0f; 
    [SerializeField] private float timeOn = 2f; 
    [SerializeField] private float timeOff = 1.5f; 
    [SerializeField] private bool startOn = true; 

    [Header("Visual do Curto-Circuito (Raio)")]
    [SerializeField] private LineRenderer arcRenderer; 
    [SerializeField] private int segments = 10; 
    [SerializeField] private float arcVolatility = 0.5f; 
    [SerializeField] private float fps = 20f; 

    private BoxCollider2D laserCollider;
    private float timer;
    private bool isLaserActive;
    private float arcTimer; 
    
    private AudioSource meuAudio; 
    private bool aguardandoDelay; // Variável para controlar o estado do Delay

    private void Awake()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        laserCollider.isTrigger = true; 
    }

    private void Start()
    {
        SetupLaserTransform();
        ConfigurarAudio3D(); 

        // LÓGICA DO NOVO DELAY INICIAL
        if (delayInicial > 0f)
        {
            // Durante a largada, força o laser a ficar desligado enquanto espera
            isLaserActive = false; 
            timer = delayInicial;
            aguardandoDelay = true;
        }
        else
        {
            // Se não tem delay, começa o jogo na hora usando o StartOn
            isLaserActive = startOn;
            timer = isLaserActive ? timeOn : timeOff;
            aguardandoDelay = false;
        }

        UpdateLaserState();
    }

    private void Update()
    {
        // === 1. FASE DE ESPERA DO DELAY ===
        if (aguardandoDelay)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                aguardandoDelay = false; // Saiu do delay inicial!
                
                // Agora ele assume o estado de inicialização real
                isLaserActive = startOn;
                timer = isLaserActive ? timeOn : timeOff;
                UpdateLaserState();
            }
            return; // Impede que o resto do código rode enquanto estiver no delay
        }

        // === 2. LOOP NORMAL DO JOGO (PISCA-PISCA) ===
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isLaserActive = !isLaserActive;
            timer = isLaserActive ? timeOn : timeOff;
            UpdateLaserState();
        }

        // === 3. EFEITO VISUAL DO RAIO TREMENDO ===
        if (isLaserActive)
        {
            arcTimer += Time.deltaTime;
            if (arcTimer >= 1f / fps)
            {
                DrawArc();
                arcTimer = 0f;
            }
        }
    }

    private void SetupLaserTransform()
    {
        if (baseA != null && baseB != null)
        {
            Vector2 start = baseA.position;
            Vector2 end = baseB.position;

            Vector2 center = (start + end) / 2f;
            transform.position = center;

            Vector2 dir = end - start;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            float length = Vector2.Distance(start, end);
            laserCollider.size = new Vector2(length, laserThickness);

            if (arcRenderer != null)
            {
                arcRenderer.positionCount = segments;
            }
        }
    }

    private void ConfigurarAudio3D()
    {
        meuAudio = GetComponent<AudioSource>();
        if (meuAudio == null)
        {
            meuAudio = gameObject.AddComponent<AudioSource>();
        }

        meuAudio.playOnAwake = false;
        meuAudio.loop = true;
        meuAudio.spatialBlend = 1f;
        meuAudio.rolloffMode = AudioRolloffMode.Linear;
        meuAudio.minDistance = 2f;
        meuAudio.maxDistance = 15f;

        if (SoundManager.instance != null)
        {
            meuAudio.clip = SoundManager.instance.obstaculoLaser;
            meuAudio.volume = SoundManager.instance.volumeLaser;
            meuAudio.pitch = Random.Range(0.95f, 1.05f); 
        }
    }

    private void UpdateLaserState()
    {
        laserCollider.enabled = isLaserActive;
        if (arcRenderer != null) arcRenderer.enabled = isLaserActive;

        // === CONTROLE DO SOM ===
        if (meuAudio != null && meuAudio.clip != null)
        {
            if (isLaserActive)
            {
                if (!meuAudio.isPlaying) meuAudio.Play(); 
            }
            else
            {
                if (meuAudio.isPlaying) meuAudio.Stop(); 
            }
        }
    }

    private void DrawArc()
    {
        if (arcRenderer == null || baseA == null || baseB == null) return;

        Vector2 start = baseA.position;
        Vector2 end = baseB.position;
        
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        arcRenderer.SetPosition(0, start); 

        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector2 basePos = Vector2.Lerp(start, end, t);

            float randomOffset = Random.Range(-arcVolatility, arcVolatility);
            Vector2 point = basePos + (perpendicular * randomOffset);
            
            arcRenderer.SetPosition(i, point);
        }

        arcRenderer.SetPosition(segments - 1, end);
    }
}