using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProgrammerLifeSimulator.Services;
using ProgrammerLifeSimulator.ViewModels;
using ProgrammerLifeSimulator.Views;

namespace ProgrammerLifeSimulator;

public partial class App : Application
{
    // 步骤 1: 必须添加这个静态属性来存储和访问服务容器
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. 配置 Services (S1 和 S2)
        var services = new ServiceCollection();
        services.AddSingleton<IRandomService, RandomService>();      // IService2: 不可测试
        services.AddSingleton<IGameEngineService, GameEngineService>(); // IService1: 核心逻辑

        // 2. 注册 ViewModel (依赖 Services)
        services.AddTransient<MainWindowViewModel>();

        // 3. 构建服务提供者并存储
        ServiceProvider = services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation(); 
            
            // 4. 从 ServiceProvider 中解析 MainWindowViewModel
            var mainViewModel = ServiceProvider!.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new Views.MainWindow
            {
                // 使用解析出的 ViewModel 实例
                DataContext = mainViewModel, 
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}