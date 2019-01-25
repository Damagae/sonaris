using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

	private ArrayList sizes;
	private int outputLayer;
	private ArrayList biases;
	private ArrayList weights;
	private ArrayList outputs;
	private ArrayList deltas;
	private ArrayList changes;
	private ArrayList errors;
	private int errorCheckInterval = 1;
	private float[] inputLookup = null;
	private int inputLookupLength = -1;
	private float[] outputLookup = null;
	private int outputLookupLength = -1;

	// Default
	private float leakyReluAlpha = 0.01f;
	private float binaryThresh = 0.5f;
	private ArrayList hiddenLayers = null;
	public string activation = "sigmoid"; // leaky-relu et relu ne marchent pas
	private Func<float> runActivation = null;
	private Func<float> deltaCalculation = null;

	// Train Default
	private int iterations = 20000;    // the maximum times to iterate the training data
	private float errorThresh = 0.005f;   // the acceptable error percentage from training data
	private bool log = false;           // true to use console.log, when a function is supplied it is used
	private int logPeriod = 10;        // iterations between logging out
	private float learningRate = 0.3f;    // multiply's against the input and the delta then adds to momentum
	private float momentum = 0.1f;        // multiply's against the specified "change" then adds to learning rate for change
	private double timeout = double.PositiveInfinity;    // the max number of milliseconds to train for

	private int inputLength = 0; // Replace data[0].input.length
	private int outputLength = 0; // Replace data[0].output.length

	// Use this for initialization
	void Start () {
		sizes = new ArrayList();

	}

	private void Initialize(InputData input, OutputData output) {
		inputLength = input.length;
		outputLength = output.length;
	  sizes.Add(inputLength);
	  if (hiddenLayers != null) {
	    sizes.Add(Math.Max(3, Math.Floor((double) (inputLength / 2))));
	  } else {
			foreach(int size in hiddenLayers) {
				sizes.Add(size);
			}
	  }
	  sizes.Add(outputLength);

	  outputLayer = sizes.Count - 1; // index du niveau de sortie
	  biases = new ArrayList(); // weights for bias nodes
	  weights = new ArrayList();
	  outputs = new ArrayList();

	  // state for training
	  deltas = new ArrayList();
	  changes = new ArrayList();
	  errors = new ArrayList();

	  // // On remplit chaque niveau du réseau
	  // for (int layer = 0; layer <= outputLayer; ++layer) {
	  //   int size = (int) sizes[layer];       // numéro du niveau
	  //   deltas[layer] = zeros(size);   // On remplit les deltas du niveau avec des zéros
	  //   errors[layer] = zeros(size);   // On remplit les erreurs du niveau avec des zéros
	  //   outputs[layer] = zeros(size);  // On remplit la sortie du niveau avec des zéros
		//
	  //   if (layer > 0) {
	  //     biases[layer] = randos(size);      // les biais sont initialisés de façon random
	  //     weights[layer] = new ArrayList(size);  // les poids sont vides
	  //     changes[layer] = new ArrayList(size);  // les modifs sont vides
		//
	  //     for (int node = 0; node < size; ++node) { // pour chaque neurone du niveau
	  //       int prevSize = sizes[layer - 1];
	  //       weights[layer][node] = randos(prevSize); // le poids du neurone est random
	  //       changes[layer][node] = zeros(prevSize);  // les modifs valent 0
	  //     }
	  //   }
	  // }
	  // console.log(outputs)
	  // setActivation()
	}

	private ArrayList zeros(int size) {
		ArrayList array = new ArrayList();
		for (int i = 0; i < size; ++i) {
			array.Add(0);
		}
	  return array;
	}

	// Update is called once per frame
	void Update () {

	}
}
