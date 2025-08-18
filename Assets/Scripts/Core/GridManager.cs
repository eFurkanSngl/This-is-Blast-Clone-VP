using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private IEnumerator RainDownRoutine()
    {
        for(int x = _gridY - 2; x >= 0; x--)
        {
            for(int y = 0; y < _gridX; y++)
            {
                if (_tiles[x, y] == null) continue;

                int targetX = x;

                while(targetX+ 1 < _gridY && _tiles[targetX + 1, y] == null)
                {
                    targetX++;
                }

                if(targetX != x)
                {
                    Tile fallingTile = _tiles[x, y];
                    _tiles[targetX,y] = fallingTile;
                    _tiles[x, y] = null;

                    Vector3 targetPos = GetWorldPos(targetX, y);
                    fallingTile.transform.DOMove(targetPos,0.3f).SetEase(Ease.InOutBack);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }

    private Vector3 GetWorldPos(int row, int col)
    {
        return new Vector3(
           transform.position.x + col * _spacing,
           transform.position.y + (_gridY - 1 - row) * _spacing,  
           0f
       );
    }
    private List<Transform> GetTilesBetween(Vector3 startPos, Vector3 targetPos, int row = 0)
    {
      List<Transform> transList = new List<Transform>();
        if (_tiles == null) return transList;

        float minX = Mathf.Min(startPos.x, targetPos.x);
        float maxX= Mathf.Min(startPos.y, targetPos.y);

        for(int x = 0; x < _gridX; x++)
        {
            Tile tile = _tiles[row, x]; 
            if (tile == null) continue;

            if(tile.transform.position.x > minX && tile.transform.position.x < maxX)
            {
                transList.Add(tile.transform);
            }

        }
        return transList;
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
                if (tile == null || tile.IsCompletelyDestroyed()) continue;

                if (tile.GetId() == targetId)
                {
                    found = true;
                    Vector3 startPos = goalItem.transform.position;
                    Vector3 endPos = tile.transform.position;

                    RotateGoalItem(goalItem, endPos);

                    int tileId = tile.GetId();

                    // Hangi layer'a vuracağımızı belirle
                    bool shouldHitTop = tile.ShouldHitTopLayer();

                    _bulletManager.FireBullet(startPos, endPos, () =>
                    {
                        if (shouldHitTop)
                        {
                            // Top layer'a vur
                            tile.HitLayer(true, () =>
                            {
                                goalItem.DecreaseCount(1);

                                // Top layer yok oldu, tile hala duruyor mu kontrol et
                                if (tile.IsCompletelyDestroyed())
                                {
                                    _tiles[0, x] = null;
                                    _pool.ReturnTile(tileId, tile.gameObject);
                                    StartCoroutine(RainDownRoutine());
                                }
                            });
                        }
                        else
                        {
                            // Base layer'a vur
                            tile.HitLayer(false, () =>
                            {

                                // Base layer yok oldu, tile tamamen bitti
                                if (tile.IsCompletelyDestroyed())
                                {
                                    _pool.ReturnTile(tileId, tile.gameObject);
                                    _tiles[0, x] = null;

                                    StartCoroutine(RainDownRoutine());
                                }
                                goalItem.DecreaseCount(1);

                            });
                        }
                    });

                    yield return new WaitForSeconds(0.2f);
                }
                break;  
            }
            if (!found)
            {
                Debug.Log($"GoalItem {targetId} için eşleşen tile bulunamadı!");
                yield break;
            }

            // Her vuruş arasında kısa bir bekleme
            //yield return new WaitForSeconds(0.1f); // Çok daha hızlı geçiş
        }

        if (goalItem != null)
            Debug.Log("GoalItem tamamen bitti");
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
    public Tile GetTileAtGridPos(Vector2Int gridPos)
    {
        int row = gridPos.y;
        int col = gridPos.x;

        if (row < 0 || row >= _gridY || col < 0 || col >= _gridX)
            return null;

        return _tiles[row, col];
    }


}
