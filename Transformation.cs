using System.Windows;
using System.Windows.Media;

namespace WpfApp
{
    public class Transformation
    {
        //3x3 Affine transformation matrix
        public Matrix Matrix { get; set; }
        public Vector Vector { get; set; }

        //https://msdn.microsoft.com/en-us/library/x09w60w4(v=vs.110).aspx
        public Matrix GetTranslationsMatrix(Point sourcePoint, Point dstPoint)
        {
            var dx = dstPoint.X - sourcePoint.X;
            var dy = dstPoint.Y - sourcePoint.Y;
            var matrix = new Matrix();
            matrix.OffsetX = dx;
            matrix.OffsetY = dy;
            //matrix.Invert();
            return matrix;
        }
        
    }
}
