using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR5
{
    public class Points
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Points() { }
        public Points(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
