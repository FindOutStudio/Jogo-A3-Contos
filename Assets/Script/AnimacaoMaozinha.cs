using System.Collections;
using UnityEngine;

// Isso garante que a Unity saiba que o objeto tem que ter um SpriteRenderer
[RequireComponent(typeof(SpriteRenderer))]
public class AnimacaoMaozinha : MonoBehaviour
{
    [Header("Configuração Visual (Sprites)")]
    [Tooltip("A imagem da mãozinha relaxada (sem clicar)")]
    public Sprite spriteNormal;
    [Tooltip("A imagem da mãozinha fechada/apertando a tela")]
    public Sprite spriteClicando;
    
    private SpriteRenderer renderizador;

    [Header("Configuração do Puxão (Código)")]
    [Tooltip("Para onde a mão deve puxar? (Ex: X: -1.5, Y: -1.5 para puxar na diagonal)")]
    public Vector3 posicaoPuxao = new Vector3(-1.5f, -1.5f, 0f);
    
    [Tooltip("Velocidade que a mão arrasta para trás")]
    public float velocidadePuxao = 2f;
    
    [Tooltip("Tempo que ela fica parada antes de repetir a animação")]
    public float tempoDeEspera = 0.5f;

    private Vector3 posicaoInicial;

    private void Awake()
    {
        // Pega o componente que desenha a imagem na tela
        renderizador = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        posicaoInicial = transform.localPosition;
        StartCoroutine(RotinaDeAnimacao());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator RotinaDeAnimacao()
    {
        while (true) 
        {
            // 1. Volta pro começo e fica "Normal" (relaxada)
            transform.localPosition = posicaoInicial;
            if (renderizador != null && spriteNormal != null)
            {
                renderizador.sprite = spriteNormal;
            }
            
            // Dá um tempinho minúsculo pro jogador ver a mão parada antes de clicar
            yield return new WaitForSecondsRealtime(0.2f);

            // 2. Aperta a tela (Muda pro sprite clicando)
            if (renderizador != null && spriteClicando != null)
            {
                renderizador.sprite = spriteClicando;
            }

            // 3. Faz o movimento suave de puxar para trás
            float progresso = 0f;
            while (progresso < 1f)
            {
                progresso += Time.unscaledDeltaTime * velocidadePuxao;
                transform.localPosition = Vector3.Lerp(posicaoInicial, posicaoInicial + posicaoPuxao, progresso);
                yield return null;
            }

            // 4. Solta a tela no final do puxão (Simula o arremesso)
            if (renderizador != null && spriteNormal != null)
            {
                renderizador.sprite = spriteNormal;
            }

            // 5. Espera um tempinho pra recomeçar
            yield return new WaitForSecondsRealtime(tempoDeEspera);
        }
    }
}