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
    private LauncherManager _launcherManager;
    private int _gridX;
    private int _gridY;
    private Tile[,] _tiles;


    [Inject]
    public void StructInject(GridData gridData, TilePool tilePool, LauncherManager launcherManager)
    {
        _gridData = gridData;
        _pool = tilePool;
        _launcherManager = launcherManager;
    }

    public void GoalItemMatchRoutine(GoalItem goalItem)
    {
        StartCoroutine(GoalItemMatch(goalItem));
    }
    private IEnumerator GoalItemMatch(GoalItem goalItem)
    {
        int targetId = goalItem.GetID();

        while (goalItem != null && goalItem.CurrentCount > 0)
        {
            bool found = false;

            for (int x = 0; x < _gridX; x++)
            {
                Tile tile = _tiles[0, x];
                if (tile == null) continue;

                if (tile.GetId() == targetId)
                {
                    Debug.Log("eþleþme bulundu");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                yield return new WaitForSeconds(0.1f); 
                continue;
            }

            // Burada ateþleme kodu çalýþýr
            yield break;
        }
    }


    private void Start()
    {
        GenerateGrid();
        _gridData.DebugTileCounts();
    }

    private void GenerateGrid()
    {
        int[,] layout = _gridData.GetGridLayOut();

        //int[,] stacks = _gridData.GetStackCounts();

        _gridY = layout.GetLength(0); 
        _gridX = layout.GetLength(1);

        _tiles = new Tile[_gridY, _gridX];

            for (int i = 0; i < _gridY; i++)
            {
                for (int j = 0; j < _gridX; j++)
                {
                    int id = layout[i, j];
                    Vector3 pos = new Vector3(
                     transform.position.x + j * _spacing,
                     transform.position.y + i * _spacing,
                     0f
                    );
                    GameObject newTile = _pool.GetTile(id);
                    if (newTile == null)
                    {
                        Debug.Log("is not enough");
                        continue;
                    }
                    newTile.transform.position = pos;
                    newTile.transform.SetParent(transform, false);

                    Tile tile = newTile.GetComponent<Tile>();
                    if (tile != null)
                    {
                    //int layerHealth = Math.Clamp(stacks[i, j], 1, 2);
                        tile.Initialize((Tile.TileColor)id);
                        _tiles[i, j] = tile;
                    }
                }
            }
    }
}
