using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<CardSO> player1Hand = new List<CardSO>();
    public List<CardSO> player2Hand = new List<CardSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCardToPlayer1Hand(CardSO card)
    {
        player1Hand.Add(card);
    }

    public void AddCardToPlayer2Hand(CardSO card)
    {
        player2Hand.Add(card);
    }

    public void InitializeHands()
    {
        foreach (var card in CardManager.Instance.cards)
        {
            AddCardToPlayer1Hand(card);
            AddCardToPlayer2Hand(card);
        }
    }
}
