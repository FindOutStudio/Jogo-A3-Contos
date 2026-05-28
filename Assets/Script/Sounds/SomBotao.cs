using UnityEngine;
using UnityEngine.EventSystems; 

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
        tamanhoOriginal = transform.localScale;
        tamanhoAlvo = tamanhoOriginal;
    }

    private void Update()
    {
        if (transform.localScale != tamanhoAlvo)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, tamanhoAlvo, Time.unscaledDeltaTime * velocidadeAnimacao);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiHover);
        }
        
        tamanhoAlvo = tamanhoOriginal * tamanhoAoPassarMouse;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tamanhoAlvo = tamanhoOriginal;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiSelecao);
        }

        // ======= A CORREÇÃO ESTÁ AQUI =======
        // Tira o "foco" do botão imediatamente após o clique para não travar a troca de Sprite!
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDisable()
    {
        transform.localScale = tamanhoOriginal;
        tamanhoAlvo = tamanhoOriginal;
    }
}