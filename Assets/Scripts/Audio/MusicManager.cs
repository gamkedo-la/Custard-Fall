using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour {
	public AudioSource musicPrefab;

	private AudioSource currentBase, currentTop;
	private List<AudioClip> currentBaseList = new List<AudioClip>();
	private double startTime = 0.0, nextCueTime = 0.0;
	private float bpm = 90f;

	private void Play() {
		
	}
}
