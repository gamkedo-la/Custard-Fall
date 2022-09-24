
    using System;
    using System.Collections.Generic;
    using Custard;
    using UnityEngine;

    public class Tidesmanager : MonoBehaviour
    {
        public CustardManager CustardManager;
        public Player Player;

        public List<TideStep> tidesPlan = new List<TideStep>();
        public float speedFactor = 1f;
        public bool pause = false;
        public bool realCustardFallCycle;

        public int currentTideStep;
        private float _updateCountdown;

        private void Start()
        {
            InitPlannedTideBehavior();
            currentTideStep = 0;
            var tideStep = tidesPlan[currentTideStep];
            _updateCountdown = tideStep.getSecondsToLast();
            CustardManager.targetTideLevel = tideStep.getLevel();
        }

        private void FixedUpdate()
        {
            if (!CustardManager.pauseIterationCountDown && !pause)
                _updateCountdown -= Time.deltaTime * speedFactor;
            
                if (_updateCountdown <= 0.001f)
                {
                    // cycle tide play
                    currentTideStep++;
                    if (currentTideStep >= tidesPlan.Count)
                        currentTideStep = 0;
                    
                    // reset the countdown
                    var tideStep = tidesPlan[currentTideStep];
                    _updateCountdown = tideStep.getSecondsToLast();

                    // custard ai
                    var nextTideLevel = tideStep.getLevel();
                    CustardManager.targetTideLevel = nextTideLevel;
                    // TODO center custard update roughly at some distance around the player
                    CustardManager.SeedCustardUpdate((int)Math.Floor(Time.time * 1000));
                    CustardManager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/2);
                    CustardManager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/4);
                    CustardManager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/8);
                }
        }

        protected void InitPlannedTideBehavior()
        {
            // targeting a 8min day like in Don't Starve
            if (realCustardFallCycle)
            {
                var baseDuration = 45f;
                tidesPlan.Add(new TideStep(7, baseDuration));
                tidesPlan.Add(new TideStep(5, baseDuration));
                tidesPlan.Add(new TideStep(3, baseDuration * 2));
                tidesPlan.Add(new TideStep(7, baseDuration));
                tidesPlan.Add(new TideStep(8, baseDuration));
                tidesPlan.Add(new TideStep(9, baseDuration));
                tidesPlan.Add(new TideStep(10, baseDuration));
                // supposed to be a nice shocker
                tidesPlan.Add(new TideStep(10, 30f));
            }
            else
            {
                var baseDuration = 45f;
                tidesPlan.Add(new TideStep(3, baseDuration));
                tidesPlan.Add(new TideStep(2, baseDuration *2));
                tidesPlan.Add(new TideStep(1, baseDuration));
                tidesPlan.Add(new TideStep(2, baseDuration *2));
                tidesPlan.Add(new TideStep(3, baseDuration));
                tidesPlan.Add(new TideStep(4, baseDuration));
                // supposed to be a nice shocker
                tidesPlan.Add(new TideStep(5, 15f));
            }
        }


        public class TideStep
        {
            private int level;
            private float secondsToLast;

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
