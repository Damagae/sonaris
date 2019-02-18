using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAnimation : MonoBehaviour {

	public GameObject lightGO;
	private List<Light> lightsInGame;

	// DEBUG
	public bool lightOnSacreCoeur = true;
	public bool done = false;

	// Use this for initialization
	void Start () {
		lightsInGame = new List<Light>();
		if (lightOnSacreCoeur) {
			AddLight(new Vector3(-17.65f, 150.85f, -49.31f));
		}
	}

	// Update is called once per frame
	void Update () {

		// DEBUG
		if (!lightOnSacreCoeur && !done) {
			if (Exists(new Vector3(-17.65f, 150.85f, -49.31f))) {
				var index = GetIndex(new Vector3(-17.65f, 150.85f, -49.31f));
				Debug.Log("index = " + index);
				RemoveLight(index);
				done = true;
			}
		}

	}

	public void AddLight(Vector3 position) {
		Light light = new Light();
		light.position = position;
		light.go = Instantiate(lightGO);
		light.animator = light.go.GetComponent<Animator>();
		light.go.SetActive(true);
		lightsInGame.Add(light);
		light.animator.SetBool("lightOn", true);
	}

	public void AddLight(float x, float y, float z) {
		AddLight(new Vector3(x,y,z));
	}

	public bool Exists(Vector3 position) {
		foreach (var light in lightsInGame) {
			if (light.position == position) {
				return true;
			}
		}
		return false;
	}

	public int GetIndex(Vector3 position) {
		for (int i = 0; i < lightsInGame.Count; ++i) {
			if (lightsInGame[i].position == position) {
				return i;
			}
		}
		return -1;
	}

	public void RemoveLight(int index) {
		var light = lightsInGame[index];
		light.animator.SetBool("lightOn", false);
		Destroy(light.go, 3);
		lightsInGame.Remove(lightsInGame[index]);
	}

	class Light {
		public GameObject go;
		public Vector3 position;
		public Animator animator;
	}
}
