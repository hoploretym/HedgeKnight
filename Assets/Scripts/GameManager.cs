using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public CharacterManager playerController;
    public CharacterManager enemyController;
    public HandManager playerHand;
    public EnemyHandManager enemyHand;
    public GameUI gameUI;

    private Character player;
    private Character enemy;
    private Card playerSelectedCard;
    private Card enemyChosenCard;
    private bool waitingForChoices = true;
    public bool battleEnded = false;
    private int turnNumber = 1;
    private bool playerSkipped = false;
    private bool enemySkipped = false;

    private Character pendingLoser = null;

    public enum Outcome
    {
        Win,
        Lose,
        Tie,
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        player = playerController.GetCharacter();
        enemy = enemyController.GetCharacter();
        player.IsPlayer = true;
        enemy.IsPlayer = false;
    }

    public void StartBattle()
    {
        Debug.Log("Бой начался!");
        gameUI.ClearLog();
        Deck playerDeck = GameObject.Find("DeckManager")?.GetComponent<Deck>();
        Deck enemyDeck = GameObject.Find("DeckManagerEnemy")?.GetComponent<Deck>();

        if (playerDeck == null || enemyDeck == null)
        {
            Debug.LogError("Ошибка: Не найдены DeckManager или DeckManagerEnemy!");
            return;
        }

        if (playerHand == null || enemyHand == null)
        {
            Debug.LogError("Ошибка: Не найдены HandManager у игрока или противника!");
            return;
        }

        playerHand.Initialize(playerDeck);
        enemyHand.Initialize(enemyDeck);

        player.ResetEnergy();
        enemy.ResetEnergy();
        player.ApplyStartOfTurnEffects();
        enemy.ApplyStartOfTurnEffects();

        gameUI.UpdateEnergy(player, enemy);

        playerHand.DrawNewHand();
        enemyHand.DrawNewHand();
    }

    public void SkipTurn()
    {
        if (!waitingForChoices)
            return;

        playerSelectedCard = null;
        playerSkipped = true;
        enemySkipped = false;

        Debug.Log("[GameManager] Игрок нажал Skip Turn.");
        FinishTurn();
    }

    public void PlayerSelectCard(int index)
    {
        if (!waitingForChoices)
            return;
        if (index < 0 || index >= playerHand.cardsInHand.Count)
            return;

        Card selected = playerHand.cardsInHand[index];

        // ❗ Проверка энергии
        if (player.Energy < selected.EnergyCost)
        {
            Debug.Log("Недостаточно энергии для использования этой карты!");
            gameUI.LogAction("<color=red>Недостаточно энергии!</color>");
            return;
        }

        playerSelectedCard = selected;
        gameUI.UpdateCardSelection(new List<Card> { selected });
    }

    private string GetCardHexColor(CardType type)
    {
        return type switch
        {
            CardType.Attack => "#FF0000", // red
            CardType.Defense => "#0000FF", // blue
            CardType.Special => "#FFA500", // orange
            _ => "#FFFFFF", // white (fallback)
        };
    }

    public void FinishTurn()
    {
        if (playerSelectedCard != null)
        {
            playerSkipped = false;
        }
        if (playerSelectedCard == null && !playerSkipped)
        {
            gameUI.LogAction("<color=red>Select a card before ending the turn!</color>");
            return;
        }

        waitingForChoices = false;
        player.ResetDefense();
        enemy.ResetDefense();

        // Выбор карты врагом с учётом энергии
        enemySkipped = false;
        enemyChosenCard = null;

        foreach (Card card in enemyHand.cardsInHand)
        {
            int cost = card.EnergyCost;
            if (
                (card.Type == CardType.Attack || card.Type == CardType.Special)
                && enemy.HasDebuff("Legs")
            )
                cost += 1;

            if (enemy.Energy >= cost)
            {
                enemyChosenCard = card;
                break;
            }
        }

        // Если подходящей карты нет, добираем карты до появления защиты или пока не закончатся
        while (enemyChosenCard == null)
        {
            Card drawn = enemyHand.DrawOneCard();
            if (drawn == null)
            {
                enemySkipped = true;
                break;
            }

            int cost = drawn.EnergyCost;
            if (
                (drawn.Type == CardType.Attack || drawn.Type == CardType.Special)
                && enemy.HasDebuff("Legs")
            )
                cost += 1;

            if (enemy.Energy >= cost)
            {
                enemyChosenCard = drawn;
            }
        }

        List<string> roundLog = new List<string> { $"<b>Turn {turnNumber}.</b>" };

        if (!playerSkipped && playerSelectedCard != null)
        {
            string playerColor = GetCardHexColor(playerSelectedCard.Type);
            roundLog.Add(
                $"<b>Card played:</b> <color={playerColor}>{playerSelectedCard.Name} [{playerSelectedCard.TargetBodyPart}]</color>"
            );
        }
        else
        {
            roundLog.Add("<b>Card played:</b> <color=grey>Skipped</color>");
        }

        if (!enemySkipped && enemyChosenCard != null)
        {
            string enemyColor = GetCardHexColor(enemyChosenCard.Type);
            roundLog.Add(
                $"<b>Enemy played:</b> <color={enemyColor}>{enemyChosenCard.Name} [{enemyChosenCard.TargetBodyPart}]</color>"
            );
        }

        if (playerSkipped && !enemySkipped)
        {
            roundLog.Add($"<color=grey>{player.Name} skips their turn.</color>");
            roundLog.Add($"Effect: {ApplyCardEffects(enemyChosenCard, enemy, player, null)}");
        }
        else if (!playerSkipped && enemySkipped)
        {
            roundLog.Add($"<color=grey>{enemy.Name} skips their turn.</color>");
            roundLog.Add($"Effect: {ApplyCardEffects(playerSelectedCard, player, enemy, null)}");
        }
        else
        {
            roundLog.Add(
                $"Effect: {ApplyCardEffects(playerSelectedCard, player, enemy, enemyChosenCard)}"
            );
        }

        // Энергия игрока
        if (!playerSkipped && playerSelectedCard != null)
        {
            if (playerSelectedCard.Type == CardType.Defense)
                player.GainEnergy(Mathf.Abs(playerSelectedCard.EnergyCost));
            else
            {
                int cost = playerSelectedCard.EnergyCost;
                if (
                    (
                        playerSelectedCard.Type == CardType.Attack
                        || playerSelectedCard.Type == CardType.Special
                    ) && player.HasDebuff("Legs")
                )
                    cost += 1;
                player.UseEnergy(cost);
            }
        }

        // Энергия врага
        if (!enemySkipped && enemyChosenCard != null)
        {
            if (enemyChosenCard.Type == CardType.Defense)
                enemy.GainEnergy(Mathf.Abs(enemyChosenCard.EnergyCost));
            else
            {
                int cost = enemyChosenCard.EnergyCost;
                if (
                    (
                        enemyChosenCard.Type == CardType.Attack
                        || enemyChosenCard.Type == CardType.Special
                    ) && enemy.HasDebuff("Legs")
                )
                    cost += 1;
                enemy.UseEnergy(cost);
            }
        }

        // Сброс карт
        if (playerSelectedCard != null)
            playerHand.RemoveCard(playerSelectedCard);
        if (enemyChosenCard != null)
            enemyHand.RemoveCard(enemyChosenCard);
        playerSelectedCard = null;
        enemyChosenCard = null;

        // Добор карт
        if (playerHand.DrawOneCard() is Card playerDrawn)
            Debug.Log($"Player draws: {playerDrawn.Name}");
        if (enemyHand.DrawOneCard() is Card enemyDrawn)
            Debug.Log($"Enemy draws: {enemyDrawn.Name}");

        // Обновление
        playerHand.UpdateHandUI();
        gameUI.UpdateEnergy(player, enemy);
        gameUI.RefreshHandUI();
        gameUI.LogRoundResults(roundLog);
        gameUI.UpdateAllHPText();

        if (player.IsDead() && enemy.IsDead())
        {
            EndBattle(player);
            return;
        }
        if (pendingLoser != null)
        {
            EndBattle(pendingLoser);
            return;
        }
        if (CheckBattleEnd())
            return;

        if (player.HasDebuff("Head"))
            player.IncrementBrokenHelmetCounter();

        turnNumber++;
        player.ApplyStartOfTurnEffects();
        enemy.ApplyStartOfTurnEffects();
        waitingForChoices = true;
    }

    private Outcome GetOutcome(CardType attacker, CardType defender)
    {
        if (attacker == defender)
            return Outcome.Tie;

        if (
            (attacker == CardType.Attack && defender == CardType.Special)
            || (attacker == CardType.Special && defender == CardType.Defense)
            || (attacker == CardType.Defense && defender == CardType.Attack)
        )
        {
            return Outcome.Win;
        }

        return Outcome.Lose;
    }

    public string ApplyCardEffects(
        Card card,
        Character attacker,
        Character defender,
        Card enemyCard
    )
    {
        // ✅ Обработка случая: игрок пропускает ход
        if (card == null)
        {
            return ApplySoloCardEffect(enemyCard, defender, attacker);
        }

        if (enemyCard == null)
        {
            return ApplySoloCardEffect(card, attacker, defender);
        }

        // 🔽 Обычная логика
        Outcome result = GetOutcome(card.Type, enemyCard.Type);
        bool sameTarget = card.TargetBodyPart == enemyCard.TargetBodyPart;

        int finalDamage = card.Damage;
        if (
            (card.Type == CardType.Attack || card.Type == CardType.Special)
            && attacker.HasDebuff("Arms")
        )
        {
            finalDamage = Mathf.Max(0, finalDamage - 1);
            Debug.Log($"{attacker.Name} имеет дебафф Unsteady Grip — урон снижен до {finalDamage}");
        }
        if (sameTarget)
            finalDamage *= 2;

        string log = $"{card.Name} -> {card.TargetBodyPart}";
        if (sameTarget)
            log += " (x2)";

        switch (card.Type)
        {
            case CardType.Attack:
                switch (result)
                {
                    case Outcome.Win:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        log += " – Hit!";
                        break;

                    case Outcome.Lose:
                        log += " – Blocked.";
                        break;

                    case Outcome.Tie:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);

                        int retaliationDmg = enemyCard.Damage;
                        if (enemyCard.TargetBodyPart == card.TargetBodyPart)
                            retaliationDmg *= 2;

                        attacker.TakeDamage(retaliationDmg, enemyCard.TargetBodyPart);
                        log +=
                            $" – Tie. {defender.Name} takes {finalDamage}, {attacker.Name} takes {retaliationDmg}";
                        break;
                }
                break;

            case CardType.Defense:
                switch (result)
                {
                    case Outcome.Win:
                        log += " – Energy restored.";
                        break;

                    case Outcome.Lose:
                        int incomingDamage = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(incomingDamage, enemyCard.TargetBodyPart);
                        log += $" – Block failed. Took {incomingDamage}.";
                        break;

                    case Outcome.Tie:
                        log += " – Partial block.";
                        break;
                }
                break;

            case CardType.Special:
                switch (result)
                {
                    case Outcome.Win:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        log += " – Special success!";
                        break;

                    case Outcome.Lose:
                        int dmg = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(dmg, enemyCard.TargetBodyPart);
                        log += $" – Countered. Took {dmg}.";
                        break;

                    case Outcome.Tie:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);

                        int counterDmg = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(counterDmg, enemyCard.TargetBodyPart);
                        log +=
                            $" – Tie. {defender.Name} takes {finalDamage}, {attacker.Name} takes {counterDmg}";
                        break;
                }
                break;
        }

        return log;
    }

    private string ApplySoloCardEffect(Card card, Character attacker, Character target)
    {
        if (card == null)
            return $"{attacker.Name} skipped their turn.";

        int finalDamage = card.Damage;
        if (
            (card.Type == CardType.Attack || card.Type == CardType.Special)
            && attacker.HasDebuff("Arms")
        )
        {
            finalDamage = Mathf.Max(0, finalDamage - 1);
        }

        switch (card.Type)
        {
            case CardType.Attack:
            case CardType.Special:
                target.TakeDamage(finalDamage, card.TargetBodyPart);
                return $"{attacker.Name} plays {card.Name} -> {card.TargetBodyPart} – Hit!";

            case CardType.Defense:
                attacker.SetDefense(card.TargetBodyPart);
                return $"{attacker.Name} braces for impact (defends {card.TargetBodyPart}).";

            default:
                return $"{attacker.Name} plays an unknown move.";
        }
    }

    public void RegisterPendingDeath(Character c)
    {
        if (!battleEnded && pendingLoser == null)
        {
            pendingLoser = c;
        }
    }

    public bool CheckBattleEnd()
    {
        if (battleEnded)
            return true;

        if (player.HeadHP <= 0 || player.TorsoHP <= 0)
        {
            EndBattle(player);
            return true;
        }

        if (enemy.HeadHP <= 0 || enemy.TorsoHP <= 0)
        {
            EndBattle(enemy);
            return true;
        }

        return false;
    }

    public void EndBattle(Character loser)
    {
        if (battleEnded)
            return;

        battleEnded = true;

        bool playerLost = loser.IsPlayer;
        string resultMessage = playerLost ? "Игрок проиграл!" : "Игрок победил!";

        Debug.Log(resultMessage);
        gameUI.ShowGameResult(!playerLost);
    }
}
