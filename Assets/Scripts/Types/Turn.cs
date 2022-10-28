using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    public class Turn
    {
        public PlayerID PlayerID;
        public List<DiscardActionParameters> DiscardActionParameters;
        public bool enemyDiscard;
        public Turn(PlayerID playerID)
        {
            PlayerID = playerID;
            this.DiscardActionParameters = new List<DiscardActionParameters>();
            enemyDiscard = true;
        }
    }
}