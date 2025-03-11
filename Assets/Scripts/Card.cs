using UnityEngine;

public class Card
{
    public string Name { get; private set; }
    public int EnergyCost { get; private set; }
    public string TargetBodyPart { get; private set; }
    public bool IsDefense { get; private set; }

    public Card(string name, int energyCost, string targetBodyPart, bool isDefense = false)
    {
        Name = name;
        EnergyCost = energyCost;
        TargetBodyPart = targetBodyPart;
        IsDefense = isDefense;
    }

    public void PlayCard(Character attacker, Character target)
    {
        if (attacker.Energy < EnergyCost) return;

        if (IsDefense)
        {
            Debug.Log($"{attacker.name} активирует защиту на {TargetBodyPart}!");
            attacker.SetDefense(TargetBodyPart);
        }
        else
        {
            if (!target.IsDefended(TargetBodyPart))
            {
                target.TakeDamage(TargetBodyPart);
            }
            else
            {
                Debug.Log($"{target.name} заблокировал удар!");
            }
        }
        attacker.UseEnergy(EnergyCost);
    }
}
