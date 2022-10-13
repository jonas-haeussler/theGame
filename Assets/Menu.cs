using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Button backButton;

        [SerializeField] private MainMenuScreen mainMenu;
        [SerializeField] private LobbyScreen lobbyScreen;
        [SerializeField] private JoinLobbyScreen joinLobbyScreen;

        private MenuScreen currentScreen;


        private bool waitingForHost;
        private bool waitingForClient;

        private void Awake()
        {
            currentScreen = mainMenu;
            createButton.onClick.AddListener(() => {

                currentScreen = lobbyScreen;
                onScreenUpdate();
                backButton.gameObject.SetActive(true);
            });
            joinButton.onClick.AddListener(() => {
                currentScreen = joinLobbyScreen;
                onScreenUpdate();
                backButton.gameObject.SetActive(true);
            });
            backButton.onClick.AddListener(() =>
            {
                if(currentScreen.Equals(lobbyScreen))
                {
                    switch (lobbyScreen.lobbyState)
                    {
                        case LobbyScreen.LobbyState.create:
                            currentScreen = mainMenu;
                            onScreenUpdate();
                            backButton.gameObject.SetActive(false);
                            break;
                        case LobbyScreen.LobbyState.waitingForClient:
                            lobbyScreen.leaveLobby();
                            break;
                        case LobbyScreen.LobbyState.waitingForHost:
                            lobbyScreen.leaveLobby();
                            currentScreen = joinLobbyScreen;
                            onScreenUpdate();
                            break;
                    }
                }
                else
                {
                    currentScreen = mainMenu;
                    onScreenUpdate();
                    backButton.gameObject.SetActive(false);
                }
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

        private void onScreenUpdate()
        {
            mainMenu.gameObject.SetActive(false);
            lobbyScreen.gameObject.SetActive(false);
            joinLobbyScreen.gameObject.SetActive(false);
            currentScreen.gameObject.SetActive(true);
        }
    }
}
