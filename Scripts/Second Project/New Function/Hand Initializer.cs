using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInitializer : MonoBehaviour
{
    public Transform player1HandPanel;
    public Transform player2HandPanel;
    public GameObject cardPrefab;

    private void Start()
    {
        // Inicializar las manos en DeckManager
        DeckManager.Instance.InitializeHands();

        // Asignar las cartas a los paneles de la mano
        foreach (var card in DeckManager.Instance.player1Hand)
        {
            GameObject cardObject = Instantiate(cardPrefab, player1HandPanel);
            Cards cardComponent = cardObject.GetComponent<Cards>();
            cardComponent.card = card;
        }

        foreach (var card in DeckManager.Instance.player2Hand)
        {
            GameObject cardObject = Instantiate(cardPrefab, player2HandPanel);
            Cards cardComponent = cardObject.GetComponent<Cards>();
            cardComponent.card = card;
        }
    }
}
