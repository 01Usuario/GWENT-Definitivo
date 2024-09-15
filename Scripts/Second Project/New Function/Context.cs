using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Context : MonoBehaviour
{
    public int TriggerPlayer { get; set; }
    public List<CardSO> Board { get; set; } = new List<CardSO>();

    private Dictionary<int, List<CardSO>> hands = new Dictionary<int, List<CardSO>>();
    private Dictionary<int, List<CardSO>> fields = new Dictionary<int, List<CardSO>>();
    private Dictionary<int, List<CardSO>> graveyards = new Dictionary<int, List<CardSO>>();
    private Dictionary<int, List<CardSO>> decks = new Dictionary<int, List<CardSO>>();

    public List<CardSO> HandOfPlayer(int player)
    {
        return hands.ContainsKey(player) ? hands[player] : new List<CardSO>();
    }

    public List<CardSO> FieldOfPlayer(int player)
    {
        return fields.ContainsKey(player) ? fields[player] : new List<CardSO>();
    }

    public List<CardSO> GraveyardOfPlayer(int player)
    {
        return graveyards.ContainsKey(player) ? graveyards[player] : new List<CardSO>();
    }

    public List<CardSO> DeckOfPlayer(int player)
    {
        return decks.ContainsKey(player) ? decks[player] : new List<CardSO>();
    }

    public List<CardSO> Hand => HandOfPlayer(TriggerPlayer);
    public List<CardSO> Deck => DeckOfPlayer(TriggerPlayer);
    public List<CardSO> Field => FieldOfPlayer(TriggerPlayer);
    public List<CardSO> Graveyard => GraveyardOfPlayer(TriggerPlayer);

    public List<CardSO> Find(Predicate<CardSO> predicate)
    {
        return Hand.FindAll(predicate);
    }



    public CardSO Pop(List<CardSO> list)
    {
        if (list.Count == 0)
            throw new InvalidOperationException("No hay cartas en la lista.");

        var card = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return card;
    }

    public void Push(List<CardSO> list, CardSO card)
    {
        list.Add(card);
    }

    public void Shuffle(List<CardSO> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CardSO value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }



    public void SendBottom(CardSO card)
    {
        Hand.Insert(0, card);
    }

    public void Remove(CardSO card)
    {
        Hand.Remove(card);
    }


    public List<CardSO> SelectCards(Selector selector, List<CardSO> parentTargets = null)
    {
        List<CardSO> sourceList = new List<CardSO>();

        switch (selector.Source)
        {
            case "hand":
                sourceList = Hand;
                break;
            case "otherHand":
                sourceList = HandOfPlayer(TriggerPlayer == 1 ? 2 : 1);
                break;
            case "deck":
                sourceList = Deck;
                break;
            case "otherDeck":
                sourceList = DeckOfPlayer(TriggerPlayer == 1 ? 2 : 1);
                break;
            case "field":
                sourceList = Field;
                break;
            case "otherField":
                sourceList = FieldOfPlayer(TriggerPlayer == 1 ? 2 : 1);
                break;
            case "parent":
                if (parentTargets != null)
                {
                    sourceList = parentTargets;
                }
                break;
        }

        var selectedCards = sourceList.FindAll(selector.Predicate);
        if (selector.Single && selectedCards.Count > 0)
        {
            return new List<CardSO> { selectedCards[0] };
        }

        return selectedCards;
    }
}
