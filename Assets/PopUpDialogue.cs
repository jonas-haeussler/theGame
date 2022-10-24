using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets
{
    public class PopUpDialogue : MonoBehaviour
    {
        [SerializeField] private Button posButton;
        [SerializeField] private Button negButton;
        [SerializeField] private TextMeshProUGUI text;
        // Start is called before the first frame update
        void Start()
        {
            negButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OpenDialogue(string text, string posButtonText, string negButtonText, Action posCallback)
        {
            posButton.onClick.RemoveAllListeners();
            this.text.text = text;
            posButton.GetComponentInChildren<TextMeshProUGUI>().text = posButtonText;
            negButton.GetComponentInChildren<TextMeshProUGUI>().text = negButtonText;
            posButton.onClick.AddListener(() =>
            {
                posCallback.Invoke();
                gameObject.SetActive(false);
            });
            gameObject.SetActive(true);
        }
    }
}
