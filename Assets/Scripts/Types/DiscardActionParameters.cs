using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    public class DiscardActionParameters
    {
        public PlayerID PlayerID;
        public PileType PileType;
        public int CardNumber;

        public DiscardActionParameters(PlayerID playerID, PileType pileType, int cardNumber)
        {
            this.PlayerID = playerID;
            this.PileType = pileType;
            this.CardNumber = cardNumber;
        }
    }
}
