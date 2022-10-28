using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    public class Card : MonoBehaviour
    {

        [SerializeField] private GameObject backSide;
        [SerializeField] private GameObject frontSide;
        public int Number;
        public bool Hidden;
        public bool isDragging;
        public Color color;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (color != null)
            {
                backSide.GetComponentInChildren<Image>().color = color;
                frontSide.GetComponentInChildren<Image>().color = color;
                backSide.GetComponentInChildren<TextMeshProUGUI>().color = color;
            }
            if (Hidden)
            {
                backSide.SetActive(true);
                frontSide.SetActive(false);
            }
            else
            {
                frontSide.SetActive(true);
                backSide.SetActive(false);
            }

            foreach (Transform child in transform)
            {
                var text = child.GetComponent<TextMeshProUGUI>();
                if (text == null) continue;
                if (Hidden) text.text = "";

                else text.text = Number.ToString();
            }
        }
    }
}
