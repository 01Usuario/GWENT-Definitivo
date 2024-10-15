using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Card")]
public class CardSO : ScriptableObject
{
    public enum Range { Melee, Ranged, Siege, W1, W2, W3 }
    public enum CardType { Oro, Plata, Unit, Aumento,Clima }
    public string cardName;
    public Sprite cardImage;
    public string cardDescription;
    public string cardFaction;
    public List<Range> ranges = new List<Range>();
    public int originalPoints;
    public int Power;
    public string Effect;
    public Color borderColor;
    public List<CardEffect> OnActivation = new List<CardEffect>();
    public CardType type;
    public int Owner;
    public EffectNode postAction;
}
