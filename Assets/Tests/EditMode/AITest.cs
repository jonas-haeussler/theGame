using NUnit.Framework;
using Players;
using System.Collections.Generic;
using Types;
using UnityEngine;


public class AITest
{

    [Test]
    public void FindCardPlays()
    {
        Queue<DiscardActionParameters> cardPlays;
        var gameObject = new GameObject();
        DiscardActionParameters parameters;
        AI ai = gameObject.AddComponent<AI>();
        // Test 1
        var cards1 = new List<int>() { 10, 20, 51, 46, 35, 37 };
        AI.CurrentState state1 = new AI.CurrentState(cards1,
                                                    PileType.Player1Ascending,
                                                    PileType.Player1Descending,
                                                    PileType.Player2Ascending,
                                                    PileType.Player2Descending,
                                                    12,
                                                    49,
                                                    13,
                                                    39);
        cardPlays = ai.findCardPlays(state1);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(46, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Descending, parameters.PileType);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(20, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        Assert.IsEmpty(cardPlays);





        // Test 2
        var cards2 = new List<int>() { 12, 22, 21, 20, 19, 58 };
        AI.CurrentState state2 = new AI.CurrentState(cards2,
                                                    PileType.Player1Ascending,
                                                    PileType.Player1Descending,
                                                    PileType.Player2Ascending,
                                                    PileType.Player2Descending,
                                                    10,
                                                    49,
                                                    15,
                                                    39);

        cardPlays = ai.findCardPlays(state2);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(19, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(20, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(21, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(22, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        parameters = cardPlays.Dequeue();
        Assert.AreEqual(12, parameters.CardNumber);
        Assert.AreEqual(PileType.Player1Ascending, parameters.PileType);
        Assert.IsEmpty(cardPlays);
    }

}
