using UnityEngine;

public class CircularMover : MonoBehaviour
{
    [Header("Configurações do Círculo")]
    [SerializeField] private Transform pivotPoint; // O objeto vazio que será o centro do giro
    [SerializeField] private float speed = 2f;     // Velocidade da órbita
    [SerializeField] private float radius = 3f;    // O tamanho do círculo (distância do centro)
    [SerializeField] private bool clockwise = true; // Sentido Horário ou Anti-Horário

    private float angle;

    private void Update()
    {
        if (pivotPoint == null) return;

        // 1. O Tempo: Faz o ângulo girar sem parar
        // Se for horário, diminui o ângulo. Se for anti-horário, aumenta.
        angle += (clockwise ? -1f : 1f) * speed * Time.deltaTime;

        // 2. A Matemática Pura (Trigonometria):
        // Cosseno (Cos) cuida do eixo X e Seno (Sin) cuida do eixo Y.
        float x = pivotPoint.position.x + Mathf.Cos(angle) * radius;
        float y = pivotPoint.position.y + Mathf.Sin(angle) * radius;

        // 3. Aplica a nova posição na base
        transform.position = new Vector2(x, y);
    }
}
