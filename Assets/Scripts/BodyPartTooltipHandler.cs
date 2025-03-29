using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodyPartTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string bodyPart; // ← Название части тела: "Head", "Torso", "Arms", "Legs"
    public bool isPlayer; // ← true для игрока, false для врага

    void Awake()
    {
        // Добавляем поддержку Alpha Hit Test по прозрачности спрайта
        var image = GetComponent<Image>();
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f;
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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUIController.Instance.Hide();
    }
}
