using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour {
	public static MusicManager Instance;

	public AudioSource musicSourcePrefab;

	public bool playMusicOnStart = true;
	private bool musicPlaying = false;
	private bool musicUnder = false;

	public MusicTrack currentTrack;
	private MusicClip currentClip;
	private AudioSource currentBaseSource, currentUnderSource;

	public double currentTime = 0.0;
	private double trackStartTime = 0.0, nextTrackTime = 0.0, beatTime = 0.0, nextBeatTime = 0.0, bufferTime = 0.5;
	[SerializeField]
	private float bpm = 90f;
	[SerializeField]
	private float fadeTime = 0.5f;
	private int beatLength = 2;
	[SerializeField] private float volumeBase = 0.5f;
	[SerializeField] private float volumeUnder = 1;

	void Awake() {
		transform.parent = null;
		currentTime = AudioSettings.dspTime;

		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		if (playMusicOnStart && currentTrack != null) {
			StartTrack(currentTrack);
		}
	}

	void Update() {
		currentTime = AudioSettings.dspTime;

		if (musicPlaying) {
			if (currentTime >= nextBeatTime - bufferTime) {
				nextBeatTime += beatTime;
			}

			if (currentTime >= nextTrackTime - bufferTime) {
				PlayNewClip(nextTrackTime - currentTime);
			}
		}
	}

	public void StartTrack(MusicTrack track) {
		currentTime = AudioSettings.dspTime;

		if (musicPlaying) {
			currentTrack = track;
		} else {
			currentTrack = track;
			PlayNewClip(bufferTime + 2);

			musicPlaying = true;
		}
	}

	public AudioSource SchedualTop(AudioClip topClip, bool loop = true) {
		AudioSource freshSource = Instantiate(musicSourcePrefab).GetComponent<AudioSource>();
		freshSource.clip = topClip;
		freshSource.loop = loop;
		freshSource.volume = volumeBase;
		freshSource.PlayScheduled(nextBeatTime);
		Debug.Log("play next "+topClip.name);
		return freshSource;
	}
	
	public AudioSource SchedualTop(AudioClip topClip, float volume, bool loop = true) {
		AudioSource freshSource = Instantiate(musicSourcePrefab).GetComponent<AudioSource>();
		freshSource.clip = topClip;
		freshSource.loop = loop;
		freshSource.volume = volume;
		freshSource.PlayScheduled(nextBeatTime);
		// Debug.Log("play next "+topClip.name);
		return freshSource;
	}

	public void SetUnder(bool isUnder) {
		if (isUnder == musicUnder) return;
		else {
			if (!musicPlaying) {
				musicUnder = isUnder;
				return;
			}
			if (isUnder) {
				StartCoroutine(FadeOut(currentBaseSource, fadeTime));
				StartCoroutine(FadeIn(currentUnderSource, fadeTime));
			} else {
				StartCoroutine(FadeOut(currentUnderSource, fadeTime));
				StartCoroutine(FadeIn(currentBaseSource, fadeTime));
			}
			musicUnder = isUnder;
		}
	}

	private void PlayNewClip(double delay) {
		currentTime = AudioSettings.dspTime;
		currentClip = currentTrack.GetMusicClip();

		//Debug.Log("<<< " + currentTime + " " + nextTrackTime + " " + (nextTrackTime - currentTime));

		trackStartTime = currentTime + delay;
		nextTrackTime = trackStartTime + currentClip.endTime;
		nextBeatTime = trackStartTime;
		CalculateBeatTime();

		currentBaseSource = Instantiate(musicSourcePrefab, gameObject.transform, true);
		currentUnderSource = Instantiate(musicSourcePrefab, gameObject.transform, true);
		currentBaseSource.clip = currentClip.clip;
		currentUnderSource.clip = currentClip.underClip;
		currentBaseSource.name = currentClip.clip.name;
		currentUnderSource.name = currentClip.underClip.name;
		currentBaseSource.volume = volumeBase;
		currentUnderSource.volume = volumeUnder;
		currentBaseSource.PlayScheduled(trackStartTime);
		currentUnderSource.PlayScheduled(trackStartTime);

		if (musicUnder) currentBaseSource.volume = 0f;
		else currentUnderSource.volume = 0f;

		Destroy(currentBaseSource.gameObject, currentBaseSource.clip.length);
		Destroy(currentUnderSource.gameObject, currentUnderSource.clip.length);

		//Debug.Log(">>> " + currentTime + " " + trackStartTime + " " + (trackStartTime - currentTime));
		//Debug.Log("vvv " + trackStartTime + " " + nextTrackTime + " " + (nextTrackTime - trackStartTime));
	}

	private void CalculateBeatTime() {
		if (currentClip != null) beatLength = currentClip.BeatInEightNotes;
		beatTime = (30.0 * beatLength) / bpm;
	}

    //--//Automation
    public IEnumerator FadeIn(AudioSource source, float fadeTime) {
		float startTime = Time.unscaledTime;
		float currentTime = 0f;

		source.volume = 0f;

		while (startTime + fadeTime > Time.unscaledTime) {
			currentTime = Time.unscaledTime - startTime;

			source.volume = Mathf.Lerp(0f, volumeBase, currentTime / fadeTime);
			yield return null;
		}

		source.volume = volumeBase;
	}

	public IEnumerator FadeOut(AudioSource source, float fadeTime) {
		float startTime = Time.unscaledTime;
		float currentTime = 0f;

		source.volume = volumeBase;
		float startVolume = source.volume;

		while (startTime + fadeTime > Time.unscaledTime) {
			currentTime = Time.unscaledTime - startTime;

			if (!source) yield break;
			source.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
			yield return null;
		}

		if (!source) yield break;
		source.volume = 0f;
	}

}

[System.Serializable]
public class MusicClip {
	public AudioClip clip;
	public AudioClip underClip;
	public double endTime;
	public int BeatInEightNotes = 2;
}

[System.Serializable]
public class MusicClipWeightPair {
	public MusicClip clip;
	public float weight = 1f;
}

[System.Serializable]
public class MusicTrack {
	[NonReorderable]
	public List<MusicClipWeightPair> clips;

	public MusicClip GetMusicClip() {
		float total = 0f;
		for (int i = 0; i < clips.Count; i++) {
			total += clips[i].weight;
		}
		float randValue = Random.Range(0f, total);
		float counted = 0f;
		for (int i = 0; i < clips.Count; i++) {
			if (randValue <= clips[i].weight + counted) return clips[i].clip;
			counted += clips[i].weight;
		}

		return clips[0].clip;
	}
}