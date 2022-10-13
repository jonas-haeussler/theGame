using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
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
                privateButton.GetComponent<Image>().color = new Color(0.63f, 0.63f, 0.63f); ;
                publicButton.GetComponent<Image>().color = new Color(0.39f, 0.39f, 0.39f);
            });
            privateButton.onClick.AddListener(() =>
            {
                isPrivate = true;
                publicButton.GetComponent<Image>().color = new Color(0.63f, 0.63f, 0.63f);
                privateButton.GetComponent<Image>().color = new Color(0.39f, 0.39f, 0.39f);
            });
        }
        // Start is called before the first frame update
        void Start()
        {
            isPrivate = true;
            publicButton.GetComponent<Image>().color = new Color(0.63f, 0.63f, 0.63f);
            privateButton.GetComponent<Image>().color = new Color(0.39f, 0.39f, 0.39f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

