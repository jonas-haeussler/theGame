using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menu
{
    public class VisibilityToggleGroup : MonoBehaviour
    {

        
        [SerializeField] private Button publicButton;
        [SerializeField] private Button privateButton;

        internal bool isPrivate;

        private void Awake()
        {
            publicButton.onClick.AddListener(() =>
            {
                isPrivate = false;
                privateButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                privateButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f);
                publicButton.GetComponent<Image>().color = new Color(1, 0.8f, 0, 1);
                publicButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.9f);
            });
            privateButton.onClick.AddListener(() =>
            {
                isPrivate = true;
                publicButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                publicButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f);
                privateButton.GetComponent<Image>().color = new Color(1, 0.8f, 0, 1);
                privateButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.9f);
            });
        }
        // Start is called before the first frame update
        void Start()
        {
            isPrivate = true;
            publicButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            publicButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f);
            privateButton.GetComponent<Image>().color = new Color(1, 0.8f, 0, 1);
            privateButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.9f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

