using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System;

namespace Menu
{
    public class ProcessDeepLinkMngr : MonoBehaviour
    {
        public static ProcessDeepLinkMngr Instance { get; private set; }
        private string deeplinkURL;

        public string joinCode;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Application.deepLinkActivated += onDeepLinkActivated;
                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    onDeepLinkActivated(Application.absoluteURL);
                }
                // Initialize DeepLink Manager global variable.
                else deeplinkURL = "[none]";
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if(!joinCode.Equals(""))
            {
                onDeepLinkActivated($"test?{joinCode}");
            }
        }

        private void onDeepLinkActivated(string url)
        {
            // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
            deeplinkURL = url;

            // Decode the URL to determine action. 
            // In this example, the app expects a link formatted like this:
            // unitydl://mylink?scene1
            joinCode = url.Split("?"[0])[1];

            SceneManager.LoadScene("LoadGameFromLink", LoadSceneMode.Additive);

        }
    }
}