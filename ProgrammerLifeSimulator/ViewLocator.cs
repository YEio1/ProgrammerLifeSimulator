using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ProgrammerLifeSimulator.ViewModels;

namespace ProgrammerLifeSimulator;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        // 获取完整类名，例如：ProgrammerLifeSimulator.ViewModels.GameViewModel
        var name = data.GetType().FullName!.Replace("ViewModels", "Views", StringComparison.Ordinal);
    
        // 如果类名是以 ViewModel 结尾的，将其替换为 View
        if (name.EndsWith("ViewModel"))
        {
            name = name.Substring(0, name.Length - "ViewModel".Length) + "View";
        }

        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        // 如果找不到，会在界面显示报错路径，方便你调试
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}