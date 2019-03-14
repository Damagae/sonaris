using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class BrainData {

	public List<int> _sizes;
	public int _outputLayer;
	public List<List<double>> _biases;
	public List<List<double>> _outputs;
	public List<List<double>> _deltas;
	public List<List<double>> _errors;
	private List3D<double> _weights;
	private List3D<double> _changes;
	private AssociativeArray<int> _inputLookup;
	private AssociativeArray<int> _outputLookup;

	public BrainData(List<int> sizes, int outputLayer, List2D<double> biases,
									List3D<double> weights, List2D<double> outputs, List2D<double> deltas,
									List3D<double> changes, List2D<double> errors,
									AssociativeArray<int> inputLookup, AssociativeArray<int> outputLookup) {
		_sizes = sizes;
		_outputLayer = outputLayer;
		_biases = (List<List<double>>) biases;
		_weights = weights;
		_outputs = (List<List<double>>) outputs;
		_deltas = (List<List<double>>) deltas;
		_changes = changes;
		_errors = (List<List<double>>) errors;
		_inputLookup = inputLookup;
		_outputLookup = outputLookup;

	}

	public List<int> GetSizes() {
		return _sizes;
	}

	public int GetOutputLayer() {
		return _outputLayer;
	}

	public List2D<double> GetBiases() {
		return (List2D<double>) _biases;
	}

	public List3D<double> GetWeights() {
		return _weights;
	}

	public List2D<double> GetOutputs() {
		return (List2D<double>) _outputs;
	}

	public List2D<double> GetDeltas() {
		return (List2D<double>) _deltas;
	}

	public List3D<double> GetChanges() {
		return _changes;
	}

	public List2D<double> GetErrors() {
		return (List2D<double>) _errors;
	}

	public AssociativeArray<int> GetInputLookup() {
		return _inputLookup;
	}

	public AssociativeArray<int> GetOutputLookup() {
		return _outputLookup;
	}


}
