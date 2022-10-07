using ArtStreamHelper.Core.ViewModels.Config;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtStreamHelper.WinUI.Views.Config;

public class ConfigTemplateSelector : DataTemplateSelector
{
    public DataTemplate CheckBox { get; set; }
    public DataTemplate Button { get; set; }
    public DataTemplate TextBox { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            ButtonConfigViewModel => Button,
            CheckBoxConfigViewModel => CheckBox,
            TextBoxConfigViewModel => TextBox,
            _ => CheckBox
        };
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}