using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Types;


namespace Core
{
    public class DiscardPile : MonoBehaviour
    {

        public GameObject Overlay;

        public List<Card> Cards;
        public PileType Type;
        // Start is called before the first frame update
        void Start()
        {
            SetDisabled();
        }

        // Update is called once per frame
        void Update()
        {
            Overlay.transform.SetSiblingIndex(transform.childCount - 1);
        }
        public void SetEnabled()
        {
            Color color;
            foreach(var go in Overlay.GetComponentsInChildren<Image>())
            {
                color = go.color;
                color.a = 0.9f;
                go.color = color;
            }
            color = Overlay.GetComponent<Image>().color;
            color.a = 0.3f;
            Overlay.GetComponent<Image>().color = color;
        }

        public void SetDisabled()
        {
            Color color;
            foreach (var go in Overlay.GetComponentsInChildren<Image>())
            {
                color = go.color;
                color.a = 0.4f;
                go.color = color;
            }
            color = Overlay.GetComponent<Image>().color;
            color.a = 0.1f;
            Overlay.GetComponent<Image>().color = color;
        }

        public void InitPile(GameObject cardPrefab)
        {
            var go = GameObject.Instantiate(cardPrefab, transform);
            var card = go.GetComponent<Card>();
            Cards = new List<Card> { card };
            if (Type.Equals(PileType.Player1Ascending) || Type.Equals(PileType.Player2Ascending))
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
