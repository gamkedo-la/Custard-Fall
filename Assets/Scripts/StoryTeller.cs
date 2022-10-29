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
    private float _timeInbetweenMessages = .8f;
    private float _timeForRethoricalPause = 1f;
    private float _timeToReadAMessage = 5f;


    // Start is called before the first frame update

    void Awake()
    {
        TimeManager.onDayComplete += (sender, day) =>
        {
            if (day == 1)
            {
                // we do not want to overwhelm the player with infos on the start of the game
                timeTillNextMessage = 1.5f;
                messages.Enqueue("The world wants to nibble at you,\nso munch back and survive!");
                messages.Enqueue("7 days left till next calamity...");
             }
            else if (day == 3)
            {
                messages.Clear();
                timeTillNextMessage = 1.5f;
                messages.Enqueue("The moon feels stronger today...");
            }
            else if (day == 6)
            {
                messages.Clear();
                timeTillNextMessage = 1.5f;
                messages.Enqueue("The world will change tonight...\nthe custard falls.");
            }
        };

        StartCoroutine(DoCinematicMagic());
    }

    private IEnumerator DoCinematicMagic()
    {
        cinematicOverlay.gameObject.SetActive(true);
        // cinematicBar.gameObject.SetActive(true);
        cinematicOverlay.CrossFadeAlpha(1,0,true);
        // cinematicBar.CrossFadeAlpha(1, 0, true);
        yield return new WaitForSeconds(6f);
        cinematicOverlay.CrossFadeAlpha(0,1.5f,true);
        yield return new WaitForSeconds(1.6f);
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
                timeTillNextMessage = _timeInbetweenMessages;
            }
        }
        else
        {
            timeTillNextMessage = 0f;
            text.SetText("");
        }
    }
}