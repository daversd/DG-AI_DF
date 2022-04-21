using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum VoxelState { White, Black, Red, Empty };
public class Voxel : IEquatable<Voxel>
{
    #region Public fields

    public Vector3Int Index;
    public List<Face> Faces = new List<Face>(6);
    public Vector3 Center => (Index + _voxelGrid.Origin) * _size;
    public VoxelState State { get; private set; }

    #endregion

    #region Private fields

    VoxelGrid _voxelGrid;
    float _size;
    GameObject _voxelGO;

    #endregion

    #region Contructors

    /// <summary>
    /// Creates a regular voxel on a voxel grid
    /// </summary>
    /// <param name="index">The index of the Voxel</param>
    /// <param name="voxelgrid">The <see cref="VoxelGrid"/> this <see cref="Voxel"/> is attached to</param>
    /// <param name="voxelGameObject">The <see cref="GameObject"/> used on the Voxel</param>
    public Voxel(Vector3Int index, VoxelGrid voxelGrid, GameObject prefab, VoxelState state = VoxelState.Empty, Transform parent = null, float sizeFactor = 1f)
    {
        Index = index;
        _voxelGrid = voxelGrid;
        _size = _voxelGrid.VoxelSize;
        _voxelGO = GameObject.Instantiate(prefab, index, Quaternion.identity, parent);
        _voxelGO.transform.localScale = Vector3.one * sizeFactor;
        var trigger = _voxelGO.AddComponent<VoxelTrigger>();
        trigger.Voxel = this;
        SetState(state);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Get the neighbouring voxels at each face, if it exists
    /// </summary>
    /// <returns>All neighbour voxels</returns>
    public IEnumerable<Voxel> GetFaceNeighbours()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) yield return _voxelGrid.Voxels[x + 1, y, z];
        if (x != 0) yield return _voxelGrid.Voxels[x - 1, y, z];

        if (y != s.y - 1) yield return _voxelGrid.Voxels[x, y + 1, z];
        if (y != 0) yield return _voxelGrid.Voxels[x, y - 1, z];

        if (z != s.z - 1) yield return _voxelGrid.Voxels[x, y, z + 1];
        if (z != 0) yield return _voxelGrid.Voxels[x, y, z - 1];
    }

    public Voxel[] GetFaceNeighboursArray()
    {
        Voxel[] result = new Voxel[6];

        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) result[0] = _voxelGrid.Voxels[x + 1, y, z];
        else result[0] = null;

        if (x != 0) result[1] = _voxelGrid.Voxels[x - 1, y, z];
        else result[1] = null;

        if (y != s.y - 1) result[2] = _voxelGrid.Voxels[x, y + 1, z];
        else result[2] = null;

        if (y != 0) result[3] = _voxelGrid.Voxels[x, y - 1, z];
        else result[3] = null;

        if (z != s.z - 1) result[4] = _voxelGrid.Voxels[x, y, z + 1];
        else result[4] = null;

        if (z != 0) result[5] = _voxelGrid.Voxels[x, y, z - 1];
        else result[5] = null;

        return result;
    }

    public void SetState(VoxelState newState)
    {
        // 28 Set the new state
        State = newState;
        // 29 Get GO components
        var renderer = _voxelGO.GetComponent<MeshRenderer>();
        var collider = _voxelGO.GetComponent<Collider>();
        // 30 Set properties for each state
        if (State == VoxelState.White)
        {
            renderer.enabled = true;
            collider.enabled = true;
            renderer.material = Resources.Load<Material>("Materials/White");
        }
        else if (State == VoxelState.Black)
        {
            renderer.enabled = true;
            collider.enabled = true;
            renderer.material = Resources.Load<Material>("Materials/Black");
        }
        else if (State == VoxelState.Empty)
        {
            renderer.enabled = false;
            collider.enabled = false;

        }
    }

    #endregion

    #region Equality checks

    /// <summary>
    /// Checks if two Voxels are equal based on their Index
    /// </summary>
    /// <param name="other">The <see cref="Voxel"/> to compare with</param>
    /// <returns>True if the Voxels are equal</returns>
    public bool Equals(Voxel other)
    {
        return (other != null) && (Index == other.Index);
    }

    /// <summary>
    /// Get the HashCode of this <see cref="Voxel"/> based on its Index
    /// </summary>
    /// <returns>The HashCode as an Int</returns>
    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    #endregion
}