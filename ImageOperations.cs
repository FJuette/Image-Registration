using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace WpfApp
{
    public class ImageOperations
    {
        public Image CurImage { get; set; }
        private readonly Stack<Matrix> _transformStack;
        private PointF _offset = PointF.Empty;

        public ImageOperations(Image image)
        {
            CurImage = image;
            _transformStack = new Stack<Matrix>();
        }

        public void SetTranslate(float dx, float dy)
        {
            _transformStack.Push(
                new Matrix(1, 0, 0, 1, dx, dy)
                );
        }

        public void SetScale(float scaleX, float scaleY)
        {
            // as an example we scale at the top-left corner
            Matrix m = new Matrix(scaleX, 0, 0, scaleY, 
                _offset.X - scaleX * _offset.X, 
                _offset.Y - scaleY * _offset.Y);
            _transformStack.Push(m);
        }

        public void SetRotate(float angleDegrees)
        {
            Matrix m = new Matrix();
            // as an example we rotate around the centre of the CurImage
            PointF[] pts = {
                new PointF(0, 0),
                new PointF(CurImage.Width, 0),
                new PointF(CurImage.Width, CurImage.Height),
                new PointF(0, CurImage.Height)
            };
            GetTransform().TransformPoints(pts);
            var centre = GetCentroid(pts.ToList());
            m.RotateAt(angleDegrees, new PointF(centre.X, centre.Y));

            _transformStack.Push(m);
        }

        private Matrix GetTransform()
        {
            Matrix m = new Matrix();
            foreach (var item in _transformStack.Reverse())
                m.Multiply(item, MatrixOrder.Append);
            return m;
        }

        public Image ApplyTransform(bool onlyTranslation = false)
        {
            Matrix matrix = GetTransform();
            Image returnImage = null;
            if (!onlyTranslation) // we do not need to redraw the CurImage if transformation is pure translation
            {
                // transform the 4 vertices to know the output size
                PointF[] pts =
                {
                    new PointF(0, 0),
                    new PointF(CurImage.Width, 0),
                    new PointF(0, CurImage.Height),
                    new PointF(CurImage.Width, CurImage.Height)
                };
                matrix.TransformPoints(pts);
                float minX = pts.Min(p => p.X);
                float maxX = pts.Max(p => p.X);
                float minY = pts.Min(p => p.Y);
                float maxY = pts.Max(p => p.Y);
                Bitmap bmpDest = new Bitmap(Convert.ToInt32(maxX - minX), Convert.ToInt32(maxY - minY));
                //bmpDest.SetResolution(CurImage.HorizontalResolution, CurImage.VerticalResolution);

                // remove the offset from the points defining the destination for the CurImage (we need only 3 vertices)
                PointF[] destPts =
                {
                    new PointF(pts[0].X - minX, pts[0].Y - minY),
                    new PointF(pts[1].X - minX, pts[1].Y - minY),
                    new PointF(pts[2].X - minX, pts[2].Y - minY)
                };
                using (Graphics gDest = Graphics.FromImage(bmpDest))
                    gDest.DrawImage(CurImage, destPts);
                returnImage = bmpDest;
            }
            // keep the offset
            _offset = new PointF(matrix.OffsetX, matrix.OffsetY);
            return returnImage;
        }

        /// <summary>
        /// Method to compute the centroid of a polygon. This does NOT work for a complex polygon.
        /// </summary>
        /// <param name="poly">points that define the polygon</param>
        /// <returns>centroid point, or PointF.Empty if something wrong</returns>
        public static PointF GetCentroid(List<PointF> poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                float temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
                accumulatedArea += temp;
                centerX += (poly[i].X + poly[j].X) * temp;
                centerY += (poly[i].Y + poly[j].Y) * temp;
            }

            if (accumulatedArea < 1E-7f)
                return PointF.Empty;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new PointF(centerX / accumulatedArea, centerY / accumulatedArea);
        }
    }
}
