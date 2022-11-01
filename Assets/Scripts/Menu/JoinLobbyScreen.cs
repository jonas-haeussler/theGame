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
                    JoinAllocation joinAllocation = await joinRelay(lobby);
                    startClient(joinAllocation);
                    onLobbyJoined(lobby);
                    codeInput.text = "";
                }
                catch (LobbyServiceException e)
                {
                    if (e.ErrorCode == 16000 || e.ErrorCode == 16001) onWrongCodeInserted.Invoke();
                }
            });

        }
        protected override void onInitFinished()
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
                        JoinAllocation joinAllocation = await joinRelay(lobby);
                        startClient(joinAllocation);
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