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

	private bool day = true;

	// Use this for initialization
	void Start () {
		lightAnim = lightGO.GetComponent<Animator>();

		// dayFogColor = new Color(0.835f, 0.984f, 1, 1);
		// rainDayFogColor = new Color(0.835f, 0.984f, 1, 1);
		//
		// nightFogColor = new Color(0.56f, 0.533f, 0.878f, 1);
		// rainNightFogColor = new Color(0.56f, 0.533f, 0.878f, 1);

		Color pollution = new Color(0.8679245f, 0.6673193f, 8104347f, 1);

		dayFogColor = pollution;
		rainDayFogColor = pollution;

		nightFogColor = pollution;
		rainNightFogColor = pollution;

		RenderSettings.fogColor = dayFogColor;
	}

	private void SetDayOn() {
		Debug.Log("Day");
		// Light day
		lightAnim.SetBool("day", true);
		// if (rain) {
		// 	AnimateFog(rainNightFogColor, rainDayFogColor);
		// } else {
		// 	AnimateFog(nightFogColor, dayFogColor);
		// }
		// RenderSettings.fogColor = dayFogColor;

	}

	private void SetNightOn() {
		Debug.Log("Night");
		// Light moon
		lightAnim.SetBool("day", false);
		// if (rain) {
		// 	AnimateFog(rainDayFogColor, rainNightFogColor);
		// } else {
		// 	AnimateFog(dayFogColor, nightFogColor);
		// }
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
		RenderSettings.fogColor = Color.Lerp(current, target, Mathf.PingPong(Time.time, 1));
	}



	// Update is called once per frame
	void Update () {

		if (day && state == "night") {
			SetNightOn();
			state = "night";
			day = false;
		} else if (!day && state == "day") {
			SetDayOn();
			state = "day";
			day = true;
		}

		if (rain) {
			rainGO.SetActive(true);
		} else {
			rainGO.SetActive(false);
		}

	}
}
