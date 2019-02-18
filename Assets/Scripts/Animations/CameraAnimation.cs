using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimation : MonoBehaviour {

	public Transform[] positions;
	private Transform currentPosition;

	// DEBUG
	public int indexPos = 0;

	// Use this for initialization
	void Start () {
		currentPosition = positions[indexPos];
		Debug.Log(currentPosition.position);
	}

	// Update is called once per frame
	void Update () {
		if (currentPosition != positions[indexPos]) {
			currentPosition = positions[indexPos];
			transform.position = currentPosition.position;
			Debug.Log(currentPosition.position);
		}
	}

	public void SetCameraIndex(int index) {
		if (index < positions.Length) {
			indexPos = index;
		}
	}
}
