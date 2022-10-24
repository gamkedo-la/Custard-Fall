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
    private float _timeInbetweenMessages = 1f;


    // Start is called before the first frame update

    void Awake()
    {
        TimeManager.onDayComplete += (sender, day) =>
        {
            if (day == 1)
            {
                // we do not want to overwhelm the player with infos on the start of the game
                timeTillNextMessage = 2f;
                messages.Enqueue("Since the Custard fell,\n\nit has been following the tides\n\never since!");
                messages.Enqueue("");
                messages.Enqueue("");
                messages.Enqueue("Midday, it crawls down\n\n - back to the old places.");
            }
            else if (day == 4)
            {
                messages.Clear();
                messages.Enqueue("The moon is strong today...");
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
    }

    private void DisplayStoryText(string message)
    {
        text.SetText(message);
        if (message == "")
        {
            // rhetorical pause
            timeTillNextMessage = 1f;
        }
        else
        {
            timeTillNextMessage = 4.5f;
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