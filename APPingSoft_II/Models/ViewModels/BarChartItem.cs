using System.Windows.Media;

namespace APPingSoft_II.Models.ViewModels;

public class BarChartItem
{
    public string Label     { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public double BarHeight { get; set; }
    public Brush  Fill      { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4A6B"));
}
