using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

namespace Assets
{
    public abstract class Player : MonoBehaviour
    {
        [SerializeField] private DrawPile DrawPile;
        [SerializeField] internal Hand Hand;

        [SerializeField] internal DiscardPile AscendingPile;
        [SerializeField] internal DiscardPile DescendingPile;
        

        protected bool isMyTurn;
        protected bool turnFinishAllowed;
        protected bool enemyDiscard;

        protected Action<Game.DiscardActionParameters> playCardCallback;
        protected Action finishTurnCallback;

        protected List<DiscardPile> discardPiles;

        internal Game.PlayerID PlayerID;

        protected float cardPlayAnimTime;

        internal bool active;


        internal virtual void initGame(Game.PlayerID myPlayerID, GameObject cardPrefab, Action<Game.DiscardActionParameters> playCard, Action finishTurn, List<int> ordering)
        {
            this.PlayerID = myPlayerID;

            if (myPlayerID.Equals(Game.PlayerID.Player1))
            {
                DescendingPile.Type = Game.PileType.Player1Descending;
                AscendingPile.Type = Game.PileType.Player1Ascending;
            }
            else
            {
                DescendingPile.Type = Game.PileType.Player2Descending;
                AscendingPile.Type = Game.PileType.Player2Ascending;
            }
            discardPiles = FindObjectsOfType<DiscardPile>().ToList();
            foreach (var pile in discardPiles)
            {
                pile.InitPile(cardPrefab);
            }

            DrawPile.InitPile(cardPrefab, ordering);
            Hand.initHand(cardPrefab.GetComponent<RectTransform>().sizeDelta.x);
            foreach(var card in DrawPile.Cards)
            {
                card.color = PlayerID.Equals(Game.PlayerID.Player1) ? new Color(1, 0.8f, 0) : new Color(0.7f, 0.7f, 0.7f);
            }
            if (myPlayerID.Equals(Game.PlayerID.Player1))
            {
                isMyTurn = true;
            }
            else
            {
                isMyTurn = false;
            }

            playCardCallback = playCard;
            finishTurnCallback = finishTurn;
            gameObject.SetActive(true);
            active = true;
            drawCards(true);
        }

        private void drawCards(bool fill)
        {
            var handCards = Hand.GetHandCards();
            int drawAmount = fill ? 6 - handCards.Count : 2;
            Hand.OnHandCardsChange(handCards.Count + drawAmount);
            IEnumerator drawCardsCoRoutine(Action callback)
            {
                
                for (int i = 0; i < drawAmount; i++)
                {

                    var newCard = DrawPile.DrawCard();
                    var newPosition = Hand.GetNewCardPosition(drawAmount - i);
                    newCard.transform.SetParent(Hand.gameObject.transform);
                    newCard.GetComponent<CustomAnimator>().AddAnimation(newPosition, .5f, true);
                    newCard.enabled = true;
                    Hand.UpdateRotations(drawAmount - i);
                    // Hand.OnHandCardsChange();
                    // Hand.handCards.Insert(0, newCard);
                    yield return new WaitForSeconds(.5f);
                }
                callback();
            }

            StartCoroutine(drawCardsCoRoutine(() => Hand.OnHandCardsChange()));

        }

        protected bool isValidTurn(int pileTopNumber, int cardNumber, Game.PileType pileType)
        {
            var ownAscending = PlayerID.Equals(Game.PlayerID.Player1) ? Game.PileType.Player1Ascending : Game.PileType.Player2Ascending;
            var ownDescending = PlayerID.Equals(Game.PlayerID.Player1) ? Game.PileType.Player1Descending : Game.PileType.Player2Descending;
            var enemyAscending = PlayerID.Equals(Game.PlayerID.Player1) ? Game.PileType.Player2Ascending : Game.PileType.Player1Ascending;
            var enemyDescending = PlayerID.Equals(Game.PlayerID.Player1) ? Game.PileType.Player2Descending : Game.PileType.Player1Descending;
            if ((pileType.Equals(enemyAscending) || pileType.Equals(enemyDescending)) && !enemyDiscard)
            {
                return false;
            }
            if (pileType.Equals(enemyAscending) && pileTopNumber > cardNumber) return true;
            if (pileType.Equals(enemyDescending) && pileTopNumber < cardNumber) return true;
            if (pileType.Equals(ownAscending) && pileTopNumber < cardNumber) return true;
            if (pileType.Equals(ownDescending) && pileTopNumber > cardNumber) return true;
            if (pileType.Equals(ownAscending) && pileTopNumber - 10 == cardNumber) return true;
            if (pileType.Equals(ownDescending) && pileTopNumber == cardNumber - 10) return true;
            return false;
        }

        public virtual void TurnChanged(Game.Turn currentTurn)
        {
            if (isMyTurn && !currentTurn.PlayerID.Equals(PlayerID))
            {
                drawCards(!enemyDiscard);
            }
            isMyTurn = currentTurn.PlayerID.Equals(PlayerID);
            var handCards = Hand.GetHandCards();
            if (isMyTurn) {
                var playerIdNumber = PlayerID.Equals(Game.PlayerID.Player1) ? 1 : 2;
                Debug.Log($"Handling turn change for Player {playerIdNumber}");
                foreach (var discardAction in currentTurn.DiscardActionParameters)
                { 
                    var card = handCards.Find(handCard => handCard.Number == discardAction.CardNumber);
                    if (card is null) continue;
                    var pile = discardPiles.Find(pile => discardAction.PileType.Equals(pile.Type));
                    card.GetComponent<CustomAnimator>().AddAnimation(pile.transform.position, this.cardPlayAnimTime, false);
                    card.transform.SetParent(pile.transform);
                    card.Hidden = false;
                    card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    Hand.OnHandCardsChange();
                }
            }
            turnFinishAllowed = currentTurn.DiscardActionParameters.Count >= 2;
            enemyDiscard = currentTurn.enemyDiscard;
        }

        public bool HasTurnLeft()
        {
            if (turnFinishAllowed) return true;
            foreach (var card in Hand.GetHandCards())
            {
                foreach (var pile in discardPiles)
                {
                    if (isValidTurn(pile.Cards[pile.Cards.Count - 1].Number, card.Number, pile.Type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasCardsLeft()
        {
            return DrawPile.Cards.Count > 0 || Hand.GetHandCards().Count > 0;
        }

    }
}
