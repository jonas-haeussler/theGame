using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Services.Relay.Models;
using TMPro;

namespace Menu
{
    public abstract class OnlineMenuScreen : MonoBehaviour
    {

        [SerializeField] protected TextMeshProUGUI heading;

        private JoinAllocation joinAllocation;

        protected abstract void onInitFinished();
        async protected virtual void Start()
        {
            if(UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log(UnityServices.State);
                SetupEvents();
            }
            onInitFinished();
                
        }

        // Setup authentication event handlers if desired
        private void SetupEvents()
        {
            AuthenticationService.Instance.SignedIn += () => {
                // Shows how to get a playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

                // Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

            };

            AuthenticationService.Instance.SignInFailed += (err) => {
                Debug.LogError(err);
            };

            AuthenticationService.Instance.SignedOut += () => {
                Debug.Log("Player signed out.");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session could not be refreshed and expired.");
            };
        }

        
        protected async Task SignInAnonymouslyAsync()
        {
            try
            {
                AuthenticationService.Instance.ClearSessionToken();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        protected async Task<JoinAllocation> joinRelay(Lobby lobby)
        {
            try
            {
                DataObject relayJoinCode;
                lobby.Data.TryGetValue("JoinCode", out relayJoinCode);
                Debug.Log($"JoinCode received from Lobby: {relayJoinCode.Value}");
                joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode.Value);
                Debug.Log($"Allocation joined: {joinAllocation.AllocationId}");
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
            return null;
        }
        protected void startClient(JoinAllocation joinAllocation)
        {

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinAllocation.RelayServer.IpV4,
                                                                                     (ushort)joinAllocation.RelayServer.Port,
                                                                                     joinAllocation.AllocationIdBytes,
                                                                                     joinAllocation.Key,
                                                                                     joinAllocation.ConnectionData,
                                                                                     joinAllocation.HostConnectionData);
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started");
        }
    }
}
