using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GridDataInstaller", menuName = "Installers/GridDataInstaller")]
public class GridDataInstaller: ScriptableObjectInstaller<GridDataInstaller>
{
    [SerializeField] private GridData _gridData;

    public override void InstallBindings()
    {
        Container.Bind<GridData>().FromInstance(_gridData).AsSingle();
    }
}