using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

//Copied from Vicente Soler https://github.com/ADRC4/Voxel/blob/master/Assets/Scripts/Util/Util.cs

public enum Axis { X, Y, Z };
public enum BoundaryType { Inside = 0, Left = -1, Right = 1, Outside = 2 }

static class Util
{
    public static Vector3Int[] XZDirections =
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left
    };

    /// <summary>
    /// Save the voxels of the input grid that pass the input filter to the designated path
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="filter"></param>
    /// <param name="path"></param>
    public static void SaveVoxels(VoxelGrid grid, List<VoxelState> filters, string path)
    {
        var filteredVoxels = grid.GetVoxels().Where(v => filters.Contains(v.State));

        using (StreamWriter sw = new StreamWriter(path))
        {
            foreach (var voxel in filteredVoxels)
            {
                var index = voxel.Index;
                var line = $"{index.x},{index.y},{index.z},{voxel.State}";
                sw.WriteLine(line);
            }
        }
    }

    public static Vector3 Average(this IEnumerable<Vector3> vectors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var vector in vectors)
        {
            sum += vector;
            count++;
        }

        sum /= count;
        return sum;
    }

    public static T MinBy<T>(this IEnumerable<T> items, Func<T, double> selector)
    {
        double minValue = double.MaxValue;
        T minItem = items.First();

        foreach (var item in items)
        {
            var value = selector(item);

            if (value < minValue)
            {
                minValue = value;
                minItem = item;
            }
        }

        return minItem;
    }

    public static bool IsInsideGrid(this Vector3Int index, Vector3Int size)
    {
        if (index.x < 0 || index.x >= size.x) return false;
        if (index.y < 0 || index.y >= size.y) return false;
        if (index.z < 0 || index.z >= size.z) return false;

        return true;
    }

    /// <summary>
    /// Returns a shuffled copy of the input grid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(this List<T> list)
    {
        List<T> copy = new List<T>(list);
        return copy.OrderBy(t => UnityEngine.Random.value).ToList();
    }

    public static float Normalise(float v, float a1, float a2, float b1, float b2)
    {
        float result = b1 + (v - a1) * (b2 - b1) / (a2 - a1);

        return result;
    }

}