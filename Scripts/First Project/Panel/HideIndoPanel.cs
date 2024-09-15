using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HideIndoPanel : MonoBehaviour, IPointerClickHandler

{
    public CardInfoPanel cardInfoPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        cardInfoPanel.HideCardInfo();
    }
}


