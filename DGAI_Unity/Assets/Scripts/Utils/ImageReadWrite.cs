using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ImageReadWrite
{

    /// <summary>
    /// Redimensiona uma imagem para 256x256 pixels
    /// </summary>
    /// <param name="image">A imagem a ser redimensionada</param>
    /// <param name="border">A cor da borda a ser aplicada caso a imagem de origem não seja quadrada</param>
    /// <returns>The resized <see cref="Texture2D"/></returns>
    public static Texture2D Resize256(Texture2D image, Color border)
    {
        Texture2D resultImage;

        if (image.width == image.height)
        {
            TextureScale.Point(image, 256, 256);
            image.Apply();

            resultImage = image;
        }
        else
        {
            resultImage = new Texture2D(256, 256);
            Color[] borderPixels = new Color[256 * 256];

            for (int i = 0; i < borderPixels.Length; i++)
            {
                borderPixels[i] = border;
            }

            resultImage.SetPixels(borderPixels);
            resultImage.Apply();

            if (image.width > image.height)
            {
                float ratio = image.height / (image.width * 1f);

                int newHeight = Mathf.RoundToInt(256 * ratio);

                TextureScale.Point(image, 256, newHeight);
                image.Apply();
            }
            else
            {
                float ratio = image.width / (image.height * 1f);

                int newWidth = Mathf.RoundToInt(256 * ratio);

                TextureScale.Point(image, newWidth, 256);
                image.Apply();
            }

            for (int i = 0; i < image.width; i++)
            {
                for (int j = 0; j < image.height; j++)
                {
                    int x = i;
                    int y = resultImage.height - image.height + j;
                    resultImage.SetPixel(x, y, image.GetPixel(i, j));
                }
            }
            resultImage.Apply();
        }

        return resultImage;
    }

    /// <summary>
    /// Salva uma imagem para o determinado caimnho e com o determinado nome
    /// O formato pode ser "caminhoA/.../nomeDoArquivo.png" ou "nome do arquivo.png"
    /// Se o caminho não existir, ele será criado.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="fileName"></param>
    public static void SaveImage(Texture2D image, string filePath)
    {
        //string filePath = _folder + $"/{fileName}" + ".png";
        byte[] data = image.EncodeToPNG();
        string path = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllBytes(filePath, data);
    }
}