using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] // O BoxCollider é essencial pra esticar certinho
public class IntermittentLaser : MonoBehaviour
{
    [Header("Bases do Laser")]
    [SerializeField] private Transform baseA;
    [SerializeField] private Transform baseB;
    [SerializeField] private float laserThickness = 0.5f; // A grossura do seu laser

    [Header("Configurações de Tempo")]
    [SerializeField] private float timeOn = 2f; 
    [SerializeField] private float timeOff = 1.5f; 
    [SerializeField] private bool startOn = true; 

    [Header("Visual")]
    [SerializeField] private SpriteRenderer laserVisual; 

    private BoxCollider2D laserCollider;
    private float timer;
    private bool isLaserActive;

    private void Awake()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        laserCollider.isTrigger = true; 
    }

    private void Start()
    {
        // Posiciona e estica o laser antes do jogo começar a rodar a lógica
        SetupLaserTransform();

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
    }

    private void SetupLaserTransform()
    {
        if (baseA == null || baseB == null) return;

        // 1. A Posição: Ponto médio entre a Base A e a Base B
        transform.position = (baseA.position + baseB.position) / 2f;

        // 2. A Distância e Direção
        Vector2 direction = baseB.position - baseA.position;
        float distance = direction.magnitude;

        // 3. A Rotação: Calcula o ângulo usando a tangente arco (Atan2) e converte pra graus
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. O Tamanho (Scale): Estica o X até dar a distância exata, e o Y fica a grossura
        // O BoxCollider2D vai esticar automaticamente junto com o visual!
        transform.localScale = new Vector3(distance, laserThickness, 1f);
    }

    private void UpdateLaserState()
    {
        laserCollider.enabled = isLaserActive;

        if (laserVisual != null)
        {
            laserVisual.enabled = isLaserActive;
        }
    }
}