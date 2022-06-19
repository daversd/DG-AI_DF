using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Grid size sliders
    [SerializeField]
    Slider _sliderX;
    [SerializeField]
    Slider _sliderY;
    [SerializeField]
    Slider _sliderZ;

    // Grid charcteristics sliders
    [SerializeField]
    Slider _sliderBase;
    [SerializeField]
    Slider _sliderTop;
    [SerializeField]
    Slider _sliderThickness;
    [SerializeField]
    Slider _sliderSensitivity;

    // Save file name
    [SerializeField]
    TMP_InputField _saveName;
    
    [SerializeField]
    GameObject _mouseTag;

    // Preview images
    [SerializeField]
    Image _inputPreview;
    [SerializeField]
    Image _outputPreview;

    // Selection dropdown
    [SerializeField]
    TMP_Dropdown _sourceDropdown;
    Dictionary<int, Texture2D> _sourceImages;

    EnvironmentManager _envManager;

    public Vector3Int GridSize => new Vector3Int(
        (int)_sliderX.value,
        (int)_sliderY.value,
        (int)_sliderZ.value);

    public Vector3Int MaxGridSize => new Vector3Int(
        (int)_sliderX.maxValue,
        (int)_sliderY.maxValue,
        (int)_sliderZ.maxValue);

    public void SetInputImage(Sprite sprite) => _inputPreview.sprite = sprite;
    public void SetOutputImage(Sprite sprite) => _outputPreview.sprite = sprite;

    public void SetMouseTagText(string text)
    {
        _mouseTag.SetActive(true);
        _mouseTag.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = text;
    }

    public void SetMouseTagPosition(Vector3 pos) => _mouseTag.transform.position = pos;
    public void HideMouseTag() => _mouseTag.SetActive(false);

    void Start()
    {
        _mouseTag.SetActive(false);
        _envManager = GameObject.Find("Manager").transform.GetComponent<EnvironmentManager>();
    }

    void Update()
    {
        
    }

    public void CallGridUpdate()
    {
        _envManager.UpdateGridSize(GridSize);
    }

    public void CallGridPreview()
    {
        _envManager.PreviewGridSize(GridSize);
    }

    public void ReadImageButton() => _envManager.ReadImage();

    public void SaveGridButton() => _envManager.SaveGrid(_saveName.text);

    public float GetSturctureBase() => _sliderBase.value;
    public float GetSturctureTop() => _sliderTop.value;
    public float GetSturctureSensitivity() => _sliderSensitivity.value;
    public int GetSturctureThickness() => (int) _sliderThickness.value;

    public Texture2D SetDropdownSources()
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
        _sourceDropdown.value = 0;
        return images[0];
    }

    public Texture2D GetCurrentImage() => _sourceImages[_sourceDropdown.value];

}
