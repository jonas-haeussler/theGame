using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

namespace Networking
{
    public class NetworkConnection : MonoBehaviour
    {

        private List<Action<ulong>> disconnectCallbacks;
        private List<Action<ulong>> connectCallbacks;
        private List<Action<string, LoadSceneMode, List<ulong>, List<ulong>>> sceneLoadFinishedCallbacks;

        // Start is called before the first frame update
        void Start()
        {
            disconnectCallbacks = new List<Action<ulong>>();
            connectCallbacks = new List<Action<ulong>>();
            sceneLoadFinishedCallbacks = new List<Action<string, LoadSceneMode, List<ulong>, List<ulong>>>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ClearSceneLoadFinishedCallbacks()
        {
            foreach(var callback in sceneLoadFinishedCallbacks)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= callback.Invoke;
            }
        }

        public void AddSceneLoadFinishedCallback(Action<string, LoadSceneMode, List<ulong>, List<ulong>> sceneLoadFinishedCallback)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += sceneLoadFinishedCallback.Invoke;
            sceneLoadFinishedCallbacks.Add(sceneLoadFinishedCallback);
        }

        public void AddDisconnectCallback(Action<ulong> disconnectCallback)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += disconnectCallback;
            disconnectCallbacks.Add(disconnectCallback);
        }

        public void AddConnectCallback(Action<ulong> connectCallback)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += connectCallback;
            connectCallbacks.Add(connectCallback);
        }
        public void ClearDisconnectCallbacks()
        {
            foreach (var callback in disconnectCallbacks)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= callback;
            }
        }

        public void ClearConnectCallbacks()
        {
            foreach (var callback in connectCallbacks)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= callback;
            }
        }
    }
}
