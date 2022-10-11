
using System.Collections;
using System;
using UnityEngine;

namespace Assets
{
    public class AI : RemotePlayer
    {

        private IEnumerator sleep(float sleepAmount, Action callback)
        {
            yield return new WaitForSeconds(sleepAmount);
            callback();
        }

        private void playCard()
        {
            
            foreach (var card in Hand.GetHandCards())
            {
                foreach (var pile in discardPiles)
                {
                    if (isValidTurn(pile.Cards[pile.Cards.Count - 1].Number, card.Number, pile.Type))
                    {
                        playCardCallback(new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number));
                        return;
                    }
                }
            }
            
        }

        public override void TurnChanged(Game.Turn currentTurn)
        {
            base.TurnChanged(currentTurn);
            if (isMyTurn)
            {
                if (turnFinishAllowed)
                {
                    finishTurnCallback();
                }
                else
                {
                    StartCoroutine(sleep(1f, playCard));
                }
            } 
        }
    }
}
