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
        
        [SerializeField] protected GameObject CardArea;

        protected bool isMyTurn;
        protected bool turnFinishAllowed;
        protected bool enemyDiscard;

        protected Action<Game.DiscardActionParameters> playCardCallback;
        protected Action finishTurnCallback;

        protected List<DiscardPile> discardPiles;

        internal Game.PlayerID PlayerID;

        protected float cardPlayAnimTime;

        protected bool init;


        internal virtual void initGame(Game.PlayerID myPlayerID, GameObject cardPrefab, Action<Game.DiscardActionParameters> playCard, Action finishTurn)
        {
            discardPiles = FindObjectsOfType<DiscardPile>().ToList();
            foreach(var pile in discardPiles)
            {
                pile.InitPile(cardPrefab);
            }
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

            DrawPile.InitPile(cardPrefab);
            Hand.initHand(cardPrefab.GetComponent<RectTransform>().sizeDelta.x);
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
            init = true;
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
                    newCard.transform.SetParent(CardArea.transform);
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
            Debug.Log("Turn Changed!");
            if (isMyTurn && !currentTurn.PlayerID.Equals(PlayerID))
            {
                drawCards(!enemyDiscard);
            }
            isMyTurn = currentTurn.PlayerID.Equals(PlayerID);
            var handCards = Hand.GetHandCards();
            if (isMyTurn) {
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

    }
}
