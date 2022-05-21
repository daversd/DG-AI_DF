using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class Pix2Pix
{
    #region Fields and Properties

    NNModel _modelAsset;
    Model _loadedModel;
    IWorker _worker;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor for a regualar Pix2Pix inference object
    /// </summary>
    public Pix2Pix()
    {
        // Initialise the model
        _modelAsset = Resources.Load<NNModel>("NeuralModels/treinado");
        _loadedModel = ModelLoader.Load(_modelAsset);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _loadedModel);
    }

    #endregion

    #region Public Methods

    // 38 Create the prediction method
    /// <summary>
    /// Runs the inference model on an input image to generate a new translated image
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public Texture2D Predict(Texture2D image)
    {
        // 39 Translate the input image into a 3 channels (RGB) tensor
        Tensor imageTensor = new Tensor(image, channels: 3);

        // 40 Normalise the tensor to the model's expected range
        var normalisedInput = NormaliseTensor(imageTensor, 0f, 1f, -1f, 1f);

        // 41 Execute the tensor
        _worker.Execute(normalisedInput);

        // 42 Get the result prediction and normalised back to image range
        var outputTensor = _worker.PeekOutput();

        // 43 Translate the tensor into an image
        Texture2D prediction = Tensor2Image(outputTensor, image);

        // 44 Dispose of used tensors
        imageTensor.Dispose();
        normalisedInput.Dispose();
        outputTensor.Dispose();

        return prediction;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Translates a Tensor into a Texture2D
    /// </summary>
    /// <param name="inputTensor">The Tensor to be translated</param>
    /// <param name="inputTexture">A reference Texture2D for formatting</param>
    /// <returns></returns>
    Texture2D Tensor2Image(Tensor inputTensor, Texture2D inputTexture)
    {
        //Apply output tensor to a temp RenderTexture
        var tempRT = new RenderTexture(256, 256, 24);
        inputTensor.ToRenderTexture(tempRT);
        RenderTexture.active = tempRT;

        //Assign temp RenderTexture to a new Texture2D
        var resultTexture = new Texture2D(inputTexture.width, inputTexture.height);
        resultTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        resultTexture.Apply();

        //Destroy temp RenderTexture
        RenderTexture.active = null;
        tempRT.DiscardContents();

        return resultTexture;
    }

    /// <summary>
    /// Normalizes a tensor to a target range
    /// </summary>
    /// <param name="inputTensor"></param>
    /// <param name="a1">Original range minimum</param>
    /// <param name="a2">Original range maximum</param>
    /// <param name="b1">Target range minimum</param>
    /// <param name="b2">Target range maximum</param>
    /// <returns>The normalised tensor</returns>
    Tensor NormaliseTensor(Tensor inputTensor, float a1, float a2, float b1, float b2)
    {
        var data = inputTensor.data.Download(inputTensor.shape);
        float[] normalized = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            normalized[i] = Util.Normalise(data[i], a1, a2, b1, b2);
        }

        return new Tensor(inputTensor.shape, normalized);
    }

    #endregion
}