using UnityEngine;

public enum CardType { Attack, Defense }

public class Card
{
    public string Name { get; private set; }
    public int EnergyCost { get; private set; }
    public string TargetBodyPart { get; private set; }
    public int Damage { get; private set; }
    public CardType Type { get; private set; }
    public string[] Counters { get; private set; } // Карты, которые эта защита блокирует

    public Card(string name, int energyCost, string targetBodyPart, int damage, CardType type, string[] counters = null)
    {
        Name = name;
        EnergyCost = energyCost;
        TargetBodyPart = targetBodyPart;
        Damage = damage;
        Type = type;
        Counters = counters ?? new string[0]; // По умолчанию пустой массив
    }

    public void PlayCard(Character attacker, Character target, Card opponentCard)
    {
        if (attacker.Energy < EnergyCost)
        {
            Debug.Log($"{attacker.name} не хватает энергии для {Name}");
            return;
        }

        // 🛡 Проверяем, заблокирована ли атака
        if (opponentCard != null && opponentCard.Type == CardType.Defense && System.Array.Exists(opponentCard.Counters, c => c == this.Name))
        {
            Debug.Log($"{attacker.name} атакует {target.name}, но {opponentCard.Name} блокирует!");
            return; 
        }

        // ⚔️ Наносим урон
        Debug.Log($"{attacker.name} атакует {target.name} и наносит {Damage} урона в {TargetBodyPart}");
        target.TakeDamage(Damage, TargetBodyPart);

        // 🔋 Тратим энергию
        attacker.UseEnergy(EnergyCost);

        // 🔄 Обновляем UI
        GameManager.Instance.gameUI.UpdateCharacterDamage(attacker, TargetBodyPart, Damage);
    }
}
