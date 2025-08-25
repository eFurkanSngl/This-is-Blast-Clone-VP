using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InstallSignalBus();
        Container.Bind<TilePool>().FromComponentInHierarchy().AsSingle();
        Container.Bind<LauncherManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IMoveAnim>().To<MoveAnimation>().AsSingle();
        Container.Bind<IClickAnim>().To<ClickAnimation>().AsSingle();
        Container.Bind<GoalItem>().To<GoalItem>().AsSingle();
        Container.Bind<ISwapAnim>().To<SwapAnimation>().AsSingle();
        Container.Bind<IMergeAnim>().To<MergeAnim>().AsSingle();
        Container.Bind<GridManager>().FromComponentInHierarchy().AsSingle();
        Container.DeclareSignal<MergeSignal>();
        Container.DeclareSignal<ClickSignalBus>();
        Container.DeclareSignal<SwipeSignalBus>();
        Container.Bind<BulletPool>().FromComponentInHierarchy().AsSingle();
        Container.Bind<BulletManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IGoalItemExitScreen>().To<GoalItemExitScreen>().AsSingle();
        Container.DeclareSignal<DestorySignal>();
        Container.Bind<SplashPool>().FromComponentInHierarchy().AsSingle();
    }


    private void InstallSignalBus()
    {
        SignalBusInstaller.Install(Container);
    }
}
