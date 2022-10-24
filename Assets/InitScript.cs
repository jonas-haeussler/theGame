using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Assets
{

    public class InitScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            NetworkManager.Singleton.StartHost();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
