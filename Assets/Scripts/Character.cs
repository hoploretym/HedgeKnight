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

    public bool HasArmDebuff() => isArmDisabled;

    public bool HasLegDebuff() => isLegDisabled;

    private bool isArmDisabled = false;
    private bool isLegDisabled = false;
    public string Name;
    public bool IsPlayer;
    private bool isDead = false;

    private int headDebuffTurnCounter = 0;

    private Dictionary<string, bool> debuffs = new Dictionary<string, bool>();

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

    public bool IsDead()
    {
        return HeadHP <= 0 || TorsoHP <= 0;
    }

    public void UseEnergy(int amount)
    {
        Energy = Mathf.Max(0, Energy - amount);
    }

    public void GainEnergy(int amount)
    {
        Energy = Mathf.Min(MaxEnergy, Energy + amount);
    }

    void CheckAndApplyDebuff(string part, int currentHP, int maxHP)
    {
        if (currentHP < maxHP / 2 && !HasDebuff(part))
        {
            debuffs[part] = true;
            Debug.Log($"Дебафф наложен: {GetDebuffName(part)} на {Name}");
            GameManager.Instance.gameUI.ShowDebuffIcon(this, part, GetDebuffName(part));
        }
    }

    public void ApplyStartOfTurnEffects()
    {
        // Эффект от Broken Stance
        if (HasDebuff("Torso"))
        {
            UseEnergy(1);
            Debug.Log($"{Name} теряет 1 энергию из-за Broken Stance.");
            GameManager.Instance.gameUI.LogAction(
                $"<color=orange>{Name} теряет 1 энергию из-за Broken Stance.</color>"
            );
        }

        // В будущем сюда добавим другие эффекты
    }

    public void TakeDamage(int amount, string bodyPart)
    {
        if (isDead)
            return;

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
                CheckAndApplyDebuff("Head", HeadHP, MaxHeadHP);
                break;

            case "Torso":
                TorsoHP = Mathf.Max(0, TorsoHP - amount);
                CheckAndApplyDebuff("Torso", TorsoHP, MaxTorsoHP);
                break;

            case "Arms":
                ArmsHP = Mathf.Max(0, ArmsHP - amount);
                CheckAndApplyDebuff("Arms", ArmsHP, MaxArmsHP);
                if (ArmsHP == 0)
                    isArmDisabled = true;
                break;

            case "Legs":
                LegsHP = Mathf.Max(0, LegsHP - amount);
                CheckAndApplyDebuff("Legs", LegsHP, MaxLegsHP);
                if (LegsHP == 0)
                    isLegDisabled = true;
                break;
        }

        if (HasDebuff("Head"))
        {
            headDebuffTurnCounter++;
        }

        GameManager.Instance.gameUI.UpdateCharacterDamage(this, bodyPart, GetCurrentHP(bodyPart));

        if ((HeadHP == 0 || TorsoHP == 0) && !isDead)
        {
            Die();
        }
    }

    public bool HasDebuff(string bodyPart)
    {
        return debuffs.ContainsKey(bodyPart) && debuffs[bodyPart];
    }

    public string GetDebuffName(string bodyPart)
    {
        return bodyPart switch
        {
            "Head" => "Broken Helmet",
            "Torso" => "Broken Stance",
            "Arms" => "Unsteady Grip",
            "Legs" => "Slow Movement",
            _ => "",
        };
    }

    public bool ShouldDrawOneLessCard()
    {
        return HasDebuff("Head") && headDebuffTurnCounter % 2 == 1;
    }

    public void IncrementBrokenHelmetCounter()
    {
        headDebuffTurnCounter++;
    }

    public void ResetDebuffCounters()
    {
        headDebuffTurnCounter = 0;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"{name} погиб!");
        GameManager.Instance.RegisterPendingDeath(this);
    }

    public int GetCurrentHP(string bodyPart)
    {
        return bodyPart switch
        {
            "Head" => HeadHP,
            "Torso" => TorsoHP,
            "Arms" => ArmsHP,
            "Legs" => LegsHP,
            _ => 0,
        };
    }

    public int GetMaxHP(string bodyPart)
    {
        return bodyPart switch
        {
            "Head" => MaxHeadHP,
            "Torso" => MaxTorsoHP,
            "Arms" => MaxArmsHP,
            "Legs" => MaxLegsHP,
            _ => 0,
        };
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
