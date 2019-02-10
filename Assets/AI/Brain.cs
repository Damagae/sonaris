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
	private int iterations = 20000;    // the maximum times to iterate the training data
	private float errorThresh = 0.005f;   // the acceptable error percentage from training data
	private bool log = false;           // true to use console.log, when a function is supplied it is used
	private int logPeriod = 10;        // iterations between logging out
	private float learningRate = 0.3f;    // multiply's against the input and the delta then adds to momentum
	private float momentum = 0.1f;        // multiply's against the specified "change" then adds to learning rate for change
	// private double timeout = double.PositiveInfinity;    // the max number of milliseconds to train for
  private int timeout = 3;

	private int inputLength = 0; // Replace data[0].input.length
	private int outputLength = 0; // Replace data[0].output.length

	// Use this for initialization
	void Start () {
    InputData trainingInput = new InputData();
    OutputData trainingOutput = new OutputData();
    string[] inputKeys = {"rouge", "vert", "bleu"};
    string[] outputKeys = {"rouge", "vert", "bleu", "jaune", "cyan", "rose", "blanc"};
    trainingInput.Add(inputKeys, new double[] {255, 0, 0});
    trainingOutput.Add(outputKeys, new double[] {1, 0, 0, 0, 0, 0, 0.3});
    trainingInput.Add(inputKeys, new double[] {0, 255, 0});
		trainingOutput.Add(outputKeys, new double[] {0, 1, 0, 0, 0, 0, 0.3});
		trainingInput.Add(inputKeys, new double[] {0, 0, 255});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 1, 0, 0, 0, 0.3});
		trainingInput.Add(inputKeys, new double[] {255, 255, 0});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 1, 0, 0, 0.6});
		trainingInput.Add(inputKeys, new double[] {0, 255, 255});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 1, 0, 0.6});
		trainingInput.Add(inputKeys, new double[] {255, 0, 255});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 0, 1, 0.6});
		trainingInput.Add(inputKeys, new double[] {255, 255, 255});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 0, 0, 1});
		trainingInput.Add(inputKeys, new double[] {0, 0, 0});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 0, 0, 0});
		trainingInput.Add(inputKeys, new double[] {128, 128, 128});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 0, 0, 0.5});
		trainingInput.Add(inputKeys, new double[] {128, 0, 128});
		trainingOutput.Add(outputKeys, new double[] {0, 0, 0, 0, 0, 0.5, 0.5});


    var stats = Train(trainingInput, trainingOutput);
		var input = new AssociativeArray<double>();
		input.Add(inputKeys, new double[] {240, 2, 20});
		Debug.Log(Run(input).ToString());

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

	public AssociativeArray<double> Train(InputData input, OutputData output) {
	  AssociativeArray<double> status = new AssociativeArray<double>();
	  CreateInputLookup(input);
	  CreateOutputLookup(output);
	  Initialize(input, output);
	  DateTime endTime = DateTime.Now;
    endTime.AddDays(timeout);
	  status.Add("error", 1);
		status.Add("iterations", 0);

    bool goOn = true;
	  while (goOn) {
        goOn = TrainingTick(input, output, status, endTime);
    }
	  return status;
	}

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

	// Update is called once per frame
	void Update () {

	}
}
