using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Players;
using Types;
using UI;
using Networking;
using Core;

namespace Gameplay {
    public class Game : NetworkBehaviour
    {

        public static Game Instance;

        private static EndingProperties endingProperties;

        private static Player LocalPlayer;
        private static Player RemotePlayer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private EndScreen endScreen;
        [SerializeField] private MenuScreen menuScreen;
        [SerializeField] private Button menuButton;
        [SerializeField] private Fader fader;
        [SerializeField] private bool local;
        private static Turn currentTurn;
        private static List<Turn> allTurns;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {

            menuButton.onClick.AddListener(() =>
            {

                menuScreen.gameObject.SetActive(!menuScreen.gameObject.activeInHierarchy);
                LocalPlayer.active = menuScreen.gameObject.activeInHierarchy;
                RemotePlayer.active = menuScreen.gameObject.activeInHierarchy;
                
            });
            menuScreen.menuAction = () =>
            {
                PopUpDialogue.Instance.OpenDialogue("Möchtest du wirklich das Spiel verlassen?", "Ja", "Nein", () =>
                {
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene("MenuScene");
                });
            };
            NetworkManager.Singleton.GetComponent<NetworkConnection>().ClearDisconnectCallbacks();
            NetworkManager.Singleton.GetComponent<NetworkConnection>().AddDisconnectCallback((clientId) =>
            {
                PopUpDialogue.Instance.OpenDialogue("Dein Gegner hat das Spiel verlassen. Möchtest du zum Menu zurückkehren?", "Ja", "Nein", () =>
                {
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene("MenuScene");
                });
            });

            if (local)
            {
                onSceneLoadFinished();
            }
            


        }
        

        public static void onSceneLoadFinished()
        {
            
            var random = new System.Random();
            List<int> hostOrdering = Enumerable.Range(2, 58).OrderBy(a => random.Next()).ToList();
            List<int> clientOrdering = Enumerable.Range(2, 58).OrderBy(a => random.Next()).ToList();
            PlayerID hostPlayerId = random.Next(2) == 0 ? PlayerID.Player1 : PlayerID.Player2;
            PlayerID clientPlayerId = hostPlayerId.Equals(PlayerID.Player2) ? PlayerID.Player1 : PlayerID.Player2;
            if (Instance.local)
            {
                setupGameClient(hostPlayerId, clientPlayerId, hostOrdering.ToArray(), clientOrdering.ToArray());
            }
            else 
            {
                Instance.setupGameClientRpc(hostPlayerId, clientPlayerId, hostOrdering.ToArray(), clientOrdering.ToArray());
            }
        }

        private void Update()
        {
            if (gameState.Equals(GameState.Rounds))
            {
                if (menuScreen.gameObject.activeInHierarchy)
                {
                    LocalPlayer.active = false;
                    RemotePlayer.active = false;
                }
                else
                {
                    LocalPlayer.active = true;
                    RemotePlayer.active = true;
                }
            }

        }
        [ClientRpc]
        private void setupGameClientRpc(PlayerID hostPlayerId, PlayerID clientPlayerId, int[] hostOrdering, int[] clientOrdering)
        {
            Debug.Log($"Setup game on client {NetworkManager.Singleton.LocalClientId}");
            setupGameClient(hostPlayerId, clientPlayerId, hostOrdering, clientOrdering);
        }

        private static void setupGameClient(PlayerID hostPlayerId, PlayerID clientPlayerId, int[] hostOrdering, int[] clientOrdering)
        {
       
            PlayerID myPlayerId;
            PlayerID otherPlayerId;
            List<int> myOrdering;
            List<int> otherOrdering;
            if (Instance.local || NetworkManager.Singleton.IsHost)
            {
                myPlayerId = hostPlayerId;
                otherPlayerId = clientPlayerId;
                myOrdering = new List<int>(hostOrdering);
                otherOrdering = new List<int>(clientOrdering);
            }
            else
            {
                myPlayerId = clientPlayerId;
                otherPlayerId = hostPlayerId;
                myOrdering = new List<int>(clientOrdering);
                otherOrdering = new List<int>(hostOrdering);
            }
            gameState = GameState.Init;
            LocalPlayer = FindObjectOfType<LocalPlayer>();
            RemotePlayer = FindObjectOfType<RemotePlayer>();
            Instance.fader.StartFading();
            LocalPlayer.initGame(myPlayerId, Instance.cardPrefab, onCardPlayedLocal, onTurnFinishedLocal, myOrdering);
            RemotePlayer.initGame(otherPlayerId, Instance.cardPrefab, onCardPlayedLocal, onTurnFinishedLocal, otherOrdering);
            currentTurn = new Turn(PlayerID.Player1);
            gameState = GameState.Rounds;
            allTurns = new List<Turn>();
        }
        private static GameState gameState;

        


        private static void updateHand(DiscardActionParameters parameters)
        {
            var currentPlayer = currentTurn.PlayerID.Equals(LocalPlayer.PlayerID) ? LocalPlayer : RemotePlayer;
            var player1 = LocalPlayer.PlayerID.Equals(PlayerID.Player1) ? LocalPlayer : RemotePlayer;
            var player2 = RemotePlayer.PlayerID.Equals(PlayerID.Player1) ? LocalPlayer : RemotePlayer;
            var handCards = currentPlayer.Hand.GetHandCards();
            for (int i = 0; i < handCards.Count; i++)
            {
                if (parameters.CardNumber == handCards[i].Number)
                {
                    switch (parameters.PileType)
                    {
                        case PileType.Player1Ascending:
                            player1.AscendingPile.AddCard(handCards[i]);
                            if (currentPlayer.Equals(player2)) currentTurn.enemyDiscard = false;
                            break;
                        case PileType.Player1Descending:
                            player1.DescendingPile.AddCard(handCards[i]);
                            if (currentPlayer.Equals(player2)) currentTurn.enemyDiscard = false;
                            break;
                        case PileType.Player2Ascending:
                            player2.AscendingPile.AddCard(handCards[i]);
                            if (currentPlayer.Equals(player1)) currentTurn.enemyDiscard = false;
                            break;
                        case PileType.Player2Descending:
                            player2.DescendingPile.AddCard(handCards[i]);
                            if (currentPlayer.Equals(player1)) currentTurn.enemyDiscard = false;
                            break;
                    }
                }
            }
        }

        private static void onCardPlayedLocal(DiscardActionParameters parameters)
        {
            if (Instance.local)
            {
                onCardPlayed(parameters);
                if (gameFinished())
                {
                    onGameFinished(endingProperties.winner, endingProperties.noTurnsLeftCondition);
                }
            }
            else
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("Sending Clients request to play card");
                    Instance.onCardPlayedClientRpc(parameters.PlayerID, parameters.PileType, parameters.CardNumber);
                    if (gameFinished())
                    {
                        Instance.onGameFinishedClientRpc(endingProperties.winner, endingProperties.noTurnsLeftCondition);
                    }
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("Sending Host request to accept card play");
                    Instance.onCardPlayedServerRpc(parameters.PlayerID, parameters.PileType, parameters.CardNumber);
                }
            }
        }

        private static void onCardPlayed(DiscardActionParameters parameters)
        {
            updateHand(parameters);
            currentTurn.DiscardActionParameters.Add(parameters);
            LocalPlayer.TurnChanged(currentTurn);
            RemotePlayer.TurnChanged(currentTurn);
        }

        [ClientRpc]
        private void onGameFinishedClientRpc(PlayerID winner, bool noTurnsLeftCondition)
        {
            onGameFinished(winner, noTurnsLeftCondition);

        }

        private static void onGameFinished(PlayerID winner, bool noTurnsLeftCondition)
        {
            gameState = GameState.Finished;
            Instance.endScreen.victory = winner.Equals(LocalPlayer.PlayerID);
            Instance.endScreen.noTurnsLeftCondition = noTurnsLeftCondition;
            Instance.endScreen.onReplayCallback = onReplayRequest;
            Instance.endScreen.gameObject.SetActive(true);
            LocalPlayer.active = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void onCardPlayedServerRpc(PlayerID playerID, PileType pileType, int cardNumber)
        {
            Debug.Log("Client request for card play received");
            onCardPlayedLocal(new DiscardActionParameters(playerID, pileType, cardNumber));
        }

        [ClientRpc]
        private void onCardPlayedClientRpc(PlayerID playerID, PileType pileType, int cardNumber)
        {
            Debug.Log($"Play card on client {playerID}");
            var parameters = new DiscardActionParameters(playerID, pileType, cardNumber);
            onCardPlayed(parameters);
        }

        [ServerRpc(RequireOwnership = false)]
        private void onTurnFinishedServerRpc()
        {
            Debug.Log("Turn finish request from Client received");
            onTurnFinishedLocal();
        }

        [ClientRpc]
        private void onTurnFinishedClientRpc()
        {
            Debug.Log("Turn finish on client");
            onTurnFinished();

        }


        private static void onTurnFinishedLocal()
        {
            if (Instance.local)
            {
                onTurnFinished();
                if (gameFinished())
                {
                    Instance.onGameFinishedClientRpc(endingProperties.winner, endingProperties.noTurnsLeftCondition);
                }
            }
            else
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("Sending Turn Finish Request to Clients");
                    Instance.onTurnFinishedClientRpc();
                    if (gameFinished())
                    {
                        Instance.onGameFinishedClientRpc(endingProperties.winner, endingProperties.noTurnsLeftCondition);
                    }
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("Sending Turn finish request to Server");
                    Instance.onTurnFinishedServerRpc();
                }
            }
        }

        private static void onTurnFinished()
        {
            currentTurn = new Turn(currentTurn.PlayerID.Equals(PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1);
            allTurns.Add(currentTurn);
            LocalPlayer.TurnChanged(currentTurn);
            RemotePlayer.TurnChanged(currentTurn);
        }
        private static bool gameFinished()
        {
            var currentPlayer = currentTurn.PlayerID.Equals(LocalPlayer.PlayerID) ? LocalPlayer : RemotePlayer;
            if(!currentPlayer.HasTurnLeft())
            {
                var winner = currentTurn.PlayerID.Equals(LocalPlayer.PlayerID) ? RemotePlayer : LocalPlayer;
                endingProperties = new EndingProperties(winner.PlayerID, true);
                return true;
            }
            if(!currentPlayer.HasCardsLeft())
            {
                endingProperties = new EndingProperties(currentPlayer.PlayerID, false);
                return true;
            }
            return false;
        }

        private static void onReplayRequest()
        {
            if (Instance.local) 
            {
                SceneManager.LoadScene("LocalGameScene");
            }
            else
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    Instance.onReplayRequestClientRpc(LocalPlayer.PlayerID);
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    Instance.onReplayRequestServerRpc(LocalPlayer.PlayerID);
                }
            }
            
        }

        [ClientRpc]
        private void onReplayRequestClientRpc(PlayerID playerID)
        {
            if (LocalPlayer.PlayerID.Equals(playerID)) return;
            else
            {
                PopUpDialogue.Instance.OpenDialogue("Dein Gegner möchte nochmal spielen. Du auch?", "Ja", "Nein", onReplayConfirm);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void onReplayRequestServerRpc(PlayerID playerID)
        {
            onReplayRequestClientRpc(playerID);
        }

        private void onReplayConfirm()
        {
            onReplayConfirmServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void onReplayConfirmServerRpc()
        {
            NetworkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

    }
}


