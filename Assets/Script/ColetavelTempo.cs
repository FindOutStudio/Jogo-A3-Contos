using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColetavelTempo : MonoBehaviour
{
    [Header("Configuração do Reloginho")]
    [Tooltip("Quantos segundos esse item adiciona ao relógio?")]
    public float tempoParaAdicionar = 1f;

    private void Awake()
    {
        // Garante que o colisor seja um "Fantasma" (Trigger) para o player atravessar e coletar
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se quem encostou foi o Player...
        if (collision.CompareTag("Player"))
        {
            // Manda o relógio adicionar o tempo!
            if (LevelTimer.instance != null)
            {
                LevelTimer.instance.AdicionarTempo(tempoParaAdicionar);
            }


            // Destrói o item da cena para não pegar duas vezes
            Destroy(gameObject);
        }
    }
}