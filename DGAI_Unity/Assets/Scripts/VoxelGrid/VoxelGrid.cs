using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Um grid tridimensional de <see cref="Voxel"/>
/// </summary>
public class VoxelGrid
{
    #region Campos públicos

    public Vector3Int Size;
    public Voxel[,,] Voxels;
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }

    #endregion

    #region Campos privados

    Voxel[,,] _allVoxels;
    Vector3Int _maxSize;

    List<Voxel> _currentSelection;
    Voxel[] _currentCorners;

    GameObject _voxelPrefab;
    Transform _parent;

    GameObject _previewCube;

    #endregion

    #region Construtores

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3Int maxSize, Vector3 origin, float voxelSize, Transform parent = null)
    {
        Size = size;

        Origin = origin;
        VoxelSize = voxelSize;
        _maxSize = maxSize;

        _voxelPrefab = Resources.Load<GameObject>("Prefabs/cube");
        _parent = parent;
        CreateVoxels();

        var previewPrefab = Resources.Load<GameObject>("Prefabs/PreviewCube");
        _previewCube = GameObject.Instantiate(previewPrefab);
        _previewCube.transform.SetParent(_parent);
        _previewCube.SetActive(false);
        UpdatePreview(Size);
    }


    #endregion

    #region Métodos públicos

    /// <summary>
    /// Modifica o tamanho deste grid, ativando ou desativando os voxels necessários
    /// </summary>
    /// <param name="newSize"></param>
    public void ChangeGridSize(Vector3Int newSize)
    {
        Voxel[,,] tempVoxels = new Voxel[newSize.x, newSize.y, newSize.z];

        int endX = Mathf.Max(Size.x, newSize.x);
        
        int endY = Mathf.Max(Size.y, newSize.y);

        int endZ = Mathf.Max(Size.z, newSize.z);


        for (int x = 0; x < endX; x++)
        {
            for (int y = 0; y < endY; y++)
            {
                for (int z = 0; z < endZ; z++)
                {
                    var index = new Vector3Int(x, y, z);
                    if (Util.IsInsideGrid(index, Size) && !Util.IsInsideGrid(index, newSize))
                    {
                        // remove
                        _allVoxels[x, y, z].SetState(VoxelState.NotUsed);
                    }
                    else if(!Util.IsInsideGrid(index, Size) && Util.IsInsideGrid(index, newSize))
                    {
                        // add
                        var voxel = _allVoxels[x, y, z];
                        if(y == 0) voxel.SetState(VoxelState.White);
                        else voxel.SetState(VoxelState.Empty);
                        tempVoxels[x, y, z] = voxel;
                    }
                    else
                    {
                        var voxel = _allVoxels[x, y, z];
                        tempVoxels[x, y, z] = voxel;
                    }
                }
            }
        }
        
        Voxels = tempVoxels;
        Size = newSize;
    }

    /// <summary>
    /// Define os cantos atuais da área de seleção
    /// </summary>
    /// <param name="corners"></param>
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

    /// <summary>
    /// Cria uma "caixa" de voxels a partir dos cantos atuais e com uma determinada altura
    /// </summary>
    /// <param name="height"></param>
    public void MakeBox(int height)
    {
        if (_currentSelection == null || _currentSelection.Count == 0) return;

        int baseLevel = _currentSelection.Min(v => v.Index.y);
        int topLevel = Mathf.RoundToInt(Size.y * height);

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

    /// <summary>
    /// Cria uma "caixa" de voxels a partir de um de seus cantos, com o tamanho indicado
    /// </summary>
    /// <param name="corner"></param>
    /// <param name="size"></param>
    public void BoxFromCorner(Voxel corner, Vector3Int size)
    {
        var c0 = corner.Index;
        var c1 = DiagonalCornerFromSize(corner.Index, size);
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

    /// <summary>
    /// Limpa o grid, reestabelecendo os <see cref="VoxelState"/> de todos seus voxels
    /// </summary>
    public void ClearGrid()
    {
        foreach (Voxel voxel in Voxels)
        {
            if (voxel.Index.y == 0) voxel.SetState(VoxelState.White);
            else voxel.SetState(VoxelState.Empty);
        }
    }

    /// <summary>
    /// Limpa o grid, removendo os Voxels com VoxelState.Red
    /// </summary>
    public void ClearReds()
    {
        foreach (Voxel voxel in Voxels)
        {
            if (voxel.State == VoxelState.Red)
            {
                if (voxel.Index.y == 0) voxel.SetState(VoxelState.White);
                else voxel.SetState(VoxelState.Empty);
            }
            
        }
    }

    /// <summary>
    /// Cria uma imagem do grid a partir dos <see cref="VoxelState"/> de seus voxels
    /// </summary>
    /// <param name="layer">Layer padrão é 0</param>
    /// <returns></returns>
    public Texture2D ImageFromGrid(int layer = 0, bool transparent = false)
    {
        TextureFormat textureFormat;
        if (transparent) textureFormat = TextureFormat.RGBA32;
        else textureFormat = TextureFormat.RGB24;

        Texture2D gridImage = new Texture2D(Size.x, Size.z, textureFormat, true, true);

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

    /// <summary>
    /// Define os <see cref="VoxelState"/> dos <see cref="Voxel"/> do grid a partir de uma imagem,
    /// criando caixas para os pixels pretos, e a estrutura voxelizada para os pixels vermelhos
    /// </summary>
    /// <param name="image">A imagem de referência</param>
    /// <param name="bottomLimit">A altura do limite inferior da estrutura</param>
    /// <param name="topLimit">A altura do limite superior da estrutura</param>
    /// <param name="thickness">A espessura da estura, ativando voxels extra</param>
    /// <param name="sensitivity">A sensibilidade na leitura da imagem</param>
    /// <param name="setBlacks">Define se os pixels pretos serão utilizados</param>
    public void SetStatesFromImage(Texture2D image, float bottomLimit, float topLimit, int thickness, float sensitivity, bool setBlacks = false)
    {
        int startY = Mathf.RoundToInt(bottomLimit * (Size.y - 1));
        int endY = Mathf.RoundToInt(topLimit * (Size.y - 1));
        var resized = new Texture2D(image.width, image.height);
        resized.SetPixels(image.GetPixels());
        resized.Apply();
        TextureScale.Point(resized, Size.x, Size.z);
        // Iterate through the XZ plane
        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.z; z++)
            {
                // Get the pixel color from the image
                var pixel = resized.GetPixel(x, z);

                // Check if pixel is red
                if (pixel.r > pixel.g && pixel.r > pixel.b && pixel.grayscale < sensitivity)
                {
                    Color.RGBToHSV(pixel, out float h, out float s, out float v);
                    // Set respective color to voxel
                    var y = Mathf.RoundToInt((endY - startY) * s) + startY;
                    if (y == 0) break;
                    Voxels[x, y, z].SetState(VoxelState.Red);
                    if (thickness > 0)
                    {
                        for (int i = 1; i < thickness; i++)
                        {
                            int newY = y - i;
                            if (newY == 0) break;
                            if (newY >= 0 && newY < Size.y) Voxels[x, newY, z].SetState(VoxelState.Red);
                        }
                    }
                }
                else if (setBlacks && pixel == Color.black)
                {
                    for (int y = 1; y < Size.y; y++)
                    {
                        Voxels[x, y, z].SetState(VoxelState.Black);
                    }  
                }
            }
        }
    }

    /// <summary>
    /// Atualiza o preview do novo tamanho do grid
    /// </summary>
    /// <param name="size"></param>
    public void UpdatePreview(Vector3 size)
    {
        _previewCube.transform.localScale = size * 1.01f;
        _previewCube.transform.localPosition = size * 0.5f - Vector3.one * 0.5f;
    }

    /// <summary>
    /// Determina a visibilidade do preview do novo tamanho do grid
    /// </summary>
    /// <param name="state"></param>
    public void ShowPreview(bool state)
    {
        _previewCube.SetActive(state);
    }

    #endregion

    #region Métodos privaods

    /// <summary>
    /// Cria os voxels do grid
    /// </summary>
    void CreateVoxels()
    {
        if (_allVoxels == null) _allVoxels = new Voxel[_maxSize.x, _maxSize.y, _maxSize.z];

        Voxels = new Voxel[Size.x, Size.y, Size.z];

        for (int x = 0; x < _maxSize.x; x++)
        {
            for (int y = 0; y < _maxSize.y; y++)
            {
                for (int z = 0; z < _maxSize.z; z++)
                {
                    var index = new Vector3Int(x, y, z);
                    _allVoxels[x, y, z] = new Voxel(
                        index,
                        this,
                        _voxelPrefab,
                        state: VoxelState.NotUsed,
                        //state: y == 0 ? VoxelState.White : VoxelState.Empty,
                        parent: _parent,
                        sizeFactor: 0.96f);
                    if (Util.IsInsideGrid(index, Size))
                    {
                        Voxels[x, y, z] = _allVoxels[x, y, z];
                        Voxels[x, y, z].SetState(y == 0 ? VoxelState.White : VoxelState.Empty);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Retorna o segundo canto possível a partir de uma origem e um tamanho
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    Vector3Int DiagonalCornerFromSize(Vector3Int origin, Vector3Int size)
    {
        var xDirections = new List<int> { 1, -1 }.Shuffle();
        var zDirections = new List<int> { 1, -1 }.Shuffle();

        Vector3Int secondCorner = origin;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                secondCorner = new Vector3Int(origin.x + xDirections[i] * size.x, origin.y + size.y, origin.z + zDirections[j] * size.z);
                if (secondCorner.IsInsideGrid(Size)) return secondCorner;
            }
        }
        return secondCorner;
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                for (int z = 0; z < Size.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    #endregion


}