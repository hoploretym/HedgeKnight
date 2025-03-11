using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int energy = 3; // Начальная энергия

    public void UseEnergy(int amount)
    {
        energy -= amount;
        if (energy < 0) energy = 0;
    }

    public void GainEnergy(int amount)
    {
        energy += amount;
    }

    public void ResetEnergy()
    {
        energy = 3; // Восстанавливаем в начале хода
    }
}
