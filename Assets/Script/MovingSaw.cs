using UnityEngine;

public class MovingSaw : MonoBehaviour
{
    [Header("Caminho da Serra")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    
    [Header("Configurações")]
    [SerializeField] private float speed = 5f;

    private Transform currentTarget;

    private void Start()
    {
        // A serra sempre começa o jogo indo em direção ao Ponto B
        currentTarget = pointB;
    }

    private void Update()
    {
        // 1. Move a serra suavemente na direção do alvo atual
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

        // 2. Checa a distância. Se chegou a menos de 0.1 de distância do alvo, inverte a direção!
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            if (currentTarget == pointA)
            {
                currentTarget = pointB;
            }
            else
            {
                currentTarget = pointA;
            }
        }
    }
}