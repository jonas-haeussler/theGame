using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System;

namespace Assets
{
    public class LobbyScreen : OnlineMenuScreen
    {
        public enum LobbyState
        {
            create, waitingForClient, waitingForHost, ready
        }
        [SerializeField] private Button createButton;
        [SerializeField] private Button gamestartButton;
        [SerializeField] private TMP_InputField lobbyName;
        [SerializeField] private VisibilityToggleGroup visibilityToggle;
        [SerializeField] private GameObject spinner;

        internal LobbyState lobbyState;

        private Lobby lobby;

        private void Awake()
        {
            heading.text = "Neues Spiel erstellen";

            createButton.onClick.AddListener(async () =>
            {
                if (lobbyName.text.Equals(""))
                {
                    Debug.Log("Lobbyname empty");
                    return;
                }
                if(!AuthenticationService.Instance.IsAuthorized)
                    await SignInAnonymouslyAsync();
                int maxPlayers = 2;
                CreateLobbyOptions options = new CreateLobbyOptions();
                options.IsPrivate = visibilityToggle.isPrivate;
                lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName.text, maxPlayers, options);
                lobbyState = LobbyState.waitingForClient;
            });

        }
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();


        }

        // Update is called once per frame
        void Update()
        {
            switch(lobbyState)
            {
                case LobbyState.create:
                    heading.text = "Neues Spiel erstellen";
                    createButton.gameObject.SetActive(true);
                    visibilityToggle.gameObject.SetActive(true);
                    lobbyName.gameObject.SetActive(true);
                    gamestartButton.gameObject.SetActive(false);
                    spinner.SetActive(false);
                    break;
                case LobbyState.waitingForClient:
                    heading.text = "Warten auf Spieler für " + lobbyName.text;
                    createButton.gameObject.SetActive(false);
                    visibilityToggle.gameObject.SetActive(false);
                    lobbyName.gameObject.SetActive(false);
                    gamestartButton.gameObject.SetActive(false);
                    spinner.SetActive(true);
                    if (lobby.Players.Count == 2)
                        lobbyState = LobbyState.ready;
                    break;
                case LobbyState.waitingForHost:
                    break;
                case LobbyState.ready:
                    heading.text = "Spieler gefunden!";
                    createButton.gameObject.SetActive(false);
                    visibilityToggle.gameObject.SetActive(false);
                    lobbyName.gameObject.SetActive(false);
                    gamestartButton.gameObject.SetActive(true);
                    spinner.SetActive(false);
                    break;
            }

        }
        public async void leaveLobby()
        {
            if (lobbyState.Equals(LobbyState.waitingForClient))
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
            }
            else
            {
                try
                {
                    //Ensure you sign-in before calling Authentication Instance
                    //See IAuthenticationService interface
                    string playerId = AuthenticationService.Instance.PlayerId;
                    await LobbyService.Instance.RemovePlayerAsync("lobbyId", playerId);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
            lobbyState = LobbyState.create;

        }
        
    }
}
