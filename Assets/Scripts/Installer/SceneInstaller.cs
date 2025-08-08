using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InstallSignalBus();
        Container.Bind<TilePool>().FromComponentInHierarchy().AsSingle();
        Container.Bind<LauncherManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IMoveAnim>().To<MoveAnimation>().AsSingle();
        Container.DeclareSignal<MergeSignal>();
        Container.DeclareSignal<ClickSignalBus>();
        Container.DeclareSignal<SwipeSignalBus>();
    }


    private void InstallSignalBus()
    {
        SignalBusInstaller.Install(Container);
    }
}
