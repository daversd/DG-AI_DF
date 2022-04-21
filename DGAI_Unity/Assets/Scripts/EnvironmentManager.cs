using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{

    VoxelGrid _grid;

    // Start is called before the first frame update
    void Start()
    {
        var gridSize = new Vector3Int(32, 5, 32);
        _grid = new VoxelGrid(gridSize, transform.position, 1f, transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    Voxel voxel = hit.transform.GetComponent<VoxelTrigger>().Voxel;
                    if (voxel.State == VoxelState.White) voxel.SetState(VoxelState.Black);
                    else if (voxel.State == VoxelState.Black) voxel.SetState(VoxelState.White);
                }
            }
        }
    }
}
