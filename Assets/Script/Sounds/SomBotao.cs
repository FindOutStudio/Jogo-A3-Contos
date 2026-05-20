using UnityEngine;
using UnityEngine.EventSystems; // Necessário para detectar o Hover do mouse

public class SomBotao : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // Quando o mouse passa por cima do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiHover);
        }
    }

    // Quando o cara clica no botão
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.TocarSFX(SoundManager.instance.uiSelecao);
        }
    }
}