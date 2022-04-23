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

    #endregion

    #region Private methods



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