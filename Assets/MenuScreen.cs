using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Assets
{
    public class MenuScreen : MonoBehaviour
    {


        [SerializeField] private Button continueButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;
        // Start is called before the first frame update
        void Start()
        {
            continueButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            menuButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("MenuScene");
            });
            quitButton.onClick.AddListener(() =>
            {

            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
