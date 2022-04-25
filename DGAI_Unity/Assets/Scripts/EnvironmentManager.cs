using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;

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
        var gridSize = new Vector3Int(64, 10, 64);
        _height = gridSize.y;
        _grid = new VoxelGrid(gridSize, transform.position, 1f, transform);
    }

    // Update is called once per frame
    void Update()
    {
        HandleDrawing();
        HandleHeight();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PopulateRandomBoxes(10, 10, 15, 10, 15);
        }
        if (Input.GetKeyDown(KeyCode.R)) _grid.ClearGrid();
        if (Input.GetKeyDown(KeyCode.S))
        {
            var image = _grid.ImageFromGrid(transparent: true);
            var resized = ImageReadWrite.Resize256(image, Color.white);

            ImageReadWrite.SaveImage(resized, "Images/test");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            var img = Resources.Load<Texture2D>("Data/map");
            _grid.SetStatesFromImage(img, 4, 9);
        }
    }

    private void CreateRandomBox(int minX, int maxX, int minZ, int maxZ)
    {
        var oX = Random.Range(0, _grid.GridSize.x);
        var oY = Random.Range(0, _grid.GridSize.y);
        var oZ = Random.Range(0, _grid.GridSize.z);

        var origin = new Vector3Int(oX, 0, oZ);

        var sizeX = Random.Range(minX, maxX);
        var sizeY = Random.Range(3, _grid.GridSize.y);
        var sizeZ = Random.Range(minZ, maxZ);
        var size = new Vector3Int(sizeX, sizeY, sizeZ);
        _grid.RectangleFromCorner(_grid.Voxels[origin.x, origin.y, origin.z], size);
    }

    private void PopulateRandomBoxes(int quantity, int minX, int maxX, int minZ, int maxZ)
    {
        for (int i = 0; i < quantity; i++)
        {
            CreateRandomBox(minX, maxX, minZ, maxZ);
        }
    }

    private void HandleDrawing()
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
                            MouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = _height.ToString();
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
                }
            }
        }
        if (_stage == AppStage.Selecting)
        {
            MouseTag.transform.position = Input.mousePosition + new Vector3(50, 0, 15);
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

    private void HandleHeight()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            _height = Mathf.Clamp(_height + 1, 1, _grid.GridSize.y);
            MouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = _height.ToString();
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            _height = Mathf.Clamp(_height - 1, 1, _grid.GridSize.y);
            MouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = _height.ToString();
        }
    }
}
