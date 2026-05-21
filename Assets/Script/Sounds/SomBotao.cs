using UnityEngine;
using UnityEngine.EventSystems; 

// Adicionamos o IPointerExitHandler para detectar quando o mouse SAI do botão
public class SomBotao : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [Header("Efeito Visual (Hover)")]
    [Tooltip("O quanto o botão vai crescer (Ex: 1.1 para 10% maior)")]
    public float tamanhoAoPassarMouse = 1.3f;
    [Tooltip("Velocidade da animação de crescer/diminuir")]
    public float velocidadeAnimacao = 15f;

    private Vector3 tamanhoOriginal;
    private Vector3 tamanhoAlvo;

    private void Awake()
    {
        // Salva o tamanho original do botão logo que a tela carrega
        tamanhoOriginal = transform.localScale;
        tamanhoAlvo = tamanhoOriginal;
    }

    private void Update()
    {
        // Faz o botão crescer ou diminuir de forma suave até o tamanho alvo
        if (transform.localScale != tamanhoAlvo)
        {
            // Usamos unscaledDeltaTime para a animação funcionar mesmo se o jogo estiver pausado (Time.timeScale = 0)
            transform.localScale = Vector3.Lerp(transform.localScale, tamanhoAlvo, Time.unscaledDeltaTime * velocidadeAnimacao);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Toca o som de hover
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiHover);
        }
        
        // Define que o alvo agora é ficar maiorzinho
        tamanhoAlvo = tamanhoOriginal * tamanhoAoPassarMouse;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Define que o alvo agora é voltar ao normal
        tamanhoAlvo = tamanhoOriginal;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Toca o som de clique
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiSelecao);
        }
    }

    // TRAVA DE SEGURANÇA: Se o botão for desligado (ex: o painel fechou) com o mouse em cima, 
    // ele volta ao normal para não bugar o tamanho na próxima vez que a tela abrir.
    private void OnDisable()
    {
        transform.localScale = tamanhoOriginal;
        tamanhoAlvo = tamanhoOriginal;
    }
}