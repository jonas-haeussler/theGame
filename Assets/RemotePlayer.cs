using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class RemotePlayer : Player
    {
        private void Start()
        {
            cardPlayAnimTime = 0.5f;
        }

        public override void TurnChanged(Game.Turn currentTurn)
        {
            base.TurnChanged(currentTurn);
        }

    }
}
