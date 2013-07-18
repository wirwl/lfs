using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LFStudio.Utils
{
    class MathUtils
    {
        public static Point DoublePointToIntPoint(Point p)
        {
            return new Point((int)p.X,(int)p.Y);
        }
        public static Vector DoubleVectorToIntVector(Vector v)
        {
            return new Vector((int)v.X, (int)v.Y);
        }

    }
}
