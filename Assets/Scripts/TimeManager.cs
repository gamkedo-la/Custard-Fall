using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public static EventHandler<int> onDayComplete;
    public static EventHandler<EventArgs> onNoonPassed;
    public static EventHandler<EventArgs> onMorningStarted;
    public static EventHandler<EventArgs> onNightStarted;
    public static EventHandler<EventArgs> onMidnightPassed;

    public enum DayNightState
    {
        Daytime,
        Nightime
    };

    public DayNightState state;
    public TextMeshProUGUI dayCountText;
    public Animator dayCountAnimatorController;

    [Range(0.0f, 1.0f)] public float time;
    public float fullDayLength = 300f;
    public float startTime = 0.25f;
    private float timeRate;

    [SerializeField] public float dayStart = 0.25f;
    [SerializeField] public float nightStart = 0.75f;

    [NonSerialized] int currentDay;
    [NonSerialized] int previousDay;
    [NonSerialized] bool noonPassed;
    [NonSerialized] bool eveningStarted;
    [NonSerialized] bool morningStarted;

    [SerializeField] private bool pause;

    public void Pause()
    {
        pause = true;
    }

    public int Days => currentDay;

    public bool IsDayTime
    {
        get { return dayStart <= time && time < nightStart; }
    }

    public float TimeRate
    {
        get => timeRate;
    }

    public int CurrentDay
    {
        get => currentDay;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.LogWarning("Time Manager already exists, deleting...");
            return;
        }

        Instance = this;
        previousDay = 0;
        currentDay = 0;

        if (state == DayNightState.Daytime && startTime >= nightStart)
        {
            startTime = dayStart;
        }

        if (state == DayNightState.Nightime && startTime < nightStart)
        {
            startTime = nightStart;
        }
    }

    void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        state = IsDayTime ? DayNightState.Daytime : DayNightState.Nightime;
        dayCountText.SetText("Day " + currentDay);
    }

    private IEnumerator StartFirstDayAnimation()
    {
        yield return new WaitForSeconds(8.3f);
        dayCountAnimatorController.SetTrigger("NewDayTrigger");
    }

    private void OnValidate()
    {
        timeRate = 1.0f / fullDayLength;
    }

    private void Update()
    {
        if (pause)
            return;

        time += Math.Clamp(TimeManager.Instance.TimeRate * Time.deltaTime, 0.0f, 1.0f);

        if (time >= 1.0f)
        {
            time = 0.0f;
            previousDay = currentDay;
            onMidnightPassed?.Invoke(this, EventArgs.Empty);
        }

        if (time >= dayStart && previousDay >= currentDay)
        {
            currentDay++;
            dayCountText.SetText("Day " + currentDay.ToString());
            if (currentDay == 1)
            {
                StartCoroutine(StartFirstDayAnimation());
            }
            else
            {
                dayCountAnimatorController.SetTrigger("NewDayTrigger");
            }

            noonPassed = false;
            onDayComplete?.Invoke(this, currentDay);
        }

        if (time >= 0.5 && !noonPassed)
        {
            noonPassed = true;

            onNoonPassed?.Invoke(this, EventArgs.Empty);
        }

        if (IsDayTime && state == DayNightState.Nightime)
        {
            state = DayNightState.Daytime;

            onMorningStarted?.Invoke(this, EventArgs.Empty);
        }

        if (!IsDayTime && state == DayNightState.Daytime)
        {
            state = DayNightState.Nightime;

            onNightStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}