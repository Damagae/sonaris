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
	private AnimationData previousData;

	// Use this for initialization
	void Start () {
		data = new AnimationData();

		if (data.IsReady()) {
			cameraAnim.SetCameraIndex(data.cameraIndex);
			lightAnim.AddLight(data.lightLocationIndex);
		}
		previousData = data;
	}

	public void Animate() {
		if (data.IsReady()) {
			cameraAnim.SetCameraIndex(data.cameraIndex);
		}
	}

	public void SetData(AssociativeArray<double> array) {
		data.Fill(array);
	}
}
