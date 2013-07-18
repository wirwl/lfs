using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Media;

namespace LFStudio
{
    class WPFBrushList : List<WPFBrush>
    {
        public WPFBrushList()
        {
            Type BrushesType = typeof(Brushes);
            PropertyInfo[] brushesProperty = BrushesType.GetProperties();       
            foreach (PropertyInfo property in brushesProperty)
            {
                BrushConverter brushConverter = new BrushConverter();
                Brush brush = (Brush)brushConverter.ConvertFromString(property.Name);
                Add(new WPFBrush(property.Name, brush.ToString()));
            }
        }
    }
    class WPFBrush
    {
        public WPFBrush(string name, string hex) 
        {
            Name = name;
            Hex = hex;
        }

        public string Name { get; set; }
        public string Hex { get; set; }
    }
}
