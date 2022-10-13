using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

namespace Assets
{
    public class JoinLobbyScreen : OnlineMenuScreen
    {

        [SerializeField] private Button lobbyItemPrefab;
        [SerializeField] private GameObject spinner;
        [SerializeField] private GameObject lobbyBorder;
        [SerializeField] private GameObject noItemsFoundText;

        private List<GameObject> lobbiesUI;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            heading.text = "Spiel beitreten";
            lobbiesUI = new List<GameObject>();
            updateLobbyList();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private async void updateLobbyList()
        {
            noItemsFoundText.SetActive(false);
            foreach (GameObject child in lobbiesUI)
            {
                GameObject.Destroy(child);
            }
            spinner.SetActive(true);
            if (!AuthenticationService.Instance.IsAuthorized)
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
                        await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
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