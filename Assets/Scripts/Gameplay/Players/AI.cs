
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
using Types;
using Core;
namespace Players
{
    public class AI : RemotePlayer
    {

        public class CurrentState
        {
           
            internal List<int> hand;

            internal PileType myAscendingPileType;
            internal PileType myDescendingPileType;
            internal PileType enemyAscendingPileType;
            internal PileType enemyDescendingPileType;

            internal int myAscendingPileTop;
            internal int myDescendingPileTop;
            internal int enemyAscendingPileTop;
            internal int enemyDescendingPileTop;

            public CurrentState() { }

            public CurrentState(List<int> hand, 
                                PileType myAscendingPileType, 
                                PileType myDescendingPileType, 
                                PileType enemyAscendingPileType, 
                                PileType enemyDescendingPileType, 
                                int myAscendingPileTop, 
                                int myDescendingPileTop, 
                                int enemyAscendingPileTop, 
                                int enemyDescendingPileTop)
            {
                this.hand = hand;
                this.myAscendingPileType = myAscendingPileType;
                this.myDescendingPileType = myDescendingPileType;
                this.enemyAscendingPileType = enemyAscendingPileType;
                this.enemyDescendingPileType = enemyDescendingPileType;
                this.myAscendingPileTop = myAscendingPileTop;
                this.myDescendingPileTop = myDescendingPileTop;
                this.enemyAscendingPileTop = enemyAscendingPileTop;
                this.enemyDescendingPileTop = enemyDescendingPileTop;
            }
        }


        private Queue<DiscardActionParameters> turn;

        private IEnumerator sleep(float sleepAmount, DiscardActionParameters parameters, Action<DiscardActionParameters> callback)
        {
            yield return new WaitForSeconds(sleepAmount);
            callback(parameters);
        }

        private bool findCombo(CurrentState currentState,
                               out int playCard,
                               out PileType pileType)
        {
            playCard = -1;
            pileType = PileType.Player1Ascending;
            foreach (var card in currentState.hand)
            {
                if (currentState.myAscendingPileTop - card == 10)
                {
                    playCard = card;
                    pileType = currentState.myAscendingPileType;
                    return true;
                }
                if (card - currentState.myDescendingPileTop == 10)
                {
                    playCard = card;
                    pileType = currentState.myDescendingPileType;
                    return true;
                }
            }
            return false;
        }

        public Queue<DiscardActionParameters> findCardPlays(CurrentState currentState)
        {

            var cardPlays = new Queue<DiscardActionParameters>();
            bool enemyDiscardAllowed = enemyDiscard;
            while (cardPlays.Count < 2)
            {
                int playCard;
                PileType pileType;
                if (findCombo(currentState,
                             out playCard,
                             out pileType))
                {
                    var parameters = new DiscardActionParameters(PlayerID, pileType, playCard);
                    if (pileType.Equals(currentState.myAscendingPileType)) currentState.myAscendingPileTop = playCard;
                    else if (pileType.Equals(currentState.myDescendingPileType)) currentState.myDescendingPileTop = playCard;
                    currentState.hand.Remove(playCard);
                    cardPlays.Enqueue(parameters);
                    continue;
                }
                else if(findClosestCardPlay(currentState,
                                             enemyDiscardAllowed,
                                             out playCard,
                                             out pileType))
                {
                    if(pileType.Equals(currentState.myAscendingPileType))
                    {
                        foreach(int card in currentState.hand)
                        {
                            if(card - playCard == 10)
                            {
                                // Play each card below that card
                                List<int> toRemove = new List<int>();
                                foreach(int otherCard in currentState.hand)
                                {
                                    if(otherCard < card && otherCard != playCard)
                                    {
                                        toRemove.Add(otherCard);
                                    }
                                }

                                toRemove.Sort((a, b) => a.CompareTo(b));
                                foreach (int otherCard in toRemove)
                                {
                                    var otherCardParameters = new DiscardActionParameters(PlayerID, pileType, otherCard);
                                    currentState.hand.Remove(otherCard);
                                    cardPlays.Enqueue(otherCardParameters);
                                }
                                // Then play the card itself
                                var cardParameters = new DiscardActionParameters(PlayerID, pileType, card);
                                currentState.hand.Remove(card);
                                cardPlays.Enqueue(cardParameters);
                                break;
                            }
                        }
                    }
                    else if(pileType.Equals(currentState.myDescendingPileType))
                    {
                        foreach (int card in currentState.hand)
                        {
                            if (playCard - card == 10)
                            {
                                // Play each card above that card
                                List<int> toRemove = new List<int>();
                                foreach (int otherCard in currentState.hand)
                                {
                                    if (otherCard > card && otherCard != playCard)
                                    {
                                        toRemove.Add(otherCard);
                                    }
                                }

                                toRemove.Sort((a, b) => b.CompareTo(a));
                                foreach (int otherCard in toRemove)
                                {
                                    var otherCardParameters = new DiscardActionParameters(PlayerID, pileType, otherCard);
                                    currentState.hand.Remove(otherCard);
                                    cardPlays.Enqueue(otherCardParameters);
                                }
                                // Then play the card itself
                                var cardParameters = new DiscardActionParameters(PlayerID, pileType, card);
                                currentState.hand.Remove(card);
                                cardPlays.Enqueue(cardParameters);
                                break;
                            }
                        }
                    }
                    // Play the card which is closest
                    var playCardParameters = new DiscardActionParameters(PlayerID, pileType, playCard);
                    currentState.hand.Remove(playCard);
                    cardPlays.Enqueue(playCardParameters);
                    if (pileType.Equals(currentState.myAscendingPileType)) currentState.myAscendingPileTop = playCard;
                    else if (pileType.Equals(currentState.myDescendingPileType)) currentState.myDescendingPileTop = playCard;
                    else if (pileType.Equals(currentState.enemyAscendingPileType)) currentState.enemyAscendingPileTop = playCard;
                    else if (pileType.Equals(currentState.enemyDescendingPileType)) currentState.enemyDescendingPileTop = playCard;
                    if (pileType.Equals(currentState.enemyAscendingPileType) || pileType.Equals(currentState.enemyDescendingPileType))
                    {
                        enemyDiscardAllowed = false;
                    }
                }
                else
                {
                    // No turn possible
                    return cardPlays;
                }
            }
            return cardPlays;
            
        }

        private bool findClosestCardPlay(CurrentState currentState,
                                        bool enemyDiscard,
                                        out int playCard,
                                        out PileType pileType)
        {
            int distance = int.MaxValue;
            playCard = -1;
            pileType = PileType.Player1Ascending;
            foreach (var card in currentState.hand)
            {

                if (card > currentState.myAscendingPileTop && distance > Math.Abs(card - currentState.myAscendingPileTop))
                {
                    distance = Math.Abs(card - currentState.myAscendingPileTop);
                    playCard = card;
                    pileType = currentState.myAscendingPileType;
                }
                if (card < currentState.myDescendingPileTop && distance > Math.Abs(card - currentState.myDescendingPileTop))
                {
                    distance = Math.Abs(card - currentState.myDescendingPileTop);
                    playCard = card;
                    pileType = currentState.myDescendingPileType;
                }
                if (card < currentState.enemyAscendingPileTop && distance > Math.Abs(card - currentState.enemyAscendingPileTop) && enemyDiscard)
                {
                    distance = Math.Abs(card - currentState.enemyAscendingPileTop);
                    playCard = card;
                    pileType = currentState.enemyAscendingPileType;
                }
                if (card > currentState.enemyDescendingPileTop && distance > Math.Abs(card - currentState.enemyDescendingPileTop) && enemyDiscard)
                {
                    distance = Math.Abs(card - currentState.enemyDescendingPileTop);
                    playCard = card;
                    pileType = currentState.enemyDescendingPileType;
                }
            }
            if (playCard != -1) return true;
            return false;
        }

        private CurrentState getCurrentState()
        {
            CurrentState currentState = new CurrentState();

            currentState.hand = new List<int>();


            foreach (var card in Hand.GetHandCards())
            {
                currentState.hand.Add(card.Number);
            }

            foreach (var pile in discardPiles)
            {
                if (PlayerID.Equals(PlayerID.Player1))
                {
                    switch (pile.Type)
                    {
                        case PileType.Player1Ascending:
                            currentState.myAscendingPileType = pile.Type;
                            currentState.myAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player1Descending:
                            currentState.myDescendingPileType = pile.Type;
                            currentState.myDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player2Ascending:
                            currentState.enemyAscendingPileType = pile.Type;
                            currentState.enemyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player2Descending:
                            currentState.enemyDescendingPileType = pile.Type;
                            currentState.enemyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                    }
                }
                else if (PlayerID.Equals(PlayerID.Player2))
                {
                    switch (pile.Type)
                    {
                        case PileType.Player2Ascending:
                            currentState.myAscendingPileType = pile.Type;
                            currentState.myAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player2Descending:
                            currentState.myDescendingPileType = pile.Type;
                            currentState.myDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player1Ascending:
                            currentState.enemyAscendingPileType = pile.Type;
                            currentState.enemyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case PileType.Player1Descending:
                            currentState.enemyDescendingPileType = pile.Type;
                            currentState.enemyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                    }
                }
            }
            return currentState;
        }


        public override void TurnChanged(Turn currentTurn)
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
                    makeTurn();
                }
            }
            else
            {
                turn = null;
            }
        }

        private void makeTurn()
        {

            if (turn == null)
            {
                var currentState = getCurrentState();
                turn = findCardPlays(currentState);
            }
            
            if (turn.Count > 0)
            {
                StartCoroutine(sleep(1f, turn.Dequeue(), playCardCallback));
            }
        }
        public override void initGame(PlayerID myPlayerID, GameObject cardPrefab, Action<DiscardActionParameters> playCard, Action finishTurn, List<int> ordering)
        {
            base.initGame(myPlayerID, cardPrefab, playCard, finishTurn, ordering);

            if (isMyTurn) StartCoroutine(sleep(5f, null, (nothing) => makeTurn()));
        }
    }
}
