using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class MyMatrix
    {
        public int[,] M { get; set; }

        /// <summary>
        /// Matrix needed: 
        /// 1 0 dx
        /// 0 1 dy
        /// 0 0 1
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public MyMatrix(int dx, int dy)
        {
            M = new int[3, 3];
            M[0,0] = 1;
            M[0,1] = 0;
            M[0,2] = dx; // dx

            M[1,0] = 0;
            M[1,1] = 1;
            M[1,2] = dy; // dy

            M[2,0] = 0;
            M[2,1] = 0;
            M[2,2] = 1;
        }

        public MyMatrix()
        {
            M = new int[3, 3];
        }
    }
}
