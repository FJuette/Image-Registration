using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace WpfApp
{
    public class Transformation
    {
        public MyMatrix GetTranslationsMatrix(MyVector sourceVector, MyVector destVector)
        {
            var dx = destVector.X - sourceVector.X;
            var dy = destVector.Y - sourceVector.Y;
            MyMatrix matrix = new MyMatrix(dx, dy);
            return matrix;
        }


        public string MatrixToString(MyMatrix matrix)
        {
            var m = matrix.M;
            StringBuilder sb = new StringBuilder();
            int num_rows = m.GetUpperBound(0) + 1;
            int num_cols = m.GetUpperBound(1) + 1;
            for (int row = 0; row < num_rows; row++)
            {
                for (int col = 0; col < num_cols; col++)
                    sb.Append(string.Format("{0,8:F0}", m[row, col]));
                if (row < num_rows - 1) sb.AppendLine();
            }
            return sb.ToString();
        }

        public MyVector MultiplyMatrixWithVector(MyMatrix matrix, MyVector vector)
        {
            var m = matrix.M;
            int num_rows = m.GetUpperBound(0) + 1;
            int[] result = new int[num_rows];
            for (int row = 0; row < num_rows; row++)
            {
                var tmp = 0;
                for (int col = 0; col < num_rows; col++)
                {
                    tmp += m[row, col] * vector.Vector[col];
                }
                result[row] = tmp;
            }
            MyVector v = new MyVector();
            v.Vector = result;
            return v;
        }

        // Return the matrix's inverse or null if it has none.
        public MyMatrix InvertMatrix(MyMatrix matrix)
        {
            var m = matrix.M;
            const double tiny = 0.00001;

            // Build the augmented matrix.
            int num_rows = m.GetUpperBound(0) + 1;
            int[,] augmented = new int[num_rows, 2 * num_rows];
            for (int row = 0; row < num_rows; row++)
            {
                for (int col = 0; col < num_rows; col++)
                    augmented[row, col] = m[row, col];
                augmented[row, row + num_rows] = 1;
            }

            // num_cols is the number of the augmented matrix.
            int num_cols = 2 * num_rows;

            // Solve.
            for (int row = 0; row < num_rows; row++)
            {
                // Zero out all entries in column r after this row.
                // See if this row has a non-zero entry in column r.
                if (Math.Abs(augmented[row, row]) < tiny)
                {
                    // Too close to zero. Try to swap with a later row.
                    for (int r2 = row + 1; r2 < num_rows; r2++)
                    {
                        if (Math.Abs(augmented[r2, row]) > tiny)
                        {
                            // This row will work. Swap them.
                            for (int c = 0; c < num_cols; c++)
                            {
                                int tmp = augmented[row, c];
                                augmented[row, c] = augmented[r2, c];
                                augmented[r2, c] = tmp;
                            }
                            break;
                        }
                    }
                }

                // If this row has a non-zero entry in column r, use it.
                if (Math.Abs(augmented[row, row]) > tiny)
                {
                    // Divide the row by augmented[row, row] to make this entry 1.
                    for (int col = 0; col < num_cols; col++)
                        if (col != row)
                            augmented[row, col] /= augmented[row, row];
                    augmented[row, row] = 1;

                    // Subtract this row from the other rows.
                    for (int row2 = 0; row2 < num_rows; row2++)
                    {
                        if (row2 != row)
                        {
                            double factor = augmented[row2, row] / augmented[row, row];
                            for (int col = 0; col < num_cols; col++)
                                augmented[row2, col] -= (int)(factor * augmented[row, col]);
                        }
                    }
                }
            }

            // See if we have a solution.
            if (augmented[num_rows - 1, num_rows - 1] == 0) return null;

            // Extract the inverse array.
            MyMatrix invMatrix = new MyMatrix();
            int[,] inverse = new int[num_rows, num_rows];
            for (int row = 0; row < num_rows; row++)
            {
                for (int col = 0; col < num_rows; col++)
                {
                    inverse[row, col] = augmented[row, col + num_rows];
                }
            }

            invMatrix.M = inverse;

            return invMatrix;
        }

        //public Bitmap TransformImage(Bitmap bmp, MyMatrix matrix)
        //{

        //    int pixelDepth = 4; // assume RGBA
        //    BitmapData data = bmp.LockBits(Rectangle.Empty, ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
        //    IntPtr ptr = data.Scan0;
        //    int stride = data.Stride;

        //    for (int x = 0; x < bmp.Width; x++)
        //    {
        //        for (int y = 0; y < bmp.Height; y++)
        //        {
        //            // modify your pixels here by using Marshal.Copy
        //            ptr = new IntPtr(ptr.ToInt32() + stride);
        //            Marshal.Copy(ptr, );

        //        }

        //    }

        //    return null;
        //}

        public System.Drawing.Color[][] GetBitMapColorMatrix(Bitmap b1)
        {
            int hight = b1.Height;
            int width = b1.Width;

            System.Drawing.Color[][] colorMatrix = new System.Drawing.Color[width][];
            for (int i = 0; i < width; i++)
            {
                colorMatrix[i] = new System.Drawing.Color[hight];
                for (int j = 0; j < hight; j++)
                {
                    colorMatrix[i][j] = b1.GetPixel(i, j);
                }
            }
            return colorMatrix;
        }

    }
}
