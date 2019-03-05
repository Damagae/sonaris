using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationData : MonoBehaviour {

	// Camera
	public int cameraIndex;

	// Light 1
	public float lightIntensity1;
	public Color lightColor1;
	public int lightLocationIndex1;

	// Light 2
	public float lightIntensity2;
	public Color lightColor2;
	public int lightLocationIndex2;

	// Light 3
	public float lightIntensity3;
	public Color lightColor3;
	public int lightLocationIndex3;

	// Day & Night
	public bool day;
	public bool rain;

	// // Birds
	// public int birdsNbr;
	// public float birdsLocation;
	//
	// // Leaves
	// public Vector3 leavesPosition;
	// public int leavesNbr;


	public void Fill(AssociativeArray<double> array) {
		double cameraIndex1 = 0;
		double cameraIndex2 = 0.01;
		double cameraIndex3 = 0.02;

		double cameraWeight1 = 0;
		double cameraWeight2 = 0;
		double cameraWeight3 = 0;

		float lightR1 = 1;
		float lightG1 = 1;
		float lightB1 = 1;

		float lightR2 = 1;
		float lightG2 = 1;
		float lightB2 = 1;

		float lightR3 = 1;
		float lightG3 = 1;
		float lightB3 = 1;

		double daySum = 0;
		double rainSum = 0;

		if (array["cameraIndex1"] != null) {
			cameraIndex1 = (double) Math.Round(array["cameraIndex1"], 2);
		}

		if (array["cameraWeight1"] != null) {
			cameraWeight1 = (double) array["cameraWeight1"];
		}

		if (array["cameraIndex2"] != null) {
			cameraIndex2 = (double) Math.Round(array["cameraIndex2"], 2);
		}

		if (array["cameraWeight2"] != null) {
			cameraWeight2 = (double) array["cameraWeight2"];
		}

		if (array["cameraIndex3"] != null) {
			cameraIndex3 = (double) Math.Round(array["cameraIndex3"], 2);
		}

		if (array["cameraWeight3"] != null) {
			cameraWeight3 = (double) array["cameraWeight3"];
		}

		if (array["lightIntensity1"] != null) {
			lightIntensity1 = (float) array["lightIntensity1"];
		}

		if (array["lightIntensity2"] != null) {
			lightIntensity2 = (float) array["lightIntensity2"];
		}

		if (array["lightIntensity3"] != null) {
			lightIntensity3 = (float) array["lightIntensity3"];
		}

		if (array["lightR1"] != null) {
			lightR1 = (float) array["lightR1"];
		}

		if (array["lightG1"] != null) {
			lightG1 = (float) array["lightG1"];
		}

		if (array["lightB1"] != null) {
			lightB1 = (float) array["lightB1"];
		}

		if (array["lightR2"] != null) {
			lightR2 = (float) array["lightR2"];
		}

		if (array["lightG2"] != null) {
			lightG2 = (float) array["lightG2"];
		}

		if (array["lightB2"] != null) {
			lightB2 = (float) array["lightB2"];
		}

		if (array["lightR3"] != null) {
			lightR3 = (float) array["lightR3"];
		}

		if (array["lightG3"] != null) {
			lightG3 = (float) array["lightG3"];
		}

		if (array["lightB3"] != null) {
			lightB3 = (float) array["lightB3"];
		}

		if (array["lightLocationIndex1"] != null) {
			lightLocationIndex1 = (int) Math.Round(array["lightLocationIndex1"] * 100);
		}

		if (array["lightLocationIndex2"] != null) {
			lightLocationIndex2 = (int) Math.Round(array["lightLocationIndex2"] * 100);
		}

		if (array["lightLocationIndex3"] != null) {
			lightLocationIndex3 = (int) Math.Round(array["lightLocationIndex3"] * 100);
		}

		if (array["day1"] != null) {
			daySum += (double) array["day1"];
		}

		if (array["day2"] != null) {
			daySum += (double) array["day2"];
		}

		if (array["day3"] != null) {
			daySum += (double) array["day3"];
		}

		if (array["rain1"] != null) {
			rainSum += (double) array["rain1"];
		}

		if (array["rain2"] != null) {
			rainSum += (double) array["rain2"];
		}

		if (array["rain3"] != null) {
			rainSum += (double) array["rain3"];
		}

		var binaryDay = Math.Round(daySum / 3) % 2;
		day = (binaryDay == 1);

		var binaryRain = Math.Round(rainSum / 3) % 2;
		rain = (binaryRain == 1);

		lightColor1 = new Color(lightR1, lightG1, lightB1);
		lightColor2 = new Color(lightR2, lightG2, lightB2);
		lightColor3 = new Color(lightR3, lightG3, lightB3);
		cameraIndex = GetCameraIndex(cameraIndex1, cameraIndex2, cameraIndex3, cameraWeight1, cameraWeight2, cameraWeight3);

	}

	private int GetCameraIndex(double index1, double index2, double index3, double weight1, double weight2, double weight3) {
		var maxWeight = weight1;
		maxWeight = Math.Max(maxWeight, weight2);
		maxWeight = Math.Max(maxWeight, weight2);

		if (maxWeight == weight1) {
			return (int) Math.Round(index1 * 100);
		}

		if (maxWeight == weight1) {
			return (int) Math.Round(index2 * 100);
		}

		if (maxWeight == weight3) {
			return (int) Math.Round(index3 * 100);
		}

		return (int) Math.Round(index1 * 100);
	}

	public bool IsReady() {
		return (cameraIndex != null &&
						lightIntensity1 != null && lightIntensity2 != null && lightIntensity3 != null &&
						lightColor1 != null && lightColor2 != null && lightColor3 != null &&
						lightLocationIndex1 != null && lightLocationIndex2 != null && lightLocationIndex3 != null &&
						day != null);
	}
}
