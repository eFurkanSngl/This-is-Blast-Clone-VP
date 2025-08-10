using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = " GridData",menuName = "ThisIsBlastClone / GridData")]
public class GridData: ScriptableObject
{
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;
    [SerializeField] private int[] _flatLayout;

    [SerializeField] private int[] _flatStackCount;
    public int[,] GetGridLayOut()
    {
        int[,] grid = new int[_gridY, _gridX];  // yükselik ve genişlik

        if(_flatLayout.Length != _gridX * _gridY)
        {
            Debug.Log("not same");
            return grid;
        }

        for(int i = 0; i < _gridY; i++)
        {
            for(int j = 0; j < _gridX; j++)
            {
                grid[i,j] = _flatLayout[i * _gridX + j];
            }
        }
        return grid;
    }

    public int[,] GetStackCounts()
    {
        int[,] stack = new int[_gridY,_gridX];

        if(_flatStackCount == null || _flatStackCount.Length != _gridX * _gridY)
        {
            for(int i = 0; i < _gridY; ++i)
            {
                for(int j =0; j < _gridX; ++j)
                {
                    stack[i, j] = 1;
                }
            }
            return stack;
        }

        for(int i = 0; i < _gridY; i++)
        {
            for(int j =0; j < _gridX; j++)
            {
                stack[i, j] = Mathf.Clamp(_flatStackCount[i * _gridX + j], 1, 2);
            }
        }
        return stack;
    }

    public void DebugTileCounts()
    {
        Dictionary<int, int> tileCounts = new();

        foreach (int id in _flatLayout)
        {
            if (id < 0) continue; // obstacle veya boş alan olabilir

            if (!tileCounts.ContainsKey(id))
                tileCounts[id] = 0;

            tileCounts[id]++;
        }

        Debug.Log("---- TILE COUNT DEBUG ----");

        foreach (var kvp in tileCounts)
        {
            Debug.Log($"Tile ID: {kvp.Key} → Count: {kvp.Value}");
        }

        Debug.Log("---------------------------");

    }
}