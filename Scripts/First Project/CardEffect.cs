using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;
public class CardEffect : MonoBehaviour
{
    private Context context;
    ScoreCounter scoreCounter;
    public string EffectName { get; set; }
    public Dictionary<string, string> Params { get; set; }
    public ActionNode Action { get; set; }
    public Selector Selector { get; set; }
    public CardEffect PostAction { get; set; }
    private void Start()
    {
        context = FindObjectOfType<Context>(); 
    }
    public void ApplyEffect(CardSO card, Player currentPlayer, Player otherPlayer, ScoreCounter scoreCounter, Context context)
    {

        switch (card.Effect)
        {
            case "Decrease":
                ApplyDecreaseEffect(card, currentPlayer, otherPlayer);
                break;
            case "Draw":
                StartCoroutine(DrawCardsEffect(currentPlayer.DeckPlayer.GetComponent<Deck>(), 2));
                break;
            case "Increase":
                ApplyIncreaseEffect(card, currentPlayer);
                break;
            case "Invoke":
                InvokeCard(currentPlayer, card);
                break;
            case "Destroy":
                ApplyDestroyEffect(card, currentPlayer, otherPlayer);
                break;

            default:
                break;

        }

        scoreCounter.UpdateScoreDisplay(currentPlayer);
        scoreCounter.UpdateScoreDisplay(otherPlayer);
    }
    private void ApplyDecreaseEffect(CardSO card, Player currentPlayer, Player otherPlayer)
    {
        for (int i = 0; i < card.ranges.Count; i++)
        {
            if (card.ranges[i] == CardSO.Range.W1)
            {
                DecreasePointsInZone(currentPlayer.MeleeZone.transform);
                DecreasePointsInZone(otherPlayer.MeleeZone.transform);
            }
            if (card.ranges[i] == CardSO.Range.W2)
            {
                DecreasePointsInZone(currentPlayer.RangedZone.transform);
                DecreasePointsInZone(otherPlayer.RangedZone.transform);
            }
            if (card.ranges[i] == CardSO.Range.W3)
            {
                DecreasePointsInZone(currentPlayer.SiegeZone.transform);
                DecreasePointsInZone(otherPlayer.SiegeZone.transform);
            }
        }
    }

    private void DecreasePointsInZone(Transform zone)
    {
        foreach (Transform cardTransform in zone)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null && !cardComponent.hasDecreaseEffect)
            {
                cardComponent.card.Power -= 2;
                cardComponent.cardPoint.text = cardComponent.card.Power.ToString();
                cardComponent.hasDecreaseEffect = true;

                if (cardComponent.card.Power < 0)
                {
                    cardComponent.card.Power = 0;
                    cardComponent.cardPoint.text = "0";
                }
            }
        }
    }
    private IEnumerator DrawCardsEffect(Deck deck, int numberOfCards)
    {
        yield return deck.DrawCards(numberOfCards);
    }
    private void ApplyIncreaseEffect(CardSO card, Player currentPlayer)
    {
        for (int i = 0; i < card.ranges.Count; i++)
        {
            if (card.ranges[i] == CardSO.Range.W1)
            {
                IncreasePointsInZone(currentPlayer.MeleeZone.transform);
            }
            if (card.ranges[i] == CardSO.Range.W2)
            {
                IncreasePointsInZone(currentPlayer.RangedZone.transform);
            }
            if (card.ranges[i] == CardSO.Range.W3)
            {
                IncreasePointsInZone(currentPlayer.SiegeZone.transform);
            }
        }
    }

    private void IncreasePointsInZone(Transform zone)
    {
        foreach (Transform cardTransform in zone)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null)
            {
                cardComponent.card.Power += 2;
                cardComponent.cardPoint.text = cardComponent.card.Power.ToString();
            }
        }
    }
    private void InvokeCard(Player player, CardSO card)
    {
        Transform graveyard = player.GraveyardZone.transform;
        string targetMonster = "No Muertos";
        string targetHuman = "Infanteria";
        if (card.cardFaction == "Monstruo")
        {
            foreach (Transform cardTransform in graveyard)
            {
                Cards cardComponent = cardTransform.GetComponent<Cards>();
                if (cardComponent != null && cardComponent.card.cardName == targetMonster)
                {
                    cardTransform.SetParent(player.MeleeZone.transform);
                    cardTransform.localPosition = Vector3.zero;
                    cardComponent.card.Power = cardComponent.card.originalPoints;
                    cardComponent.cardPoint.text = cardComponent.card.Power.ToString();
                    break;
                }
            }
        }
        if (card.cardFaction == "Humano")
        {
            foreach (Transform cardTransform in graveyard)
            {
                Cards cardComponent = cardTransform.GetComponent<Cards>();
                if (cardComponent != null && cardComponent.card.cardName == targetHuman)
                {
                    cardTransform.SetParent(player.MeleeZone.transform);
                    cardTransform.localPosition = Vector3.zero;
                    cardComponent.card.Power = cardComponent.card.originalPoints;
                    cardComponent.cardPoint.text = cardComponent.card.Power.ToString();
                    break;
                }
            }
        }
    }
    private void ApplyDestroyEffect(CardSO card, Player currentPlayer, Player otherPlayer)
    {
        if (card.ranges[0] == CardSO.Range.W1)
        {
            MoveCardsToGraveyard(currentPlayer.Weather1Zone.transform, currentPlayer);
            MoveCardsToGraveyard(currentPlayer.Weather2Zone.transform, currentPlayer);
            MoveCardsToGraveyard(currentPlayer.Weather3Zone.transform, currentPlayer);
            MoveCardsToGraveyard(otherPlayer.Weather1Zone.transform, otherPlayer);
            MoveCardsToGraveyard(otherPlayer.Weather2Zone.transform, otherPlayer);
            MoveCardsToGraveyard(otherPlayer.Weather3Zone.transform, otherPlayer);
            
        }
        else
        {
            DestroyRandomCardInZones(otherPlayer);
        }
    }
    private void DestroyRandomCardInZones(Player otherPlayer)
    {
        List<Transform> allCards = new List<Transform>();

        AddCardsToList(otherPlayer.MeleeZone.transform, allCards);
        AddCardsToList(otherPlayer.RangedZone.transform, allCards);
        AddCardsToList(otherPlayer.SiegeZone.transform, allCards);

        if (allCards.Count > 0)
        {
            System.Random rng = new System.Random();
            int randomIndex = rng.Next(0, allCards.Count);
            Destroy(allCards[randomIndex].gameObject);
        }
    }
    private void AddCardsToList(Transform zone, List<Transform> cardList)
    {
        foreach (Transform cardTransform in zone)
        {
            cardList.Add(cardTransform);
        }
    }
    private void MoveCardsToGraveyard(Transform zone, Player player)
    {
        List<Transform> cardsToMove = new List<Transform>();

        foreach (Transform cardTransform in zone)
        {
            cardsToMove.Add(cardTransform);
        }

        foreach (Transform cardTransform in cardsToMove)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null)
            {
                if (cardComponent.card.Effect == "Increase" || cardComponent.card.Effect == "Decrease")
                {
                    ResetPointsInZone(player.MeleeZone.transform);
                    ResetPointsInZone(player.RangedZone.transform);
                    ResetPointsInZone(player.SiegeZone.transform);
                }
                cardTransform.SetParent(player.GraveyardZone.transform);
                cardTransform.localPosition = Vector3.zero;
            }
        }
    }
    private void ResetPointsInZone(Transform zone)
    {
        foreach (Transform cardTransform in zone)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null)
            {
                cardComponent.card.Power = cardComponent.card.originalPoints;
                cardComponent.cardPoint.text = cardComponent.card.Power.ToString();
            }
        }
    }

    private List<CardSO> GetBoardCards(Player currentPlayer, Player otherPlayer)
    {
        List<CardSO> boardCards = new List<CardSO>();

        // Agregar cartas del jugador actual
        boardCards.AddRange(GetCardsInZone(currentPlayer.MeleeZone.transform));
        boardCards.AddRange(GetCardsInZone(currentPlayer.RangedZone.transform));
        boardCards.AddRange(GetCardsInZone(currentPlayer.SiegeZone.transform));
        boardCards.AddRange(GetCardsInZone(currentPlayer.Weather1Zone.transform));
        boardCards.AddRange(GetCardsInZone(currentPlayer.Weather2Zone.transform));
        boardCards.AddRange(GetCardsInZone(currentPlayer.Weather3Zone.transform));

        // Agregar cartas del otro jugador
        boardCards.AddRange(GetCardsInZone(otherPlayer.MeleeZone.transform));
        boardCards.AddRange(GetCardsInZone(otherPlayer.RangedZone.transform));
        boardCards.AddRange(GetCardsInZone(otherPlayer.SiegeZone.transform));
        boardCards.AddRange(GetCardsInZone(otherPlayer.Weather1Zone.transform));
        boardCards.AddRange(GetCardsInZone(otherPlayer.Weather2Zone.transform));
        boardCards.AddRange(GetCardsInZone(otherPlayer.Weather3Zone.transform));

        return boardCards;
    }

    private List<CardSO> GetCardsInZone(Transform zone)
    {
        List<CardSO> cardsInZone = new List<CardSO>();

        foreach (Transform cardTransform in zone)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null)
            {
                cardsInZone.Add(cardComponent.card);
            }
        }

        return cardsInZone;
    }

    private void ApplyEffectByName(EffectNode effect, CardSO card, Player currentPlayer, Player otherPlayer, ScoreCounter scoreCounter, Context context)
    {
        Selector selector = new Selector
        {
            Source = effect.Selector.Source,
            Single = effect.Selector.Single,
            Predicate = ConvertToPredicate(effect.Selector.Predicate) // Convertir NodeAST a Predicate<CardSO>
        };

        switch (effect.Name)
        {
            case "Damage":
                ApplyDamageEffect(effect, card, currentPlayer, otherPlayer, context);
                break;
            case "Draw":
                ApplyDrawEffect(effect, currentPlayer, context);
                break;
            case "ReturnToDeck":
                ApplyReturnToDeckEffect(effect, currentPlayer, context);
                break;
            default:
                break;
        }
    }

    private Predicate<CardSO> ConvertToPredicate(NodeAST node)
    {
        if (node is BinaryExpressionNode binaryNode)
        {
            var leftPredicate = ConvertToPredicate(binaryNode.Left);
            var rightPredicate = ConvertToPredicate(binaryNode.Right);

            switch (binaryNode.Operator)
            {
                case "&&":
                    return card => leftPredicate(card) && rightPredicate(card);
                case "||":
                    return card => leftPredicate(card) || rightPredicate(card);
                case "==":
                    return card => leftPredicate(card) == rightPredicate(card);
                case "!=":
                    return card => leftPredicate(card) != rightPredicate(card);
                case "<":
                    return card => Convert.ToInt32(leftPredicate(card)) < Convert.ToInt32(rightPredicate(card));
                case ">":
                    return card => Convert.ToInt32(leftPredicate(card)) > Convert.ToInt32(rightPredicate(card));
                case "<=":
                    return card => Convert.ToInt32(leftPredicate(card)) <= Convert.ToInt32(rightPredicate(card));
                case ">=":
                    return card => Convert.ToInt32(leftPredicate(card)) >= Convert.ToInt32(rightPredicate(card));
                default:
                    throw new NotSupportedException($"Operador no soportado: {binaryNode.Operator}");
            }
        }
        else if (node is UnaryExpressionNode unaryNode)
        {
            var operandPredicate = ConvertToPredicate(unaryNode.Operand);

            switch (unaryNode.Operator)
            {
                case "!":
                    return card => !operandPredicate(card);
                default:
                    throw new NotSupportedException($"Operador no soportado: {unaryNode.Operator}");
            }
        }
        else if (node is IdentifierNode identifierNode)
        {
            return card => card.cardName == identifierNode.Name;
        }
        else if (node is NumberNode numberNode)
        {
            return card => card.Power == int.Parse(numberNode.Value);
        }
        else if (node is SelectorNode selectorNode)
        {
            var sourcePredicate = ConvertToPredicate(selectorNode.Predicate);
            return card => sourcePredicate(card);
        }
        else if (node is ParamNode paramNode)
        {
            switch (paramNode.Name)
            {
                case "Type":
                    return card => card.type.ToString() == paramNode.Type;
                case "Faction":
                    return card => card.cardFaction == paramNode.Type;
                case "Range":
                    return card => card.ranges.Contains((CardSO.Range)Enum.Parse(typeof(CardSO.Range), paramNode.Type));
                default:
                    throw new NotSupportedException($"Parámetro no soportado: {paramNode.Name}");
            }
        }

        throw new NotSupportedException($"Tipo de nodo no soportado: {node.GetType().Name}");
    }

    private void ApplyPostAction(EffectNode postAction, Player currentPlayer, Player otherPlayer, ScoreCounter scoreCounter, Context context, List<CardSO> parentTargets)
    {
        Selector selector;
        if (postAction.Selector != null)
        {
            selector = new Selector
            {
                Source = postAction.Selector.Source,
                Single = postAction.Selector.Single,
                Predicate = ConvertToPredicate(postAction.Selector.Predicate)
            };
        }
        else
        {
            selector = new Selector
            {
                Source = "parent",
                Single = false,
                Predicate = unit => true
            };
        }

        List<CardSO> targets;
        if (selector.Source == "parent")
        {
            targets = parentTargets.FindAll(selector.Predicate);
        }
        else
        {
            targets = context.SelectCards(selector);
        }

        ApplyEffectByName(postAction, null, currentPlayer, otherPlayer, scoreCounter, context);
    }


    private void ApplyDrawEffect(EffectNode effect, Player currentPlayer, Context context)
    {
        CardSO topCard = context.Pop(context.Deck);
        context.Push(context.Hand, topCard);
        context.Shuffle(context.Hand);
    }

    private void ApplyReturnToDeckEffect(EffectNode effect, Player currentPlayer, Context context)
    {
        Selector selector = new Selector
        {
            Source = effect.Selector.Source,
            Single = effect.Selector.Single,
            Predicate = ConvertToPredicate(effect.Selector.Predicate)
        };

        List<CardSO> targets = context.SelectCards(selector);

        foreach (var target in targets)
        {
            int owner = target.Owner;
            List<CardSO> deck = context.DeckOfPlayer(owner);
            context.Push(deck, target);
            context.Shuffle(deck);
            context.Board.Remove(target);
        }
    }

    private void ApplyDamageEffect(EffectNode effect, CardSO card, Player currentPlayer, Player otherPlayer, Context context)
    {
        Selector selector = new Selector
        {
            Source = effect.Selector.Source,
            Single = effect.Selector.Single,
            Predicate = ConvertToPredicate(effect.Selector.Predicate)
        };
        List<CardSO> targets = context.SelectCards(selector);

        int amount = 5;

        foreach (var target in targets)
        {
            for (int i = 0; i < amount; i++)
            {
                target.Power -= 1;
            }
        }

        if (effect.PostAction != null)
        {
            ApplyPostAction(effect.PostAction, currentPlayer, otherPlayer, scoreCounter, context, targets);
        }
    }

}
   

