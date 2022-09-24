using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public static TimeManager Instance;

    public static EventHandler<int> onDayComplete;
    public static EventHandler<EventArgs> onNoonPassed;
    public static EventHandler<EventArgs> onMorningStarted;
    public static EventHandler<EventArgs> onNightStarted;

    public enum DayNightState { Daytime, Nightime };

    public DayNightState state;

    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength = 300f;
    public float startTime = 0.3f;
    private float timeRate;

    float dayStart = 0.25f;
    float nightStart = 0.75f;

    int currentDay;
    int previousDay;
    bool noonPassed;
    bool eveningStarted;
    bool morningStarted;

    public bool IsDayTime { get {
        return dayStart <= time && time < nightStart;
    }}



    public float TimeRate { get => timeRate; }
    public int CurrentDay { get => currentDay; }

    private void Awake() {
        if(Instance != null){
            Destroy(gameObject);
            Debug.LogWarning("Time Manager already exists, deleting...");
            return;
        }
        Instance = this;
        currentDay = 1;
        previousDay = 0;

        if (state == DayNightState.Daytime && startTime >= nightStart) {
            startTime = dayStart;
        }
        if (state == DayNightState.Nightime && startTime < nightStart) {
            startTime = nightStart;
        }
    }

    void Start ()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        state = IsDayTime ? DayNightState.Daytime : DayNightState.Nightime;
    }

    private void OnValidate() {
        timeRate = 1.0f / fullDayLength;
    }

    private void Update() {     
        time += TimeManager.Instance.TimeRate * Time.deltaTime;

        if(time >= 1.0f){
            time = 0.0f;
            previousDay = currentDay;
        }

        if (time >= dayStart && previousDay >= currentDay) {
            currentDay++;
            noonPassed = false;
            onDayComplete?.Invoke(this, currentDay);
        }

        if(time >= 0.5 && !noonPassed){
            noonPassed = true;
            
            onNoonPassed?.Invoke(this, EventArgs.Empty);
        }

        if( IsDayTime && state == DayNightState.Nightime){
            state = DayNightState.Daytime;

            onMorningStarted?.Invoke(this, EventArgs.Empty);
        }

        if( !IsDayTime && state == DayNightState.Daytime){
            state = DayNightState.Nightime;

            onNightStarted?.Invoke(this, EventArgs.Empty);
        }


    }


}
