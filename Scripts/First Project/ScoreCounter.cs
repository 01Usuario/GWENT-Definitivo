using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public TextMeshProUGUI scoreTextPlayer1;
    public TextMeshProUGUI scoreTextPlayer2;
    public TurnSystem turnSystem;

    private int meleePointsPlayer1;
    private int ratePointsPlayer1;
    private int siegePointsPlayer1;
    private int meleePointsPlayer2;
    private int ratePointsPlayer2;
    private int siegePointsPlayer2;

    private void Start()
    {
        player1.Points = 0;
        player2.Points = 0;
        UpdateScoreDisplay(player1);
        UpdateScoreDisplay(player2);
    }

    public void UpdateScoreDisplay(Player player)
    {
        CalculateTotalPoints(player);
        if (player == player1)
        {
            scoreTextPlayer1.text = player1.Points.ToString();
        }
        else if (player == player2)
        {
            scoreTextPlayer2.text = player2.Points.ToString();
        }
    }

    public void CalculateTotalPoints(Player player)
    {
        if (player == player1)
        {
            meleePointsPlayer1 = CalculatePointsInZone(player.MeleeZone.transform);
            ratePointsPlayer1 = CalculatePointsInZone(player.RangedZone.transform);
            siegePointsPlayer1 = CalculatePointsInZone(player.SiegeZone.transform);
            player1.Points = meleePointsPlayer1 + ratePointsPlayer1 + siegePointsPlayer1;
        }
        else if (player == player2)
        {
            meleePointsPlayer2 = CalculatePointsInZone(player.MeleeZone.transform);
            ratePointsPlayer2 = CalculatePointsInZone(player.RangedZone.transform);
            siegePointsPlayer2 = CalculatePointsInZone(player.SiegeZone.transform);
            player2.Points = meleePointsPlayer2 + ratePointsPlayer2 + siegePointsPlayer2;
        }
    }

    public int CalculatePointsInZone(Transform zone)
    {
        int points = 0;
        foreach (Transform cardTransform in zone)
        {
            Cards cardComponent = cardTransform.GetComponent<Cards>();
            if (cardComponent != null)
            {
                points += cardComponent.card.Power;
            }
        }
        return points;
    }
}
