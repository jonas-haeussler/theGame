using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

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
        [SerializeField] private TMP_InputField lobbyCodeText;
        [SerializeField] private ShareButton shareButton;

        internal LobbyState lobbyState;


        internal Lobby lobby;

        private Allocation allocation;

        private float pollTime;

        private void Awake()
        {
            heading.text = "Neues Spiel erstellen";

            createButton.onClick.AddListener(async () =>
            {
                IEnumerator animateText()
                {
                    string prefix = "Lobby eröffnen";
                    while (true)
                    {
                        string text = createButton.GetComponentInChildren<TextMeshProUGUI>().text;
                        if (text.Equals("Erstellen"))
                            text = prefix + ".";
                        else if (text.Equals(prefix + "."))
                            text = prefix + "..";
                        else if (text.Equals(prefix + ".."))
                            text = prefix + "...";
                        else if (text.Equals(prefix + "..."))
                            text = prefix + ".";
                        createButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                if (lobbyName.text.Equals(""))
                {
                    Debug.Log("Lobbyname empty");
                    return;
                }
                createButton.interactable = false;
                StartCoroutine(animateText());
                if(!AuthenticationService.Instance.IsSignedIn)
                    await SignInAnonymouslyAsync();
                int maxPlayers = 2;
                CreateLobbyOptions options = new CreateLobbyOptions();
                options.IsPrivate = visibilityToggle.isPrivate;
                lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName.text, maxPlayers, options);
                await addRelayInfo(lobby.Id);
                // var dtlsEndpoint = allocation.ServerEndpoints.Find(e => e.ConnectionType == "dtls");
                // string ipv4address = dtlsEndpoint.Host;
                // int port = dtlsEndpoint.Port;

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.RelayServer.IpV4,
                                                                                          (ushort)allocation.RelayServer.Port,
                                                                                         allocation.AllocationIdBytes,
                                                                                         allocation.Key,
                                                                                         allocation.ConnectionData);
                NetworkManager.Singleton.StartHost();
                StopAllCoroutines();
                Debug.Log("Host started");

                shareButton.joinCode = lobby.LobbyCode;
                lobbyState = LobbyState.waitingForClient;
                createButton.GetComponentInChildren<TextMeshProUGUI>().text = "Erstellen";
                createButton.interactable = true;
                lobbyCodeText.text = lobby.LobbyCode;
            });
            gamestartButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

            });


        }

        protected override void onInitFinished()
        {
            return;
        }

        

        // Update is called once per frame
        async void Update()
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
                    lobbyCodeText.gameObject.SetActive(false);
                    break;
                case LobbyState.waitingForClient:
                    heading.text = "Warten auf Spieler für " + lobbyName.text;
                    createButton.gameObject.SetActive(false);
                    visibilityToggle.gameObject.SetActive(false);
                    lobbyName.gameObject.SetActive(false);
                    gamestartButton.gameObject.SetActive(false);
                    spinner.SetActive(true);
                    lobbyCodeText.gameObject.SetActive(true);
                    if (pollTime > 15)
                    {
                        pollTime = 0;
                        await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                        Debug.Log($"Sending Heartbeat for lobby: {lobby.Id}"); 
                    }
                    pollTime += Time.deltaTime;

                    break;
                case LobbyState.waitingForHost:
                    heading.text = "Warten auf Host für Spielstart";
                    createButton.gameObject.SetActive(false);
                    visibilityToggle.gameObject.SetActive(false);
                    lobbyName.gameObject.SetActive(false);
                    gamestartButton.gameObject.SetActive(false);
                    spinner.SetActive(true);
                    lobbyCodeText.gameObject.SetActive(false);
                    break;
                case LobbyState.ready:
                    heading.text = "Spieler gefunden!";
                    createButton.gameObject.SetActive(false);
                    visibilityToggle.gameObject.SetActive(false);
                    lobbyName.gameObject.SetActive(false);
                    gamestartButton.gameObject.SetActive(true);
                    spinner.SetActive(false);
                    lobbyCodeText.gameObject.SetActive(false);
                    break;
            }

        }

        private void OnDisable()
        {
            createButton.GetComponentInChildren<TextMeshProUGUI>().text = "Erstellen";
        }

        private async Task addRelayInfo(string lobbyId)
        {
            try
            {
                // Request list of valid regions
                var regionList = await RelayService.Instance.ListRegionsAsync();

                // pick a region from the list
                var targetRegion = regionList[1].Id;

                // Request an allocation to the Relay service
                // with a maximum of 5 peer connections, for a maximum of 6 players.
                var relayMaxConnections = 2;
                allocation = await Relay.Instance.CreateAllocationAsync(relayMaxConnections, targetRegion);
                Debug.Log($"Allocation created: {allocation.AllocationId}");


                // Request the join code to the Relay service
                var joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
                UpdateLobbyOptions options = new UpdateLobbyOptions();

                //Ensure you sign-in before calling Authentication Instance
                //See IAuthenticationService interface
                options.HostId = AuthenticationService.Instance.PlayerId;

                options.Data = new Dictionary<string, DataObject>()
                   {
                       {
                           "JoinCode", new DataObject(
                               visibility: DataObject.VisibilityOptions.Public,
                               value: joinCode)
                       }
                   };

                var lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
                Debug.Log($"JoinCode added to Lobby: {joinCode}");

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

        }
        public async void leaveLobby()
        {
            try
            {
                if (lobby != null)
                {
                    if (lobbyState.Equals(LobbyState.waitingForClient) || lobbyState.Equals(LobbyState.ready))
                    {
                        await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
                    }
                    else
                    {
                        
                        string playerId = AuthenticationService.Instance.PlayerId;
                        await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);
                        Debug.Log($"Leaving lobby as player {playerId}");
                    }
                }
            } catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
            if(AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();
            lobbyState = LobbyState.create;

        }
        
    }
}
