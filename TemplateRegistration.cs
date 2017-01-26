using System;
using System.Drawing;
using System.Threading.Tasks;

namespace WpfApp
{
    public class TemplateRegistration
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Maximum { get; set; }

        /// <summary>
        /// Use the hsb information of the image to map the template, working with earth images
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="mappingImage"></param>
        public void RunHsb(Bitmap templateImage, Bitmap mappingImage)
        {
            var rgbMatrixTemplate = GetImageMatrixHsb(templateImage);
            var rgbMatrixMapping = GetImageMatrixHsb(mappingImage);

            double max = 0;
            var x = 0;
            var y = 0;

            Parallel.For(0, rgbMatrixMapping.Length - rgbMatrixTemplate.Length, i =>
            {
                for (int j = 0; j < rgbMatrixMapping[i].Length - rgbMatrixTemplate[0].Length; j++)
                {
                    var cfg = GetCfgNormalizedHsb(rgbMatrixMapping, rgbMatrixTemplate, i, j);
                    if (!(cfg > max))
                        continue;
                    max = cfg;
                    x = i;
                    y = j;
                }
            }); // Parallel.For

            X = x;
            Y = y;
            Maximum = max;
        }

        /// <summary>
        /// Use the argb information of the image to map the template
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="mappingImage"></param>
        public void RunArgb(Bitmap templateImage, Bitmap mappingImage)
        {
            var rgbMatrixTemplate = GetImageMatrixArgb(templateImage);
            var rgbMatrixMapping = GetImageMatrixArgb(mappingImage);

            double max = 0;
            var x = 0;
            var y = 0;

            Parallel.For(0, rgbMatrixMapping.Length - rgbMatrixTemplate.Length, i =>
            {
                for (int j = 0; j < rgbMatrixMapping[i].Length - rgbMatrixTemplate[0].Length; j++)
                {
                    var cfg = GetCfgNormalizedArgb(rgbMatrixMapping, rgbMatrixTemplate, i, j);
                    if (!(cfg > max))
                        continue;
                    max = cfg;
                    x = i;
                    y = j;
                }
            }); // Parallel.For

            X = x;
            Y = y;
            Maximum = max;
        }

        /// <summary>
        /// Normalized CFG value by hsb
        /// </summary>
        /// <param name="rgbMapping"></param>
        /// <param name="rgbTemplate"></param>
        /// <param name="currentRow"></param>
        /// <param name="currentCol"></param>
        /// <returns></returns>
        public double GetCfgNormalizedHsb(float[][] rgbMapping, float[][] rgbTemplate, int currentRow, int currentCol)
        {
            float cfg = 0;
            double cfgNorm = 0;
            for (int i = 0; i < rgbTemplate.Length; i++)
            {
                for (int j = 0; j < rgbTemplate[i].Length; j++)
                {
                    if (currentRow < rgbMapping.Length && currentCol < rgbMapping[i].Length)
                    {
                        cfg += rgbTemplate[i][j] * rgbMapping[i + currentRow][j + currentCol];
                        cfgNorm += Math.Pow(rgbMapping[i + currentRow][j + currentCol], 2);
                    }
                }
            }
            var result = cfg / (Math.Sqrt(cfgNorm));
            return result;
        }

        /// <summary>
        /// Normalized CFG value by Argb
        /// </summary>
        /// <param name="rgbMapping"></param>
        /// <param name="rgbTemplate"></param>
        /// <param name="currentRow"></param>
        /// <param name="currentCol"></param>
        /// <returns></returns>
        public double GetCfgNormalizedArgb(long[][] rgbMapping, long[][] rgbTemplate, int currentRow, int currentCol)
        {
            long cfg = 0;
            long cfgNorm = 0;
            for (int i = 0; i < rgbTemplate.Length; i++)
            {
                for (int j = 0; j < rgbTemplate[i].Length; j++)
                {
                    if (currentRow < rgbMapping.Length && currentCol < rgbMapping[i].Length)
                    {
                        cfg += Math.Abs(rgbTemplate[i][j]) * Math.Abs(rgbMapping[i + currentRow][j + currentCol]);
                        cfgNorm += Math.Abs(rgbMapping[i + currentRow][j + currentCol]) ^ 2;
                    }
                }
            }
            var result = cfg / (Math.Sqrt(cfgNorm));
            return result;
        }

        /// <summary>
        /// Hsb Matrix from the image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public float[][] GetImageMatrixHsb(Bitmap image)
        {
            int hight = image.Height;
            int width = image.Width;

            float[][] pixelMatrix = new float[width][];
            for (int i = 0; i < width; i++)
            {
                pixelMatrix[i] = new float[hight];
                for (int j = 0; j < hight; j++)
                {
                    var h = image.GetPixel(i, j).GetHue();
                    var s = image.GetPixel(i, j).GetSaturation();
                    var b = image.GetPixel(i, j).GetBrightness();
                    pixelMatrix[i][j] = h * s * b;
                }
            }
            return pixelMatrix;
        }

        /// <summary>
        /// Argb Matrix from the image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public long[][] GetImageMatrixArgb(Bitmap image)
        {
            int hight = image.Height;
            int width = image.Width;

            long[][] pixelMatrix = new long[width][];
            for (int i = 0; i < width; i++)
            {
                pixelMatrix[i] = new long[hight];
                for (int j = 0; j < hight; j++)
                {
                    pixelMatrix[i][j] = image.GetPixel(i, j).ToArgb();
                }
            }
            return pixelMatrix;
        }

        /// <summary>
        /// Not normalized cfg value by Argb
        /// </summary>
        /// <param name="rgbMapping"></param>
        /// <param name="rgbTemplate"></param>
        /// <param name="currentRow"></param>
        /// <param name="currentCol"></param>
        /// <returns></returns>
        public long GetNonNormalizedCfg(long[][] rgbMapping, long[][] rgbTemplate, int currentRow, int currentCol)
        {
            long result = 0;
            for (int i = 0; i < rgbTemplate.Length; i++)
            {
                for (int j = 0; j < rgbTemplate[i].Length; j++)
                {
                    if (currentRow < rgbMapping.Length && currentCol < rgbMapping[i].Length)
                    {
                        result += Math.Abs(rgbTemplate[i][j]) * Math.Abs(rgbMapping[i + currentRow][j + currentCol]);
                    }
                }
            }
            return result;
        }
    }
}
