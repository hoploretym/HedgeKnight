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

    public void TakeDamage(int amount, string bodyPart)
    {
        if (IsDefended(bodyPart))
        {
            Debug.Log($"{name} заблокировал удар в {bodyPart}!");
            return;
        }

        Debug.Log($"{name} получает {amount} урона в {bodyPart}!");

        if (bodyPart == "Head")
        {
            HeadHits++;
            if (HeadHits >= 2) Die();
        }
        else if (bodyPart == "Torso")
        {
            TorsoHits++;
            if (TorsoHits >= 3) Die();
        }

        // 🔴 **Обновляем цвет маски повреждений**
        GameManager.Instance.gameUI.UpdateCharacterDamage(this, bodyPart, bodyPart == "Head" ? HeadHits : TorsoHits);
    }

    public void Die()
    {
        Debug.Log($"{name} погиб!");

        if (IsPlayer)
        {
            GameManager.Instance.gameUI.LogAction("<b>Игрок проиграл!</b>");
        }
        else
        {
            GameManager.Instance.gameUI.LogAction("<b>Игрок победил!</b>");
        }

        GameManager.Instance.EndBattle(this);
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
