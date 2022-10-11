using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets {
    public class Hand : MonoBehaviour
    {

        private List<Card> handCards;

        private float cardSize;

        public bool local;

        // Start is called before the first frame update
        void Start()
        {
            handCards = GetComponentsInChildren<Card>().ToList();
        }

        // Update is called once per frame
        void Update()
        {
            // handCards = GetComponentsInChildren<Card>().ToList();
            
        }

        public void initHand(float cardSize) 
        {
            // handCards = new List<Card>();
            this.cardSize = cardSize;
        }

        private float getRotation(int index, float maxIndex) 
        {
            if(maxIndex < 1) return 0; 
            float maxAngle = 20;
            if(maxIndex <= 2) {
                maxAngle = 5;
            } else if(maxIndex <= 4) {
                maxAngle = 10;
            }

            float interp = Mathf.Abs(index - maxIndex / 2) / (maxIndex / 2);
            float rotation = Mathf.Lerp(-2, -maxAngle, interp * interp) * Mathf.Sign(index - maxIndex / 2);
            return local ? rotation : -rotation;
        }

        private float getLowering(int index, float maxIndex) {
            float lowering = Mathf.Abs(index - maxIndex / 2);
            return local ? lowering * lowering : - lowering * lowering;

        }

        private float getCardAreaSize(float numCards)
        {
            float sizeMultiplier = .95f * numCards / 6;
            float cardAreaSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x * sizeMultiplier - cardSize;
            return cardAreaSize;
        }

        public void OnHandCardsChange(int numHandCards)
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                if (handCards[i].isDragging) continue;
                float cardAreaSize = getCardAreaSize(numHandCards);
                int denominator = numHandCards < 2 ? 2 : numHandCards - 1;
                float lowering = getLowering(i, numHandCards - 1) * cardAreaSize / 70;
                var newPosition = new Vector3(-cardAreaSize / 2 + cardAreaSize / denominator * i, - lowering, 0);
                handCards[i].GetComponent<CustomAnimator>().AddAnimation(newPosition, .2f, true);
                float newRot = getRotation(i, numHandCards - 1);
                handCards[i].transform.localRotation = Quaternion.Euler(0, 0, newRot);
                // HandCards[i].transform.localPosition = new Vector3(-cardAreaSize / 2 + cardAreaSize / (HandCards.Count - 1) * i, 0, 0);
            }
        }

        public void OnHandCardsChange()
        {
            handCards = GetComponentsInChildren<Card>().ToList();
            OnHandCardsChange(handCards.Count);
        }

        public void UpdateRotations(int drawAmount)
        {
            handCards = GetComponentsInChildren<Card>().ToList();
            for (int i = 0; i < handCards.Count; i++)
            {
                handCards[i].transform.localRotation = Quaternion.Euler(0, 0, getRotation(i, handCards.Count + drawAmount - 1));
            }
        }

        private Vector3 getCardPosition(int index, int amountOfCards)
        {
            handCards = GetComponentsInChildren<Card>().ToList();
            float cardAreaSize = getCardAreaSize(amountOfCards);
            int denominator = amountOfCards < 2 ? 2 : amountOfCards - 1;
            float lowering = getLowering(index, amountOfCards - 1) * cardAreaSize / 70;
            var newPosition = new Vector3(-cardAreaSize / 2 + cardAreaSize / denominator * index, -lowering, 0);
            return newPosition;
        }

        public Vector3 GetNewCardPosition(int drawAmount) 
        {
            return getCardPosition(handCards.Count, handCards.Count + drawAmount);
        }

        public int GetCardIndexFromPosition(Vector3 position) 
        {
            handCards = GetComponentsInChildren<Card>().ToList();
            for (int i = 0; i < handCards.Count; i++) 
            {
                Vector3 cardPos = transform.position + getCardPosition(i, handCards.Count);

                if (Mathf.Abs(position.x - cardPos.x) < 10 && Mathf.Abs(position.y - cardPos.y) < 50)
                    return i;
            }
            return -1;
        }
        public List<Card> GetHandCards()
        {
            handCards = GetComponentsInChildren<Card>().ToList();
            return handCards;
        }
    }
}
