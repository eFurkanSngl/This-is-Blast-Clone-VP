using DG.Tweening;
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
    private BulletManager _bulletManager;


    [Inject]
    public void StructInject(GridData gridData, TilePool tilePool, LauncherManager launcherManager, BulletManager bulletManager)
    {
        _gridData = gridData;
        _pool = tilePool;
        _launcherManager = launcherManager;
        _bulletManager = bulletManager;
    }
    private List<Transform> GetTilesBetween(Vector3 startPos, Vector3 targetPos, int row = 0)
    {
        var list = new List<Transform>();
        if (_tiles == null) return list;

        // ayný satýrýn Y’ini bul (fallback: start Y)
        float rowY = startPos.y;
        for (int x = 0; x < _gridX; x++)
        {
            var t = _tiles[row, x];
            if (t != null) { rowY = t.transform.position.y; break; }
        }

        // X aralýðý (hedefi hariç tutmak için epsilon pay)
        float eps = 0.001f;
        float minX = Mathf.Min(startPos.x, targetPos.x) + eps;
        float maxX = Mathf.Max(startPos.x, targetPos.x) - eps;

        for (int x = 0; x < _gridX; x++)
        {
            var tile = _tiles[row, x];
            if (tile == null) continue;

            Vector3 p = tile.transform.position;

            // ayný satýrda ve iki noktanýn arasýnda mý?
            if (Mathf.Abs(p.y - rowY) < 0.01f && p.x > minX && p.x < maxX)
            {
                list.Add(tile.transform);
            }
        }

        return list;
    }
    private void RotateGoalItem(GoalItem goalItem,Vector3 targetPos)
    {
        Vector3 direction = targetPos - goalItem.transform.position;
        direction.y = 0; 

        if (direction == Vector3.zero)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        goalItem.transform.DOKill();

        goalItem.transform
            .DORotateQuaternion(lookRotation, 0.3f)
            .SetEase(Ease.OutBack); 
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
                    found = true;

                    Vector3 startPos = goalItem.transform.position;
                    Vector3 endPos = tile.transform.position;

                    RotateGoalItem(goalItem, endPos);

                    var between = GetTilesBetween(startPos, endPos, row: 0);

                    bool done = false;
                    int tileId = tile.GetId();

                    _bulletManager.FireBullet(startPos, endPos, between, () =>
                    {
                        _tiles[0, x] = null;
                        _pool.ReturnTile(tileId, tile.gameObject);
                        done = true;
                    });

                    while (!done) yield return null;

                    goalItem.DecreaseCount(1);


                    break; 
                }
            }

            if (!found)
            {
                yield return null;
            }
         
        }

        if (goalItem != null) Debug.Log("is destroy goal Item");
    }


    private void Start()
    {
        GenerateGrid();
        _gridData.DebugTileCounts();
    }

    private void GenerateGrid()
    {
        int[,] layout = _gridData.GetGridLayOut();

        int[,] stacks = _gridData.GetStackCounts();

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
                    int layerHealth = Math.Clamp(stacks[i, j], 1, 2);
                    tile.Initialize((Tile.TileColor)id,layerHealth);
                        _tiles[i, j] = tile;
                    }
                }
            }
    }
}
