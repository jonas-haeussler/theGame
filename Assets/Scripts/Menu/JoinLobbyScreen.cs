using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Menu
{
    public class JoinLobbyScreen : OnlineMenuScreen
    {
        
        [SerializeField] private Button lobbyItemPrefab;
        internal GameObject spinner;
        [SerializeField] private GameObject lobbyBorder;
        [SerializeField] private GameObject noItemsFoundText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button privateJoinButton;

        internal Action<Lobby> onLobbyJoined;

        internal Action onWrongCodeInserted;


        private List<GameObject> lobbiesUI;

        private JoinAllocation allocation;

        private void Awake()
        {
            lobbiesUI = new List<GameObject>();
            privateJoinButton.onClick.AddListener(async () =>
            {
                try 
                {
                    AuthenticationService.Instance.SignOut();
                    await SignInAnonymouslyAsync();
                    var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(codeInput.text);
                    await joinRelay(lobby);
                    startClient();
                    onLobbyJoined(lobby);
                    codeInput.text = "";
                }
                catch (LobbyServiceException e)
                {
                    if (e.ErrorCode == 16000 || e.ErrorCode == 16001) onWrongCodeInserted.Invoke();
                }
            });

        }
        protected async override void onInitFinished()
        {
            heading.text = "Spiel beitreten";
            updateLobbyList();
            refreshButton.onClick.AddListener(() =>
            {
                IEnumerator rotateButton()
                {
                    var rotZ = refreshButton.transform.localEulerAngles.z;
                    var rotX = refreshButton.transform.localEulerAngles.x;
                    var rotY = refreshButton.transform.localEulerAngles.y;
                    while (rotZ < 360)
                    {
                        rotZ += 15;
                        refreshButton.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
                        yield return new WaitForFixedUpdate();
                    }
                }
                StartCoroutine(rotateButton());
                updateLobbyList();

            });
            if(ProcessDeepLinkMngr.Instance.joinCode != null && !ProcessDeepLinkMngr.Instance.joinCode.Equals(""))
            {
                string joinCode = ProcessDeepLinkMngr.Instance.joinCode;
                foreach(Transform child in transform)
                {
                    transform.gameObject.SetActive(false);
                }
                ProcessDeepLinkMngr.Instance.joinCode = "";
                try
                {
                    AuthenticationService.Instance.SignOut();
                    await SignInAnonymouslyAsync();
                    var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
                    await joinRelay(lobby);
                    startClient();
                    onLobbyJoined(lobby);
                } 
                catch(LobbyServiceException e)
                {
                    if (e.ErrorCode == 16000 || e.ErrorCode == 16001) ProcessDeepLinkMngr.Instance.onWrongLink.Invoke();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        internal async void updateLobbyList()
        {
            noItemsFoundText.SetActive(false);
            foreach (GameObject child in lobbiesUI)
            {
                GameObject.Destroy(child);
            }
            lobbiesUI.Clear();
            spinner.SetActive(true);
            AuthenticationService.Instance.SignOut();
            await SignInAnonymouslyAsync();
            var lobbies = await QueryForLobbies();
            spinner.SetActive(false);
            foreach(Lobby lobby in lobbies)
            {
                var lobbyItem = GameObject.Instantiate(lobbyItemPrefab, lobbyBorder.transform);
                lobbyItem.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;
                lobbyItem.onClick.AddListener(async () =>
                {
                    try
                    {
                        var players = (await LobbyService.Instance.GetLobbyAsync(lobby.Id)).Players;
                        foreach(var player in players)
                        {
                            if (player.Id.Equals(AuthenticationService.Instance.PlayerId)) return;
                        }
                        await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
                        await joinRelay(lobby);
                        startClient();
                        onLobbyJoined(lobby);
                        
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.Log(e);
                    }
                });
                lobbiesUI.Add(lobbyItem.gameObject);
            }
            if (lobbiesUI.Count == 0)
            {
                noItemsFoundText.SetActive(true);
            }
            else
            {
                noItemsFoundText.SetActive(false);
            }


        }

        private async Task joinRelay(Lobby lobby)
        {
            try
            {
                DataObject relayJoinCode;
                lobby.Data.TryGetValue("JoinCode", out relayJoinCode);
                Debug.Log($"JoinCode received from Lobby: {relayJoinCode.Value}");
                allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode.Value);
                Debug.Log($"Allocation joined: {allocation.AllocationId}");
            } catch(RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
        private void startClient()
        {

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.RelayServer.IpV4,
                                                                                     (ushort) allocation.RelayServer.Port,
                                                                                     allocation.AllocationIdBytes,
                                                                                     allocation.Key,
                                                                                     allocation.ConnectionData,
                                                                                     allocation.HostConnectionData);
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started");
        }

        private async Task<List<Lobby>> QueryForLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Count = 25;

                // Filter for open lobbies only
                options.Filters = new List<QueryFilter>()
                {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
                };

                // Order by newest lobbies first
                options.Order = new List<QueryOrder>()
                {
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created)
                };      

                QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
                return lobbies.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            return null;
        }
    }

}