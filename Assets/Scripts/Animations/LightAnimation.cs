using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAnimation : MonoBehaviour {

	public Transform[] lightsInGame;

	public void AddLight(int index, float intensity, Color color) {
		if (index < lightsInGame.Length) {
			if (lightsInGame[index].childCount > 0) {
				foreach (Transform transform in lightsInGame[index]) {
					var light = transform.gameObject.GetComponent<Light>();
					StartCoroutine(LightOn(light, intensity, color));
				}
			} else {
				var light = lightsInGame[index].gameObject.GetComponent<Light>();
				StartCoroutine(LightOn(light, intensity, color));
			}
		}
	}

	private IEnumerator LightOn(Light light, float goalIntensity, Color color) {
 		light.intensity = Mathf.Lerp(light.intensity, goalIntensity, 0.5f * Time.deltaTime);
		light.color = color;
		yield return 0;
	}

	private IEnumerator LightOff(Light light) {
		light.intensity = Mathf.Lerp(light.intensity, 0, 0.5f * Time.deltaTime);
		yield return new WaitForSeconds(5);;
	}

	private void RemoveLight(int index) {
		if (index < lightsInGame.Length) {
			if (lightsInGame[index].childCount > 0) {
				foreach (Transform transform in lightsInGame[index]) {
					var light = transform.gameObject.GetComponent<Light>();
					StartCoroutine(LightOff(light));
				}
			} else {
				var light = lightsInGame[index].gameObject.GetComponent<Light>();
				StartCoroutine(LightOff(light));
			}
		}
	}

	public void RemoveLights() {
		if (lightsInGame != null) {
			for (int i = 0; i < lightsInGame.Length; ++i) {
				RemoveLight(i);
			}
		}
	}
}
