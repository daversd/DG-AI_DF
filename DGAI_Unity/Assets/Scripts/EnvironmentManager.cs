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
    Dictionary<int, Texture2D> _outputImages;
    Texture2D _sourceImage;

    [SerializeField]
    Slider _sliderX;
    [SerializeField]
    Slider _sliderY;
    [SerializeField]
    Slider _sliderZ;

    [SerializeField]
    Slider _sliderStart;
    [SerializeField]
    Slider _sliderEnd;
    [SerializeField]
    Slider _sliderThickness;
    [SerializeField]
    Slider _sliderSensitivity;
    [SerializeField]
    TMP_InputField _inputName;
    [SerializeField]
    TMP_Dropdown _sourceDropdown;
    [SerializeField]
    Image _inputPreview;
    [SerializeField]
    Image _outputPreview;

    public GameObject MouseTag;


    // Start is called before the first frame update
    void Start()
    {
        _pix2pix = new Pix2Pix();

        SetDropdownSources();

        Random.InitState(_seed);
        MouseTag.SetActive(false);
        _stage = AppStage.Neutral;
        _corners = new Voxel[2];
        var gridSize = new Vector3Int((int)_sliderX.value, (int)_sliderY.value, (int)_sliderZ.value);
        _height = gridSize.y;
        var maxSize = new Vector3Int((int)_sliderX.maxValue, (int)_sliderY.maxValue, (int)_sliderZ.maxValue);
        _grid = new VoxelGrid(gridSize, maxSize, transform.position, 1f, transform);
    }

    // Update is called once per frame
    void Update()
    {
        HandleDrawing();
        HandleHeight();
        if (Input.GetKeyDown(KeyCode.C)) _grid.ClearGrid();
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
            PredictAndUpdate();
        }
    }

    void PredictAndUpdate()
    {
        Debug.Log("predicting");
        var image = _grid.ImageFromGrid();
        
        var resized = ImageReadWrite.Resize256(image, Color.white);
        _sourceImage = _pix2pix.Predict(resized);
        TextureScale.Point(_sourceImage, _grid.Size.x, _grid.Size.z);
        _grid.SetStatesFromImage(_sourceImage, _sliderStart.value, _sliderEnd.value, (int)_sliderThickness.value, _sliderSensitivity.value);

        _inputPreview.sprite = Sprite.Create(resized, new Rect(0, 0, resized.width, resized.height), Vector2.one * 0.5f);
        _outputPreview.sprite = Sprite.Create(_sourceImage, new Rect(0, 0, _sourceImage.width, _sourceImage.height), Vector2.one * 0.5f);
        ImageReadWrite.SaveImage(_sourceImage, $"{Directory.GetCurrentDirectory()}/output.png");

    }

    private void HandleHeight()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            _height = Mathf.Clamp(_height + 1, 1, _grid.Size.y);
            MouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = _height.ToString();
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            _height = Mathf.Clamp(_height - 1, 1, _grid.Size.y);
            MouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = _height.ToString();
        }
    }

    public void UpdateGridSize()
    {
        _height = Mathf.Clamp(_height, 1, (int)_sliderY.value);
        _grid.ChangeGridSize(new Vector3Int((int)_sliderX.value, (int)_sliderY.value, (int)_sliderZ.value));
        _grid.ShowPreview(false);
    }

    public void PreviewGridSize()
    {
        _grid.ShowPreview(true);
        _grid.UpdatePreview(new Vector3Int((int)_sliderX.value, (int)_sliderY.value, (int)_sliderZ.value));
    }

    public void ReadImage()
    {
        //var img = Resources.Load<Texture2D>("Data/map");
        _grid.ClearGrid();
        _grid.SetStatesFromImage(_sourceImage, _sliderStart.value, _sliderEnd.value, (int)_sliderThickness.value, _sliderSensitivity.value);
    }

    public void SaveGrid()
    {
        if (_inputName.text == "")
        {
            Debug.Log("N�o � poss�vel salvar arquivo sem nome!");
            return;
        }
        var directory = Path.Combine(Directory.GetCurrentDirectory(), $"Grids");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{_inputName.text}.csv");
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

    public void SetDropdownSources()
    {
        _sourceDropdown.ClearOptions();
        _sourceImages = new Dictionary<int, Texture2D>();
        var images = Resources.LoadAll<Texture2D>("Data");
        List<string> names = new List<string>();
        for (int i = 0; i < images.Length; i++)
        {
            _sourceImages.Add(i, images[i]);
            names.Add($"image {i}");
        }
        _sourceDropdown.AddOptions(names);
        _sourceImage = images[0];
        _sourceDropdown.value = 0;
    }

    public void UpdateCurrentImage()
    {
        _sourceImage = _sourceImages[_sourceDropdown.value];
    }
}
