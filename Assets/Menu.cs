using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets 
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            createButton.onClick.AddListener(() => {

                NetworkManager.Singleton.StartHost();
            });
            joinButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartClient();
            });
        }

        // Start is called before the first frame update
        private void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
