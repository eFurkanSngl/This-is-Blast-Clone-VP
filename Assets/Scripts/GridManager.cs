using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;
    [SerializeField] private float spacing = 0.95f;

    private void Start()
    {
        GenerateGrid();
    }
    private void GenerateGrid()
    {
        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY; y++)
            {
                int randomIndex = Random.Range(0, _prefabs.Length);
                Vector3 pos = new Vector3(x * spacing, y * spacing, 0);

                GameObject newObj = Instantiate(_prefabs[randomIndex], transform);
                newObj.transform.localPosition = pos;
            }
        }
    }

}
