using UnityEngine.EventSystems;
using UnityEngine;

public class DropCard : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Transform cardHolder;
    public TurnSystem turnSystem;
    public Player owner;
    public CardSO.Range panelType;
    public ScoreCounter scoreCounter;
    public Context context;

    private void Start()
    {
        turnSystem = GameObject.FindObjectOfType<TurnSystem>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable action = eventData.pointerDrag.GetComponent<Draggable>();
        Cards cardComponent = eventData.pointerDrag.GetComponent<Cards>();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable action = eventData.pointerDrag.GetComponent<Draggable>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject card = eventData.pointerDrag;
        Draggable draggableComponent = card.GetComponent<Draggable>();
        Cards cardComponent = card.GetComponent<Cards>();

        if (draggableComponent != null && cardComponent != null && draggableComponent.isDraggable && turnSystem.playerOnTurn == owner)
        {
            if (!turnSystem.playerOnTurn.HandPanel.activeSelf)
            {
                return;
            }
            if (owner != turnSystem.playerOnTurn)
            {
                return;
            }

            if (panelType == cardComponent.card.ranges[0] || panelType == cardComponent.card.ranges[1] || panelType == cardComponent.card.ranges[2])
            {
                draggableComponent.parentToReturn = this.transform;
                draggableComponent.isDraggable = false;
                card.transform.SetParent(transform);
                scoreCounter.UpdateScoreDisplay(owner);

                Player otherPlayer;
                if (turnSystem.playerOnTurn == turnSystem.player1)
                {
                    otherPlayer = turnSystem.player2;
                }
                else
                {
                    otherPlayer = turnSystem.player1;
                }

                cardComponent.ApplyCardEffect(owner, otherPlayer, scoreCounter,context);
                if (turnSystem.playerOnTurn.HandPanel.activeSelf)
                    turnSystem.ChangeTurn();
            }

            eventData.pointerDrag.transform.SetParent(draggableComponent.parentToReturn);
            eventData.pointerDrag.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
}
