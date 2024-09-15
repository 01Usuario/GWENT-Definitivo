using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoPanel : MonoBehaviour
{
    public GameObject infoPanel;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardPowerText;
    public TextMeshProUGUI cardDescriptionText;
    public TextMeshProUGUI cardRangeText;
    public TextMeshProUGUI cardFactionText;
    public Image cardImage;
    private void Start()
    {
        infoPanel.SetActive(false); 
    }
    public void ShowCardInfo(CardSO card)
    {
        infoPanel.SetActive(true);
        cardNameText.text = card.cardName;
        cardPowerText.text = card.Power.ToString();
        cardDescriptionText.text = card.cardDescription;
        cardFactionText.text = card.cardFaction;
        cardImage.sprite = card.cardImage;
        for(int i = 0; i < card.ranges.Count; i++)
        {
            cardPowerText.text = card.ranges[i].ToString()+",";
        }
        
    }
    public void HideCardInfo()
    {
        infoPanel.SetActive(false);
    }
}

