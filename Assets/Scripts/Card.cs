public enum CardType
{
    Attack,
    Special,
    Defense,
}

public class Card
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public CardType Type { get; private set; }
    public string TargetBodyPart { get; private set; }
    public int EnergyCost { get; private set; }
    public int Damage { get; private set; }
    public string[] Counters { get; private set; }

    public Card(
        string id,
        string name,
        CardType type,
        string targetBodyPart,
        int energyCost,
        int damage,
        string[] counters = null
    )
    {
        Id = id;
        Name = name;
        Type = type;
        TargetBodyPart = targetBodyPart;
        EnergyCost = energyCost;
        Damage = damage;
        Counters = counters ?? new string[0];
    }
}
