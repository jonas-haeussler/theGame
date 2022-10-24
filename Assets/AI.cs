
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;

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
                makeTurn();
            } 
        }

        private void makeTurn()
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
        internal override void initGame(Game.PlayerID myPlayerID, GameObject cardPrefab, Action<Game.DiscardActionParameters> playCard, Action finishTurn, List<int> ordering)
        {
            base.initGame(myPlayerID, cardPrefab, playCard, finishTurn, ordering);

            if (isMyTurn) StartCoroutine(sleep(5f, this.playCard));
        }
    }
}
