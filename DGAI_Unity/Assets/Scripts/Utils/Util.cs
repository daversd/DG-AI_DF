using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

//Copied from Vicente Soler https://github.com/ADRC4/Voxel/blob/master/Assets/Scripts/Util/Util.cs

public enum Axis { X, Y, Z };
public enum BoundaryType { Inside = 0, Left = -1, Right = 1, Outside = 2 }

class Point3d
{
    public int X, Y, Z;

    public Point3d(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Point3d[] CullDuplicates(IEnumerable<Point3d> points, float tolerance)
    {
        return null;
    }
}

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



    static void CreateCurves(List<Point3d> points, List<int> heights)
    {
        List<Point3d>[] curves = new List<Point3d>[heights.Count];

        for (int i = 0; i < heights.Count; i++)
        {
            var levelPoints = points.Where(p => p.Z >= i);
            curves[i] = levelPoints.Select(p => new Point3d(p.X, p.Y, i)).ToList();
        }

        for (int i = 0; i < curves.Length; i++)
        {
            var curve = curves[i];

            var groupsX = curve.GroupBy(p => p.X).ToDictionary(g => g.Key, g => g.ToList());
            var groupsY = curve.GroupBy(p => p.Y).ToDictionary(g => g.Key, g => g.ToList());
            List<Point3d> border = new List<Point3d>();
            foreach (var group in groupsX)
            {
                var list = group.Value;
                var ordered = list.OrderBy(p => p.Y).ToArray();
                for (int j = 0; j < ordered.Length; j++)
                {
                    var current = ordered[j];
                    Point3d next;
                    Point3d previous;
                    if (j - 1 >= 0)
                    {
                        previous = ordered[j - 1];
                        if (current.Y - previous.Y == 1)
                        {
                            if (j + 1 < ordered.Length)
                            {
                                next = ordered[j + 1];
                                if (next.Y - current.Y != 1)
                                {
                                    border.Add(current);
                                }
                            }
                        }
                        else
                        {
                            border.Add(current);
                        }
                    }
                }
            }

            foreach (var group in groupsY)
            {
                var list = group.Value;
                var ordered = list.OrderBy(p => p.X).ToArray();
                for (int j = 0; j < ordered.Length; j++)
                {
                    var current = ordered[j];
                    Point3d next;
                    Point3d previous;
                    if (j - 1 >= 0)
                    {
                        previous = ordered[j - 1];
                        if (current.Y - previous.Y == 1)
                        {
                            if (j + 1 < ordered.Length)
                            {
                                next = ordered[j + 1];
                                if (next.Y - current.Y != 1)
                                {
                                    border.Add(current);
                                }
                            }
                        }
                        else
                        {
                            border.Add(current);
                        }
                    }
                }
            }
        }
    }




}