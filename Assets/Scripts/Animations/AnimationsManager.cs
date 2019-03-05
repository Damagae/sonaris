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
	public DayNightAnimation dayAnim;

	// Animation Data
	private AnimationData data;
	private AnimationData previousData;

	// Use this for initialization
	void Start () {
		data = new AnimationData();
		previousData = data;
	}

	public void Animate() {
		if (data.IsReady() && data != previousData) {
			// Camera
			cameraAnim.SetCameraIndex(data.cameraIndex);

			// Lights
			lightAnim.RemoveLights();
			lightAnim.AddLight(data.lightLocationIndex1, data.lightIntensity1, data.lightColor1);
			lightAnim.AddLight(data.lightLocationIndex2, data.lightIntensity2, data.lightColor2);
			lightAnim.AddLight(data.lightLocationIndex3, data.lightIntensity3, data.lightColor3);

			// Day Night
			dayAnim.SetState(data.day ? "day" : "night");
			dayAnim.SetRain(data.rain);

			previousData = data;
		}
	}

	public void SetData(AssociativeArray<double> array) {
		data.Fill(array);
	}
}
