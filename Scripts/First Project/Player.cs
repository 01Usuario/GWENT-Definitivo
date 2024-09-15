using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject MeleeZone;
    public GameObject RangedZone;
    public GameObject SiegeZone;
    public GameObject Weather1Zone;
    public GameObject Weather2Zone;
    public GameObject Weather3Zone;
    public GameObject GraveyardZone;
    public GameObject DeckPlayer;
    public GameObject HandPanel;
    public GameObject Board;
    public int id { get; set; }
    public bool OnTurn { get; set; }
    public int Points { get; set; }

}
