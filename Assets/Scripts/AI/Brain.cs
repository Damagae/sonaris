using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

	private List<int> sizes;
	private int outputLayer;
	private List2D<double> biases;
	private List3D<double> weights;
	private List2D<double> outputs;
	private List2D<double> deltas;
	private List3D<double> changes;
	private List2D<double> errors;
	private int errorCheckInterval = 1;
	private AssociativeArray<int> inputLookup = null;
	private int inputLookupLength = -1;
	private AssociativeArray<int> outputLookup = null;
	private int outputLookupLength = -1;

	// Default
	private double leakyReluAlpha = 0.01f;
	private double binaryThresh = 0.5f;
	private ArrayList hiddenLayers = null;
	public string activation = "sigmoid"; // leaky-relu et relu ne marchent pas
	private Func<double,double> runActivation;
	private Func<double,double,double> deltaCalculation;
	private int decimalNumber = 2;

	// Train Default
	private int iterations = 300;    // the maximum times to iterate the training data
	private float errorThresh = 0.0065f;   // the acceptable error percentage from training data
	private bool log = false;           // true to use console.log, when a function is supplied it is used
	private int logPeriod = 50;        // iterations between logging out
	private float learningRate = 0.3f;    // multiply's against the input and the delta then adds to momentum
	private float momentum = 0.1f;        // multiply's against the specified "change" then adds to learning rate for change
  private int timeout = 3;

	private int inputLength = 0; // Replace data[0].input.length
	private int outputLength = 0; // Replace data[0].output.length

	private InputData trainingInput;
	private OutputData trainingOutput;
	private string[] inputKeys;
	private string[] outputKeys;

	private bool trainingOver = false;

	void Start() {
		// hiddenLayers = new ArrayList();
		// hiddenLayers.Add(9);
		// hiddenLayers.Add(15);
		// hiddenLayers.Add(21);
	}

	public bool IsReady() {
		return trainingOver;
	}

	private void Initialize(InputData input, OutputData output) {
		inputLength = input.data[0].Count;
		outputLength = output.data[0].Count;
		Debug.Log("input size = " + inputLength + ", output size = " + outputLength);
    sizes = new List<int>();
	  sizes.Add(inputLength);
	  if (hiddenLayers == null) {
	    sizes.Add((int)Math.Max(3, Math.Floor((double) (inputLength / 2))));
	  } else {
			foreach(int size in hiddenLayers) {
				sizes.Add(size);
			}
	  }
		sizes.Add(outputLength);

	  outputLayer = sizes.Count - 1; // index du niveau de sortie
	  biases = new List2D<double>(); // weights for bias nodes
	  weights = new List3D<double>();
	  outputs = new List2D<double>();

	  // state for training
	  deltas = new List2D<double>();
	  changes = new List3D<double>();
	  errors = new List2D<double>();

	  // On remplit chaque niveau du réseau
	  for (int layer = 0; layer <= outputLayer; ++layer) {
	    int size = (int) sizes[layer];       // numéro du niveau
	    deltas.Add(Zeros(size));   // On remplit les deltas du niveau avec des zéros
	    errors.Add(Zeros(size));   // On remplit les erreurs du niveau avec des zéros
	    outputs.Add(Zeros(size));  // On remplit la sortie du niveau avec des zéros

      if (layer == 0) {
        biases.Add(new List<double>());
        weights.Add(new List2D<double>());
        changes.Add(new List2D<double>());
      }

	    if (layer > 0) {
	      biases.Add(Randos(size));      // les biais sont initialisés de façon random
	      weights.Add(new List2D<double>());  // les poids sont vides
	      changes.Add(new List2D<double>());  // les modifs sont vides

	      for (int node = 0; node < size; ++node) { // pour chaque neurone du niveau
	        int prevSize = (int) sizes[layer - 1];
	        weights[layer].Add(Randos(prevSize)); // le poids du neurone est random
	        changes[layer].Add(Zeros(prevSize));  // les modifs valent 0
	      }
	    }
	  }
	  SetActivation();
	}

	private void SetActivation() {
	  switch (activation) {
	    case "sigmoid":
	      runActivation = (double sum) => ActivationSigmoid(sum);
	      deltaCalculation = (double output, double sum) => DeltaSigmoid(output, sum);
	      break;
	    case "relu":
	      runActivation = (double sum) => ActivationRelu(sum);
	      deltaCalculation = (double output, double sum) => DeltaRelu(output, sum);
	      break;
	    case "leaky-relu":
	      runActivation = (double sum) => ActivationLeakyRelu(sum);
	      deltaCalculation = (double output, double sum) => DeltaLeakyRelu(output, sum);
	      break;
	    case "tanh":
	      runActivation = (double sum) => ActivationTanh(sum);
	      deltaCalculation = (double output, double sum) => DeltaTanh(output, sum);
	      break;
	    default:
				runActivation = (double sum) => ActivationSigmoid(sum);
				deltaCalculation = (double output, double sum) => DeltaSigmoid(output, sum);
				break;
	  }
	}

	public AssociativeArray<double> Run(AssociativeArray<double> input) {
	  var inputArray = ToArray(inputLookup, input);
	  List<double> output = RunInput(inputArray);
	  AssociativeArray<double> outputObject = ToObject(outputLookup, output);
		// Save();
	  return outputObject;
	}

  private bool TrainingTick(InputData input, OutputData output, AssociativeArray<double> status, DateTime endTime) {
    // Condition d'arrêt
    if (status["iterations"] >= iterations || status["error"] <= errorThresh /*|| DateTime.Now >= endTime*/) {
      return false;
    }

    ++status["iterations"];

    // Si on a choisi l'option "log"
    if (log && (status["iterations"] % logPeriod == 0)) {
      status["error"] = CalculateTrainingError(input, output);
      Debug.Log("iterations: " + status["iterations"] + ", training error: " + status["error"]);
    } else {
      if (status["iterations"] % errorCheckInterval == 0) {
        status["error"] = CalculateTrainingError(input, output);
      } else {
        TrainPatterns(input, output);
      }
    }

    return true;
  }

  private double CalculateTrainingError(InputData input, OutputData output)
  {
    double sum = 0;
    for (int i = 0; i < input.length; ++i) {
			var element = TrainPattern(input.data[i], output.data[i], true);
      sum += element;
    }
    return sum / input.length;
  }

  private void TrainPatterns(InputData input, OutputData output)
  {
    for (int i = 0; i < input.length; ++i) {
      TrainPattern(input.data[i], output.data[i], true);
    }
  }

  private double TrainPattern(AssociativeArray<double> valueInput, AssociativeArray<double> valueOutput, bool logErrorRate = false)
  {
    // forward propagate
    List<double> input = ToArray(inputLookup, valueInput);
    RunInput(input);

    List<double> output = ToArray(outputLookup, valueOutput);
    // back propagate
    CalculateDeltas(output);
    AdjustWeights();

    if (logErrorRate) {
      return MSE(errors[outputLayer]);
    } else {
      return 0;
    }
    return 0;
  }

  private void AdjustWeights()
  {
    // pour chaque niveau du réseau
    for (int layer = 1; layer <= outputLayer; ++layer) {
      // On récupère la sortie du niveau précédent
      List<double> incoming = outputs[layer - 1];

      // Pour chaque neurone du niveau
      for (int node = 0; node < (int) sizes[layer]; ++node) {
        double delta = deltas[layer][node]; // On récupère le delta correspondant

        // Pour chaque élément de la sortie précédente
        for (int k = 0; k < incoming.Count; ++k) {
          double change = changes[layer][node][k]; // On récupère la modif

          // On calcule la différence
          change = (learningRate * delta * incoming[k]) + (momentum * change);

          changes[layer][node][k] = change; // Mise à jour de la modif
          weights[layer][node][k] += change; // Mise à jour du poids
        }
        biases[layer][node] += learningRate * delta;
      }
    }
  }

	public IEnumerator Train(InputData input, OutputData output) {
	  AssociativeArray<double> status = new AssociativeArray<double>();
	  CreateInputLookup(input);
	  CreateOutputLookup(output);
		// if (!Load()) {
			Initialize(input, output);
		// }
	  DateTime endTime = DateTime.Now;
    endTime.AddDays(timeout);
	  status.Add("error", 1);
		status.Add("iterations", 0);

    bool goOn = true;
	  while (goOn) {
        goOn = TrainingTick(input, output, status, endTime);
				yield return null;
    }
		// Save();
		trainingOver = true;
		DisplayStatus(status);
	}

	public void DisplayStatus(AssociativeArray<double> status) {
		Debug.Log("error = " + status["error"] + ", iterations = " + status["iterations"]);
	}
	// Convenient function to train automatically the algorithm with our recorded data and the effects we desire corresponding to those data
	public void Train() {
		Debug.Log("Train");
		log = true;
		trainingInput = new InputData();
		trainingOutput = new OutputData();
		inputKeys = new string[] {"gain1", "gain2", "gain3",
															"freq1", "freq2", "freq3"};
		outputKeys = new string[] {	"cameraIndex1", "cameraIndex2", "cameraIndex3",
																"cameraWeight1",  "cameraWeight2", "cameraWeight3",
																"lightIntensity1", "lightIntensity2", "lightIntensity3",
																"lightR1", "lightR2", "lightR3",
																"lightG1", "lightG2", "lightG3",
																"lightB1", "lightB2", "lightB3",
																"lightLocationIndex1", "lightLocationIndex2", "lightLocationIndex3",
																"day1", "day2", "day3",
																"rain1", "rain2", "rain3" };

		/*{
			double cam = 1;
			// Arc de Triomphe 1
			cam = (double) 9/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.1153243  }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1386793 }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.06228814}, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.0787562 }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.5182434 , 0.5812005 }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.1153243   }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1386793  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.06228814 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.0787562  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.5182434 	}, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });

			// Arc de Triomphe 2
			cam = (double) 9/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.1320359   }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.0765625   }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.09297258  }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.09932432  }, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2606383 	}, new double[] { cam, 1, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.1320359  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.0765625  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.09297258 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.09932432 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2606383  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.02, 0.5, 0.5 });

			// Louvre 1
			cam = (double) 6/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.3143917  }, new double[] { cam, 1, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.07833038 }, new double[] { cam, 1, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.07846975 }, new double[] { cam, 1, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.07811235 }, new double[] { cam, 1, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.3923524 }, new double[] { cam, 1, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.3143917  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.07833038 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.07846975 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.07811235 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.3923524 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.3143917  }, new double[] { cam, 0, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.07833038 }, new double[] { cam, 0, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.07846975 }, new double[] { cam, 0, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.07811235 }, new double[] { cam, 0, 1, 1, 1, 1, 0.05, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.3923524 }, new double[] { cam, 0, 1, 1, 1, 1, 0.05, 0.5, 0.5 });

			// Nuit Daft Punk
			cam = (double) 15/15;
			// ~~ Gain 0.5 - Level 0.5
			CreateTrainingSet(new double[] { 0.5, 0.05991848   }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.06217105   }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.0336128    }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.05625 	   }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.06228814   }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0, 0.5 });

			// Opéra
			cam = (double) 7/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.1188679    }, new double[] { cam, 1, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1497962 	 }, new double[] { cam, 1, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.5434207 	 }, new double[] { cam, 1, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1759308    }, new double[] { cam, 1, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2273196 	 }, new double[] { cam, 1, 1, 1, 1, 1, 0.03, 0, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.1188679   }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1497962 	}, new double[] { cam, 0.5, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.5434207 	}, new double[] { cam, 0.5, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1759308   }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2273196 	}, new double[] { cam, 0.5, 1, 1, 1, 1, 0.03, 0, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.1188679  }, new double[] { 0.27, 0, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.1497962  }, new double[] { 0.27, 0, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.5434207  }, new double[] { 0.27, 0, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.1759308  }, new double[] { 0.27, 0, 1, 1, 1, 1, 0.03, 0, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.2273196  }, new double[] { 0.27, 0, 1, 1, 1, 1, 0.03, 0, 0.5 });

			// Pompidou 1
			cam = (double) 12/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.2959732  }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2949551  }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.8925212  }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.04966216 }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.2959732  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2949551  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.8925212  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.04966216 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.2959732  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.2949551  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.8925212  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.04966216 }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });

			// Pompidou 2
			cam = (double) 12/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.199247   }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2464939  }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2670504 }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.2022188  }, new double[] { cam, 1, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.2959732  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2949551  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.8925212  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.04966216 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.2959732  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.2949551  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.8925212  }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.04966216 }, new double[] { cam, 0, 1, 1, 1, 1, 0.07, 0.5, 0.5 });

			// Tour Eiffel
			cam = (double) 10/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.1759308  }, new double[] { cam, 1, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.110804   }, new double[] { cam, 1, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.9144518  }, new double[] { cam, 1, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.02916667 }, new double[] { cam, 1, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.08770883 }, new double[] { cam, 1, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.1759308  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.110804   }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.9144518  }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.02916667 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.08770883 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.1759308 }, new double[] { cam, 0, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.110804  }, new double[] { cam, 0, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.9144518 }, new double[] { cam, 0, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.02916667}, new double[] { cam, 0, 1, 1, 1, 1, 0.01, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.08770883}, new double[] { cam, 0, 1, 1, 1, 1, 0.01, 0.5, 0.5 });

			// Aube - Journée
			cam = (double) 1;
			// ~~ Gain 0.5 - Level 0.5
			CreateTrainingSet(new double[] { 0.5, 0.1011468,}, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.0399095  }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2505682,}, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.04477157 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.04856828 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.5 });

			// Circulation
			cam = (double) 1;
			// ~~ Gain 0.5 - Level 0.5
			CreateTrainingSet(new double[] { 0.5, 0.05163934 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.7 });
			CreateTrainingSet(new double[] { 0.5, 0.04176136 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.7 });
			CreateTrainingSet(new double[] { 0.5, 0.04551084}, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.7 });
			CreateTrainingSet(new double[] { 0.5, 0.04955056 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.7 });
			CreateTrainingSet(new double[] { 0.5, 0.05121951 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 1, 0.7 });

			// // Montparnasse
			// cam = (double) 11/15;
			// // ~~ Gain 1 - Level 1
			// CreateTrainingSet(new double[] { 1, 2.008787  , 0.3106548 }, new double[] { cam, 1, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.1463495 , 0.314512 }, new double[] { cam, 1, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.07399329, 0.400383 }, new double[] { cam, 1, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.5404412 , 0.1778415 }, new double[] { cam, 1, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.1425647 , 0.3776402 }, new double[] { cam, 1, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// // ~~ Gain 0.5 - Level 1
			// CreateTrainingSet(new double[] { 0.5, 0.0735 	 , 0.3106548 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.05742187, 0.314512 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.09932432, 0.400383 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.1148437 , 0.1778415 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.1569395 , 0.3776402 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// // ~~ Gain 0 - Level 1
			// CreateTrainingSet(new double[] { 0, 0.0735 	 , 0.3106548 }, new double[] { cam, 0, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.05742187, 0.314512 }, new double[] { cam, 0, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.09932432, 0.400383 }, new double[] { cam, 0, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.1148437 , 0.1778415 }, new double[] { cam, 0, 1, 1, 1, 1, 0.06, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.1569395 , 0.3776402 }, new double[] { cam, 0, 1, 1, 1, 1, 0.06, 0.5, 0.5 });

			// Train
			cam = (double) 8/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.08480769, }, new double[] { cam, 1, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.18375   , }, new double[] { cam, 1, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1070388 , }, new double[] { cam, 1, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1366736 , }, new double[] { cam, 1, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.08480769, }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.18375    }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1070388 , }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1366736 , }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.08480769, }, new double[] { cam, 0, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.18375   , }, new double[] { cam, 0, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.1070388 , }, new double[] { cam, 0, 1, 1, 1, 1, 0.04, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.1366736 , }, new double[] { cam, 0, 1, 1, 1, 1, 0.04, 0.5, 0.5 });

			// Notre Dame
			cam = (double) 5/15;
			// ~~ Gain 1 - Level 1
			CreateTrainingSet(new double[] { 1, 0.08948864, }, new double[] { cam, 1, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.03972973  }, new double[] { cam, 1, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.059918488 }, new double[] { cam, 1, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1176 ,    }, new double[] { cam, 1, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 1, 0.1344512,  }, new double[] { cam, 1, 1, 1, 1, 1, 0, 0.5, 0.5 });
			// ~~ Gain 0.5 - Level 1
			CreateTrainingSet(new double[] { 0.5, 0.07301325,}, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.1807377 ,}, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.08136531 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.2041667 ,}, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0.5, 0.07135922,}, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 0.5, 0.5 });
			// ~~ Gain 0 - Level 1
			CreateTrainingSet(new double[] { 0, 0.07301325,}, new double[] { cam, 0, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.1807377 ,}, new double[] { cam, 0, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.08136531,}, new double[] { cam, 0, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.2041667 ,} , new double[] { cam, 0, 1, 1, 1, 1, 0, 0.5, 0.5 });
			CreateTrainingSet(new double[] { 0, 0.07135922,}, new double[] { cam, 0, 1, 1, 1, 1, 0, 0.5, 0.5 });

			// // Jardin
			// cam = (double) 13/15;
			// // ~~ Gain 1 - Level 1
			// CreateTrainingSet(new double[] { 1, 2.229137 , 0.3548041 }, new double[] { cam, 1, 1, 1, 1, 1, 0,  1, 0.4 });
			// CreateTrainingSet(new double[] { 1, 2.051901 , 0.2557901 }, new double[] { cam, 1, 1, 1, 1, 1, 0,  1, 0.4 });
			// CreateTrainingSet(new double[] { 1, 1.099899 , 0.1189906 }, new double[] { cam, 1, 1, 1, 1, 1, 0,  1, 0.4 });
			// CreateTrainingSet(new double[] { 1, 1.990016 , 0.2244811 }, new double[] { cam, 1, 1, 1, 1, 1, 0,  1, 0.4 });
			// CreateTrainingSet(new double[] { 1, 2.379311 , 0.3885294  }, new double[] { cam, 1, 1, 1, 1, 1, 0, 1, 0.4 });
			// // ~~ Gain 0.5 - Level 1
			// CreateTrainingSet(new double[] { 0.5, 2.244914 , 0.3548041 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0.5, 0.7764084, 0.2557901 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0.5, 0.0230167, 0.1189906 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0.5, 2.19159  , 0.2244811 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0.5, 1.926003 , 0.3885294 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0, 1, 0.4 });
			// // ~~ Gain 0 - Level 1
			// CreateTrainingSet(new double[] { 0, 2.244914 , 0.3548041 }, new double[] { cam, 0, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0, 0.7764084, 0.2557901 }, new double[] { cam, 0, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0, 0.0230167, 0.1189906 }, new double[] { cam, 0, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0, 2.19159  , 0.2244811 }, new double[] { cam, 0, 1, 1, 1, 1, 0, 1, 0.4 });
			// CreateTrainingSet(new double[] { 0, 1.926003 , 0.3885294 }, new double[] { cam, 0, 1, 1, 1, 1, 0, 1, 0.4 });

			// Pluie
			cam = (double) 1;
			// ~~ Gain 0.5 - Level 0.5
			CreateTrainingSet(new double[] { 0.5, 0.1356923  }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 0.5, 1 });
			CreateTrainingSet(new double[] { 0.5, 0.1641439  }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 0.5, 1 });
			CreateTrainingSet(new double[] { 0.5, 0.2477528 }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 0.5, 1 });
			CreateTrainingSet(new double[] { 0.5, 0.1512865  }, new double[] { cam, 0.5, 0.5, 1, 1, 1, 0, 0.5, 1 });

			// // NHM
			// cam = (double) 14/15;
			// // ~~ Gain 1 - Level 1
			// CreateTrainingSet(new double[] { 1, 0.07301325, 0.2052012 }, new double[] { cam, 1, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.1225    , 0.5380291 }, new double[] { cam, 1, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 0.0911157 , 0.5426568 }, new double[] { cam, 1, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 1, 1.1352761 , 0.5594626 }, new double[] { cam, 1, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// // ~~ Gain 0.5 - Level 1
			// CreateTrainingSet(new double[] { 0.5, 0.07301325, 0.2052012 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.1225		 , 0.5380291 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 0.0911157 , 0.5426568 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0.5, 1.1352761 , 0.5594626 }, new double[] { cam, 0.5, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// // ~~ Gain 0 - Level 1
			// CreateTrainingSet(new double[] { 0, 0.07301325, 0.2052012 }, new double[] { cam, 0, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.1225		 , 0.5380291 }, new double[] { cam, 0, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 0.0911157 , 0.5426568 }, new double[] { cam, 0, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
			// CreateTrainingSet(new double[] { 0, 1.1352761 , 0.5594626 }, new double[] { cam, 0, 1, 1, 1, 1, 0.09, 0.5, 0.5 });
		}*/


		/*{
			double cam = 1;
			double gain = 1;

			// Arc de Triomphe
			cam = (double) 9/15
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.1153243  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1386793  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06228814 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07875    }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5182434  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1304734  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1297059  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3944866  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3960536  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.04651899 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
			}

			// Louvre
			cam = (double) 6/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.3143917  }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07833038 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07846975 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07811235 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3923524 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1998489 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2801475 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5833291 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6191946 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.03909574 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
			}

			// Nuit Daft Punk
			cam = (double) 15/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.05991848  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06217105  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0336128   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.05625 	   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06228814  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.03114407  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1378125   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.05625     }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07950721  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06336207  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
			}

			// Opéra
			cam = (double) 7/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.1188679 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1497962 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5434207 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1759308 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2273196 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5018229 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5338706 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5514337 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1086207 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5462719 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
			}

			// Pompidou 1
			cam = (double) 12/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.2959732  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2949551  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8925212  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.04966216 }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.252 		  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.198053   }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1984505  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2941288  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8864575  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2959732  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
			}

			// Pompidou 2
			cam = (double) 12/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.199247   }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2464939  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2670504  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2022188  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2429375  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2213309  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2340984  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2194979  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2368585  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
			}

			// Tour Eiffel
			cam = (double) 10/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.1759308  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.110804   }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.9144518  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.02916667 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.08770883 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.065625   }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.05864362 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.09914568 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.05864362 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1657895  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
			}

			// Aube - Journée
			cam = (double) 1;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.1011468  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0399095  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2505682  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.04477157 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.04856828 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.098 		  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.08513513 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1778226  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0389576  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.09264705 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			}

			// Circulation
			cam = (double) 1;
			gain = 0.5;
			CreateTrainingSet(new double[] { gain, 0.05163934 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.04176136 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.04551084 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.04955056 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.05121951 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.05646607 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.03795181 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.1369565  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.1491304  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			CreateTrainingSet(new double[] { gain, 0.1148437  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });


			// Montparnasse
			cam = (double) 11/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 2.008787   }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1463495  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.07399329 }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5404412  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1425647  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1473274  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0430664  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06681819 }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1466091  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1609489  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
			}

			// Train
			cam = (double) 8/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) 0.5;
				CreateTrainingSet(new double[] { gain, 0.08480769 }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.18375    }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1070388  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1366736  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0467161  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.03054017 }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.06977848 }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2476468  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0821229  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1179144  }, new double[] { cam, 0, 0, 1, 1, 1, 0.04, 0.5, 0.5 });
			}

			// Notre Dame
			cam = (double) 5/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.08948864 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.03972973 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.05991848 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1176     }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1344512  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0725329  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.362963   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.08613281 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1858146  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1352761  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
			}

			// Jardin
			cam = (double) 13/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 2.229137   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.051901   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 1.099899   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 1.990016   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.379311   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
				CreateTrainingSet(new double[] { gain, 0.08106618 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
				CreateTrainingSet(new double[] { gain, 1.996064   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.204317   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
				CreateTrainingSet(new double[] { gain, 0.2248525  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
				CreateTrainingSet(new double[] { gain, 1.977612   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
			}

			// Pluie
			cam = (double) 1;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.1356923  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.1641439  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.2477528  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.1512865  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.105      }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.06363636 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.1099751  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.1293255  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.06116505 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.1235294  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
			}

			// NHM
			cam = (double) 14/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.07301325 }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1225     }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.0911157  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.1352761  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.4842061  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.008287   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3987983  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3906068  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.424466   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3669867  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.456967   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			}
		}*/

		{
			double cam = 1;
			double gain = 1;

			// Arc de Triomphe
			cam = (double) 9/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.3501317  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3520868  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1873702 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5802632    }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3424701  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1849947  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6681818  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3394651  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7875  }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5653846 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3409859 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 3.15 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.321053 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3416887 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3435357 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 3.392308 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.98 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3604293 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3548098 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3417826 }, new double[] { cam, gain, gain, 1, 1, 1, 0.02, 0.5, 0.5 });
			}

			// Louvre
			cam = (double) 6/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.7112904  }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6890625 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7603449 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1860879 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6125  	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5802632 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6681818 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7875 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.1025 	}, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8166667 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8480769 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.9 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6125 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5378048 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.882 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5378048 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3457407 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6485294 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6125 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.735 }, new double[] { cam, gain, gain, 1, 1, 1, 0.05, 0.5, 0.5 });
			}

			// Nuit Daft Punk
			cam = (double) 15/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 1.225  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5378048  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.91875   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.1025 	   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8166667  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0, 0.5 });
			}

			// Opéra
			cam = (double) 7/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.5802632 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5959459 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5582278 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6681818 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6485294 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5011364 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5802632 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5582278 }, new double[] { cam, gain, gain, 1, 1, 1, 0.03, 0, 0.5 });
			}

			// Pompidou 1
			cam = (double) 12/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.2959732  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2949551  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8925212  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.04966216 }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.252 		  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.198053   }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1984505  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2941288  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8864575  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.2959732  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
			}

			// Pompidou 2
			cam = (double) 12/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 1.575   }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3461736  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3495481  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.205  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3484435  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3503235  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.32105  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3366297  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6125  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3431654  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7875  }, new double[] { cam, gain, gain, 1, 1, 1, 0.07, 0.5, 0.5 });
			}

			// Tour Eiffel
			cam = (double) 10/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.5802632  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.8166667   }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5011364  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7736842 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.525 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.575   }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.378125 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.503409 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.8375 }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.7112904  }, new double[] { cam, gain, gain, 1, 1, 1, 0.01, 0.5, 0.5 });
			}

			// Aube - Journée
			cam = (double) 1;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 1.696154  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.1  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.696154  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.575 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.1 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.004545  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5127907 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.378125  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1864348  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.160526 }, new double[] { cam, 0, 0, 1, 1, 1, 0, 1, 0.5 });
			}


			// Montparnasse
			cam = (double) 11/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 2.004545   }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.633333  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.187415 }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.5404412  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3381333  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1473274  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.1  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.764 }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.07561  }, new double[] { cam, gain, gain, 1, 1, 1, 0.06, 0.5, 0.5 });
			}

			// Train
			cam = (double) 8/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) 0.5;
				CreateTrainingSet(new double[] { gain, 1.633333 }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 2.1    }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.6485294  }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.378125  }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.764  }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.1855732 }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.438043 }, new double[] { cam, gain, gain, 1, 1, 1, 0.04, 0.5, 0.5 });
			}

			// Notre Dame
			cam = (double) 5/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 0.3608075 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.49 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.575 }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3488649     }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 0.3588086  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
				CreateTrainingSet(new double[] { gain, 1.696154  }, new double[] { cam, gain, gain, 1, 1, 1, 0, 0.5, 0.5 });
			}

			// Jardin
			cam = (double) 13/15;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 2.1   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.321053   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.205   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.133871   }, new double[] { cam, gain, gain, 1, 1, 1, 0,  1, 0.4 });
				CreateTrainingSet(new double[] { gain, 2.379311   }, new double[] { cam, gain, gain, 1, 1, 1, 0, 1, 0.4 });
			}

			// Pluie
			cam = (double) 1;
			for (int i = 0; i < 3; ++i) {
				gain = (double) i * 0.5;
				CreateTrainingSet(new double[] { gain, 1.297059  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 0.8647059  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
				CreateTrainingSet(new double[] { gain, 1.378125  }, new double[] { cam, 0, 0, 1, 1, 1, 0, 0.5, 1 });
			}

			// // NHM
			// cam = (double) 14/15;
			// for (int i = 0; i < 3; ++i) {
			// 	gain = (double) i * 0.5;
			// 	CreateTrainingSet(new double[] { gain, 0.07301325 }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.1225     }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.0911157  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 1.1352761  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.4842061  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 1.008287   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.3987983  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.3906068  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 2.424466   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 0.3669867  }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// 	CreateTrainingSet(new double[] { gain, 2.456967   }, new double[] { cam, gain, gain, 1, 1, 1, 0.09, 0.5, 0.5 });
			// }
		}

		StartCoroutine(Train(trainingInput, trainingOutput));
	}

	private void CreateTrainingSet(double[] inputValues, double[] outputValues) {
		if (inputValues.Length == (inputKeys.Length / 3) && outputValues.Length == (outputKeys.Length / 3)) {
			trainingInput.Add(inputKeys, new double[] {inputValues[0], 0, 0, inputValues[1], 0, 0});
			trainingOutput.Add(outputKeys, new double[] { outputValues[0], 0, 0,
																										outputValues[1], 0, 0,
																										outputValues[2], 0, 0,
																										outputValues[3], 0, 0,
																										outputValues[4], 0, 0,
																										outputValues[5], 0, 0,
																										outputValues[6], 0, 0,
																										outputValues[7], 0, 0,
																										outputValues[8], 0, 0 });

			trainingInput.Add(inputKeys, new double[] {0, inputValues[0], 0, 0, inputValues[1], 0});
			trainingOutput.Add(outputKeys, new double[] { 0, outputValues[0], 0,
																										0, outputValues[1], 0,
																										0, outputValues[2], 0,
																										0, outputValues[3], 0,
																										0, outputValues[4], 0,
																										0, outputValues[5], 0,
																										0, outputValues[6], 0,
																										0, outputValues[7], 0,
																										0, outputValues[8], 0 });

			trainingInput.Add(inputKeys, new double[] {0, 0, inputValues[0], 0, 0, inputValues[1]});
			trainingOutput.Add(outputKeys, new double[] { 0, 0, outputValues[0],
																										0, 0, outputValues[1],
																										0, 0, outputValues[2],
																										0, 0, outputValues[3],
																										0, 0, outputValues[4],
																										0, 0, outputValues[5],
																										0, 0, outputValues[6],
																										0, 0, outputValues[7],
																										0, 0, outputValues[8] });
		}
	}

	// Convert input format into export format by running the network
	private List<double> RunInput(List<double> input) {
	  outputs[0] = input; // set output stat of input layer

	  List<double> output = new List<double>();

	  for (int layer = 1; layer <= outputLayer; ++layer) {
	    for (int node = 0; node < (int) sizes[layer]; ++node) {
	      var thisWeights = weights[layer][node];

	      var sum = biases[layer][node];

	      for (var k = 0; k < thisWeights.Count; ++k) {
	        sum += thisWeights[k] * input[k];
	      }
				outputs[layer][node] = runActivation(sum);
	    }
	    output = input = outputs[layer];
	  }
	  return output;
	}

  private void CalculateDeltas(List<double> target)
  {
    for (int layer = outputLayer; layer >= 0; layer--) {
      for (int node = 0; node < (int) sizes[layer]; ++node) {
        double output = outputs[layer][node];

        double error = 0;
        // Si c'est le dernier niveau = sortie
        if (layer == outputLayer) {
          error = target[node] - output;
        } else {
          List<double> thisDeltas = deltas[layer + 1]; // PB avec thisDeltas = 0
          for (int k = 0; k < thisDeltas.Count; ++k) {
            error += thisDeltas[k] * weights[layer + 1][k][node];
          }
        }
        errors[layer][node] = error;
        deltas[layer][node] = deltaCalculation(output, error);
      }
    }
  }

	private void CreateInputLookup(InputData input) {
		if (input.data.Count > 0) {
			var data = input.data[0];
		  int length = 0;
		  AssociativeArray<int> lookup = new AssociativeArray<int>();
		  for (int i = 0; i < data.Count; ++i) {
		    foreach (var p in data) {
		      if (lookup.ContainsKey(p.Key)) continue;
		      lookup[p.Key] = (int) length++;
		    }
		  }
		  inputLookup = lookup;
		} else {
			Debug.Log("No input");
		}

	}

	private void CreateOutputLookup(OutputData output) {
		var data = output.data[0];
	  int length = 0;
	  AssociativeArray<int> lookup = new AssociativeArray<int>();
	  for (int i = 0; i < data.Count; ++i) {
	    foreach (var p in data) {
	      if (lookup.ContainsKey(p.Key)) continue;
	      lookup[p.Key] = length++;
	    }
	  }
	  outputLookup = lookup;
	}

	// performs {a: 0, b: 1}, {a: 6} -> [6, 0]
	private List<double> ToArray(AssociativeArray<int> lookup, AssociativeArray<double> obj) {
	  int arrayLength = 0;
	  foreach (var p in lookup) {
	    ++arrayLength;
	  }
	  var result = Zeros(arrayLength);
	  foreach (var p in lookup) {
      int key = lookup[p.Key];
	    result[key] = obj.ContainsKey(p.Key) ? obj[p.Key] : 0;
	  }
	  return result;
	}

	// performs {a: 0, b: 1}, [6, 7] -> {a: 6, b: 7}
	private AssociativeArray<double> ToObject(AssociativeArray<int> lookup, List<double> array) {
	  AssociativeArray<double> obj = new AssociativeArray<double>();
	  foreach (var p in lookup) {
      int key = lookup[p.Key];
	    obj[p.Key] = Math.Round(array[key], decimalNumber);
	  }
	  return obj;
	}

	private double ActivationSigmoid(double sum) {
	  return 1 / (1 + Math.Exp(-sum));
	}

	private double ActivationRelu(double sum) {
	  return (sum < 0 ? 0 : sum);
	}

	private double ActivationLeakyRelu(double sum) {
	  // return (sum < 0 ? 0 : alpha * sum);
		return 0;
	}

	private double ActivationTanh(double sum) {
	  return Math.Tanh(sum);
	}

	private double DeltaSigmoid(double output, double error) {
	  return error * output * (1 - output);
	}

	private double DeltaRelu(double output, double error) {
	  return output > 0 ? error : 0;
	}

	private double DeltaLeakyRelu(double output, double error) {
	  return output > 0 ? error : leakyReluAlpha * error;
	}

	private double DeltaTanh(double output, double error) {
	  return (1 - output * output) * error;
	}

  private double MSE(List<double> errors)
  {
    // mean square error
    double sum = 0;
    for (int i = 0; i < errors.Count; ++i) {
      sum += errors[i] * errors[i];
    }
    return sum / errors.Count;
  }

	private List<double> Zeros(int size) {
		List<double> array = new List<double>();
		for (int i = 0; i < size; ++i) {
			array.Add(0);
		}
	  return array;
	}

	private List<double> Randos(int size) {
		System.Random rnd = new System.Random();
	  List<double> array = new List<double>();
	  for (int i = 0; i < size; ++i) {
	    array.Add(rnd.Next(0, 100) * 0.01 * 0.4 - 0.2 );
	  }
	  return array;
	}

	private void DisplayList(List<double> list)
	{
		string result = "{ ";
		foreach (var e in list) {
      result += e + ", ";
    }
    result += "}";
    Debug.Log(result);
	}

	private void Save() {
		var brainData = new BrainData(sizes, outputLayer, biases, weights, outputs, deltas, changes, errors, inputLookup, outputLookup);
		SaveManager.Save(brainData);
	}

	private bool Load() {
		if (SaveManager.SaveExists()) {
			var brainData = SaveManager.Fetch();

			sizes = brainData.GetSizes();
			outputLayer = brainData.GetOutputLayer();
			biases = brainData.GetBiases();
			weights = brainData.GetWeights();
			outputs = brainData.GetOutputs();
			deltas = brainData.GetDeltas();
			changes = brainData.GetChanges();
			errors = brainData.GetErrors();
			inputLookup = brainData.GetInputLookup();
			outputLookup = brainData.GetOutputLookup();

			return true;
		}
		return false;
	}
}
