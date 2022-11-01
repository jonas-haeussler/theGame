using System.Collections;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Core;

namespace UI
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI heading;
        [SerializeField] private TextMeshProUGUI subHeading;
        [SerializeField] private Image image;

        [SerializeField] private Sprite victorySprite;
        [SerializeField] private Sprite defeatSprite;

        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;

        public bool victory;
        public bool noTurnsLeftCondition;
        public Action onReplayCallback;

        private void Awake()
        {
            replayButton.onClick.AddListener(() =>
            {
                IEnumerator animateText()
                {
                    string prefix = "Anfrage versendet";
                    while (true)
                    {
                        string text = replayButton.GetComponentInChildren<TextMeshProUGUI>().text;
                        if (text.Equals("Nochmal spielen"))
                            text = prefix + ".";
                        else if (text.Equals(prefix + "."))
                            text = prefix + "..";
                        else if (text.Equals(prefix + ".."))
                            text = prefix + "...";
                        else if (text.Equals(prefix + "..."))
                            text = prefix + ".";
                        replayButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
                        yield return new WaitForSeconds(0.4f);
                    }
                }

                onReplayCallback.Invoke();
                animateText();

            });
            menuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MenuScene");
            });
            quitButton.onClick.AddListener(() =>
            {
                PopUpDialogue.Instance.OpenDialogue("Willst du das Spiel wirklich verlassen?", "Ja", "Nein", () => Application.Quit());
            });
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (victory)
            {
                image.sprite = victorySprite;
                heading.text = "Du hast gewonnen!";
                heading.color = new Color(1, 0.8f, 0);
                if (noTurnsLeftCondition)
                {
                    subHeading.text = "Dein Gegner hat keine Züge mehr!";
                }
                else
                {
                    subHeading.text = "Du hast alle deine Karten abgelegt!";
                }
            }
            else
            {
                image.sprite = defeatSprite;
                heading.text = "Leider verloren!";
                heading.color = new Color(0.5f, 0, 0);
                if (noTurnsLeftCondition)
                {
                    subHeading.text = "Du hast keine Züge mehr!";
                }
                else
                {
                    subHeading.text = "Dein Gegner hat all seine Karten abgelegt!";
                }
            }
        }
    }
}
