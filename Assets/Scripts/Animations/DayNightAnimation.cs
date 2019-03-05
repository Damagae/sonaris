using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightAnimation : MonoBehaviour {

	public GameObject lightGO;
	public GameObject rainGO;

	private Animator lightAnim;

	private Color dayFogColor;
	private Color nightFogColor;

	private Color rainDayFogColor;
	private Color rainNightFogColor;

	private bool rain;

	private string state = "day";

	// DEBUG
	public bool day = true;

	// Use this for initialization
	void Start () {
		lightAnim = lightGO.GetComponent<Animator>();

		dayFogColor = new Color(0.835f, 0.984f, 1, 1);
		rainDayFogColor = new Color(0.835f, 0.984f, 1, 1);

		nightFogColor = new Color(0.56f, 0.533f, 0.878f, 1);
		rainNightFogColor = new Color(0.56f, 0.533f, 0.878f, 1);

		RenderSettings.fogColor = dayFogColor;
	}

	private void SetDayOn() {
		// Light day
		lightAnim.SetBool("day", true);
		if (rain) {
			AnimateFog(rainNightFogColor, rainDayFogColor);
		} else {
			AnimateFog(nightFogColor, dayFogColor);
		}
		// RenderSettings.fogColor = dayFogColor;

	}

	private void SetNightOn() {
		// Light moon
		lightAnim.SetBool("day", false);
		if (rain) {
			AnimateFog(rainDayFogColor, rainNightFogColor);
		} else {
			AnimateFog(dayFogColor, nightFogColor);
		}
		// RenderSettings.fogColor = nightFogColor;
	}

	public void SetState(string newState) {
		if (newState == "night" || newState == "day") {
			state = newState;
		}
	}

	public void SetRain(bool setRain) {
		rain = setRain;
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

		if (rain) {
			rainGO.SetActive(true);
		} else {
			rainGO.SetActive(false);
		}

	}
}
