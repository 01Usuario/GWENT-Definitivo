using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardFactory
{
    public static CardSO CreateCardFromAST(NodeAST ast)
    {
        if (ast is CardNode cardNode)
        {
            CardSO newCard = ScriptableObject.CreateInstance<CardSO>();
            newCard.cardName = cardNode.Name;
            newCard.cardFaction = cardNode.Faction;
            newCard.originalPoints = cardNode.Power;
            newCard.Power = cardNode.Power;
            newCard.type = (CardSO.CardType)Enum.Parse(typeof(CardSO.CardType), cardNode.Type, true);
            newCard.ranges = cardNode.Range.Select(r => (CardSO.Range)Enum.Parse(typeof(CardSO.Range), r, true)).ToList();
            newCard.effects = cardNode.OnActivation.Select(e => new CardEffect { EffectName = e.Name }).ToList();
            return newCard;
        }
        throw new Exception("El AST no contiene una definición de carta válida.");
    }
}

