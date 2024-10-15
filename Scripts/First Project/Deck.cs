
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Deck : MonoBehaviour
{
    public List<CardSO> deck;
    public Transform handPanel;
    public GameObject cardPrefab;
    public GameObject cardInDeck1;
    public GameObject cardInDeck2;
    public GameObject cardInDeck3;
    public TextMeshProUGUI cardsAmount;

    private void Start()
    {

        StartNewGame();
    }


    void Update()
    {
        UpdateDeckVisual();
    }

    public void StartNewGame()
    {
        foreach (CardSO card in deck)
        {
            card.Power = card.originalPoints;
        }
        Shuffle();
        StartCoroutine(DrawCards(10));

    }

    public void StartNewRound()
    {
        StartCoroutine(DrawCards(2));

    }

    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardSO temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public IEnumerator DrawCards(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            if (deck.Count > 0)
            {
                CardSO cardToDraw = deck[0];
                deck.RemoveAt(0);
                GameObject cardObject = Instantiate(cardPrefab, handPanel);
                Cards cardComponent = cardObject.GetComponent<Cards>();
                cardComponent.card = cardToDraw;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    void UpdateDeckVisual()
    {
        if (deck.Count < 10)
            cardInDeck1.SetActive(false);

        if (deck.Count < 5)
            cardInDeck2.SetActive(false);

        if (deck.Count < 1)
            cardInDeck3.SetActive(false);
        cardsAmount.text = "" + deck.Count.ToString();
    }
}

