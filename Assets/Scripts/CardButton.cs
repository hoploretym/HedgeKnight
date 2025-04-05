using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardButton : MonoBehaviour
{
    private HandManager handManager;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI energyCostText;
    public Image CardImage;
    public Outline cardOutline; // ← ВОТ ЗДЕСЬ!
    private Button button;
    public TextMeshProUGUI descriptionText;

    public TextMeshProUGUI targetBodyPartText;

    private Color originalColor;
    public Card Card { get; private set; }

    public void Initialize(Card card, HandManager hand)
    {
        if (card == null)
        {
            Debug.LogError("[CardButton] Ошибка: передана NULL-карта в Initialize!");
            return;
        }

        handManager = hand;
        Card = card;

        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnCardClicked);

        if (cardNameText != null)
            cardNameText.text = card.Name;

        if (energyCostText != null)
        {
            int displayCost = Card.EnergyCost;
            bool hasSlowMovement = false;

            if ((Card.Type == CardType.Attack || Card.Type == CardType.Special))
            {
                Character player = GameManager.Instance?.playerController?.GetCharacter();
                if (player != null && player.HasDebuff("Legs"))
                {
                    displayCost += 1;
                    hasSlowMovement = true;
                }
            }

            string prefix = displayCost < 0 ? "+" : "-";
            string costText = hasSlowMovement
                ? $"{prefix}<color=red>{Mathf.Abs(displayCost)}</color>"
                : $"{prefix}{Mathf.Abs(displayCost)}";

            energyCostText.text = costText;
        }

        if (targetBodyPartText != null)
            targetBodyPartText.text = card.TargetBodyPart.ToString();

        if (descriptionText != null)
        {
            if (Card.Type == CardType.Attack || Card.Type == CardType.Special)
            {
                int displayDamage = Card.Damage;

                Character player = GameManager.Instance?.playerController?.GetCharacter();
                if (player != null && player.HasDebuff("Arms"))
                {
                    displayDamage = Mathf.Max(0, displayDamage - 1);
                    descriptionText.text =
                        $"<color=red>Deals {displayDamage}</color> damage to {Card.TargetBodyPart} <size=70%><i>(Unsteady Grip)</i></size>";
                }
                else
                {
                    descriptionText.text = $"Deals {displayDamage} damage to {Card.TargetBodyPart}";
                }
            }
            else if (Card.Type == CardType.Defense)
            {
                descriptionText.text = $"Defends all parts but {Card.TargetBodyPart}";
            }
            else
            {
                descriptionText.text = "";
            }
        }

        UpdateCardAppearance();

        if (cardOutline != null)
            cardOutline.enabled = false;
    }

    public void UpdateDescription()
    {
        if (descriptionText == null || Card == null)
            return;

        if (Card.Type == CardType.Attack || Card.Type == CardType.Special)
        {
            int displayDamage = Card.Damage;

            Character player = GameManager.Instance?.playerController?.GetCharacter();
            if (player != null && player.HasDebuff("Arms"))
            {
                displayDamage = Mathf.Max(0, displayDamage - 1);
                descriptionText.text =
                    $"Deals <color=red>{displayDamage}</color> damage to {Card.TargetBodyPart} <size=70%><i>(Unsteady Grip)</i></size>";
            }
            else
            {
                descriptionText.text = $"Deals {displayDamage} damage to {Card.TargetBodyPart}";
            }
        }
        else if (Card.Type == CardType.Defense)
        {
            descriptionText.text = $"Defends all parts but {Card.TargetBodyPart}";
        }
        else
        {
            descriptionText.text = "";
        }
    }

    public void UpdateEnergyCost()
    {
        if (energyCostText == null || Card == null)
            return;

        int displayCost = Card.EnergyCost;
        bool hasSlowMovement = false;

        if ((Card.Type == CardType.Attack || Card.Type == CardType.Special))
        {
            Character player = GameManager.Instance?.playerController?.GetCharacter();
            if (player != null && player.HasDebuff("Legs"))
            {
                displayCost += 1;
                hasSlowMovement = true;
            }
        }

        string prefix = displayCost < 0 ? "+" : "-";
        string costText = hasSlowMovement
            ? $"{prefix}<color=red>{Mathf.Abs(displayCost)}</color>"
            : $"{prefix}{Mathf.Abs(displayCost)}";

        energyCostText.text = costText;
    }

    public void SetOutline(bool enabled)
    {
        if (cardOutline != null)
        {
            cardOutline.enabled = enabled;
            Debug.Log($"Outline {(enabled ? "включён" : "выключен")} на карте: {Card.Name}");
        }
        else
            Debug.LogError("cardOutline не назначен!");
    }

    private void UpdateCardAppearance()
    {
        if (CardImage == null)
            return;

        if (Card.Type == CardType.Attack)
            originalColor = new Color32(255, 0, 0, 255);
        else if (Card.Type == CardType.Defense)
            originalColor = new Color32(0, 0, 255, 255);
        else if (Card.Type == CardType.Special)
            originalColor = new Color32(255, 165, 0, 255);

        CardImage.color = originalColor;
    }

    public void OnCardClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[CardButton] Ошибка: GameManager.Instance == null!");
            return;
        }

        int index = handManager.cardsInHand.IndexOf(Card);

        if (index == -1)
        {
            Debug.LogError($"[CardButton] Ошибка: Карта {Card.Name} не найдена в руке!");
            return;
        }

        Character player = GameManager.Instance.playerController.GetCharacter();
        Debug.Log($"[DEBUG] Клик по карте {Card.Name} (индекс {index})");
        GameManager.Instance.PlayerSelectCard(index);
    }
}
