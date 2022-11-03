using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Types;

namespace Players
{
    public class LocalPlayer : Player
    {
        [SerializeField] private Button turnFinishedButton;
        [SerializeField] private TextMeshProUGUI turnText;
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
            if (!active) return;
            if (isMyTurn) turnText.text = "Du bist am Zug!";
            else turnText.text = "Dein Gegner ist am Zug!";
            var scaleFactor = Hand.GetComponentInParent<Canvas>().scaleFactor;
            var handCards = Hand.GetHandCards();
            foreach (var card in handCards) card.Hidden = false;

            
            var imColor = turnFinishedButton.GetComponent<Image>().color;
            var textColor = turnFinishedButton.GetComponentInChildren<TextMeshProUGUI>().color;
            float alpha;
            if (turnFinishedButton.enabled)
            {
                alpha = 1f;
            }
            else
            {
                alpha = 0.4f;
            }
            imColor.a = alpha;
            textColor.a = alpha;
            turnFinishedButton.GetComponent<Image>().color = imColor;
            turnFinishedButton.GetComponentInChildren<TextMeshProUGUI>().color = textColor;

            if (isMyTurn)
            {
                if (turnFinishAllowed) turnFinishedButton.enabled = true;
               
                else turnFinishedButton.enabled = false;
                
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
                                foreach (var pile in discardPiles)
                                {
                                    pile.SetEnabled();
                                }
                                dragObject = handCard;
                            }
                        }
                        if(dragObject != null)
                        {
                            oldSiblingIndex = dragObject.transform.GetSiblingIndex();
                            // dragObject.transform.SetParent(Hand.GetComponentInParent<Canvas>().transform);
                        }

                    }
                    else if (touch.phase == TouchPhase.Moved && dragObject != null)
                    {
                        dragObject.GetComponent<RectTransform>().position += new Vector3(touch.deltaPosition.x, touch.deltaPosition.y, 0);
                        dragObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        dragObject.isDragging = true;
                        int indexInHand = Hand.GetCardIndexFromPosition(dragObject.transform);
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
                        foreach (var pile in discardPiles)
                        {
                            pile.SetDisabled();
                        }
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
                                playCardCallback(new DiscardActionParameters(PlayerID, pile.Type, dragObject.Number));
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
                turnFinishedButton.enabled = false;
            }
        }

        public override void initGame(PlayerID myPlayerID, GameObject cardPrefab, Action<DiscardActionParameters> playCard, Action finishTurn, List<int> ordering)
        {
            base.initGame(myPlayerID, cardPrefab, playCard, finishTurn, ordering);
            turnFinishedButton.onClick.AddListener(() =>
            {
                finishTurn();
            });
        }
  

    }
}
