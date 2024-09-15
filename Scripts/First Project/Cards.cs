using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class Cards : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public TextMeshProUGUI cardPoint;
    public Image cardBorder;
    public CardSO card;
    private CardEffect effectHandler;
    public bool hasDecreaseEffect;
    private Context context;
    private void Start()
    {
        if (card != null)
        {
            cardName.text = card.cardName;
            cardImage.sprite = card.cardImage;
            cardPoint.text = card.Power.ToString();
            cardBorder.color = GetBorderColor(card.type);
        }
        effectHandler = FindObjectOfType<CardEffect>();
        context = FindObjectOfType<Context>();
    }

    private Color GetBorderColor(CardSO.CardType type)
    {
        switch (type)
        {
            case CardSO.CardType.Oro:
                return new Color32(255, 215, 0, 255); // Oro
            case CardSO.CardType.Plata:
                return new Color32(192, 192, 192, 255); // Plata
            case CardSO.CardType.Unit:
                return new Color32(205, 127, 50, 255); // Bronce
            case CardSO.CardType.Clima:
                return new Color32(19,91,80,255);
            case CardSO.CardType.Aumento:
                return Color.gray;
            default:
                return Color.black;

        }
    }
    public void ApplyCardEffect(Player currentPlayer, Player otherPlayer, ScoreCounter scoreCounter,Context context)
    {
        if (effectHandler != null)
        {
            effectHandler.ApplyEffect(card, currentPlayer, otherPlayer, scoreCounter,context);
        }
    }
}

