using UnityEngine;

public class PlataformaSinuca : MonoBehaviour
{
    [Header("Direção da Sinuca Maluca")]
    [Tooltip("Para onde essa parede vai jogar o player? (Ex: X=1, Y=0 joga para a Direita)")]
    public Vector2 direcaoDoRicochete = new Vector2(0, 1); // Padrão: Joga para cima
    
    [Tooltip("A força com que o player é ejetado daqui")]
    public float forcaDoRicochete = 15f;
}