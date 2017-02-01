using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace WpfApp
{
    public class Transformation
    {
        public DrawingVisual Affine(ImageSource image, Point s1, Point s2, Point s3, Point d1, Point d2, Point d3)
        {
            TransformGroup transformGroup = new TransformGroup();
            
            Vector v1 = s2 - s1;
            Vector v2 = d2 - d1;
            Vector v3 = s3 - s1;
            Vector v4 = d3 - d1;

            var vc1 = Vector.CrossProduct(v3, v1) / v1.Length;
            var vc2 = Vector.CrossProduct(v4, v2) / v2.Length;

            transformGroup.Children.Add(
                new TranslateTransform(-s1.X, -s1.Y));
            transformGroup.Children.Add(
                new ScaleTransform(v2.Length / v1.Length, vc2 / vc1));
            transformGroup.Children.Add(
                new RotateTransform(Vector.AngleBetween(v1, v2)));
            transformGroup.Children.Add(
                new TranslateTransform(d1.X, d1.Y));

            DrawingVisual vis = new DrawingVisual();

            DrawingContext dc = vis.RenderOpen();
            dc.PushTransform(transformGroup);
            dc.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
            dc.Pop();
            dc.Close();
            return vis;
        }
    }
}
