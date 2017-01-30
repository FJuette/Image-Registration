using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace WpfApp
{
    public class Transformation
    {
        public DrawingVisual Affine(ImageSource image, Point s1, Point s2, Point s3, Point d1, Point d2, Point d3)
        {
            TransformGroup transform = new TransformGroup();
            //Point s1 = new Point(51, 28);
            //Point s2 = new Point(177, 28);
            //Point s3 = new Point(111, 7);

            //Point d1 = new Point(335, 118);
            //Point d2 = new Point(395, 60);
            //Point d3 = new Point(345, 75);

            Vector vs21 = s2 - s1;
            Vector vd21 = d2 - d1;
            Vector vs31 = s3 - s1;
            Vector vd31 = d3 - d1;

            double y1 = Vector.CrossProduct(vs31, vs21) / vs21.Length;
            double y2 = Vector.CrossProduct(vd31, vd21) / vd21.Length;

            transform.Children.Add(new TranslateTransform(-s1.X, -s1.Y));
            transform.Children.Add(new ScaleTransform(vd21.Length / vs21.Length, y2 / y1));
            transform.Children.Add(new RotateTransform(Vector.AngleBetween(vs21, vd21)));
            transform.Children.Add(new TranslateTransform(d1.X, d1.Y));

            DrawingVisual vis = new DrawingVisual();

            DrawingContext dc = vis.RenderOpen();
            dc.PushTransform(transform);
            dc.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
            dc.Pop();
            dc.Close();
            return vis;
        }
    }
}
