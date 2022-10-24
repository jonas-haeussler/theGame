using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

namespace Assets
{
    public class NetworkConnection : MonoBehaviour
    {


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddDisconnectCallback(Action<ulong> disconnectCallback)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += disconnectCallback;
        }

        public void AddConnectCallback(Action<ulong> connectCallback)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += connectCallback;
        }
    }
}
