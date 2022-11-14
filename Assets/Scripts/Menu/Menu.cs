using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using Networking;
using Core;

namespace Menu 
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button localButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject spinner;

        [SerializeField] private GameObject mainMenu;
        [SerializeField] private LobbyScreen lobbyScreen;
        [SerializeField] private JoinLobbyScreen joinLobbyScreenPrefab;

        private JoinLobbyScreen joinLobbyScreenInst;

        private GameObject currentScreen;


        private void Awake()
        {
            NetworkManager.Singleton.Shutdown();
            currentScreen = mainMenu.gameObject;

            localButton.onClick.AddListener(() =>
            {
               
                SceneManager.LoadScene("LocalGameScene");
            });

            createButton.onClick.AddListener(() => {

                currentScreen = lobbyScreen.gameObject;
                onScreenUpdate();
                backButton.gameObject.SetActive(true);
            });
            joinButton.onClick.AddListener(() => {
                currentScreen = createNewJoinLobbyInstance().gameObject;
                onScreenUpdate();
                backButton.gameObject.SetActive(true);
                // joinLobbyScreen.updateLobbyList();
            });
            backButton.onClick.AddListener(() =>
            {
                spinner.SetActive(false);
                if(currentScreen.Equals(lobbyScreen.gameObject))
                {
                    switch (lobbyScreen.lobbyState)
                    {
                        case LobbyScreen.LobbyState.create:
                            // lobbyScreen.leaveLobby();
                            currentScreen = mainMenu.gameObject;
                            onScreenUpdate();
                            backButton.gameObject.SetActive(false);
                            break;
                        case LobbyScreen.LobbyState.waitingForClient:
                        case LobbyScreen.LobbyState.ready:
                            PopUpDialogue.Instance.OpenDialogue("Willst du die Lobby wirklich schließen?", "Ja", "Nein", () =>
                            {
                                NetworkManager.Singleton.Shutdown();
                                lobbyScreen.leaveLobby();
                            }); 
                            break;
                        case LobbyScreen.LobbyState.waitingForHost:
                            PopUpDialogue.Instance.OpenDialogue("Willst du die Lobby wirklich verlassen?", "Ja", "Nein", () =>
                            {
                                NetworkManager.Singleton.Shutdown();
                                lobbyScreen.leaveLobby();
                                currentScreen = createNewJoinLobbyInstance().gameObject;
                                onScreenUpdate();
                            });
                            break;

                    }
                }
                else
                {
                    AuthenticationService.Instance.SignOut();
                    currentScreen = mainMenu.gameObject;
                    onScreenUpdate();
                    backButton.gameObject.SetActive(false);
                }
            });

            quitButton.onClick.AddListener(() =>
            {
                PopUpDialogue.Instance.OpenDialogue("Willst du das Spiel wirklich verlassen?", "Ja", "Nein", () => Application.Quit());
            });


        }

        // Start is called before the first frame update
        private void Start()
        {
            NetworkManager.Singleton.GetComponent<NetworkConnection>().ClearConnectCallbacks();
            NetworkManager.Singleton.GetComponent<NetworkConnection>().ClearDisconnectCallbacks();
            NetworkManager.Singleton.GetComponent<NetworkConnection>().AddConnectCallback(async (clientId) =>
            {
                Debug.Log($"Client connected: {clientId}");
                lobbyScreen.lobby = await LobbyService.Instance.GetLobbyAsync(lobbyScreen.lobby.Id);
                if (NetworkManager.Singleton.IsHost && lobbyScreen.lobby.Players.Count == 2)
                {
                    lobbyScreen.lobbyState = LobbyScreen.LobbyState.ready;
                }
            });
            NetworkManager.Singleton.GetComponent<NetworkConnection>().AddDisconnectCallback(async (clientId) =>
            {
                Debug.Log($"Client disconnected: {clientId}");
                await LobbyService.Instance.RemovePlayerAsync(lobbyScreen.lobby.Id, lobbyScreen.lobby.Players[(int)clientId].Id);
                if (NetworkManager.Singleton.IsHost)
                {
                    PopUpDialogue.Instance.OpenDialogue("Dein Mitspieler hat die Lobby verlassen.", "OK", () =>
                    {
                        lobbyScreen.lobbyState = LobbyScreen.LobbyState.waitingForClient;
                    });
                }
                else
                {
                    PopUpDialogue.Instance.OpenDialogue("Der Host hat die Lobby verlassen.", "OK", () =>
                    {
                        lobbyScreen.lobbyState = LobbyScreen.LobbyState.create;
                        currentScreen = createNewJoinLobbyInstance().gameObject;
                        onScreenUpdate();
                    });
                }
            });
            
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void onScreenUpdate()
        {
            mainMenu.gameObject.SetActive(false);
            lobbyScreen.gameObject.SetActive(false);
            if(joinLobbyScreenInst != null)
                joinLobbyScreenInst.gameObject.SetActive(false);
            currentScreen.gameObject.SetActive(true);
        }

        private JoinLobbyScreen createNewJoinLobbyInstance()
        {
            if (joinLobbyScreenInst != null)
            {
                GameObject.Destroy(joinLobbyScreenInst.gameObject);
            }
            joinLobbyScreenInst = GameObject.Instantiate(joinLobbyScreenPrefab, transform);
            joinLobbyScreenInst.spinner = spinner;
            joinLobbyScreenInst.onLobbyJoined = (lobby) =>
            {
                currentScreen = lobbyScreen.gameObject;
                lobbyScreen.lobby = lobby;
                lobbyScreen.lobbyState = LobbyScreen.LobbyState.waitingForHost;
                onScreenUpdate();
            };
            joinLobbyScreenInst.onWrongCodeInserted = () =>
            {
                PopUpDialogue.Instance.OpenDialogue("Eine Lobby mit diesem Code wurde nicht gefunden!", "OK");
            };
            joinLobbyScreenInst.transform.SetAsFirstSibling();
            return joinLobbyScreenInst;
        }
    }
}
