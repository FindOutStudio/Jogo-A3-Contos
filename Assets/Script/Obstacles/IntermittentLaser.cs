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
    [SerializeField] private LineRenderer arcRenderer; 
    [SerializeField] private int segments = 10; 
    [SerializeField] private float arcVolatility = 0.5f; 
    [SerializeField] private float fps = 20f; 

    private BoxCollider2D laserCollider;
    private float timer;
    private bool isLaserActive;
    private float arcTimer; 
    
    private AudioSource meuAudio; 

    private void Awake()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        laserCollider.isTrigger = true; 
    }

    private void Start()
    {
        SetupLaserTransform();
        ConfigurarAudio3D(); 

        isLaserActive = startOn;
        timer = isLaserActive ? timeOn : timeOff;
        UpdateLaserState();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isLaserActive = !isLaserActive;
            timer = isLaserActive ? timeOn : timeOff;
            UpdateLaserState();
        }

        if (isLaserActive)
        {
            arcTimer -= Time.deltaTime;
            if (arcTimer <= 0f)
            {
                DrawArc();
                arcTimer = 1f / fps; 
            }
        }
    }

    private void SetupLaserTransform()
    {
        if (baseA == null || baseB == null) return;

        transform.position = (baseA.position + baseB.position) / 2f;
        Vector2 direction = baseB.position - baseA.position;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector3(distance, laserThickness, 1f);

        if (arcRenderer != null)
        {
            arcRenderer.positionCount = segments + 1;
        }
    }

    // === PREPARAÇÃO DO SOM (COM BLINDAGEM) ===
    private void ConfigurarAudio3D()
    {
        if (SoundManager.instance != null && SoundManager.instance.obstaculoLaser != null)
        {
            // 1. Tenta pegar uma caixa de som que já exista. Se não achar, aí sim fabrica uma!
            meuAudio = GetComponent<AudioSource>();
            if (meuAudio == null)
            {
                meuAudio = gameObject.AddComponent<AudioSource>();
            }

            // 2. Trava de segurança: Proíbe a Unity de tocar o som automaticamente
            meuAudio.playOnAwake = false;

            meuAudio.spatialBlend = 1f; 
            meuAudio.rolloffMode = AudioRolloffMode.Linear;
            meuAudio.minDistance = 2f;
            meuAudio.maxDistance = 15f; 
            meuAudio.loop = true;

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

            float randomJitter = Random.Range(-arcVolatility, arcVolatility);
            Vector2 finalPos = basePos + (perpendicular * randomJitter);

            arcRenderer.SetPosition(i, finalPos);
        }

        arcRenderer.SetPosition(segments, end); 
    }
}