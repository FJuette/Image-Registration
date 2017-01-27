using System;
using System.Drawing;
using System.Text;
using OpenCV.Net;

namespace WpfApp
{
    public class Transformation
    {

        public void TestOpenCv()
        {
            Point2f[] srcPoints = new Point2f[3];
            Point2f p1 = new Point2f(144, 95);
            Point2f p2 = new Point2f(146, 454);
            Point2f p3 = new Point2f(679, 95);

            Point2f[] dstPoints = new Point2f[3];
            Point2f pd1 = new Point2f(0, 95*0.33f);
            Point2f pd2 = new Point2f(146*0.85f, 454*0.25f);
            Point2f pd3 = new Point2f(679*0.15f, 95*0.7f);

            Mat src, warpDst, warpRotateDst;
            src = CV.LoadImageM(@"G:\Dropbox\FH\Mastersemester 3\Wissenschaftliches Projekt\Einfaches_Rechteck.bmp", LoadImageFlags.Color);

            Mat rotMat = new Mat(2, 3, Depth.F32, src.ElementType);
            Mat warpMat = new Mat(2, 3, Depth.F32, src.ElementType);
            Mat mapMat = new Mat(2, 3, Depth.F32, src.ElementType);



            warpDst = Mat.Zeros(src.Rows, src.Cols, Depth.F32, src.ElementType);

            warpMat = CV.GetAffineTransform(srcPoints, dstPoints, warpDst);

            CV.WarpAffine(src, warpDst, warpMat, WarpFlags.NearestNeighbor, Scalar.Rgb(100, 100, 100));
            OpenCV.Net.Point2f center = new OpenCV.Net.Point2f(warpDst.Cols / 2, warpDst.Rows / 2);
            double angle = -50;
            double scale = 0.6;

            rotMat = CV.GetRotationMatrix2D(center, angle, scale, mapMat);

            //CV.WarpAffine(warpDst, warpRotateDst, rotMat, WarpFlags.NearestNeighbor, Scalar.Rgb(100, 100, 100));

            var t = "";
        }

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
            int numRows = m.GetUpperBound(0) + 1;
            int numCols = m.GetUpperBound(1) + 1;
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                    sb.Append($"{m[row, col],8:F0}");
                if (row < numRows - 1) sb.AppendLine();
            }
            return sb.ToString();
        }

        public MyVector MultiplyMatrixWithVector(MyMatrix matrix, MyVector vector)
        {
            var m = matrix.M;
            int numRows = m.GetUpperBound(0) + 1;
            int[] result = new int[numRows];
            for (int row = 0; row < numRows; row++)
            {
                var tmp = 0;
                for (int col = 0; col < numRows; col++)
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
            int numRows = m.GetUpperBound(0) + 1;
            int[,] augmented = new int[numRows, 2 * numRows];
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numRows; col++)
                    augmented[row, col] = m[row, col];
                augmented[row, row + numRows] = 1;
            }

            // num_cols is the number of the augmented matrix.
            int numCols = 2 * numRows;

            // Solve.
            for (int row = 0; row < numRows; row++)
            {
                // Zero out all entries in column r after this row.
                // See if this row has a non-zero entry in column r.
                if (Math.Abs(augmented[row, row]) < tiny)
                {
                    // Too close to zero. Try to swap with a later row.
                    for (int r2 = row + 1; r2 < numRows; r2++)
                    {
                        if (Math.Abs(augmented[r2, row]) > tiny)
                        {
                            // This row will work. Swap them.
                            for (int c = 0; c < numCols; c++)
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
                    for (int col = 0; col < numCols; col++)
                        if (col != row)
                            augmented[row, col] /= augmented[row, row];
                    augmented[row, row] = 1;

                    // Subtract this row from the other rows.
                    for (int row2 = 0; row2 < numRows; row2++)
                    {
                        if (row2 != row)
                        {
                            double factor = augmented[row2, row] / augmented[row, row];
                            for (int col = 0; col < numCols; col++)
                                augmented[row2, col] -= (int)(factor * augmented[row, col]);
                        }
                    }
                }
            }

            // See if we have a solution.
            if (augmented[numRows - 1, numRows - 1] == 0) return null;

            // Extract the inverse array.
            MyMatrix invMatrix = new MyMatrix();
            int[,] inverse = new int[numRows, numRows];
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numRows; col++)
                {
                    inverse[row, col] = augmented[row, col + numRows];
                }
            }

            invMatrix.M = inverse;

            return invMatrix;
        }

        public Color[][] GetBitMapColorMatrix(Bitmap b1)
        {
            int hight = b1.Height;
            int width = b1.Width;

            Color[][] colorMatrix = new System.Drawing.Color[width][];
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
