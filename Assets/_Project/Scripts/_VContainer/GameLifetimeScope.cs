using _Project.Scripts._GlobalLogic;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Factories;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.Services;
using _Project.Scripts.SO;
using _Project.Scripts.UI.Windows;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts._VContainer
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] protected WindowsManager _windowsManager;
        [SerializeField] protected PoolsManager _poolsManager;
        
        [Header("Configs")]
        [SerializeField] protected PlayableBlocksConfig _playableBlocksConfig;
        [SerializeField] protected WindowsConfig _windowsConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(InjectManager.Initialize);
            
            RegisterGameManager(builder);
            RegisterAppData(builder);
            RegisterWindows(builder);
            RegisterRegistries(builder);
            RegisterPools(builder);
            RegisterFactories(builder);
            RegisterSO(builder);
            RegisterServices(builder);
        }

        public virtual void RegisterGameManager(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton).AsSelf().As<IAsyncStartable>();
        }
        
        private void RegisterAppData(IContainerBuilder builder)
        {
            builder.Register<AppData>(Lifetime.Singleton).AsSelf().As<IInitializable>();
        }
        
        private void RegisterWindows(IContainerBuilder builder)
        {
            builder.RegisterInstance(_windowsManager).AsSelf().As<IInitializable>();
        }
        
        private void RegisterRegistries(IContainerBuilder builder)
        {
            builder.Register<ObjectsRegistry>(Lifetime.Singleton).AsSelf();
        }
        
        private void RegisterPools(IContainerBuilder builder)
        {
            builder.Register<DraggablePool>(Lifetime.Singleton).AsSelf();
        }
        
        private void RegisterFactories(IContainerBuilder builder)
        {
            builder.Register<DraggableFactory>(Lifetime.Singleton).AsSelf();
        }
        
        private void RegisterSO(IContainerBuilder builder)
        {
            builder.RegisterInstance(_playableBlocksConfig).AsSelf();
            builder.RegisterInstance(_windowsConfig).AsSelf().As<IInitializable>();
        }

        private void RegisterServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(_poolsManager).AsSelf();
            builder.Register<GameTimer>(Lifetime.Singleton).As<GameTimer, ITickable>();
            builder.Register<ResetLevelService>(Lifetime.Singleton).AsSelf();
            builder.Register<SaveLoadLevelService>(Lifetime.Singleton).AsSelf();
        }
    }
}