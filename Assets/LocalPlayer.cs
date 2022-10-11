using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class LocalPlayer : Player
    {
        [SerializeField] private Button TurnFinishedButton;
        private Card dragObject;

        private int oldSiblingIndex;

        // Start is called before the first frame update
        void Start()
        {
            cardPlayAnimTime = 0.1f;
        }

        // Update is called once per frame
        protected void Update()
        {
            if (!init) return;
            var scaleFactor = CardArea.GetComponentInParent<Canvas>().scaleFactor;
            var handCards = Hand.GetHandCards();
            foreach (var card in handCards) card.Hidden = false;

            if (isMyTurn)
            {
                if (turnFinishAllowed) TurnFinishedButton.enabled = true;
               
                else TurnFinishedButton.enabled = false;
                
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began && dragObject == null)
                    {
                        foreach (var handCard in handCards)
                        {
                            var cardTransform = handCard.GetComponent<RectTransform>();
                            Vector2 scaledSize = cardTransform.sizeDelta * scaleFactor;
                            Rect boundingRect = new Rect(cardTransform.position.x - scaledSize.x / 2, 
                                                         cardTransform.position.y - scaledSize.y / 2, 
                                                         scaledSize.x, 
                                                         scaledSize.y);
                            var touchPosition = touch.position;
                            if (boundingRect.Contains(touchPosition))
                            {
                                dragObject = handCard;
                            }
                        }
                        if(dragObject != null)
                        {
                            oldSiblingIndex = dragObject.transform.GetSiblingIndex();
                            dragObject.transform.SetParent(CardArea.GetComponentInParent<Canvas>().transform);
                        }

                    }
                    else if (touch.phase == TouchPhase.Moved && dragObject != null)
                    {
                        dragObject.GetComponent<RectTransform>().position += new Vector3(touch.deltaPosition.x, touch.deltaPosition.y, 0);
                        dragObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        dragObject.isDragging = true;
                        int indexInHand = Hand.GetCardIndexFromPosition(dragObject.transform.position);
                        Debug.Log(indexInHand);
                        if (indexInHand != -1 && !Hand.GetHandCards()[indexInHand].Equals(dragObject)) 
                        {
                            dragObject.transform.SetParent(Hand.transform);
                            dragObject.transform.SetSiblingIndex(indexInHand);
                            oldSiblingIndex = indexInHand;
                            Hand.OnHandCardsChange();
                        }
                    }
                    else if (touch.phase == TouchPhase.Ended && dragObject != null)
                    {
                        bool filed = false;
                        foreach (var pile in discardPiles)
                        {
                            var difference = dragObject.transform.position - pile.transform.position;
                            if (Mathf.Abs(difference.x) < 30 * scaleFactor && 
                                Mathf.Abs(difference.y) < 30 * scaleFactor && 
                                isValidTurn(pile.Cards[pile.Cards.Count - 1].Number, dragObject.Number, pile.Type))
                            {
                                dragObject.transform.SetParent(Hand.transform);
                                dragObject.transform.SetSiblingIndex(oldSiblingIndex);
                                playCardCallback(new Game.DiscardActionParameters(PlayerID, pile.Type, dragObject.Number));
                                filed = true;
                                break;
                            }
                        }
                        dragObject.isDragging = false;
                        
                        if (!filed)
                        {
                            dragObject.transform.SetParent(Hand.transform);
                            dragObject.transform.SetSiblingIndex(oldSiblingIndex);
                            Hand.OnHandCardsChange();
                        }
                        dragObject = null;
                        Debug.Log("Touch ended");
                    }
                }
            }
            else
            {
                TurnFinishedButton.enabled = false;
            }
        }
        internal override void initGame(Game.PlayerID myPlayerID, GameObject cardPrefab, Action<Game.DiscardActionParameters> playCard, Action finishTurn)
        {
            base.initGame(myPlayerID, cardPrefab, playCard, finishTurn);
            TurnFinishedButton.onClick.AddListener(() =>
            {
                finishTurn();
            });
        }
  

    }
}
