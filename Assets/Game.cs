using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets {
    public class Game : MonoBehaviour
    {
        private Player LocalPlayer;
        private Player RemotePlayer;
        [SerializeField] private GameObject cardPrefab;
        public enum PlayerID
        {
            Player1, Player2
        }
        public class Turn
        {
            public PlayerID PlayerID;
            public List<DiscardActionParameters> DiscardActionParameters;
            public bool enemyDiscard;
            public Turn(PlayerID playerID)
            {
                PlayerID = playerID;
                this.DiscardActionParameters = new List<DiscardActionParameters>();
                enemyDiscard = true;
            }
        }
        private Turn currentTurn;
        private List<Turn> allTurns;

        private void Start()
        {
            gameState = GameState.Init;
            LocalPlayer = FindObjectOfType<LocalPlayer>();
            RemotePlayer = FindObjectOfType<RemotePlayer>();
            LocalPlayer.initGame(PlayerID.Player1, cardPrefab, onCardPlayed, onTurnFinished);
            RemotePlayer.initGame(PlayerID.Player2, cardPrefab, onCardPlayed, onTurnFinished);
            currentTurn = new Turn(PlayerID.Player1);
            gameState = GameState.Rounds;
            allTurns = new List<Turn>();

        }

        public enum GameState
        {
            Init, Rounds, Finished
        }
        private GameState gameState;

        public enum PileType
        {
            Player1Ascending, Player1Descending, Player2Ascending, Player2Descending
        }

        public class DiscardActionParameters
        {
            public PlayerID PlayerID;
            public PileType PileType;
            public int CardNumber;

            public DiscardActionParameters(PlayerID playerID, PileType pileType, int cardNumber)
            {
                this.PlayerID = playerID;
                this.PileType = pileType;
                this.CardNumber = cardNumber;
            }
        }

        private void onCardPlayed(DiscardActionParameters parameters)
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
            currentTurn.DiscardActionParameters.Add(parameters);
            LocalPlayer.TurnChanged(currentTurn);
            RemotePlayer.TurnChanged(currentTurn);
        }

        private void onTurnFinished()
        {            
            currentTurn = new Turn(currentTurn.PlayerID.Equals(PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1);
            allTurns.Add(currentTurn);
            LocalPlayer.TurnChanged(currentTurn);
            RemotePlayer.TurnChanged(currentTurn);
        }

    
    }
}


