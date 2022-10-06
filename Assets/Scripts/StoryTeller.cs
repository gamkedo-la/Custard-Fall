using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryTeller : MonoBehaviour
{
    public TextMeshProUGUI text;

    private Queue<String> messages = new();

    private float messageDisplayTime = 0f;



    // Start is called before the first frame update
    void Awake()
    {
        TimeManager.onDayComplete += (sender, day) =>
        {
            if (day == 1)
            {
                messages.Enqueue("When the Custard fell, everything changed.");
                messages.Enqueue("Midday, it crawls to the places that used to be.");
                messages.Enqueue("At night, we flee into the mountains.");
                messages.Enqueue("Every day, we hunt for resources and the treasures of the past.");
            }
            else if (day == 3)
            {
                messages.Clear();
                messages.Enqueue("Every fourth day, the moon gets strong.");
            }
        };
    }

    private void DisplayStoryText(string message)
    {
        text.SetText(message);
        messageDisplayTime = message.Length * .25f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (messageDisplayTime > 0f)
            messageDisplayTime -= Time.deltaTime;
        else if(messages.Count != 0)
        {
            DisplayStoryText(messages.Dequeue());
        }
        else
        {
            messageDisplayTime = 0f;
            text.SetText("");
        }
    }
}
