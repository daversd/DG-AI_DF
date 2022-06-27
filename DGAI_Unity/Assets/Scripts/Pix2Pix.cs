using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Esta classe permite a execução de um modelo treinado de Pix2Pix para
/// realizer inferências a partir de imagens novas
/// </summary>
public class Pix2Pix
{
    #region Campos e propriedades

    NNModel _modelAsset;
    Model _loadedModel;
    IWorker _worker;

    #endregion

    #region Construtor

    /// <summary>
    /// Construtor de um objeto de inferência Pix2pix
    /// </summary>
    public Pix2Pix()
    {
        _modelAsset = Resources.Load<NNModel>("NeuralModels/treinado");
        _loadedModel = ModelLoader.Load(_modelAsset);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _loadedModel);
    }

    #endregion

    #region Métodos públicos

    // 01 Criar o método de previsão do modelo
    /// <summary>
    /// Executa o modelo de inferência em uma imagem e gera a previsão de uma imagem traduzida
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public Texture2D Predict(Texture2D image)
    {
        // 02 Traduz a imagem original para um tensor de 3 canais (RGB)
        Tensor imageTensor = new Tensor(image, channels: 3);

        // 03 Normaliza o tensor para o campo que o modelo espera
        var normalisedInput = NormaliseTensor(imageTensor, 0f, 1f, -1f, 1f);

        // 04 Executa o modelo no tensor
        _worker.Execute(normalisedInput);

        // 05 Retorna o resultado da previsão do modelo
        var outputTensor = _worker.PeekOutput();

        // 07 Traduz o tensor para uma imagem
        Texture2D prediction = Tensor2Image(outputTensor, image);

        // 08 Descarta os tensores utilizados
        imageTensor.Dispose();
        normalisedInput.Dispose();
        outputTensor.Dispose();

        return prediction;
    }

    #endregion

    #region Métodos privados

    /// <summary>
    /// Traduz um tensor em Texture2D
    /// </summary>
    /// <param name="inputTensor"></param>
    /// <param name="inputTexture"></param>
    /// <returns></returns>
    Texture2D Tensor2Image(Tensor inputTensor, Texture2D inputTexture)
    {
        var tempRT = new RenderTexture(256, 256, 24);
        inputTensor.ToRenderTexture(tempRT);
        RenderTexture.active = tempRT;

        var resultTexture = new Texture2D(inputTexture.width, inputTexture.height);
        resultTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        resultTexture.Apply();

        RenderTexture.active = null;
        tempRT.DiscardContents();

        return resultTexture;
    }

    /// <summary>
    /// Normaliza um tensor para um campo determinado
    /// </summary>
    /// <param name="inputTensor"></param>
    /// <param name="a1">Mínimo original</param>
    /// <param name="a2">Máximo original</param>
    /// <param name="b1">Mínimo esperado</param>
    /// <param name="b2">Máximo esperado</param>
    /// <returns></returns>
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