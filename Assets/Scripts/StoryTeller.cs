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
    private float _timeInbetweenMessages = 1.2f;
    private float _timeForRethoricalPause = 1f;
    private float _timeToReadAMessage = 4.2f;


    // Start is called before the first frame update

    void Awake()
    {
        text.CrossFadeAlpha(0, 0, true);

        TimeManager.onDayComplete += (sender, day) =>
        {
            var relativeDay = day % 7;
            if (day == 1)
            {
                // we do not want to overwhelm the player with infos on the start of the game
                timeTillNextMessage = 0.3f;
                messages.Enqueue(
                    "\"Before the Custard fell, I had no aspiration. Alas, my ample collection is ready to expand beyond the memories of the past.\"\n\n                                   - The Collector");
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
        yield return new WaitForSeconds(5f);
        // cinematicOverlay.CrossFadeAlpha(0, .6f, true);
        text.CrossFadeAlpha(0, .6f, true);
        yield return new WaitForSeconds(.64f);
        cinematicOverlay.gameObject.SetActive(false);
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
            if (text.text == "")
            {
                DisplayStoryText(messages.Dequeue());
            }
            else
            {
                text.SetText("");
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