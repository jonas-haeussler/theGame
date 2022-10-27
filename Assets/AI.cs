
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

            var parameters = findCombo();
            if (parameters != null)
            {
                playCardCallback(parameters);
                return;
            }

            parameters = findClosestTurn();
            if (parameters != null)
            {
                playCardCallback(parameters);
                return;
            }

        }

        private Game.DiscardActionParameters findCombo()
        {
            Game.DiscardActionParameters combo = null;
            foreach (var card in Hand.GetHandCards())
            {
                foreach (var pile in discardPiles)
                {
                    int dist = Math.Abs(pile.Cards[pile.Cards.Count - 1].Number - card.Number);
                    if (dist == 10)
                    {
                        if (PlayerID.Equals(Game.PlayerID.Player1))
                        {
                            if (pile.Type.Equals(Game.PileType.Player1Ascending) && card.Number < pile.Cards[pile.Cards.Count - 1].Number)
                            {
                                combo = new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number);
                                return combo;
                            }
                            else if (pile.Type.Equals(Game.PileType.Player1Descending) && card.Number > pile.Cards[pile.Cards.Count - 1].Number)
                            {
                                combo = new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number);
                                return combo;
                            }
                        }
                        else if (PlayerID.Equals(Game.PlayerID.Player2))
                        {
                            if (pile.Type.Equals(Game.PileType.Player2Ascending) && card.Number < pile.Cards[pile.Cards.Count - 1].Number)
                            {
                                combo = new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number);
                                return combo;
                            }
                            else if (pile.Type.Equals(Game.PileType.Player2Descending) && card.Number > pile.Cards[pile.Cards.Count - 1].Number)
                            {
                                combo = new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number);
                                return combo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Game.DiscardActionParameters findClosestTurn()
        {
            Game.DiscardActionParameters closestTurn = null;
            int distance = int.MaxValue;
            foreach (var card in Hand.GetHandCards())
            {
                foreach (var pile in discardPiles)
                {
                    if (isValidTurn(pile.Cards[pile.Cards.Count - 1].Number, card.Number, pile.Type))
                    {
                        int newDist = Math.Abs(pile.Cards[pile.Cards.Count - 1].Number - card.Number);

                        if (newDist < distance)
                        {
                            closestTurn = new Game.DiscardActionParameters(PlayerID, pile.Type, card.Number);
                            distance = newDist;
                        }
                        return closestTurn;
                    }
                }
            }
            return null;
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
