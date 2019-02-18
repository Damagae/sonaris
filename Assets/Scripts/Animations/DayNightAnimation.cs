using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightAnimation : MonoBehaviour {

	public GameObject lightGO;

	private Animator lightAnim;

	// DEBUG
	public bool day = true;

	// Use this for initialization
	void Start () {
		lightAnim = lightGO.GetComponent<Animator>();
		// RenderSettings.fogColor = new Color(143, 136, 224, 255);
	}

	public void SetDayOn() {
		// Light day
		lightAnim.SetBool("day", true);
		// RenderSettings.fogColor = new Color(255, 213, 244, 255);

	}

	public void SetNightOn() {
		// Light moon
		lightAnim.SetBool("day", false);
		// RenderSettings.fogColor = new Color(143, 136, 224, 255);

	}

	// Update is called once per frame
	void Update () {

		if (day) {
			SetDayOn();
		} else {
			SetNightOn();
		}

	}
}
