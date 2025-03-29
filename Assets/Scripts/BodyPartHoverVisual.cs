using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodyPartHoverVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image lineImage;
    public TextMeshProUGUI hpText;

    private Color transparent = new Color(1f, 0f, 0f, 0.2f);
    private Color visible = new Color(1f, 0f, 0f, 1f);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (lineImage != null)
            lineImage.color = visible;
        if (hpText != null)
            hpText.color = visible;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lineImage != null)
            lineImage.color = transparent;
        if (hpText != null)
            hpText.color = transparent;
    }
}
