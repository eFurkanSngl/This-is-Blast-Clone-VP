using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridManager : MonoBehaviour
{
    [SerializeField] private float _spacing = 1f;
    private GridData _gridData;
    private TilePool _pool;
    private int _gridX;
    private int _gridY;


    [Inject]
    public void StructInject(GridData gridData, TilePool tilePool)
    {
        _gridData = gridData;
        _pool = tilePool;
    }


    private void Start()
    {
        GenerateGrid();
        _gridData.DebugTileCounts();
    }

    private void GenerateGrid()
    {
        int[,] layout = _gridData.GetGridLayOut();
        _gridY = layout.GetLength(0); 
        _gridX = layout.GetLength(1);

        for(int i = 0; i < _gridY; i++)
        {
            for(int j = 0; j < _gridX; j++)
            {
                int id = layout[i, j];
                Vector3 pos = new Vector3(
                   transform.position.x + j * _spacing,
                   transform.position.y + (_gridY - 1 - i) * _spacing,
                   0f
               );
                GameObject newTile = _pool.GetTile(id);
                if(newTile == null)
                {
                    Debug.Log("TilePool did not return Tile for ID");
                    continue;
                }
                newTile.transform.position = pos;
                newTile.transform.SetParent(transform);
            }
        }
    }
}
