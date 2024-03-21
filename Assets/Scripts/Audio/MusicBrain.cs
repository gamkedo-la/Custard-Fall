using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicBrain : MonoBehaviour
{
    [NonReorderable] public List<MusicTrackTimePair> dayNightTracks;
    public AudioClip custardDangerClip, up1Clip, up2Clip, up3Clip, down1Clip, down2Clip, down3Clip;

    private AudioSource currentCustardLevelSource;
    private int nextTimeIndex = 0;
    private bool holdForLoop = false;
    private float lastTime = 0f;

    private int currentLevel = 2;
    private bool dangerStarted = false;

    private DayNightCycle dayNightCycle;
    private Tidesmanager tidesManager;
    private Player player;

    [SerializeField] private float volumeDanger = .3f;
    [SerializeField] private float volumeStinger = 1f;

    private int _custardLevelCounter = 0;
    private bool _isAllInit = false;

    private void Start()
    {
        StartCoroutine(WaitForRealLevel());
        if (SceneManager.GetActiveScene().name == "TitleScene")
            MusicManager.Instance.StartTrack(dayNightTracks[0].musicTrack);
        dayNightCycle = FindObjectOfType<DayNightCycle>();
    }

    private IEnumerator WaitForRealLevel()
    {
        do
        {
            tidesManager = FindObjectOfType<Tidesmanager>();
            dayNightCycle = FindObjectOfType<DayNightCycle>();
            yield return new WaitForSeconds(0.5f);
        } while (!tidesManager);

        currentLevel = tidesManager.custardManager.targetTideLevel;
        _isAllInit = true;
    }

    private void Update()
    {
        // Day Night music logic
        if (dayNightCycle != null)
        {
            if (TimeManager.Instance.time < lastTime) holdForLoop = false;

            if (TimeManager.Instance.time >= dayNightTracks[nextTimeIndex].time && !holdForLoop)
            {
                MusicManager.Instance.StartTrack(dayNightTracks[nextTimeIndex].musicTrack);
                nextTimeIndex = (nextTimeIndex + 1) % dayNightTracks.Count;
                if (nextTimeIndex == 0) holdForLoop = true;
                lastTime = TimeManager.Instance.time;
            }

            lastTime = TimeManager.Instance.time;
        }

        if (_isAllInit)
        {
            int targetTide = tidesManager.custardManager.targetTideLevel;
            if (currentLevel != targetTide)
            {
                // Debug.Log("target tide " + targetTide);
                AudioClip clipToPlay = null;

                // change stinger sound every time the custard changes in the one direction or the other
                if (targetTide > currentLevel)
                {
                    if (_custardLevelCounter < 0)
                    {
                        _custardLevelCounter = 1;
                    }
                    else
                    {
                        _custardLevelCounter++;
                    }
                }
                else if (targetTide < currentLevel)
                {
                    if (_custardLevelCounter > 0)
                    {
                        _custardLevelCounter = -1;
                    }
                    else
                    {
                        _custardLevelCounter--;
                    }
                }

                if (_custardLevelCounter == 1)
                {
                    clipToPlay = up1Clip;
                }
                else if (_custardLevelCounter == 2)
                {
                    clipToPlay = up2Clip;
                }
                else if (_custardLevelCounter >= 3)
                {
                    clipToPlay = up3Clip;
                }
                else if (_custardLevelCounter == -1)
                {
                    clipToPlay = down1Clip;
                }
                else if (_custardLevelCounter == -2)
                {
                    clipToPlay = down2Clip;
                }
                else if (_custardLevelCounter <= -3)
                {
                    clipToPlay = down3Clip;
                }

                MusicManager.Instance.SchedualTop(clipToPlay, volumeStinger, false);

                currentLevel = targetTide;
            }
        }

        if (player)
        {
            if (player.IsSwimming)
            {
                Danger(true);
            }
            else
            {
                Coords cellPosition = player.worldCells.GetCellPosition(player.transform.position);
                Danger(player.worldCells.GetHeightAt(cellPosition) < currentLevel - 1);
            }
        }
        else
        {
            player = FindObjectOfType<Player>();
            Danger(false);
        }
    }

    public void Danger(bool value)
    {
        if (value)
        {
            if (!dangerStarted)
            {
                currentCustardLevelSource = MusicManager.Instance.SchedualTop(custardDangerClip, volumeDanger);
                // Debug.Log("Player in Danger");
            }

            dangerStarted = true;
        }
        else if (dangerStarted && currentCustardLevelSource != null)
        {
            StartCoroutine(MusicManager.Instance.FadeOut(currentCustardLevelSource, 0.375f));
            Destroy(currentCustardLevelSource.gameObject, 0.376f);
            currentCustardLevelSource = null;
            dangerStarted = false;
            // Debug.Log("Player out of Danger");
        }
    }
}

[System.Serializable]
public class MusicTrackTimePair
{
    public MusicTrack musicTrack;
    public float time;
}


#if UNITY_EDITOR
[CustomEditor(typeof(MusicBrain))]
[CanEditMultipleObjects]
public class MusicBrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!EditorApplication.isPlaying) return;

        if (GUILayout.Button("Stop Custard"))
        {
            (target as MusicBrain).Danger(false);
        }

        if (GUILayout.Button("Play Custard Up"))
        {
            (target as MusicBrain).Danger(true);
        }
    }
}
#endif