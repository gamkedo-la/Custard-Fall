using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryTeller : MonoBehaviour
{
    public TextMeshProUGUI text;

    private Queue<String> messages = new();

    private float timeTillNextMessage = 0f;
    private float _timeInbetweenMessages = 0.4f;


    // Start is called before the first frame update
    void Awake()
    {
        TimeManager.onDayComplete += (sender, day) =>
        {
            if (day == 1)
            {
                // we do not want to overwhelm the player with infos on the start of the game
                timeTillNextMessage = 2.5f;
                messages.Enqueue("Since the Custard fell, it rises and falls every day.");
                messages.Enqueue("Midday, it crawls down to the old places...");
                messages.Enqueue("");
                messages.Enqueue("That's when we hunt for treasures and resources!");
            }
            else if (day == 4)
            {
                messages.Clear();
                messages.Enqueue("The moon is strong today...");
            }
        };
    }

    private void DisplayStoryText(string message)
    {
        text.SetText(message);
        if (message == "")
        {
            // rhetorical pause
            timeTillNextMessage = 1.5f;
        }
        else
        {
            timeTillNextMessage = 6f;
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