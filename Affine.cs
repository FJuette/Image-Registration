using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    // vgl. https://elonen.iki.fi/code/misc-notes/affine-fit/
    public class Affine
    {
        // 4 Points needed
        public List<List<int>> SrcPoints { get; set; }
        // 4 Points needed
        public List<List<int>> DstPoints { get; set; }

        public float[][] C { get; set; }
        public float[][] Q { get; set; }
        public float[][] M { get; set; }

        public Affine(List<List<int>> srcPoints, List<List<int>> dstPoints)
        {
            SrcPoints = srcPoints;
            DstPoints = dstPoints;

            var q = SrcPoints;
            var p = DstPoints;
            var dim = 2;

            C = new float[3][];
            C[0] = new float[2];
            C[1] = new float[2];
            C[2] = new float[2];

            for (int j = 0; j < dim; j++)
            {
                for (int k = 0; k < dim + 1; k++)
                {
                    for (int i = 0; i < q.Count; i++)
                    {
                        var qt = q[i];
                        qt.Add(1);
                        C[k][j] += qt[k] * p[i][j];
                    }
                }
            }

            Q = new float[3][];
            Q[0] = new float[3];
            Q[1] = new float[3];
            Q[2] = new float[3];

            foreach (var qi in q)
            {
                var qt = qi;
                qt.Add(1);
                for (int i = 0; i < dim + 1; i++)
                {
                    for (int j = 0; j < dim + 1; j++)
                    {
                        Q[i][j] += qt[i] * qt[j];
                    }
                }
            }

            M = new float[3][];
            M[0] = new float[5];
            M[1] = new float[5];
            M[2] = new float[5];

            for (int i = 0; i < dim + 1; i++)
            {
                for (int j = 0; j < Q[i].Length; j++)
                {
                    M[i][j] = Q[i][j];
                }
                for (int j = 3; j < C[i].Length + 3; j++)
                {
                    M[i][j] = C[i][j - 3];
                }
            }

            GaussJordan();
        }

        private bool GaussJordan()
        {
            var eps = 1.0 / Math.Pow(10.0, 10.0);
            var h = M.Length;
            var w = M[0].Length;

            // Find max pivot
            for (int y = 0; y < h; y++)
            {
                var maxrow = y;
                for (int y2 = y + 1; y2 < h; y2++)
                {
                    if (Math.Abs(M[y2][y]) > Math.Abs(M[maxrow][y]))
                    {
                        maxrow = y2;
                    }
                }
                var yTemp = M[y];
                M[y] = M[maxrow];
                M[maxrow] = yTemp;

                // Singular?
                if (Math.Abs(M[y][y]) <= eps)
                {
                    return false;
                }

                // Eliminate the column y
                for (int y2 = y + 1; y2 < h; y2++)
                {
                    var c = M[y2][y] / M[y][y];
                    for (int x = y; x < w; x++)
                    {
                        M[y2][x] -= M[y][x] * c;
                    }
                }
            }

            // Backsubstitute
            for (int y = h-1; y < -1; y--)
            {
                var d = M[y][y];
                for (int y2 = 0; y2 < y; y2++)
                {
                    for (int x = w - 1; x < y - 1; x--)
                    {
                        M[y2][x] -= M[y][x] * M[y2][y] / d;
                    }
                }
                M[y][y] /= d;

                // Normalize row y
                for (int x = h; x < w; x++)
                {
                    M[y][x] /= d;
                }
            }

            return true;
        }

        public float[] TransformPoint(int[] point)
        {
            float[] res = new float[2];
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    res[j] += point[i] * M[i][j + 2 + 1];
                }
                res[j] += M[2][j + 2 + 1];
            }
            return res;
        }
    }
}
