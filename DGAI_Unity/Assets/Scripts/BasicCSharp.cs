using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCSharp : MonoBehaviour
{
    bool _test;

    // 03 Uma vari�vel p�blica � acess�vel no Editor
    public GameObject Original;

    // 05 Crie uma vari�vel para guardar GameObjects
    List<GameObject> _objects;
    int _count = 10;

    // Start is called before the first frame update
    void Start()
    {
        // 01 Podemos utilizar o console para visualizar mensagens
        string message = "Hello world!";
        //Debug.Log(message);

        // 02 Condicionais servem para controlar a execu��o do c�digo
        _test = true;
        if (_test)
        {
            Debug.Log(message);
        }

        // 04 Crie uma c�pia do GameObject e mova para uma nova posi��o
        //GameObject newSphere = Instantiate(Sphere);
        //Vector3 newPos = new Vector3(1, 0, 0);
        //newSphere.transform.position= newPos;

        // 06 Inicialize a lista 
        _objects = new List<GameObject>();

        // 07 Loop permitem repetir a execu��o de c�digo por determinadas vezes
        //for (int i = 0; i < _count; i++)
        //{
        //    GameObject newSphere = Instantiate(Sphere);
        //    Vector3 newPos = new Vector3(i * 1.5f, 0, 0);
        //    newSphere.transform.position = newPos;
        //}

        // 08 Com 2 loops, podemos criar um grid
        for (int x = 0; x < _count; x++)
        {
            for (int z = 0; z < _count; z++)
            {
                GameObject newSphere = Instantiate(Original);
                Vector3 newPos = new Vector3(x, 0, z);
                newSphere.transform.position = newPos;
            }
        }

        // 09 Com 3 loops, podemos criar um grid 3D
        //for (int x = 0; x < _count; x++)
        //{
        //    for (int y = 0; y < _count; y++)
        //    {
        //        for (int z = 0; z < _count; z++)
        //        {
        //            GameObject newSphere = Instantiate(Original);
        //            newSphere.transform.position = new Vector3(x, y, z);
        //            _objects.Add(newSphere);
        //        }
        //    }
        //}

        CreateGrid(_count);
    }

    // Update is called once per frame
    void Update()
    {
        // 11 foreach loops permitem itera��es sobre listas sem usar �ndices
        foreach (GameObject sphere in _objects)
        {
            // 12 Para criar um atrator, primeiro calculamos a distancia de cada objeto para o original
            float distance = Vector3.Distance(sphere.transform.position, Original.transform.position);
            
            // 14 Utilizando Clamp, podemos controlar o tamanho m�nimo e m�ximo
            float size = Mathf.Clamp(1 / distance, 0.1f, 2f);
            
            // 13 Escalas s�o definidas a partir de um Vector3
            //sphere.transform.localScale = Vector3.one * distance;
            sphere.transform.localScale = Vector3.one * size;
        }
    }

    // 10 Podemos extrair o loop para uma fun��o
    /// <summary>
    /// Cria um grid de esferas em 3D, com o tamanho indicado em size
    /// </summary>
    /// <param name="size"></param>
    void CreateGrid(int size)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    GameObject newSphere = Instantiate(Original);
                    newSphere.transform.position = new Vector3(x, y, z);
                    _objects.Add(newSphere);
                }
            }
        }
    }
}
