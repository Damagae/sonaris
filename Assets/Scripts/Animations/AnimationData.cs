using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationData : MonoBehaviour {

	// Camera
	public int cameraIndex;

	// Light on
	public float lightIntensity;
	public Color lightColor;
	public Vector3 lightLocation;

	// Birds
	public int birdsNbr;
	public float birdsLocation;

	// Rain
	public float rainDensity;
	public float rainSpeed;

	// Day
	public Vector3 sunPosition;
	public float sunIntensity;
	public Color sunColor;

	// Nights
	public Vector3 moonPosition;
	public float moonIntensity;
	public Color moonColor;

	// Leaves
	public Vector3 leavesPosition;
	public int leavesNbr;


	public void Fill(AssociativeArray<double> array) {


	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}
