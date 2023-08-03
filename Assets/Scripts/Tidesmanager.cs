using System;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using UnityEngine.Serialization;

public class Tidesmanager : MonoBehaviour
{
    public CustardManager CustardManager;
    public TimeManager timeManager;
    public Player Player;

    private readonly SortedDictionary<float, TideStep> _tidesPlanFirstTime = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal0 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal1 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal2 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal3 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal4 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanNormal5 = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanFullMoon = new SortedDictionary<float, TideStep>();
    private readonly SortedDictionary<float, TideStep> _tidesPlanCalamity = new SortedDictionary<float, TideStep>();

    // a Custard Fall week has 4 days and ends always with a full moon
    public int dayOfWeek = -1;
    private SortedDictionary<float, TideStep>[] _weeklyPlan;
    private SortedDictionary<float, TideStep> _currentDailyTidesPlan;

    private List<float> _tidesPlanIndices;
    public bool pause = false;
    public bool realCustardFallCycle;

    [SerializeField] private bool useSerializedCycle;
    [SerializeField] private List<TideDay> cycle = new();

    public int indexOfCurrentDayTimeTideLevel;

    private void Start()
    {
        InitPlannedTideBehavior();

        if (!useSerializedCycle)
        {
            _weeklyPlan = new[]
            {
                _tidesPlanFirstTime, _tidesPlanNormal, _tidesPlanNormal1, _tidesPlanNormal2, _tidesPlanNormal3,
                _tidesPlanNormal4, _tidesPlanNormal5, _tidesPlanFullMoon
            };
        }

        SetTidesDayOfWeek(0);

        TimeManager.onMidnightPassed += (sender, arg) => StartNextTidesDayOfWeek();

        indexOfCurrentDayTimeTideLevel = 0;
        var tideStep = _currentDailyTidesPlan[indexOfCurrentDayTimeTideLevel];
        CustardManager.targetTideLevel = tideStep.GetLevel();
    }

    private void StartNextTidesDayOfWeek()
    {
        SetTidesDayOfWeek(++dayOfWeek);
    }

    private void SetTidesDayOfWeek(int newDayOfWeek)
    {
        if (newDayOfWeek >= _weeklyPlan.Length)
        {
            // we need a more epic custard animation the first time when the level starts
            if (!useSerializedCycle && _weeklyPlan[0] == _tidesPlanFirstTime)
            {
                _weeklyPlan[0] = _tidesPlanNormal0;
            }

            newDayOfWeek = 0;
        }

        this.dayOfWeek = newDayOfWeek;
        _currentDailyTidesPlan = _weeklyPlan[newDayOfWeek];
        _tidesPlanIndices = new List<float>(_currentDailyTidesPlan.Keys);
    }

    private void FixedUpdate()
    {
        if (!CustardManager.pauseIterationCountDown && !pause)
        {
            int timeIndex = FindLowestBound(_tidesPlanIndices, timeManager.time);
            if (timeIndex == -1)
            {
                Debug.Log("Tide step not found... no tides update");
                return;
            }

            if (timeIndex != indexOfCurrentDayTimeTideLevel)
            {
                Debug.Log($"Tide step {timeIndex}");
                TideStep tideStep = _currentDailyTidesPlan.GetValueOrDefault(_tidesPlanIndices[timeIndex]);
                indexOfCurrentDayTimeTideLevel = timeIndex;

                // custard ai
                var nextTideLevel = tideStep.GetLevel();
                var currentTideLevel = CustardManager.targetTideLevel;
                CustardManager.targetTideLevel = nextTideLevel;

                // spread custard hotspots
                if (currentTideLevel > nextTideLevel)
                {
                    CustardManager.RimCustardUpdate();
                }

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
        if (useSerializedCycle)
        {
            _weeklyPlan = new SortedDictionary<float, TideStep>[cycle.Count];
            for (int i = 0; i < cycle.Count; i++)
            {
                var tideDay = cycle[i];
                Debug.Log("day " + i);
                SortedDictionary<float, TideStep> normalSteps = new();
                foreach (var tideStep in tideDay.TideSteps)
                {
                    normalSteps.Add(tideStep.GetTime(), tideStep);
                    Debug.Log("fraction of day " + tideStep.GetTime() + " " + tideStep.GetLevel());
                }

                _weeklyPlan[i] = normalSteps;
            }
        }
        else if (realCustardFallCycle)
        {
            _tidesPlanFirstTime.Add(0f, new TideStep(9));
            _tidesPlanFirstTime.Add(5 / 16f, new TideStep(5));
            _tidesPlanFirstTime.Add(13 / 16f, new TideStep(7));
            _tidesPlanFirstTime.Add(13.5f / 16f, new TideStep(8));

            _tidesPlanNormal0.Add(0f, new TideStep(8));
            _tidesPlanNormal0.Add(4 / 16f, new TideStep(5));
            _tidesPlanNormal0.Add(13 / 16f, new TideStep(6));
            _tidesPlanNormal0.Add(13.5f / 16f, new TideStep(7));

            _tidesPlanNormal.Add(0f, new TideStep(8));
            _tidesPlanNormal.Add(4 / 16f, new TideStep(5));
            _tidesPlanNormal.Add(13 / 16f, new TideStep(6));
            _tidesPlanNormal.Add(13.5f / 16f, new TideStep(7));

            _tidesPlanNormal1.Add(0f, new TideStep(7));
            _tidesPlanNormal1.Add(4 / 16f, new TideStep(4));
            _tidesPlanNormal1.Add(13 / 16f, new TideStep(5));
            _tidesPlanNormal1.Add(13.5f / 16f, new TideStep(6));

            _tidesPlanNormal2.Add(0f, new TideStep(6));
            _tidesPlanNormal2.Add(4 / 16f, new TideStep(3));
            _tidesPlanNormal2.Add(13 / 16f, new TideStep(4));
            _tidesPlanNormal2.Add(13.5f / 16f, new TideStep(5));

            _tidesPlanNormal3.Add(0f, new TideStep(5));
            _tidesPlanNormal3.Add(4 / 16f, new TideStep(4));
            _tidesPlanNormal3.Add(13 / 16f, new TideStep(5));
            _tidesPlanNormal3.Add(13.5f / 16f, new TideStep(6));

            _tidesPlanNormal4.Add(0f, new TideStep(6));
            _tidesPlanNormal4.Add(4 / 16f, new TideStep(5));
            _tidesPlanNormal4.Add(13 / 16f, new TideStep(7));
            _tidesPlanNormal4.Add(13.5f / 16f, new TideStep(8));

            _tidesPlanNormal5.Add(0f, new TideStep(8));
            _tidesPlanNormal5.Add(4 / 16f, new TideStep(7));
            _tidesPlanNormal5.Add(13 / 16f, new TideStep(9));
            _tidesPlanNormal5.Add(13.5f / 16f, new TideStep(11));

            _tidesPlanFullMoon.Add(0f, new TideStep(11));
            _tidesPlanFullMoon.Add(4.1f / 16f, new TideStep(10));
            _tidesPlanFullMoon.Add(4.2f / 16f, new TideStep(9));
            _tidesPlanFullMoon.Add(4.3f / 16f, new TideStep(8));
            _tidesPlanFullMoon.Add(4.4f / 16f, new TideStep(7));
            _tidesPlanFullMoon.Add(4.5f / 16f, new TideStep(6));
            _tidesPlanFullMoon.Add(4.6f / 16f, new TideStep(5));
            _tidesPlanFullMoon.Add(4.7f / 16f, new TideStep(4));
            _tidesPlanFullMoon.Add(4.8f / 16f, new TideStep(3));
            _tidesPlanFullMoon.Add(4.9f / 16f, new TideStep(2));
            _tidesPlanFullMoon.Add(5 / 16f, new TideStep(1));
            _tidesPlanFullMoon.Add(13 / 16f, new TideStep(2));
            _tidesPlanFullMoon.Add(14 / 16f, new TideStep(5));
            _tidesPlanFullMoon.Add(14.5f / 16f, new TideStep(8));

            _tidesPlanCalamity.Add(0f, new TideStep(15));
            _tidesPlanCalamity.Add(4 / 16f, new TideStep(7));
            _tidesPlanCalamity.Add(7 / 16f, new TideStep(1));
            _tidesPlanCalamity.Add(10 / 16f, new TideStep(2));
            _tidesPlanCalamity.Add(11 / 16f, new TideStep(3));
            _tidesPlanCalamity.Add(12 / 16f, new TideStep(4));
            _tidesPlanCalamity.Add(13 / 16f, new TideStep(5));
            _tidesPlanCalamity.Add(14 / 16f, new TideStep(6));
            _tidesPlanCalamity.Add(15 / 16f, new TideStep(7));
        }
        else
        {
            // just for sample scene playing around
            _tidesPlanNormal.Add(0f, new TideStep(4));
            _tidesPlanNormal.Add(.15f, new TideStep(4));
            _tidesPlanNormal.Add(.25f, new TideStep(3));
            _tidesPlanNormal.Add(.35f, new TideStep(2));
            _tidesPlanNormal.Add(.45f, new TideStep(1));
            _tidesPlanNormal.Add(.55f, new TideStep(2));
            _tidesPlanNormal.Add(.75f, new TideStep(4));
        }
    }


    [Serializable]
    public class TideDay
    {
        [SerializeField] private List<TideStep> tideSteps;

        public List<TideStep> TideSteps => tideSteps;
    }

    [Serializable]
    public class TideStep
    {
        [SerializeField] private int level;

        [FormerlySerializedAs("fraction16")] [SerializeField]
        private float time;

        public TideStep(int level)
        {
            this.level = level;
        }

        public TideStep(int level, float time)
        {
            this.level = level;
            this.time = time;
        }

        public int GetLevel()
        {
            return level;
        }

        public float GetTime()
        {
            return time;
        }
    }
}