using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ---------------------------------------------------------------------------
// Simulation Manager
// ******************
// Fetch the audio data from Max MSP
// Runs the neural network on the given input
// Send the corresponding output to the Animation Manager
// ---------------------------------------------------------------------------
public class Simulation : MonoBehaviour {

	public Brain brain;
	public AnimationsManager animManager;
	public SimpleReceiver simpleReceiver;

	public int updatePeriod = 200; // An update every *updatePeriod* frame

	public bool train = false;
	public bool simulation = true;

	private AssociativeArray<double> input;
	private AssociativeArray<double> output;

	void Start () {
		input = new AssociativeArray<double>();
		output = new AssociativeArray<double>();

		if (train) {
			brain.Train();
		}
	}

	void Update () {
		if (Time.frameCount % updatePeriod == 0 && simulation) {
			Debug.Log("update");
			GetInputData(); // Fetch the data from Max MSP
			if (input.Count > 0) {
				Debug.Log(input.ToString());
				// output = brain.Run(input); // Feed it to the neural network *brain*
				// animManager.SetData(output); // Send the resulting output to the animation manager
				// animManager.Animate();
			}
		}
	}

	private void GetInputData() {
		var data = simpleReceiver.GetData();
		input.Clear();
		foreach (var element in data) {
			if (element.Count == 2) {
				string key = (string) element[0];
				float value = (float) element[1];
				input.Add(key, value);
			}
		}
	}
}
