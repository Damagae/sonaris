using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ---------------------------------------------------------------------------
// Animations Manager
// Triggers animations depending on the values of Animation Data
// ---------------------------------------------------------------------------
public class AnimationsManager : MonoBehaviour {

	// Toutes les animations
	public CameraAnimation cameraAnim;
	public LightAnimation lightAnim;

	// Animation Data
	private AnimationData data;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		cameraAnim.SetCameraIndex(data.cameraIndex);
	}

	public void SetData(AssociativeArray<double> array) {
		data.Fill(array);
	}
}
