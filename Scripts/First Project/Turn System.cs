using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class TurnSystem : MonoBehaviour
{
    public GameObject greenLight;
    public GameObject greenLight2;
    public Player player1;
    public Player player2;
    public Player playerOnTurn;
    private Context context;

    private void Start()
    {
        player1.id = 1;
        player2.id = 2;
        player1.OnTurn = true;
        playerOnTurn = player1;
        context = FindObjectOfType<Context>();
        context.TriggerPlayer = player2.id;
    }

    private void Update()
    {
        if (player1.OnTurn)
        {
           greenLight.SetActive(true);
            greenLight2.SetActive(false);
        }
        else
        {
            greenLight2.SetActive(true);
            greenLight.SetActive(false);
        }
    }

    public void ChangeTurn()
    {
        if (playerOnTurn == player1)
        {
            player1.OnTurn = false;
            player2.OnTurn = true;
            playerOnTurn = player2;
           context.TriggerPlayer = player2.id;
        }
        else
        {
            player2.OnTurn = false;
            player1.OnTurn = true;
            playerOnTurn = player1;
            context.TriggerPlayer = player1.id;
        }
    }

    public void SetTurn(Player player)
    {
        playerOnTurn = player;
        player1.OnTurn = (player == player1);
        player2.OnTurn = (player == player2);
        context.TriggerPlayer = player.id;
    }
}
