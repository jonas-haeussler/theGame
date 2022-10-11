using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets
{
    public class DiscardPile : MonoBehaviour
    {
        internal List<Card> Cards;
        internal Game.PileType Type;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitPile(GameObject cardPrefab)
        {
            var go = GameObject.Instantiate(cardPrefab, transform);
            var card = go.GetComponent<Card>();
            Cards = new List<Card> { card };
            if (Type.Equals(Game.PileType.Player1Ascending) || Type.Equals(Game.PileType.Player2Ascending))
            {
                card.Number = 1;
            }
            else
            {
                card.Number = 60;
            }
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

    }
}
