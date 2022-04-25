using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ImageReadWrite
{
    static string _folder = Directory.GetCurrentDirectory();

    /// <summary>
    /// Resizes the input image to 256x256 pixels
    /// </summary>
    /// <param name="image">The input image</param>
    /// <param name="border">The colour to be applied to the border if input is not square</param>
    /// <returns>The resized <see cref="Texture2D"/></returns>
    public static Texture2D Resize256(Texture2D image, Color border)
    {
        Texture2D resultImage;

        if (image.width == image.height)
        {
            // Image's ratio is 1:1, scale directly
            TextureScale.Point(image, 256, 256);
            image.Apply();

            resultImage = image;
        }
        else
        {
            // Image's ratio is not 1:1, scale according to bigger size
            // Create output image with border colour pixels
            resultImage = new Texture2D(256, 256);
            Color[] borderPixels = new Color[256 * 256];

            for (int i = 0; i < borderPixels.Length; i++)
            {
                borderPixels[i] = border;
            }

            resultImage.SetPixels(borderPixels);
            resultImage.Apply();

            // Scale image preserving original ratio
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

            // Apply scaled image to result
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
    /// Saves an image to disk to the specified file name
    /// Format can be "pathA/.../file name" or "file name"
    /// No extension or leading slash
    /// If the target directory does not exist, it gets created
    /// </summary>
    /// <param name="image"></param>
    /// <param name="fileName"></param>
    public static void SaveImage(Texture2D image, string fileName)
    {
        string filePath = _folder + $"/{fileName}" + ".png";
        byte[] data = image.EncodeToPNG();
        string path = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllBytes(filePath, data);
    }
}