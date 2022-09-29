using System;
using System.Collections.Generic;
using Custard;
using UnityEngine;

public class Tidesmanager : MonoBehaviour
{
    public CustardManager CustardManager;
    public TimeManager timeManager;
    public Player Player;

    private SortedDictionary<float, TideStep> _tidesPlan = new SortedDictionary<float, TideStep>();
    private List<float> _tidesPlanIndixes;
    public float speedFactor = 1f;
    public bool pause = false;
    public bool realCustardFallCycle;

    public int currentTideIndex;

    private void Start()
    {
        InitPlannedTideBehavior();
        currentTideIndex = 0;
        var tideStep = _tidesPlan[currentTideIndex];
        CustardManager.targetTideLevel = tideStep.getLevel();
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
            _tidesPlan.Add(0f, new TideStep(11));
            _tidesPlan.Add(2 / 16f, new TideStep(9));
            _tidesPlan.Add(4 / 16f, new TideStep(7));
            _tidesPlan.Add(5 / 16f, new TideStep(5));
            _tidesPlan.Add(7 / 16f, new TideStep(3));
            _tidesPlan.Add(10 / 16f, new TideStep(4));
            _tidesPlan.Add(11 / 16f, new TideStep(6));
            _tidesPlan.Add(12 / 16f, new TideStep(8));
            _tidesPlan.Add(15 / 16f, new TideStep(11));
        }
        else
        {
            _tidesPlan.Add(0f, new TideStep(4));
            _tidesPlan.Add(.15f, new TideStep(4));
            _tidesPlan.Add(.25f, new TideStep(3));
            _tidesPlan.Add(.35f, new TideStep(2));
            _tidesPlan.Add(.45f, new TideStep(1));
            _tidesPlan.Add(.55f, new TideStep(2));
            _tidesPlan.Add(.75f, new TideStep(4));
        }

        _tidesPlanIndixes = new List<float>(_tidesPlan.Keys);
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