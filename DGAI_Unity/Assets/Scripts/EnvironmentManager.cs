using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Determina o estágio atual da aplicação
/// </summary>
public enum AppStage { Neutral = 0, Selecting = 1, Done = 2 }

/// <summary>
/// Classe que gerencia o ambiente da aplicação
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    #region Campos privados

    VoxelGrid _grid;
    Voxel[] _corners;
    AppStage _stage;
    /// <summary>Determina a altura atual da caixas a serem criadas</summary>
    int _height;

    /// <summary>Seed para controle dos números aleatórios</summary>
    int _seed = 666;

    // 09 (p2p) Cria o objeto de inferência do modelo Pix2Pix
    /// <summary>Objeto de previsão do modelo Pix2Pix</summary>
    Pix2Pix _pix2pix;

    Texture2D _sourceImage;
    UIManager _uiManager;

    #endregion

    #region Métodos do Unity
    void Start()
    {
        // Coleta o UIManager
        _uiManager = GameObject.Find("UIManager").transform.GetComponent<UIManager>();
        if (_uiManager == null) Debug.LogError("UIManager não foi encontrado!");
        
        // Define as imagens que podem ser utilizadas manualmente
        _sourceImage = _uiManager.SetDropdownSources();

        // Inicializa o motor de números aleatórios e a aplicação
        Random.InitState(_seed);
        _stage = AppStage.Neutral;

        // Inicializa o grid que será trabalhado
        _corners = new Voxel[2];
        var gridSize = _uiManager.GridSize;
        _height = gridSize.y;
        var maxSize = _uiManager.MaxGridSize;
        _grid = new VoxelGrid(gridSize, maxSize, transform.position, 1f, transform);

        // 10 (p2p) Inicializa o objeto de inferência do modelo Pix2Pix
        _pix2pix = new Pix2Pix();
    }

    void Update()
    {
        HandleDrawing();
        HandleHeight();
        // Limpar o grid utilizando a tecla "C"
        if (Input.GetKeyDown(KeyCode.C)) _grid.ClearGrid();
        
        // 05 Cria caixas aleatórias utilizando a tecla "A"
        if (Input.GetKeyDown(KeyCode.A))
        {
            // 06 Limpa o grid
            _grid.ClearGrid();
            // 07 Cria uma caixa aleatória
            //CreateRandomBox(3, 10, 5, 15);

            // 10 Cria diversas caixas aleatórias
            PopulateRandomBoxes(Random.Range(3, 10), 5, 15, 5, 15);
            var image = _grid.ImageFromGrid();
            _uiManager.SetInputImage(Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f));
        }
    }

    #endregion

    // 01 Criar método para criação de caixas aleatórias
    /// <summary>
    /// Cria uma caixa aleatóriamente no grid, dentro dos limities de tamanho definidos
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="minZ"></param>
    /// <param name="maxZ"></param>
    private void CreateRandomBox(int minX, int maxX, int minZ, int maxZ)
    {
        // 02 Define as coordenadas de origem aleatoriamente no grid
        var oX = Random.Range(0, _grid.Size.x);
        var oZ = Random.Range(0, _grid.Size.z);

        var origin = new Vector3Int(oX, 0, oZ);

        // 03 Define o tamaho da caixa 
        var sizeX = Random.Range(minX, maxX);
        var sizeY = Random.Range(3, _grid.Size.y);
        var sizeZ = Random.Range(minZ, maxZ);
        var size = new Vector3Int(sizeX, sizeY, sizeZ);

        // 04 Cria a caixa
        _grid.BoxFromCorner(_grid.Voxels[origin.x, origin.y, origin.z], size);
    }

    // 08 Criar o método para criação de diversas caixas aleatórias
    /// <summary>
    /// Cria múltiplas caixas aleatórias no grid com as propriedades definidas
    /// </summary>
    /// <param name="quantity">Quantidade de caixas</param>
    /// <param name="minX">Tamanho mínimo em X</param>
    /// <param name="maxX">Tamanho máximo em X</param>
    /// <param name="minZ">Tamanho mínimo em Z</param>
    /// <param name="maxZ">Tamanho máximo em Z</param>
    private void PopulateRandomBoxes(int quantity, int minX, int maxX, int minZ, int maxZ)
    {
        // 09 Repetir o método de acordo com a quantidade
        for (int i = 0; i < quantity; i++)
        {
            CreateRandomBox(minX, maxX, minZ, maxZ);
        }
    }

    // 11 Criar o método para criação do set de treinamento
    /// <summary>
    /// Cria múltiplas caixas aleatórias no grid, diversas vezes e 
    /// com quantidades variáveis, de acordo com as propriedades definidas,
    /// e salva imagens correspondentes no disco
    /// </summary>
    /// <param name="samples">Quantidade de grids/imagens a gerar</param>
    /// <param name="minQuantity">Quantidade mínima de caixas por sample</param>
    /// <param name="maxQuantity">Quantidade máxima de caixas por sample</param>
    /// <param name="minX">Tamanho mínimo em X</param>
    /// <param name="maxX">Tamanho máximo em X</param>
    /// <param name="minZ">Tamanho mínimo em Z</param>
    /// <param name="maxZ">Tamanho máximo em Z</param>
    public void PopulateBoxesAndSave(int samples, int minQuantity, int maxQuantity, int minX, int maxX, int minZ, int maxZ)
    {
        // 12 Garatir que a pasta de destino existe
        string directory = Path.Combine(Directory.GetCurrentDirectory(), "Samples");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        // 13 Iterar de acordo com a quantidade de samples
        for (int i = 0; i < samples; i++)
        {
            // 14 Limpar o grid
            _grid.ClearGrid();
            // 15 Definir quantidade aleatória e criar as caixas aleatoriamente
            int quantity = Random.Range(minQuantity, maxQuantity);
            PopulateRandomBoxes(quantity, minX, maxX, minZ, maxZ);

            // 16 Definir a imagem para o atual estado do grid
            var image = _grid.ImageFromGrid(transparent: true);
            // 17 Redimensionar a imagem para 256 x 256 px
            var resized = ImageReadWrite.Resize256(image, Color.white);

            // 18 Criar o arquivo e salvar a imagem no disco
            var fileName = Path.Combine(directory, $"sample_{i.ToString("D4")}.png");
            ImageReadWrite.SaveImage(resized, fileName);
        }
    }

    /// <summary>
    /// Expõe o método de criação do set de treinamento para um botão
    /// </summary>
    public void GenerateSampleSet()
    {
        // 19 Criar e salvar as imagens aleatórias
        PopulateBoxesAndSave(500, 3, 10, 3, 10, 3, 10);
    }

    // 11 (p2p) Criar o método de previsão e atualização do grid
    /// <summary>
    /// Executa o modelo Pix2Pix no atual estado do <see cref="VoxelGrid"/> e atualiza
    /// o estado dos Voxels de acordo com os pixels da imagem resultante
    /// </summary>
    public void PredictAndUpdate()
    {
        // 12 (p2p) Limpar voxels vermelhos e gerar imagem
        _grid.ClearReds();
        var image = _grid.ImageFromGrid();

        // 13 (p2p) Redimensionar image
        var resized = ImageReadWrite.Resize256(image, Color.white);

        // 14 (p2p) Gerar previsão
        _sourceImage = _pix2pix.Predict(resized);

        // 15 (p2p) Redimensionar imagem para o tamnho do grid e atualizar os voxels vermelhos
        TextureScale.Point(_sourceImage, _grid.Size.x, _grid.Size.z);
        UpdateReds();

        // 16 (p2p) Exibir as imagens produzidas na UI
        _uiManager.SetInputImage(Sprite.Create(resized, new Rect(0, 0, resized.width, resized.height), Vector2.one * 0.5f));
        _uiManager.SetOutputImage(Sprite.Create(_sourceImage, new Rect(0, 0, _sourceImage.width, _sourceImage.height), Vector2.one * 0.5f));

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

    }

    /// <summary>
    /// Expõe a função de atualizar os voxels com estado <see cref="VoxelState.Red"/>
    /// </summary>
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

    /// <summary>
    /// Gerencia o processo de desenho de caixas na interface
    /// </summary>
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

            // 17 (p2p) Executar a previsão após o término do processo de desenho
            PredictAndUpdate();
        }
    }

    /// <summary>
    /// Gerencia o controle da altura das caixas a serem criadas na interface
    /// </summary>
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

    /// <summary>
    /// Atualiza o tamanho do grid
    /// </summary>
    /// <param name="newSize"></param>
    public void UpdateGridSize(Vector3Int newSize)
    {
        _height = Mathf.Clamp(_height, 1, newSize.y);
        _grid.ChangeGridSize(newSize);
        _grid.ShowPreview(false);
    }

    /// <summary>
    /// Executa a previsão do novo tamanho do grid
    /// </summary>
    /// <param name="newSize"></param>
    public void PreviewGridSize(Vector3Int newSize)
    {
        _grid.ShowPreview(true);
        _grid.UpdatePreview(newSize);
    }

    /// <summary>
    /// Expõe a função de ler uma imagem para a UI
    /// </summary>
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

    

    /// <summary>
    /// Expõe a funlçao de limpar o grid para a UI
    /// </summary>
    public void ClearGrid()
    {
        _grid.ClearGrid();
    }

    /// <summary>
    /// Expõe a função de atualizar a imagem atual pela UI
    /// </summary>
    public void UpdateCurrentImage()
    {
        _sourceImage = _uiManager.GetCurrentImage();
    }
}
