using System;
using Avalonia.Media;

namespace Puzzles
{
    public static class ProgramData
    {


        public const double TOP_POINT = 50;
        public const double LEFT_POINT = 40;
        public const double SEGMENT_SIZE = 20;
        public static readonly (IBrush Fill, IBrush Stroke)[] ColorPairs = new (IBrush, IBrush)[40]
        {
            // Красные оттенки (5 пар)
            (Brushes.LightCoral, new SolidColorBrush(Color.Parse("#CD5C5C"))),
            (Brushes.Salmon, new SolidColorBrush(Color.Parse("#E9967A"))),
            (Brushes.IndianRed, new SolidColorBrush(Color.Parse("#B22222"))),
            (Brushes.Crimson, new SolidColorBrush(Color.Parse("#8B0000"))),
            (Brushes.Firebrick, new SolidColorBrush(Color.Parse("#B22222"))),
        
            // Розовые/пурпурные (5 пар)
            (Brushes.Pink, new SolidColorBrush(Color.Parse("#DB7093"))),
            (Brushes.LightPink, new SolidColorBrush(Color.Parse("#FF69B4"))),
            (Brushes.HotPink, new SolidColorBrush(Color.Parse("#C71585"))),
            (Brushes.DeepPink, new SolidColorBrush(Color.Parse("#8B0A50"))),
            (Brushes.MediumVioletRed, new SolidColorBrush(Color.Parse("#8B008B"))),
        
            // Оранжевые (5 пар)
            (Brushes.Coral, new SolidColorBrush(Color.Parse("#FF6347"))),
            (Brushes.Tomato, new SolidColorBrush(Color.Parse("#CD4F39"))),
            (Brushes.OrangeRed, new SolidColorBrush(Color.Parse("#EE4000"))),
            (Brushes.DarkOrange, new SolidColorBrush(Color.Parse("#FF8C00"))),
            (Brushes.Orange, new SolidColorBrush(Color.Parse("#CD8500"))),
        
            //// Желтые (5 пар)
            //(Brushes.Gold, new SolidColorBrush(Color.Parse("#CDAD00"))),
            //(Brushes.Yellow, new SolidColorBrush(Color.Parse("#CDCD00"))),
            //(Brushes.LightYellow, new SolidColorBrush(Color.Parse("#EEE8AA"))),
            //(Brushes.LemonChiffon, new SolidColorBrush(Color.Parse("#CDC9A5"))),
            //(Brushes.PaleGoldenrod, new SolidColorBrush(Color.Parse("#EEE8AA"))),

            // Желтые (5 пар)
            (Brushes.Gold, Brushes.Goldenrod),
            (Brushes.Yellow, Brushes.Goldenrod),
            (Brushes.LightYellow, Brushes.Goldenrod),
            (Brushes.LemonChiffon, Brushes.Goldenrod),
            (Brushes.PaleGoldenrod, Brushes.Goldenrod),
        
            // Зеленые (5 пар)
            (Brushes.LightGreen, new SolidColorBrush(Color.Parse("#698B69"))),
            (Brushes.LimeGreen, new SolidColorBrush(Color.Parse("#32CD32"))),
            (Brushes.ForestGreen, new SolidColorBrush(Color.Parse("#228B22"))),
            (Brushes.SeaGreen, new SolidColorBrush(Color.Parse("#2E8B57"))),
            (Brushes.MediumSeaGreen, new SolidColorBrush(Color.Parse("#3CB371"))),
        
            //// Голубые/бирюзовые (5 пар)
            //(Brushes.Aqua, new SolidColorBrush(Color.Parse("#00CED1"))),
            //(Brushes.Turquoise, new SolidColorBrush(Color.Parse("#40E0D0"))),
            //(Brushes.MediumTurquoise, new SolidColorBrush(Color.Parse("#48D1CC"))),
            //(Brushes.DarkTurquoise, new SolidColorBrush(Color.Parse("#00CED1"))),
            //(Brushes.CadetBlue, new SolidColorBrush(Color.Parse("#5F9EA0"))),

            // Голубые/бирюзовые (5 пар)
            (Brushes.Aqua, Brushes.Blue),
            (Brushes.Turquoise, Brushes.Blue),
            (Brushes.MediumTurquoise, Brushes.Blue),
            (Brushes.DarkTurquoise, Brushes.Blue),
            (Brushes.CadetBlue, Brushes.Blue),
        
            // Синие (5 пар)
            (Brushes.CornflowerBlue, new SolidColorBrush(Color.Parse("#6495ED"))),
            (Brushes.RoyalBlue, new SolidColorBrush(Color.Parse("#4169E1"))),
            (Brushes.DodgerBlue, new SolidColorBrush(Color.Parse("#1E90FF"))),
            (Brushes.DeepSkyBlue, new SolidColorBrush(Color.Parse("#00BFFF"))),
            (Brushes.LightSkyBlue, new SolidColorBrush(Color.Parse("#87CEFA"))),
        
            // Фиолетовые (5 пар)
            (Brushes.Plum, new SolidColorBrush(Color.Parse("#DDA0DD"))),
            (Brushes.Violet, new SolidColorBrush(Color.Parse("#EE82EE"))),
            (Brushes.Orchid, new SolidColorBrush(Color.Parse("#DA70D6"))),
            (Brushes.MediumOrchid, new SolidColorBrush(Color.Parse("#BA55D3"))),
            (Brushes.DarkOrchid, new SolidColorBrush(Color.Parse("#9932CC")))
        };

        // Метод для получения случайной пары цветов
        public static (IBrush Fill, IBrush Stroke) GetRandomColorPair()
        {
            var random = new Random();
            return ColorPairs[random.Next(ColorPairs.Length)];
        }

    }
}
