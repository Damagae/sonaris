using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationData : MonoBehaviour {

	// Camera
	public int cameraIndex = 2;

	// Light 1
	public float lightIntensity1 = 0;
	public Color lightColor1 = Color.white;
	public int lightLocationIndex1 = 0;

	// Light 2
	public float lightIntensity2 = 0;
	public Color lightColor2 = Color.white;
	public int lightLocationIndex2;

	// Light 3
	public float lightIntensity3 = 0;
	public Color lightColor3 = Color.white;
	public int lightLocationIndex3 = 0;

	// Day & Night
	public bool day = true;
	public bool rain = false;

	// PRIVATE
	private double cameraIndex1 = 0;
	private double cameraIndex2 = 0.5;
	private double cameraIndex3 = 1;

	private double cameraWeight1 = 1;
	private double cameraWeight2 = 0;
	private double cameraWeight3 = 0;

	private float lightR1 = 1;
	private float lightG1 = 1;
	private float lightB1 = 1;

	private float lightR2 = 1;
	private float lightG2 = 1;
	private float lightB2 = 1;

	private float lightR3 = 1;
	private float lightG3 = 1;
	private float lightB3 = 1;

	private double daySum = 0;
	private double rainSum = 0;


	public void Fill(AssociativeArray<double> array) {

		if (array["cameraIndex1"] >= 0) {
			cameraIndex1 = (double) Math.Round(array["cameraIndex1"], 2);
		}

		if (array["cameraWeight1"] >= 0) {
			cameraWeight1 = (double) array["cameraWeight1"];
		}

		if (array["cameraIndex2"] >= 0) {
			cameraIndex2 = (double) Math.Round(array["cameraIndex2"], 2);
		}

		if (array["cameraWeight2"] >= 0) {
			cameraWeight2 = (double) array["cameraWeight2"];
		}

		if (array["cameraIndex3"] >= 0) {
			cameraIndex3 = (double) Math.Round(array["cameraIndex3"], 2);
		}

		if (array["cameraWeight3"] >= 0) {
			cameraWeight3 = (double) array["cameraWeight3"];
		}

		if (array["lightIntensity1"] >= 0) {
			lightIntensity1 = (float) array["lightIntensity1"] * 150;
		}

		if (array["lightIntensity2"] >= 0) {
			lightIntensity2 = (float) array["lightIntensity2"] * 150;
		}

		if (array["lightIntensity3"] >= 0) {
			lightIntensity3 = (float) array["lightIntensity3"] * 150;
		}

		if (array["lightR1"] >= 0) {
			lightR1 = (float) array["lightR1"];
		}

		if (array["lightG1"] >= 0) {
			lightG1 = (float) array["lightG1"];
		}

		if (array["lightB1"] >= 0) {
			lightB1 = (float) array["lightB1"];
		}

		if (array["lightR2"] >= 0) {
			lightR2 = (float) array["lightR2"];
		}

		if (array["lightG2"] >= 0) {
			lightG2 = (float) array["lightG2"];
		}

		if (array["lightB2"] >= 0) {
			lightB2 = (float) array["lightB2"];
		}

		if (array["lightR3"] >= 0) {
			lightR3 = (float) array["lightR3"];
		}

		if (array["lightG3"] >= 0) {
			lightG3 = (float) array["lightG3"];
		}

		if (array["lightB3"] >= 0) {
			lightB3 = (float) array["lightB3"];
		}

		if (array["lightLocationIndex1"] >= 0) {
			lightLocationIndex1 = (int) Math.Round(array["lightLocationIndex1"] * 100);
		}

		if (array["lightLocationIndex2"] >= 0) {
			lightLocationIndex2 = (int) Math.Round(array["lightLocationIndex2"] * 100);
		}

		if (array["lightLocationIndex3"] >= 0) {
			lightLocationIndex3 = (int) Math.Round(array["lightLocationIndex3"] * 100);
		}

		if (array["day1"] >= 0) {
			daySum += (double) array["day1"];
		}

		if (array["day2"] >= 0) {
			daySum += (double) array["day2"];
		}

		if (array["day3"] >= 0) {
			daySum += (double) array["day3"];
		}

		if (array["rain1"] >= 0) {
			rainSum += (double) array["rain1"];
		}

		if (array["rain2"] >= 0) {
			rainSum += (double) array["rain2"];
		}

		if (array["rain3"] >= 0) {
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
		double maxWeight = weight1;
		if (weight2 > maxWeight) {
			maxWeight = weight2;
		} else if (weight3 > maxWeight) {
			maxWeight = weight3;
		}
		// Debug.Log("weight 1 = " + weight1 + " - weight 2 = " + weight2 + " - weight 3 = " + weight3);

		if (maxWeight == weight1) {
			// Debug.Log("index1 = " + index1 + " = " + Math.Round(index1 * 15));
			return (int) Math.Round(index1 * 15);
		}

		if (maxWeight == weight2) {
			return (int) Math.Round(index2 * 15);
		}

		if (maxWeight == weight3) {
			return (int) Math.Round(index3 * 15);
		}

		return (int) Math.Round(index1 * 15);
	}

	public bool IsReady() {
		// return (cameraIndex != null &&
		// 				lightIntensity1 != null && lightIntensity2 != null && lightIntensity3 != null &&
		// 				lightColor1 != null && lightColor2 != null && lightColor3 != null &&
		// 				lightLocationIndex1 != null && lightLocationIndex2 != null && lightLocationIndex3 != null &&
		// 				day != null);
		// }
		return true;
	}
}
