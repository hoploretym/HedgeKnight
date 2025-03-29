using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int Energy { get; private set; } = 30;
    private const int MaxEnergy = 30;
    public int HeadHP { get; private set; } = 20;
    public int TorsoHP { get; private set; } = 50;
    public int ArmsHP { get; private set; } = 30;
    public int LegsHP { get; private set; } = 30;

    private const int MaxHeadHP = 20;
    private const int MaxTorsoHP = 50;
    private const int MaxArmsHP = 30;
    private const int MaxLegsHP = 30;

    public bool HasArmDebuff()
    {
        return isArmDisabled;
    }

    public bool HasLegDebuff()
    {
        return isLegDisabled;
    }

    private bool isArmDisabled = false;
    private bool isLegDisabled = false;

    public bool IsPlayer;

    private Dictionary<string, bool> defense = new Dictionary<string, bool>();

    void Awake()
    {
        ResetDefense();
    }

    public void ResetEnergy()
    {
        Energy = MaxEnergy;
        ResetDefense();
    }

    public void UseEnergy(int amount)
    {
        Energy = Mathf.Max(0, Energy - amount);
    }

    public void GainEnergy(int amount)
    {
        Energy = Mathf.Min(MaxEnergy, Energy + amount);
    }

    public void TakeDamage(int amount, string bodyPart)
    {
        if (IsDefended(bodyPart))
        {
            Debug.Log($"{name} заблокировал удар в {bodyPart}!");
            return;
        }

        Debug.Log($"{name} получает {amount} урона в {bodyPart}!");

        switch (bodyPart)
        {
            case "Head":
                HeadHP = Mathf.Max(0, HeadHP - amount);
                if (HeadHP == 0)
                    Die();
                break;
            case "Torso":
                TorsoHP = Mathf.Max(0, TorsoHP - amount);
                if (TorsoHP == 0)
                    Die();
                break;
            case "Arms":
                ArmsHP = Mathf.Max(0, ArmsHP - amount);
                if (ArmsHP == 0)
                    isArmDisabled = true;
                break;
            case "Legs":
                LegsHP = Mathf.Max(0, LegsHP - amount);
                if (LegsHP == 0)
                    isLegDisabled = true;
                break;
        }

        GameManager.Instance.gameUI.UpdateCharacterDamage(this, bodyPart, GetCurrentHP(bodyPart));
    }

    public int GetCurrentHP(string bodyPart)
    {
        switch (bodyPart)
        {
            case "Head":
                return HeadHP;
            case "Torso":
                return TorsoHP;
            case "Arms":
                return ArmsHP;
            case "Legs":
                return LegsHP;
            default:
                return 0;
        }
    }

    public int GetMaxHP(string bodyPart)
    {
        switch (bodyPart)
        {
            case "Head":
                return MaxHeadHP;
            case "Torso":
                return MaxTorsoHP;
            case "Arms":
                return MaxArmsHP;
            case "Legs":
                return MaxLegsHP;
            default:
                return 0;
        }
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

    public void ResetDefense()
    {
        defense.Clear();
    }
}
