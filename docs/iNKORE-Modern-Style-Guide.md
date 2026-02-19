# iNKORE.UI.WPF.Modern 应用模式指南

本文档旨在阐明 `iNKORE.UI.WPF.Modern` 库的核心应用模式，帮助开发者正确、高效地构建现代化WPF应用程序界面。

## 核心设计哲学：非侵入式美化

`iNKORE.UI.WPF.Modern` 遵循一种“非侵入式”或“装饰者模式”的设计哲学。它不强制要求开发者继承其提供的特定基类（如 `ModernWindow`），而是通过WPF的 **附加属性（Attached Properties）** 机制，为标准的WPF原生控件“附加”上现代化的样式和行为。

这种模式的主要优点是 **高灵活性** 和 **低侵入性**，使得UI样式的应用与具体的业务逻辑或MVVM框架保持解耦。

## 窗口（Window）的现代化应用

这是最关键也是最容易产生误解的部分。要将一个标准的 `System.Windows.Window` 改造为具有Mica（云母）或Acrylic（亚克力）背景的现代化窗口，请严格遵循以下步骤。

### 1. XAML: 使用 `WindowHelper` 附加属性

在你的主窗口XAML文件（如 `MainWindow.xaml`）中，对根节点 `<Window>` 添加 `WindowHelper` 的附加属性。

**不要** 将 `<Window>` 修改为 `<ui:Window>`，因为后者在库中并不存在。

```xml
<Window x:Class="AutoLaunchController.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:AutoLaunchController"
        xmlns:ui="http://schemas.inkore.net/lib/ui/wpf/modern"
        mc:Ignorable="d"
        
        <!-- 关键：应用现代化样式的附加属性 -->
        ui:WindowHelper.UseModernWindowStyle="True"
        ui:WindowHelper.SystemBackdropType="Mica"
        
        Title="AutoLaunchController - SkyLight" Height="600" Width="960">

    <!-- 窗口内容，例如一个NavigationView -->
    <ui:NavigationView>
        <!-- ... -->
    </ui:NavigationView>

</Window>
```

-   `ui:WindowHelper.UseModernWindowStyle="True"`: 启用现代化窗口样式，包括自定义的标题栏和边框。
-   `ui:WindowHelper.SystemBackdropType="Mica"`: 设置窗口的背景材质。可选值包括 `Mica`、`Acrylic` 和 `None`。

### 2. C# 后台代码: 保持继承标准 `Window`

在对应的后台代码文件（如 `MainWindow.xaml.cs`）中，确保你的窗口类继续继承自标准的 `System.Windows.Window`。

```csharp
using System.Windows;

namespace AutoLaunchController
{
    /// <summary>
    /// 应用程序的主窗口。
    /// 注意：此类继承自标准的 System.Windows.Window。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
```

## 窗口内部控件的使用

对于窗口内部的控件，如 `Button`, `TextBox`, `NavigationView` 等，你可以直接使用由 `iNKORE.UI.WPF.Modern` 库在 `ui:` 命名空间下提供的现代化版本。

这些控件是库中实际定义的类，可以直接在XAML中实例化。

```xml
<ui:NavigationView Style="{StaticResource DefaultNavigationViewStyle}">
    <ui:NavigationView.MenuItems>
        <ui:NavigationViewItem Content="主页" Icon="Home" PageType="local:HomePage"/>
        <ui:NavigationViewItem Content="规则管理" Icon="List" PageType="local:RulesPage"/>
    </ui:NavigationView.MenuItems>

    <ui:Frame x:Name="RootFrame" Style="{StaticResource DefaultFrameStyle}"/>
</ui:NavigationView>
```

## 总结

-   **对于窗口本身**：通过 **附加属性** (`WindowHelper`) 来“装饰”一个 **标准 `Window`**。
-   **对于窗口内容**：直接使用库提供的 **现代化控件** (如 `ui:NavigationView`)。

遵循此模式可以确保你的项目能够正确编译，并能充分利用 `iNKORE.UI.WPF.Modern` 提供的所有视觉特性。