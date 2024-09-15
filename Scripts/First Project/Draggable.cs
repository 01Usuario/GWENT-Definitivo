using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JetBrains.Annotations;


public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Transform parentToReturn = null;
    public bool isDraggable; 
    private CanvasGroup canvasGroup;
   
    private void Awake()
    {

        isDraggable = true;
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable)
            return;
      
        parentToReturn = this.transform.parent;
      
        this.transform.SetParent(this.transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable)
            return;
        this.transform.position = eventData.position;
    }  

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable)
        {
            this.transform.SetParent(parentToReturn);
            this.transform.position = parentToReturn.position;

        }

        DropCard dropCard = parentToReturn.GetComponent<DropCard>();
        if (dropCard != null)
        {
            isDraggable = false;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

        }
        else
        {
            transform.SetParent(parentToReturn);
            transform.position = parentToReturn.position;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;
        }


    }
}
