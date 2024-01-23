using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicBrain : MonoBehaviour {
	[NonReorderable]
	public List<MusicTrackTimePair> dayNightTracks;
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

	void Update() {
		// Day Night music logic
		if (dayNightCycle != null) {
			if (TimeManager.Instance.time < lastTime) holdForLoop = false;

			if (TimeManager.Instance.time >= dayNightTracks[nextTimeIndex].time && !holdForLoop) {
				MusicManager.Instance.StartTrack(dayNightTracks[nextTimeIndex].musicTrack);
				nextTimeIndex = (nextTimeIndex + 1) % dayNightTracks.Count;
				if (nextTimeIndex == 0) holdForLoop = true;
				lastTime = TimeManager.Instance.time;
			}

			lastTime = TimeManager.Instance.time;
		} else {
			if (SceneManager.GetActiveScene().name == "TitleScene")
				MusicManager.Instance.StartTrack(dayNightTracks[0].musicTrack);
			dayNightCycle = FindObjectOfType<DayNightCycle>();
		}

		if (tidesManager != null) {
			int targetTide = tidesManager.CustardManager.targetTideLevel;
			if (currentLevel != targetTide) {
				//Debug.Log(custardManager.currentTideIndex + " " + currentLevel);
				AudioClip clipToPlay = null;

				if (targetTide - currentLevel == 1) {
					clipToPlay = up1Clip;
				} else if (targetTide - currentLevel == 2) {
					clipToPlay = up2Clip;
				} else if (targetTide - currentLevel >= 3) {
					clipToPlay = up3Clip;
				} else if (targetTide - currentLevel == -1) {
					clipToPlay = down1Clip;
				} else if (targetTide - currentLevel == -2) {
					clipToPlay = down2Clip;
				} else if (targetTide - currentLevel <= -3) {
					clipToPlay = down3Clip;
				}

				MusicManager.Instance.SchedualTop(clipToPlay, false);

				currentLevel = targetTide;
			}
		} else if(tidesManager != null){
			tidesManager = FindObjectOfType<Tidesmanager>();
			currentLevel = tidesManager.CustardManager.targetTideLevel;
        }

		if (player) {
			Coords cellPosition = player.worldCells.GetCellPosition(player.transform.position.x, player.transform.position.z);
			Danger(player.worldCells.GetTerrainHeightAt(cellPosition)+1 < currentLevel);
		} else {
			player = FindObjectOfType<Player>();
			Danger(false);
		}
	}

	public void Danger(bool value) {
		if (value) {
			if (!dangerStarted) currentCustardLevelSource = MusicManager.Instance.SchedualTop(custardDangerClip);
			dangerStarted = true;
		} else if (currentCustardLevelSource != null) {
			StartCoroutine(MusicManager.Instance.FadeOut(currentCustardLevelSource, 0.25f));
			Destroy(currentCustardLevelSource.gameObject, 0.26f);
			dangerStarted = false;
		}
	}

}

[System.Serializable]
public class MusicTrackTimePair {
	public MusicTrack musicTrack;
	public float time;
}



#if UNITY_EDITOR
[CustomEditor(typeof(MusicBrain))]
[CanEditMultipleObjects]
public class MusicBrainEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		if (!EditorApplication.isPlaying) return;

		if (GUILayout.Button("Stop Custard")) {
			(target as MusicBrain).Danger(false);
		}

		if (GUILayout.Button("Play Custard Up")) {
			(target as MusicBrain).Danger(true);
		}
	}
}
#endif