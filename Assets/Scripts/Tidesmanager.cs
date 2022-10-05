using System;
using System.Collections.Generic;
using Custard;
using UnityEngine;

public class Tidesmanager : MonoBehaviour
{
    public CustardManager CustardManager;
    public TimeManager timeManager;
    public Player Player;

    private SortedDictionary<float, TideStep> _tidesPlanNormal = new SortedDictionary<float, TideStep>();
    private SortedDictionary<float, TideStep> _tidesPlanFullMoon = new SortedDictionary<float, TideStep>();
    private SortedDictionary<float, TideStep> _tidesPlanDayAfterFullMoon = new SortedDictionary<float, TideStep>();

    // a Custard Fall week has 4 days and ends always with a full moon
    public int _dayOfWeek = -1;
    private SortedDictionary<float, TideStep>[] _weeklyPlan;
    private SortedDictionary<float, TideStep> _tidesPlan;

    private List<float> _tidesPlanIndixes;
    public float speedFactor = 1f;
    public bool pause = false;
    public bool realCustardFallCycle;

    public int currentTideIndex;

    private void Start()
    {
        InitPlannedTideBehavior();

        _weeklyPlan = new[]
            {_tidesPlanDayAfterFullMoon, _tidesPlanNormal, _tidesPlanNormal, _tidesPlanFullMoon};
        SetDayOfWeek(0);

        TimeManager.onMidnightPassed += (sender, arg) => StartNextDayOfWeek();
        
        currentTideIndex = 0;
        var tideStep = _tidesPlan[currentTideIndex];
        CustardManager.targetTideLevel = tideStep.getLevel();
    }

    private void StartNextDayOfWeek()
    {
        SetDayOfWeek(++_dayOfWeek);
    }

    private void SetDayOfWeek(int dayOfWeek)
    {
        if (dayOfWeek >= _weeklyPlan.Length)
            dayOfWeek = 0;

        _dayOfWeek = dayOfWeek;
        _tidesPlan = _weeklyPlan[dayOfWeek];
        _tidesPlanIndixes = new List<float>(_tidesPlan.Keys);
    }

    private void FixedUpdate()
    {
        if (!CustardManager.pauseIterationCountDown && !pause)
        {
            int timeIndex = FindLowestBound(_tidesPlanIndixes, timeManager.time);
            if (timeIndex == -1)
            {
                Debug.Log("Tide step not found... no tides update");
                return;
            }

            if (timeIndex != currentTideIndex)
            {
                TideStep tideStep = _tidesPlan.GetValueOrDefault(_tidesPlanIndixes[timeIndex]);
                currentTideIndex = timeIndex;

                // custard ai
                var nextTideLevel = tideStep.getLevel();
                CustardManager.targetTideLevel = nextTideLevel;

                // spread custard hotspots
                // TODO center custard update roughly at some distance around the player
                CustardManager.SeedCustardUpdate((int) Math.Floor(Time.time * 1000));
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 2);
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 4);
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 8);
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 16);
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 32);
                CustardManager.SeedCustardUpdate(((int) Math.Floor(Time.time * 1000)) / 64);
            }
        }
    }

    private static int FindLowestBound(List<float> list, float value)
    {
        // classic binary search for the lowest bound 
        if (list.Count == 0)
            return -1;
        int minIndex = 0;
        int maxIndex = list.Count - 1;

        while (minIndex < maxIndex)
        {
            var midIndex = (minIndex + maxIndex) / 2;
            if (value >= list[midIndex])
            {
                if (value < list[midIndex + 1])
                {
                    return midIndex;
                }
                else
                    minIndex = midIndex + 1;
            }
            else
            {
                maxIndex = midIndex - 1;
            }
        }

        if (value >= list[minIndex])
        {
            return minIndex;
        }
        else
        {
            return -1;
        }
    }

    private void InitPlannedTideBehavior()
    {
        if (realCustardFallCycle)
        {
            _tidesPlanNormal.Add(0f, new TideStep(11));
            _tidesPlanNormal.Add(2 / 16f, new TideStep(9));
            _tidesPlanNormal.Add(4 / 16f, new TideStep(7));
            _tidesPlanNormal.Add(5 / 16f, new TideStep(5));
            _tidesPlanNormal.Add(7 / 16f, new TideStep(3));
            _tidesPlanNormal.Add(10 / 16f, new TideStep(4));
            _tidesPlanNormal.Add(11 / 16f, new TideStep(6));
            _tidesPlanNormal.Add(12 / 16f, new TideStep(8));
            _tidesPlanNormal.Add(15 / 16f, new TideStep(11));

            _tidesPlanFullMoon.Add(0f, new TideStep(11));
            _tidesPlanFullMoon.Add(1 / 16f, new TideStep(12));
            _tidesPlanFullMoon.Add(2 / 16f, new TideStep(8));
            _tidesPlanFullMoon.Add(4 / 16f, new TideStep(6));
            _tidesPlanFullMoon.Add(5 / 16f, new TideStep(1));
            _tidesPlanFullMoon.Add(7 / 16f, new TideStep(2));
            _tidesPlanFullMoon.Add(10 / 16f, new TideStep(5));
            _tidesPlanFullMoon.Add(12 / 16f, new TideStep(8));
            _tidesPlanFullMoon.Add(15 / 16f, new TideStep(11));
            
            _tidesPlanDayAfterFullMoon.Add(0f, new TideStep(11));
            _tidesPlanDayAfterFullMoon.Add(1 / 16f, new TideStep(12));
            _tidesPlanDayAfterFullMoon.Add(2 / 16f, new TideStep(9));
            _tidesPlanDayAfterFullMoon.Add(4 / 16f, new TideStep(7));
            _tidesPlanDayAfterFullMoon.Add(5 / 16f, new TideStep(5));
            _tidesPlanDayAfterFullMoon.Add(7 / 16f, new TideStep(3));
            _tidesPlanDayAfterFullMoon.Add(10 / 16f, new TideStep(4));
            _tidesPlanDayAfterFullMoon.Add(11 / 16f, new TideStep(6));
            _tidesPlanDayAfterFullMoon.Add(12 / 16f, new TideStep(8));
            _tidesPlanDayAfterFullMoon.Add(15 / 16f, new TideStep(11));
        }
        else
        {
            _tidesPlanNormal.Add(0f, new TideStep(4));
            _tidesPlanNormal.Add(.15f, new TideStep(4));
            _tidesPlanNormal.Add(.25f, new TideStep(3));
            _tidesPlanNormal.Add(.35f, new TideStep(2));
            _tidesPlanNormal.Add(.45f, new TideStep(1));
            _tidesPlanNormal.Add(.55f, new TideStep(2));
            _tidesPlanNormal.Add(.75f, new TideStep(4));
        }
    }


    public class TideStep
    {
        private int level;

        // currently not used
        private float secondsToLast;

        public TideStep(int level)
        {
            this.level = level;
        }

        public TideStep(int level, float secondsToLast)
        {
            this.level = level;
            this.secondsToLast = secondsToLast;
        }

        public int getLevel()
        {
            return level;
        }

        public float getSecondsToLast()
        {
            return secondsToLast;
        }
    }
}