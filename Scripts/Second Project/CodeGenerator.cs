using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System;
using System.IO;

public class CodeGenerator
{
    private List<NodeAST> nodes;

    public CodeGenerator(List<NodeAST> nodesParser)
    {
        nodes = nodesParser;
    }

    public void GenerateCode()
    {
        foreach (var node in nodes)
        {
            if (node is CardNode cardNode)
            {
                CreateCardInstance(cardNode);
            }
        }
    }

    private void CreateCardInstance(CardNode cardNode)
    {
        CardSO cardInstance = ScriptableObject.CreateInstance<CardSO>();
        cardInstance.cardName = cardNode.Name;
        cardInstance.type = (CardSO.CardType)Enum.Parse(typeof(CardSO.CardType), cardNode.Type);
        cardInstance.cardFaction = cardNode.Faction;
        cardInstance.Power = cardNode.Power;
        cardInstance.ranges = cardNode.Range.Select(r => (CardSO.Range)Enum.Parse(typeof(CardSO.Range), r)).ToList();
        //cardInstance.OnActivation = cardNode.OnActivation.Select(CreateEffect).ToList();

        // Añadir la carta directamente a la mano del jugador 1
        DeckManager.Instance.AddCardToPlayer1Hand(cardInstance);
    }
}
