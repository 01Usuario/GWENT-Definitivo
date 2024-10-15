using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



/*effect {
     Name: "Damage",
     Params: {
         Amount: Number
     },
     Action: (targets, context) => {
         for target in targets {
             i = 0;
             while (i++ < Amount) {
                 target.Power -= 1;
             }
         }
     }
 }
 effect {
     Name: "Draw",
     Action: (targets, context) => {
         topCard = context.Deck.Pop();
         context.Hand.Add(topCard);
         context.Hand.Shuffle();
     }
 }
 effect {
     Name: "ReturnToDeck",
     Action: (targets, context) => {
         for target in targets {
             owner = target.Owner;
             deck = context.DeckOfPlayer(owner);
             deck.Push(target);
             deck.Shuffle();
             context.Board.Remove(target);
         }
     }
 }
 card {
     Type: "Oro",
     Name: "Beluga",
     Faction: "Northern Realms",
     Power: 10,
     Range: ["Melee", "Ranged"],
     OnActivation: [
         {
             effect: {
                 Name: "Damage",
                 Amount: 5,
                 Selector: {
                     Source: "board",
                     Single: false,
                     Predicate: (unit) => unit.Faction == "Northern" @@ "Realms"
                 },
                 PostAction: {
                     Type: "ReturnToDeck",
                     Selector: {
                         Source: "parent",
                         Single: false,
                         Predicate: (unit) => unit.Power < 1
                     },
                 }
             }
         },
         {
             effect: "Draw"
         }
     ]
 } */


 /*effect {
     Name: "Punch",
     Params: {
         Amount: Number
     },
     Action: (targets, context) => {
         for target in targets {
             target.Power -= Amount;
         }
     }
 }

 card {
     Type: "Plata",
     Name: "Griffin",
     Faction: "Monsters",
     Power: 8,
     Range: ["Siege"],
     OnActivation: [
         {
             effect: {
                 Name: "Punch",
                 Amount: 3,
                 Selector: {
                     Source: "board",
                     Single: true,
                     Predicate: (unit) => unit.Faction == "Monsters"
                 }
             }
         }
     ]
 }*/



