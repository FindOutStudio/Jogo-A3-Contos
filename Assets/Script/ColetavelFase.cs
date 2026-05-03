using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColetavelFase : MonoBehaviour
{
    private void Awake()
    {
        // Garante que o colisor seja Trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Avisa o Gerenciador que pegou uma moeda
            if (GerenciadorMoedas.instance != null)
            {
                GerenciadorMoedas.instance.AdicionarMoeda();
            }

            // Destrói a moeda da fase
            Destroy(gameObject);
        }
    }
}