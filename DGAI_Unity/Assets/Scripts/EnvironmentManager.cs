using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum AppStage { Neutral = 0, Selecting = 1, Done = 2 }

public class EnvironmentManager : MonoBehaviour
{

    VoxelGrid _grid;
    Voxel[] _corners;
    AppStage _stage;
    int _height;
    int _seed = 666;

    Pix2Pix _pix2pix;

    Dictionary<int, Texture2D> _sourceImages;
    Texture2D _sourceImage;

    UIManager _uiManager;

    


    // Start is called before the first frame update
    void Start()
    {
        _uiManager = GameObject.Find("UIManager").transform.GetComponent<UIManager>();
        if (_uiManager == null) Debug.LogError("UIManager não foi encontrado!");
        _pix2pix = new Pix2Pix();

        _sourceImage = _uiManager.SetDropdownSources();

        Random.InitState(_seed);
        _stage = AppStage.Neutral;
        _corners = new Voxel[2];
        var gridSize = _uiManager.GridSize;
        _height = gridSize.y;
        var maxSize = _uiManager.MaxGridSize;
        _grid = new VoxelGrid(gridSize, maxSize, transform.position, 1f, transform);
    }

    // Update is called once per frame
    void Update()
    {
        HandleDrawing();
        HandleHeight();
        if (Input.GetKeyDown(KeyCode.C)) _grid.ClearGrid();
        if (Input.GetKeyDown(KeyCode.A))
        {
            _grid.ClearGrid();
            PopulateRandomBoxes(Random.Range(3, 10), 5, 15, 5, 15);
            var image = _grid.ImageFromGrid();
            _uiManager.SetInputImage(Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f));
        }
    }

    private void CreateRandomBox(int minX, int maxX, int minZ, int maxZ)
    {
        var oX = Random.Range(0, _grid.Size.x);
        var oZ = Random.Range(0, _grid.Size.z);

        var origin = new Vector3Int(oX, 0, oZ);

        var sizeX = Random.Range(minX, maxX);
        var sizeY = Random.Range(3, _grid.Size.y);
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

    public void PopulateBoxesAndSave(int samples, int minQuantity, int maxQuantity, int minX, int maxX, int minZ, int maxZ)
    {
        string directory = Path.Combine(Directory.GetCurrentDirectory(), "Samples");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        for (int i = 0; i < samples; i++)
        {
            _grid.ClearGrid();
            int quantity = Random.Range(minQuantity, maxQuantity);
            PopulateRandomBoxes(quantity, minX, maxX, minZ, maxZ);

            var image = _grid.ImageFromGrid(transparent: true);
            var resized = ImageReadWrite.Resize256(image, Color.white);

            var fileName = Path.Combine(directory, $"sample_{i.ToString("D4")}.png");
            ImageReadWrite.SaveImage(resized, fileName);
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
                    if (voxel.State == VoxelState.White || voxel.State == VoxelState.Yellow)
                    {
                        if (_stage == AppStage.Neutral)
                        {
                            _uiManager.SetMouseTagText(_height.ToString());
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
            _uiManager.SetMouseTagPosition(Input.mousePosition + new Vector3(50, 0, 15));
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_stage == AppStage.Selecting) _stage = AppStage.Done;
            _uiManager.HideMouseTag();
        }
        if (_stage == AppStage.Done)
        {
            _grid.MakeBox(_height);
            _stage = AppStage.Neutral;
            PredictAndUpdate();
        }
    }

    public void PredictAndUpdate()
    {
        _grid.ClearReds();
        var image = _grid.ImageFromGrid();
        
        var resized = ImageReadWrite.Resize256(image, Color.white);
        _sourceImage = _pix2pix.Predict(resized);
        TextureScale.Point(_sourceImage, _grid.Size.x, _grid.Size.z);
        UpdateReds();

        _uiManager.SetInputImage(Sprite.Create(resized, new Rect(0, 0, resized.width, resized.height), Vector2.one * 0.5f));
        _uiManager.SetOutputImage(Sprite.Create(_sourceImage, new Rect(0, 0, _sourceImage.width, _sourceImage.height), Vector2.one * 0.5f));

    }

    private void HandleHeight()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            _height = Mathf.Clamp(_height + 1, 1, _grid.Size.y);
            _uiManager.SetMouseTagText(_height.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            _height = Mathf.Clamp(_height - 1, 1, _grid.Size.y);
            _uiManager.SetMouseTagText(_height.ToString());
        }
    }

    public void UpdateGridSize(Vector3Int newSize)
    {
        _height = Mathf.Clamp(_height, 1, newSize.y);
        _grid.ChangeGridSize(newSize);
        _grid.ShowPreview(false);
    }

    public void PreviewGridSize(Vector3Int newSize)
    {
        _grid.ShowPreview(true);
        _grid.UpdatePreview(newSize);
    }

    public void ReadImage()
    {
        _grid.ClearGrid();
        _grid.SetStatesFromImage(
            _sourceImage, 
            _uiManager.GetSturctureBase(), 
            _uiManager.GetSturctureTop(), 
            _uiManager.GetSturctureThickness(), 
            _uiManager.GetSturctureSensitivity());
    }

    public void UpdateReds()
    {
        _grid.ClearReds();
        _grid.SetStatesFromImage(_sourceImage, 
            _uiManager.GetSturctureBase(),
            _uiManager.GetSturctureTop(),
            _uiManager.GetSturctureThickness(),
            _uiManager.GetSturctureSensitivity(), 
            setBlacks: false);
    }

    public void ClearGrid()
    {
        _grid.ClearGrid();
    }

    public void SaveGrid(string name)
    {
        if (name == "")
        {
            Debug.Log("Não é possível salvar arquivo sem nome!");
            return;
        }
        var directory = Path.Combine(Directory.GetCurrentDirectory(), $"Grids");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{name}.csv");
        Util.SaveVoxels(_grid, new List<VoxelState>() { VoxelState.Red, VoxelState.Black }, path);

        //foreach (var key in _sourceImages.Keys)
        //{
        //    var name = $"grid {key}";
        //    var image = _sourceImages[key];
        //    _grid.ClearGrid();
        //    _grid.SetStatesFromImage(image, _sliderStart.value, _sliderEnd.value, (int)_sliderThickness.value, _sliderSensitivity.value);
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), $"Grids");
        //    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        //    var path = Path.Combine(directory, $"{name}.csv");
        //    Util.SaveVoxels(_grid, new List<VoxelState>() { VoxelState.Red, VoxelState.Black }, path);
        //}
    }

    public void GenerateSampleSet()
    {
        PopulateBoxesAndSave(500, 3, 10, 3, 10, 3, 10);
    }

    public void UpdateCurrentImage()
    {
        _sourceImage = _uiManager.GetCurrentImage();
    }
}
