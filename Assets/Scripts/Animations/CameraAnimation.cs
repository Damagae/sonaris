using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimation : MonoBehaviour {

	private Animator animator;

	// DEBUG
	public int indexPos = 1;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		animator.SetInteger("index", indexPos);
	}

	// Update is called once per frame
	void Update () {
		if (animator.GetInteger("index") != indexPos) {
			Debug.Log("Camera " + indexPos);
			animator.SetInteger("index", indexPos);
		}
	}

	public void SetCameraIndex(int index) {
		indexPos = index;
	}
}
