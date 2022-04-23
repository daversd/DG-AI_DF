using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AppStage { Neutral = 0, Selecting = 1, Done = 2 }

public class EnvironmentManager : MonoBehaviour
{

    VoxelGrid _grid;
    Voxel[] _corners;
    AppStage _stage;
    int _height;

    public GameObject MouseTag;
    

    // Start is called before the first frame update
    void Start()
    {
        MouseTag.SetActive(false);
        _stage = AppStage.Neutral;
        _corners = new Voxel[2];
        var gridSize = new Vector3Int(64, 5, 64);
        _height = gridSize.y;
        _grid = new VoxelGrid(gridSize, transform.position, 1f, transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                
                if (hit.transform.CompareTag("Voxel"))
                {
                    Voxel voxel = hit.transform.GetComponent<VoxelTrigger>().Voxel;
                    if (voxel.State != VoxelState.Black)
                    {
                        if (_stage == AppStage.Neutral)
                        {
                            MouseTag.SetActive(true);
                            MouseTag.transform.GetChild(0).GetComponent<TextMesh>().text = _height.ToString();
                            _stage = AppStage.Selecting;
                            _corners[0] = voxel;
                            voxel.SetState(VoxelState.Black);
                        }
                        else if (_stage == AppStage.Selecting && voxel != _corners[0])
                        {
                            _corners[1] = voxel;
                            _grid.SetCorners(_corners);
                        }
                    }
                    
                    
                    //if (voxel.State == VoxelState.White) voxel.SetState(VoxelState.Black);
                    //else if (voxel.State == VoxelState.Black) voxel.SetState(VoxelState.White);
                }
            }
        }
        if (_stage == AppStage.Selecting)
        {
            MouseTag.transform.position = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _stage = AppStage.Done;
            MouseTag.SetActive(false);
        }
        if (_stage == AppStage.Done)
        {
            _grid.MakeBox(_height);
            _stage = AppStage.Neutral;
        }
    }
}
