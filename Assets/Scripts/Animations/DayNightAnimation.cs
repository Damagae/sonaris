using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightAnimation : MonoBehaviour {

	public GameObject lightGO;

	private Animator lightAnim;

	private Color dayFogColor;
	private Color nightFogColor;

	private string state = "day";

	// DEBUG
	public bool day = true;

	// Use this for initialization
	void Start () {
		lightAnim = lightGO.GetComponent<Animator>();
		dayFogColor = new Color(1, 0.835f, 0.878f, 1);
		nightFogColor = new Color(0.56f, 0.533f, 0.878f, 1);
		RenderSettings.fogColor = dayFogColor;
	}

	public void SetDayOn() {
		// Light day
		lightAnim.SetBool("day", true);
		AnimateFog(nightFogColor, dayFogColor);
		// RenderSettings.fogColor = dayFogColor;

	}

	public void SetNightOn() {
		// Light moon
		lightAnim.SetBool("day", false);
		AnimateFog(dayFogColor, nightFogColor);
		// RenderSettings.fogColor = nightFogColor;

	}

	private void AnimateFog(Color current, Color target) {
		Debug.Log("animation");
		RenderSettings.fogColor = Color.Lerp(current, target, Mathf.PingPong(Time.time, 1));
	}

	// Update is called once per frame
	void Update () {

		if (day && state == "night") {
			SetDayOn();
			state = "day";
		} else if (!day && state == "day") {
			SetNightOn();
			state = "night";
		}

	}
}
