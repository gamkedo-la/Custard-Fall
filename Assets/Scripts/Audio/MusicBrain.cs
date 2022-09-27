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

    private DayNightCycle dayNightCycle;
	private Tidesmanager custardManager;

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

		if (custardManager != null) {
			if (currentLevel != custardManager.currentTideIndex) {
				//Debug.Log(custardManager.currentTideIndex + " " + currentLevel);
				AudioClip clipToPlay = null;

				//if (custardManager.currentTideIndex - currentLevel == 1) {
				//	clipToPlay = up1Clip;
				//} else if (custardManager.currentTideIndex - currentLevel == 2) {
				//	clipToPlay = up2Clip;
				//} else if (custardManager.currentTideIndex - currentLevel >= 3) {
				//	clipToPlay = up3Clip;
				//} else if (custardManager.currentTideIndex - currentLevel == -1) {
				//	clipToPlay = down1Clip;
				//} else if (custardManager.currentTideIndex - currentLevel == -2) {
				//	clipToPlay = down2Clip;
				//} else if (custardManager.currentTideIndex - currentLevel <= -3) {
				//	clipToPlay = down3Clip;
				//}

				if (custardManager.currentTideIndex - currentLevel >= 1) {
					clipToPlay = up1Clip;
					if (custardManager.currentTideIndex >= 3) clipToPlay = up2Clip;
					if (custardManager.currentTideIndex >= 6) clipToPlay = up3Clip;
				} else if (custardManager.currentTideIndex - currentLevel <= -1) {
					clipToPlay = down1Clip;
					if (custardManager.currentTideIndex <= 5) clipToPlay = down2Clip;
					if (custardManager.currentTideIndex <= 3) clipToPlay = down3Clip;
				}

				MusicManager.Instance.SchedualTop(clipToPlay, false);

				currentLevel = custardManager.currentTideIndex;
			}
		} else {
			custardManager = FindObjectOfType<Tidesmanager>();
			currentLevel = custardManager.currentTideIndex;
        }
	}

	public void Danger(bool value) {
		if (value) {
			currentCustardLevelSource = MusicManager.Instance.SchedualTop(custardDangerClip);
		} else if (currentCustardLevelSource != null) {
			StartCoroutine(MusicManager.Instance.FadeOut(currentCustardLevelSource, 0.25f));
			Destroy(currentCustardLevelSource.gameObject, 0.26f);
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