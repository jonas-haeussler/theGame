using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuScreen : MonoBehaviour
    {


        [SerializeField] private Button continueButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;

        public Action menuAction;
        // Start is called before the first frame update
        void Start()
        {
            continueButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            menuButton.onClick.AddListener(() =>
            {
                menuAction.Invoke();
            });
            quitButton.onClick.AddListener(() =>
            {

            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
