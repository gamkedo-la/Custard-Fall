using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBrain : MonoBehaviour {
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
			if (dayNightCycle.time < lastTime) holdForLoop = false;

			if (dayNightCycle.time >= dayNightTracks[nextTimeIndex].time && !holdForLoop) {
				MusicManager.Instance.StartTrack(dayNightTracks[nextTimeIndex].musicTrack);
				nextTimeIndex = (nextTimeIndex + 1) % dayNightTracks.Count;
				if (nextTimeIndex == 0) holdForLoop = true;
				lastTime = dayNightCycle.time;
			}

			lastTime = dayNightCycle.time;
		} else {
			dayNightCycle = FindObjectOfType<DayNightCycle>();
		}
	}
}

[System.Serializable]
public class MusicTrackTimePair {
	public MusicTrack musicTrack;
	public float time;
}