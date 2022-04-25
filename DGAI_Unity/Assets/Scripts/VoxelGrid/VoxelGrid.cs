using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class VoxelGrid
{
    #region Public fields

    public Vector3Int GridSize;
    public Voxel[,,] Voxels;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }

    #endregion Public fields

    #region Private fields

    List<Voxel> _currentSelection;
    Voxel[] _currentCorners;

    #endregion Private fields

    #region Constructors

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize, Transform parent = null)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;

        var prefab = Resources.Load<GameObject>("Prefabs/cube");

        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    Voxels[x, y, z] = new Voxel(
                        new Vector3Int(x, y, z),
                        this,
                        prefab,
                        state: y == 0 ? VoxelState.White : VoxelState.Empty,
                        parent: parent,
                        sizeFactor: 0.96f);
                }
            }
        }

        MakeFaces();
        MakeEdges();
    }


    #endregion Constructors

    #region Public methods

    public void SetCorners(Voxel[] corners)
    {
        _currentCorners = corners;
        if (_currentSelection != null)
        {
            _currentSelection.Where(v => v.State == VoxelState.Yellow).ToList().ForEach(v => v.SetState(VoxelState.White));
            
        }
        
        _currentSelection = new List<Voxel>();
        

        Vector3Int c0 = corners[0].Index;
        Vector3Int c1 = corners[1].Index;
        int xLen = Mathf.Abs(c0.x - c1.x);
        int yLen = Mathf.Abs(c0.y - c1.y);
        int zLen = Mathf.Abs(c0.z - c1.z);

        for (int x = Mathf.Min(c0.x, c1.x); x <= Mathf.Max(c0.x, c1.x); x++)
        {
            for (int y = Mathf.Min(c0.y, c1.y); y <= Mathf.Max(c0.y, c1.y); y++)
            {
                for (int z = Mathf.Min(c0.z, c1.z); z <= Mathf.Max(c0.z, c1.z); z++)
                {
                    var voxel = Voxels[x, y, z];
                    _currentSelection.Add(voxel);
                    if (voxel.Index != c0) voxel.SetState(VoxelState.Yellow);
                }
            }
        }
    }

    public void MakeBox(int height)
    {
        if (_currentSelection == null || _currentSelection.Count == 0) return;

        int baseLevel = _currentSelection.Min(v => v.Index.y);
        int topLevel = Mathf.RoundToInt(GridSize.y * height);

        foreach (var voxel in _currentSelection)
        {
            for (int y = 0; y < height; y++)
            {
                Voxels[voxel.Index.x, y, voxel.Index.z].SetState(VoxelState.Black);
            }
        }

        _currentCorners = null;
        _currentSelection = new List<Voxel>();
    }

    public void RectangleFromCorner(Voxel corner, Vector3Int size)
    {
        var c0 = corner.Index;
        var c1 = IndexFromSize(corner.Index, size);
        if (c1 == null) return;
        for (int x = Mathf.Min(c0.x, c1.x); x < Mathf.Max(c0.x, c1.x); x++)
        {
            for (int y = Mathf.Min(c0.y, c1.y); y <= Mathf.Max(c0.y, c1.y); y++)
            {
                for (int z = Mathf.Min(c0.z, c1.z); z <= Mathf.Max(c0.z, c1.z); z++)
                {
                    var voxel = Voxels[x, y, z];
                    voxel.SetState(VoxelState.Black);
                }
            }
        }
    }

    public void ClearGrid()
    {
        foreach (Voxel voxel in Voxels)
        {
            if (voxel.Index.y == 0) voxel.SetState(VoxelState.White);
            else voxel.SetState(VoxelState.Empty);
        }
    }

    /// <summary>
    /// Get an image from the grid at the selected layer
    /// using the <see cref="VoxelState"/> of the voxels
    /// </summary>
    /// <param name="layer">Default layer set to 0</param>
    /// <returns>The image with the colored states</returns>
    public Texture2D ImageFromGrid(int layer = 0, bool transparent = false)
    {
        TextureFormat textureFormat;
        if (transparent) textureFormat = TextureFormat.RGBA32;
        else textureFormat = TextureFormat.RGB24;

        Texture2D gridImage = new Texture2D(GridSize.x, GridSize.z, textureFormat, true, true);

        for (int i = 0; i < gridImage.width; i++)
        {
            for (int j = 0; j < gridImage.height; j++)
            {
                var voxel = Voxels[i, layer, j];

                Color c;
                if (voxel.State == VoxelState.Black) c = Color.black;
                else if (voxel.State == VoxelState.Red) c = Color.red;
                else if (voxel.State == VoxelState.Yellow) c = Color.yellow;
                else c = new Color(1f, 1f, 1f, 0f);


                gridImage.SetPixel(i, j, c);
            }
        }

        gridImage.Apply();

        return gridImage;
    }

    public void SetStatesFromImage(Texture2D image, int startY, int endY)
    {
        var resized = new Texture2D(image.width, image.height);
        resized.SetPixels(image.GetPixels());
        resized.Apply();
        TextureScale.Point(resized, GridSize.x, GridSize.z);
        // Iterate through the XZ plane
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.z; z++)
            {
                // Get the pixel color from the image
                var pixel = resized.GetPixel(x, z);

                // Check if pixel is red
                if (pixel.r > pixel.g && pixel.a > 0.5)
                {
                    // Set respective color to voxel
                    var y = Mathf.RoundToInt((endY - startY) * pixel.a) + startY;
                    Voxels[x, y, z].SetState(VoxelState.Red);
                }
                else if (pixel == Color.black)
                {
                    for (int y = 0; y < GridSize.y; y++)
                    {
                        Voxels[x, y, z].SetState(VoxelState.Black);
                    }
                    
                }
            }
        }
    }

    #endregion

    #region Private methods

    Vector3Int IndexFromSize(Vector3Int origin, Vector3Int size)
    {
        var xDirections = new List<int> { 1, -1 }.Shuffle();
        var zDirections = new List<int> { 1, -1 }.Shuffle();

        Vector3Int secondCorner = origin;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                secondCorner = new Vector3Int(origin.x + xDirections[i] * size.x, origin.y + size.y, origin.z + zDirections[j] * size.z);
                if (secondCorner.IsInsideGrid(GridSize)) return secondCorner;
            }
        }
        return secondCorner;
    }

    #endregion

    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }

    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }

    #endregion Grid elements constructors

    #region Grid helpers


    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }


    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    #endregion Grid helpers


}