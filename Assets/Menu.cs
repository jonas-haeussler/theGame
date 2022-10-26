using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
namespace Assets 
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

        [SerializeField] private PopUpDialogue popUpDialogue;
        private JoinLobbyScreen joinLobbyScreenInst;

        private GameObject currentScreen;


        private void Awake()
        {
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
                            popUpDialogue.OpenDialogue("Willst du die Lobby wirklich schließen?", "Ja", "Nein", () =>
                            {
                                NetworkManager.Singleton.Shutdown();
                                lobbyScreen.leaveLobby();
                            }); 
                            break;
                        case LobbyScreen.LobbyState.waitingForHost:
                            popUpDialogue.OpenDialogue("Willst du die Lobby wirklich verlassen?", "Ja", "Nein", () =>
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

        }

        // Start is called before the first frame update
        private void Start()
        {
            NetworkManager.Singleton.GetComponent<NetworkConnection>().AddConnectCallback((clientId) =>
            {
                Debug.Log($"Client connected: {clientId}");
                if (NetworkManager.Singleton.IsHost)
                {
                    lobbyScreen.lobbyState = LobbyScreen.LobbyState.ready;
                }
            });
            NetworkManager.Singleton.GetComponent<NetworkConnection>().AddDisconnectCallback((clientId) =>
            {
                Debug.Log($"Client disconnected: {clientId}");
                
                if (NetworkManager.Singleton.IsHost)
                {
                    popUpDialogue.OpenDialogue("Dein Mitspieler hat die Lobby verlassen.", "OK", () =>
                    {
                        lobbyScreen.lobbyState = LobbyScreen.LobbyState.waitingForClient;
                    });
                }
                else
                {
                    popUpDialogue.OpenDialogue("Der Host hat die Lobby verlassen.", "OK", () =>
                    {
                        lobbyScreen.lobbyState = LobbyScreen.LobbyState.create;
                        currentScreen = createNewJoinLobbyInstance().gameObject;
                        onScreenUpdate();
                    });
                }
            });
            if(ProcessDeepLinkMngr.Instance.joinCode != null && !ProcessDeepLinkMngr.Instance.joinCode.Equals(""))
            {
                ProcessDeepLinkMngr.Instance.onWrongLink = () =>
                {
                    Debug.Log("Hier");
                    popUpDialogue.OpenDialogue("Eine Lobby mit diesem Code wurde nicht gefunden!", "OK", () =>
                    {
                        AuthenticationService.Instance.SignOut();
                        currentScreen = mainMenu.gameObject;
                        onScreenUpdate();
                        backButton.gameObject.SetActive(false);
                    });
                };
                currentScreen = createNewJoinLobbyInstance().gameObject;
                onScreenUpdate();
                backButton.gameObject.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(ProcessDeepLinkMngr.Instance.joinCode != null && !ProcessDeepLinkMngr.Instance.joinCode.Equals(""))
            {
                SceneManager.LoadScene("MenuScene");
            }
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
                popUpDialogue.OpenDialogue("Eine Lobby mit diesem Code wurde nicht gefunden!", "OK");
            };
            joinLobbyScreenInst.transform.SetAsFirstSibling();
            return joinLobbyScreenInst;
        }
    }
}
