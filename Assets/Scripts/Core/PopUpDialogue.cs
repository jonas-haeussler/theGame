using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    public class PopUpDialogue : MonoBehaviour
    {
        public static PopUpDialogue Instance;

        [SerializeField] private Button posButton;
        [SerializeField] private Button negButton;
        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }
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
            negButton.onClick.RemoveAllListeners();
            negButton.gameObject.SetActive(true);
            this.text.text = text;
            posButton.GetComponentInChildren<TextMeshProUGUI>().text = posButtonText;
            negButton.GetComponentInChildren<TextMeshProUGUI>().text = negButtonText;
            posButton.onClick.AddListener(() =>
            {
                posCallback.Invoke();
                gameObject.SetActive(false);
            });
            negButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            gameObject.SetActive(true);
        }

        public void OpenDialogue(string text, string posButtonText, string negButtonText, Action posCallback, Action negCallback)
        {
            posButton.onClick.RemoveAllListeners();
            negButton.onClick.RemoveAllListeners();
            negButton.gameObject.SetActive(true);
            this.text.text = text;
            posButton.GetComponentInChildren<TextMeshProUGUI>().text = posButtonText;
            negButton.GetComponentInChildren<TextMeshProUGUI>().text = negButtonText;
            posButton.onClick.AddListener(() =>
            {
                posCallback.Invoke();
                gameObject.SetActive(false);
            });
            negButton.onClick.AddListener(() => 
            {
                negCallback.Invoke();
                gameObject.SetActive(false);
            });
            gameObject.SetActive(true);
        }

        public void OpenDialogue(string text, string posButtonText)
        {
            posButton.onClick.RemoveAllListeners();
            this.text.text = text;
            posButton.GetComponentInChildren<TextMeshProUGUI>().text = posButtonText;
            negButton.gameObject.SetActive(false);
            posButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            gameObject.SetActive(true);
        }

        public void OpenDialogue(string text, string posButtonText, Action posCallback)
        {
            posButton.onClick.RemoveAllListeners();
            this.text.text = text;
            posButton.GetComponentInChildren<TextMeshProUGUI>().text = posButtonText;
            negButton.gameObject.SetActive(false);
            posButton.onClick.AddListener(() =>
            {
                posCallback.Invoke();
                gameObject.SetActive(false);
            });
            gameObject.SetActive(true);
        }
    }
}
