using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationData : MonoBehaviour {

	// Camera
	public int cameraIndex;

	// Light on
	public float lightIntensity;
	public Color lightColor;
	public int lightLocationIndex;

	// Day & Night
	public bool day;

	// // Birds
	// public int birdsNbr;
	// public float birdsLocation;
	//
	// // Rain
	// public float rainDensity;
	// public float rainSpeed;
	//
	// // Day
	// public Vector3 sunPosition;
	// public float sunIntensity;
	// public Color sunColor;
	//
	// // Nights
	// public Vector3 moonPosition;
	// public float moonIntensity;
	// public Color moonColor;
	//
	// // Leaves
	// public Vector3 leavesPosition;
	// public int leavesNbr;


	public void Fill(AssociativeArray<double> array) {
		float lightR = 1f;
		float lightG = 1f;
		float lightB = 1f;

		float locationX = 0;
		float locationY = 0;
		float locationZ = 0;

		if (array["cameraIndex"] != null) {
			cameraIndex = (int) Math.Round(array["cameraIndex"]);
		}

		if (array["lightIntensity"] != null) {
			lightIntensity = (float) array["lightIntensity"];
		}

		if (array["lightR"] != null) {
			lightR = (float) array["lightR"];
		}

		if (array["lightG"] != null) {
			lightG = (float) array["lightG"];
		}

		if (array["lightB"] != null) {
			lightB = (float) array["lightB"];
		}

		if (array["lightLocationIndex"] != null) {
			lightLocationIndex = (int) Math.Round(array["lightLocationIndex"]);
		}

		if (array["day"] != null) {
			var binary = Math.Round(array["day"]) % 2;
			day = (binary == 1);
		}

		lightColor = new Color(lightR, lightG, lightB);

	}

	public bool IsReady() {
		return (cameraIndex != null && lightIntensity != null && lightColor != null && lightLocationIndex != null && day != null);
	}
}
