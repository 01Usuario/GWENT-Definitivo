using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class GameMecanics : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public TurnSystem turnSystem;
    public Button endTurnButton;
    public TextMeshProUGUI roundText;
    public ScoreCounter scoreCounter;
    private Context context;
    private bool player1EndedTurn = false;
    private bool player2EndedTurn = false;
    private int player1Points = 0;
    private int player2Points = 0;
    private int player1RoundsWon = 0;
    private int player2RoundsWon = 0;

    private void Start()
    {
        context = FindObjectOfType<Context>();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        UpdateRoundText();

    }

    private void OnEndTurnButtonClicked()
    {
        if (turnSystem.playerOnTurn == player1)
        {
            player1EndedTurn = true;
            player1Points = player1.Points;
            player1.HandPanel.SetActive(false);
            turnSystem.SetTurn(player2);
        }
        else if (turnSystem.playerOnTurn == player2)
        {
            player2EndedTurn = true;
            player2Points = player2.Points;
            player2.HandPanel.SetActive(false);
            turnSystem.SetTurn(player1);
        }

        Result();
    }

    private void Result()
    {
        if (player1EndedTurn && player2EndedTurn)
        {
            if (player1Points > player2Points)
            {
                player1RoundsWon++;

            }
            else if (player2Points > player1Points)
            {
                player2RoundsWon++;
            }
            else
            {
                player1RoundsWon++;
                player2RoundsWon++;
            }

            MoveToGraveyard(player1);
            MoveToGraveyard(player2);

            player1EndedTurn = false;
            player2EndedTurn = false;
            player1.HandPanel.SetActive(true);
            player2.HandPanel.SetActive(true);
            player1.Points = 0;
            player2.Points = 0;
            player1Points = 0;
            player2Points = 0;
            UpdateRoundText();


            if (player1RoundsWon > player2RoundsWon)
            {
                turnSystem.SetTurn(player1);
            }
            else
            {
                turnSystem.SetTurn(player2);
            }

            if (player1RoundsWon == 2)
            {               
                endTurnButton.interactable = false;
            }
            else if (player2RoundsWon == 2)
            {
               
                endTurnButton.interactable = false;
            }


            player1.DeckPlayer.GetComponent<Deck>().StartNewRound();
            player2.DeckPlayer.GetComponent<Deck>().StartNewRound();
        }
    }

    private void MoveToGraveyard(Player player)
    {
        ResetCardPoints(player.MeleeZone.transform);
        ResetCardPoints(player.RangedZone.transform);
        ResetCardPoints(player.SiegeZone.transform);
        ResetCardPoints(player.Weather1Zone.transform);
        ResetCardPoints(player.Weather2Zone.transform);
        ResetCardPoints(player.Weather3Zone.transform);

        MoveCardsToGraveyard(player.MeleeZone.transform, player.GraveyardZone.transform);
        MoveCardsToGraveyard(player.RangedZone.transform, player.GraveyardZone.transform);
        MoveCardsToGraveyard(player.SiegeZone.transform, player.GraveyardZone.transform);
        MoveCardsToGraveyard(player.Weather1Zone.transform, player.GraveyardZone.transform);
        MoveCardsToGraveyard(player.Weather2Zone.transform, player.GraveyardZone.transform);
        MoveCardsToGraveyard(player.Weather3Zone.transform, player.GraveyardZone.transform);
    }

    private void ResetCardPoints(Transform zone)
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

    private void MoveCardsToGraveyard(Transform fromZone, Transform toZone)
    {
        List<Transform> cardsToMove = new List<Transform>();

        foreach (Transform card in fromZone)
        {
            cardsToMove.Add(card);
        }

        foreach (Transform card in cardsToMove)
        {
            card.SetParent(toZone);
            card.localPosition = Vector3.zero;
        }
    }

    private void UpdateRoundText()
    {
        roundText.text = "Rounds: Player 1 - " + player1RoundsWon + " | Player 2 - " + player2RoundsWon;
    }
}
