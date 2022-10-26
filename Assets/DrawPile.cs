using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace Assets
{
    public class DrawPile : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countText;
        internal List<Card> Cards;
        private void Start()
        {

        }

        private void Update()
        {
            
        }

        public void InitPile(GameObject cardPrefab, List<int> cardOrdering)
        {

            Cards = new List<Card>();
            int counter = 0;
            foreach(int number in cardOrdering)
            {
                var go = GameObject.Instantiate(cardPrefab, transform);
                go.transform.position += new Vector3(go.GetComponentInParent<Canvas>().scaleFactor * counter * 0.2f, go.GetComponentInParent<Canvas>().scaleFactor * counter * 0.2f, 0);
                var card = go.GetComponent<Card>();
                card.Number = number;
                card.Hidden = true;
                Cards.Add(card);
                counter++;
            }
            countText.text = Cards.Count.ToString();
        }

        public Card DrawCard()
        {
            Card top = Cards[0];
            Cards.RemoveAt(0);
            countText.text = Cards.Count.ToString();
            countText.transform.SetAsLastSibling();
            return top;
        }
    }
}
