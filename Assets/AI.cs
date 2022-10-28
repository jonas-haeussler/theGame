
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets
{
    public class AI : RemotePlayer
    {

        private Game.PileType myAscendingPileType;
        private int currentMyAscendingPileTop;
        private Game.PileType myDescendingPileType;
        private int currentMyDescendingPileTop;
        private Game.PileType enemyAscendingPileType;
        private int currentEnemyAscendingPileTop;
        private Game.PileType enemyDescendingPileType;
        private int currentEnemyDescendingPileTop;

        private Queue<Game.DiscardActionParameters> turn;

        private IEnumerator sleep(float sleepAmount, Game.DiscardActionParameters parameters, Action<Game.DiscardActionParameters> callback)
        {
            yield return new WaitForSeconds(sleepAmount);
            callback(parameters);
        }

        private bool findCombo(List<int> currentHand,
                               out int playCard,
                               out Game.PileType pileType)
        {
            playCard = -1;
            pileType = Game.PileType.Player1Ascending;
            foreach (var card in currentHand)
            {
                if (currentMyAscendingPileTop - card == 10)
                {
                    playCard = card;
                    pileType = myAscendingPileType;
                    return true;
                }
                if (card - currentMyDescendingPileTop == 10)
                {
                    playCard = card;
                    pileType = myDescendingPileType;
                    return true;
                }
            }
            return false;
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
                        
                    }
                }
            }
            return closestTurn;
        }

        private Queue<Game.DiscardActionParameters> findCardPlays()
        {
            List<int> currentHand = new List<int>();
            // myAscendingPileType = Game.PileType.Player1Ascending;
            // currentMyAscendingPileTop = int.MaxValue;
            // myDescendingPileType = Game.PileType.Player1Ascending;
            // currentMyDescendingPileTop = 0;
            // enemyAscendingPileType = Game.PileType.Player1Ascending;
            // currentEnemyAscendingPileTop = 0;
            // enemyDescendingPileType = Game.PileType.Player1Ascending;
            // currentEnemyDescendingPileTop = int.MaxValue;

            foreach(var card in Hand.GetHandCards())
            {
                currentHand.Add(card.Number);
            }
            foreach(var pile in discardPiles)
            {
                if(PlayerID.Equals(Game.PlayerID.Player1))
                {
                    switch(pile.Type)
                    {
                        case Game.PileType.Player1Ascending: 
                            myAscendingPileType = pile.Type;
                            currentMyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player1Descending: 
                            myDescendingPileType = pile.Type;
                            currentMyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player2Ascending: 
                            enemyAscendingPileType = pile.Type;
                            currentEnemyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player2Descending: 
                            enemyDescendingPileType = pile.Type;
                            currentEnemyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                    }
                }
                else if(PlayerID.Equals(Game.PlayerID.Player2))
                {
                    switch (pile.Type)
                    {
                        case Game.PileType.Player2Ascending:
                            myAscendingPileType = pile.Type;
                            currentMyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player2Descending:
                            myDescendingPileType = pile.Type;
                            currentMyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player1Ascending:
                            enemyAscendingPileType = pile.Type;
                            currentEnemyAscendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                        case Game.PileType.Player1Descending:
                            enemyDescendingPileType = pile.Type;
                            currentEnemyDescendingPileTop = pile.Cards[pile.Cards.Count - 1].Number;
                            break;
                    }
                }
            }

            var cardPlays = new Queue<Game.DiscardActionParameters>();
            bool enemyDiscardAllowed = enemyDiscard;
            while (cardPlays.Count < 2)
            {
                int playCard;
                Game.PileType pileType;
                if (findCombo(currentHand,
                             out playCard,
                             out pileType))
                {
                    var parameters = new Game.DiscardActionParameters(PlayerID, pileType, playCard);
                    if (pileType.Equals(myAscendingPileType)) currentMyAscendingPileTop = playCard;
                    else if (pileType.Equals(myDescendingPileType)) currentMyDescendingPileTop = playCard;
                    currentHand.Remove(playCard);
                    cardPlays.Enqueue(parameters);
                    continue;
                }
                else if(findClosestCardPlay(currentHand,
                                             enemyDiscardAllowed,
                                             out playCard,
                                             out pileType))
                {
                    if(pileType.Equals(myAscendingPileType))
                    {
                        foreach(int card in currentHand)
                        {
                            if(card - playCard == 10)
                            {
                                // Play each card below that card
                                foreach(int otherCard in currentHand)
                                {
                                    if(otherCard < card)
                                    {
                                        var otherCardParameters = new Game.DiscardActionParameters(PlayerID, pileType, otherCard);
                                        currentHand.Remove(otherCard);
                                        cardPlays.Enqueue(otherCardParameters);
                                    }
                                }
                                // Then play the card itself
                                var cardParameters = new Game.DiscardActionParameters(PlayerID, pileType, card);
                                currentHand.Remove(card);
                                cardPlays.Enqueue(cardParameters);
                            }
                        }
                    }
                    else if(pileType.Equals(myDescendingPileType))
                    {
                        foreach (int card in currentHand)
                        {
                            if (playCard - card == 10)
                            {
                                // Play each card above that card
                                foreach (int otherCard in currentHand)
                                {
                                    if (otherCard > card)
                                    {
                                        var otherCardParameters = new Game.DiscardActionParameters(PlayerID, pileType, otherCard);
                                        currentHand.Remove(otherCard);
                                        cardPlays.Enqueue(otherCardParameters);
                                    }
                                }
                                // Then play the card itself
                                var cardParameters = new Game.DiscardActionParameters(PlayerID, pileType, card);
                                currentHand.Remove(card);
                                cardPlays.Enqueue(cardParameters);
                            }
                        }
                    }
                    // Play the card which is closest
                    var playCardParameters = new Game.DiscardActionParameters(PlayerID, pileType, playCard);
                    currentHand.Remove(playCard);
                    cardPlays.Enqueue(playCardParameters);
                    if (pileType.Equals(myAscendingPileType)) currentMyAscendingPileTop = playCard;
                    else if (pileType.Equals(myDescendingPileType)) currentMyDescendingPileTop = playCard;
                    else if (pileType.Equals(enemyAscendingPileType)) currentEnemyAscendingPileTop = playCard;
                    else if (pileType.Equals(enemyDescendingPileType)) currentEnemyDescendingPileTop = playCard;
                    if (pileType.Equals(enemyAscendingPileType) || pileType.Equals(enemyDescendingPileType))
                    {
                        enemyDiscardAllowed = false;
                    }
                }
            }
            return cardPlays;
            
        }

        private bool findClosestCardPlay(List<int> currentHand,
                                        bool enemyDiscard,
                                        out int playCard,
                                        out Game.PileType pileType)
        {
            int distance = int.MaxValue;
            playCard = -1;
            pileType = Game.PileType.Player1Ascending;
            foreach (var card in currentHand)
            {

                if (card > currentMyAscendingPileTop && distance > Math.Abs(card - currentMyAscendingPileTop))
                {
                    distance = Math.Abs(card - currentMyAscendingPileTop);
                    playCard = card;
                    pileType = myAscendingPileType;
                }
                if (card < currentMyDescendingPileTop && distance > Math.Abs(card - currentMyDescendingPileTop))
                {
                    distance = Math.Abs(card - currentMyDescendingPileTop);
                    playCard = card;
                    pileType = myDescendingPileType;
                }
                if (card < currentEnemyAscendingPileTop && distance > Math.Abs(card - currentEnemyAscendingPileTop) && enemyDiscard)
                {
                    distance = Math.Abs(card - currentEnemyAscendingPileTop);
                    playCard = card;
                    pileType = enemyAscendingPileType;
                }
                if (card > currentEnemyDescendingPileTop && distance > Math.Abs(card - currentEnemyDescendingPileTop) && enemyDiscard)
                {
                    distance = Math.Abs(card - currentEnemyDescendingPileTop);
                    playCard = card;
                    pileType = enemyDescendingPileType;
                }
            }
            if (playCard != -1) return true;
            return false;
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
                    makeTurn();
                }
            }
        }

        private void makeTurn()
        {

            if (turn == null || turn.Count == 0)
            {
                turn = findCardPlays();
            }
            StartCoroutine(sleep(1f, turn.Dequeue(), playCardCallback));
        }
        internal override void initGame(Game.PlayerID myPlayerID, GameObject cardPrefab, Action<Game.DiscardActionParameters> playCard, Action finishTurn, List<int> ordering)
        {
            base.initGame(myPlayerID, cardPrefab, playCard, finishTurn, ordering);

            if (isMyTurn) makeTurn();
        }
    }
}
