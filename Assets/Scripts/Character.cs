using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int Energy { get; private set; } = 3;
    public int HeadHits { get; private set; } = 0;
    public int TorsoHits { get; private set; } = 0;
    public bool IsPlayer;
    private Dictionary<string, bool> defense = new Dictionary<string, bool>();

    void Awake()
    {
        ResetDefense();
    }

    public void ResetEnergy()
    {
        Energy = 3;
        ResetDefense();
    }

    public void UseEnergy(int amount)
    {
        Energy = Mathf.Max(0, Energy - amount);
    }

    public void TakeDamage(string bodyPart)
    {
        if (!defense.ContainsKey(bodyPart) || !defense[bodyPart])
        {
            if (bodyPart == "Head") HeadHits++;
            if (bodyPart == "Torso") TorsoHits++;
            FindObjectOfType<GameUI>().UpdateCharacterDamage(this, bodyPart, bodyPart == "Head" ? HeadHits : TorsoHits);
        }
    }

    public void SetDefense(string bodyPart)
    {
        defense[bodyPart] = true;
    }

    public bool IsDefended(string bodyPart)
    {
        return defense.ContainsKey(bodyPart) && defense[bodyPart];
    }

    private void ResetDefense()
    {
        defense.Clear();
    }
}
