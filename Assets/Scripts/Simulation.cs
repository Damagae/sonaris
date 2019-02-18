using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour {

	public Brain brain;
	public AnimationsManager animManager;

	public float updateRate = 1; // An update every *updateRate* frame


	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		AssociativeArray<double> input = new AssociativeArray<double>();
		var currentTime = Time.time;

		if (currentTime % updateRate == 0) {
			var output = brain.Run(input);
			animManager.SetData(output);
		}


	}
}
