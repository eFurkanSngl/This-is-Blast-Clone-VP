using DG.Tweening;
using System;
using System.Collections;
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
    private IGoalItemExitScreen _exitScreen;
    private SignalBus _signalBus;


    [Inject]
    public void StructInject(GridData gridData, TilePool tilePool, LauncherManager launcherManager, BulletManager bulletManager
        ,IGoalItemExitScreen exitScreen, SignalBus signalBus)
    {
        _gridData = gridData;
        _pool = tilePool;
        _launcherManager = launcherManager;
        _bulletManager = bulletManager;
        _exitScreen = exitScreen;
        _signalBus = signalBus;
    }

    private IEnumerator RainDownRoutine()
    {
        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY - 1; y++)
            {
                if (_tiles[y, x] == null)
                {
                    // yukarıdan boşluk doldurma
                    for (int k = y + 1; k < _gridY; k++)
                    {
                        if (_tiles[k, x] != null)
                        {
                            Tile fallingTile = _tiles[k, x];

                            _tiles[y, x] = fallingTile;
                            _tiles[k, x] = null;

                            fallingTile.SetGridPos(y, x);

                            Vector3 localTarget = new Vector3(x * _spacing, y * _spacing, 0f);
                            Vector3 worldTarget = transform.TransformPoint(localTarget);

                            fallingTile.transform
                                .DOMove(worldTarget, 0.2f)
                                .SetEase(Ease.OutQuad);

                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
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
            .DORotateQuaternion(lookRotation, 0.2f)
            .SetEase(Ease.OutBack); 
    }

    public void GoalItemMatchRoutine(GoalItem goalItem)
    {
        StartCoroutine(GoalItemMatch(goalItem));
    }
    private void ResetGoalItem(GoalItem goalItem)
    {
        goalItem.transform.DOKill();
        goalItem.transform
            .DORotateQuaternion(Quaternion.identity, 0.2f)
            .SetEase(Ease.OutBack);
    }
    public IEnumerator GoalItemMatch(GoalItem goalItem)
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

                    _bulletManager.FireBullet(goalItem, endPos, () =>
                    {
                        if (shouldHitTop)
                        {
                            // Top layer'a vur
                            tile.HitLayer(true, () =>
                            {
                                goalItem.DecreaseCount(1);
                                if (tile.IsCompletelyDestroyed())
                                {
                                    _pool.ReturnTile(tileId, tile.gameObject);
                                }
                            });

                        }
                        else
                        {
                            tile.HitLayer(false, () =>
                            {
                                goalItem.DecreaseCount(1);

                                if (tile.IsCompletelyDestroyed())
                                {
                                    _pool.ReturnTile(tileId, tile.gameObject);
                                    tile.gameObject.SetActive(false);
                                    HapticManager.PlayHaptic(HapticManager.HapticType.Medium);
                                    _signalBus.Fire<DestorySignal>();
                                }
                                _tiles[0, x] = null;
                                StartCoroutine(RainDownRoutine());

                            });
                        }
                    });

                    yield return new WaitForSeconds(0.2f);
                    break;

                }
            }

            if (!found)
            {
                ResetGoalItem(goalItem);
                yield return null;

            }
            if (goalItem != null && goalItem.CurrentCount == 0)
            {
                ResetGoalItem(goalItem);
                _exitScreen.PlayExit(goalItem, () =>
                {
                    Debug.Log("is work");
                });
            }
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
        int[,] stacks = _gridData.GetStackCounts();

        _gridY = layout.GetLength(0);
        _gridX = layout.GetLength(1);

        _tiles = new Tile[_gridY, _gridX];

        for (int i = 0; i < _gridY; i++)
        {
            for (int j = 0; j < _gridX; j++)
            {
                int id = layout[i, j];

                Vector3 localPos = new Vector3(j * _spacing, i * _spacing, 0f);

                Vector3 worldPos = transform.TransformPoint(localPos);

                GameObject newTile = _pool.GetTile(id);
                if (newTile == null)
                {
                    Debug.Log("not enough tiles in pool");
                    continue;
                }

                newTile.transform.position = worldPos;
                newTile.transform.SetParent(transform, true); 

                Tile tile = newTile.GetComponent<Tile>();
                if (tile != null)
                {
                    int layerHealth = Math.Clamp(stacks[i, j], 1, 2);
                    tile.SetGridPos(i, j);
                    tile.Initialize((Tile.TileColor)id, layerHealth);

                    _tiles[i, j] = tile;
                }
            }
        }
    }

}
