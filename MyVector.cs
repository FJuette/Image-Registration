using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class MyVector
    {
        public int[] Vector { get; set; }

        public MyVector(int x, int y)
        {
            Vector = new int[3];
            Vector[0] = x;
            Vector[1] = y;
            Vector[2] = 1;
        }

        public MyVector()
        {
            Vector = new int[3];
        }

        public int X => Vector[0];
        public int Y => Vector[1];
        public int Z => Vector[2];

        public int[] GetVector()
        {
            return Vector;
        }
    }
}
