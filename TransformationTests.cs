using System;
using System.Windows.Media.Imaging;
using NUnit.Framework;
using Point = System.Windows.Point;

namespace WpfApp
{
    [TestFixture]
    public class TransformationTests
    {
        [Test]
        public void Translation()
        {
            // Assert
            Transformation t = new Transformation();

            Point s1 = new Point(208, 156);
            Point s2 = new Point(749, 155);
            Point s3 = new Point(211, 518);

            Point d1 = new Point(146, 93);
            Point d2 = new Point(685, 94);
            Point d3 = new Point(146, 456);


            //// Act
            var res = t.Affine(new BitmapImage(new Uri(@"G:\Dropbox\FH\Mastersemester 3\Wissenschaftliches Projekt\Einfaches_Rechteck2.bmp")),
                s1, s2, s3, d1, d2, d3);
        }
    }
}
