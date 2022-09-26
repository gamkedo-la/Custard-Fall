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
	public AudioClip custardUpClip, custardDownClip;

    private AudioSource currentCustardLevelSource;
	private int nextTimeIndex = 0;
	private bool holdForLoop = false;
	private float lastTime = 0f;

    private DayNightCycle dayNightCycle;

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
	}

	public void CustardMusic(int level) {
		if (currentCustardLevelSource != null) {
			StartCoroutine(MusicManager.Instance.FadeOut(currentCustardLevelSource, 0.25f));
			Destroy(currentCustardLevelSource.gameObject, 0.26f);
		}

		if (level == 1) {
			currentCustardLevelSource = MusicManager.Instance.SchedualTop(custardUpClip);
		}

		if (level == 2) {
			currentCustardLevelSource = MusicManager.Instance.SchedualTop(custardDownClip);
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
			(target as MusicBrain).CustardMusic(0);
		}

		if (GUILayout.Button("Play Custard Up")) {
			(target as MusicBrain).CustardMusic(1);
		}

		if (GUILayout.Button("Play Custard Down")) {
			(target as MusicBrain).CustardMusic(2);
		}
	}
}
#endif