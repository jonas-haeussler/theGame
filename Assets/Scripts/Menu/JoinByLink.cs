using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Networking;
using Core;

namespace Menu
{
    public class JoinByLink : OnlineMenuScreen
    {
        [SerializeField] private TextMeshProUGUI waitingText;
        [SerializeField] private Button backButton;

        private bool increasingAlpha;
        protected async override void onInitFinished()
        {
            NetworkManager.Singleton.GetComponent<NetworkConnection>().ClearConnectCallbacks();
            NetworkManager.Singleton.GetComponent<NetworkConnection>().ClearDisconnectCallbacks();
            backButton.onClick.AddListener(() =>
            {
                PopUpDialogue.Instance.OpenDialogue("Willst du wirklich zum Menu zurückkehren?", "Ja", "Nein", () =>
                {
                    NetworkManager.Singleton.Shutdown();
                    if (AuthenticationService.Instance.IsSignedIn)
                        AuthenticationService.Instance.SignOut();
                    SceneManager.LoadScene("MenuScene");
                });
            });
            if (ProcessDeepLinkMngr.Instance.joinCode != null && !ProcessDeepLinkMngr.Instance.joinCode.Equals(""))
            {
                string joinCode = ProcessDeepLinkMngr.Instance.joinCode;
                ProcessDeepLinkMngr.Instance.joinCode = "";
                try
                {
                    AuthenticationService.Instance.SignOut();
                    await SignInAnonymouslyAsync();
                    var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
                    JoinAllocation joinAllocation = await joinRelay(lobby);
                    startClient(joinAllocation);
                    waitingText.gameObject.SetActive(true);
                    heading.gameObject.SetActive(true);
                }
                catch (LobbyServiceException e)
                {
                    if (e.ErrorCode == 16000 || e.ErrorCode == 16001)
                    {
                        PopUpDialogue.Instance.OpenDialogue("Das Spiel mit dem angegebenen Link wurde nicht gefunden.", "OK", () =>
                        {
                            NetworkManager.Singleton.Shutdown();
                            if (AuthenticationService.Instance.IsSignedIn)
                                AuthenticationService.Instance.SignOut();
                            SceneManager.LoadScene("MenuScene");
                        });
                    }
                }
            }
        }



        // Update is called once per frame
        void Update()
        {
            
            var alpha = waitingText.color.a;

            if (increasingAlpha) alpha += Time.deltaTime;
            else alpha -= Time.deltaTime;

            if (alpha > 1) increasingAlpha = false;
            else if (alpha < 0) increasingAlpha = true;

            var color = waitingText.color;
            color.a = alpha;
            waitingText.color = color;
        }
    }
}
