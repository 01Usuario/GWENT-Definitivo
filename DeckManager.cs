using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    public List<CardSO> Player1Deck { get; private set; }
    public List<CardSO> Player2Deck { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Player1Deck = new List<CardSO>();
            Player2Deck = new List<CardSO>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCardToDecks(CardSO card)
    {
        Player1Deck.Add(card);
        Player2Deck.Add(card);
    }
}
