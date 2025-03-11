using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public bool isPlayer;
    private Character character;

    void Awake()
    {
        character = gameObject.AddComponent<Character>(); // 🔥 Теперь правильно создается как компонент
        character.IsPlayer = isPlayer;
    }

    public Character GetCharacter()
    {
        return character;
    }
}
