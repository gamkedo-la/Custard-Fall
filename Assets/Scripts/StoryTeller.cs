using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryTeller : MonoBehaviour
{
    public TextMeshProUGUI text;

    public Image cinematicOverlay;

    // public Image cinematicBar;
    public PauseActivator pauseActivator;

    private Queue<String> messages = new();

    private float timeTillNextMessage = 0f;
    private float _timeInbetweenMessages = 1.6f;
    private float _timeForRethoricalPause = 1f;
    private float _timeToReadAMessage = 5f;

    [SerializeField] private Player player;


    // Start is called before the first frame update

    void Awake()
    {
        player.preventInhale = true;
        text.CrossFadeAlpha(0, 0, true);

        TimeManager.onDayComplete += (sender, day) =>
        {
            var relativeDay = day % 7;
            if (day == 1)
            {
                // we do not want to overwhelm the player with infos on the start of the game
                timeTillNextMessage = 0.1f;
                messages.Enqueue(
                    "Started my collection when the Custard first fell. As it spread, I preserved what I deemed worthy.\n\n                         The Collector");
                // messages.Enqueue("7 days left till next calamity...");
            }
            // else if (relativeDay == 1)
            // {
            //     messages.Clear();
            //     timeTillNextMessage = 1.5f;
            //     messages.Enqueue("7 days left till next calamity...");
            // }
            // else if (relativeDay == 3)
            // {
            //     messages.Clear();
            //     timeTillNextMessage = 1.5f;
            //     messages.Enqueue("The moon feels stronger today...");
            // }
            // else if (relativeDay == 4)
            // {
            //     messages.Clear();
            //     timeTillNextMessage = 1.5f;
            //     messages.Enqueue("3 days left till next calamity...");
            // }
            // else if (relativeDay == 6)
            // {
            //     messages.Clear();
            //     timeTillNextMessage = 1.5f;
            //     messages.Enqueue("The world will change tonight...\nthe custard falls.");
            // }
        };

        StartCoroutine(DoCinematicMagic());
    }

    private IEnumerator DoCinematicMagic()
    {
        cinematicOverlay.gameObject.SetActive(true);
        // cinematicBar.gameObject.SetActive(true);
        cinematicOverlay.CrossFadeAlpha(1, 0.5f, true);
        // cinematicBar.CrossFadeAlpha(1, 0, true);
        yield return new WaitForSeconds(6f);
        // cinematicOverlay.CrossFadeAlpha(0, .6f, true);
        text.CrossFadeAlpha(0, .24f, true);
        yield return new WaitForSeconds(.28f);
        cinematicOverlay.CrossFadeAlpha(0, .6f, true);
        yield return new WaitForSeconds(.6f);
        cinematicOverlay.gameObject.SetActive(false);
        player.preventInhale = false;
    }

    private void DisplayStoryText(string message)
    {
        text.SetText(message);
        if (message == "")
        {
            // rhetorical pause
            timeTillNextMessage = _timeForRethoricalPause;
        }
        else
        {
            text.CrossFadeAlpha(1, .45f, true);
            timeTillNextMessage = _timeToReadAMessage;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeTillNextMessage > 0f)
            timeTillNextMessage -= Time.deltaTime;
        else if (messages.Count != 0)
        {
            if (string.IsNullOrEmpty(text.text))
            {
                DisplayStoryText(messages.Dequeue());
            }
            else
            {
                // text.SetText("");
                text.CrossFadeAlpha(0, 0, true);
                timeTillNextMessage = _timeInbetweenMessages;
            }
        }
        else
        {
            timeTillNextMessage = 0f;
            text.CrossFadeAlpha(0, .3f, true);
        }
    }
}