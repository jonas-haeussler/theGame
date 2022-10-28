using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    public class EndingProperties
    {
        public PlayerID winner;
        public bool noTurnsLeftCondition;

        public EndingProperties(PlayerID winner, bool noTurnsLeftCondition)
        {
            this.winner = winner;
            this.noTurnsLeftCondition = noTurnsLeftCondition;
        }
    }
}
