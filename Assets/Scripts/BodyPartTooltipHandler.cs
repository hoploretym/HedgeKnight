using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodyPartTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string bodyPart; // Название части тела: "Head", "Torso", "Arms", "Legs"
    public bool isPlayer; // true — игрок, false — враг

    private TextMeshProUGUI hpText;

    void Awake()
    {
        // Поддержка прозрачности при наведении
        var image = GetComponent<Image>();
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f;
        }

        // Поиск текста внутри
        hpText = GetComponentInChildren<TextMeshProUGUI>();
        if (hpText != null)
        {
            var color = hpText.color;
            color.a = 0.2f;
            hpText.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var character = isPlayer
            ? GameManager.Instance.playerController.GetCharacter()
            : GameManager.Instance.enemyController.GetCharacter();

        int currentHP = character.GetCurrentHP(bodyPart);
        int maxHP = character.GetMaxHP(bodyPart);

        TooltipUIController.Instance.Show($"{bodyPart}: {currentHP}/{maxHP} HP");

        if (hpText != null)
        {
            var color = hpText.color;
            color.a = 1f;
            hpText.color = color;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUIController.Instance.Hide();

        if (hpText != null)
        {
            var color = hpText.color;
            color.a = 0.2f;
            hpText.color = color;
        }
    }
}
