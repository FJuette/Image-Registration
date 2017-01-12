using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using NUnit.Framework;

namespace WpfApp
{
    [TestFixture]
    public class TransformationTests
    {
        [Test]
        public void Translation()
        {
            Transformation t = new Transformation();
            var srcPoint = new Point(146, 93);
            var dstPoint = new Point(208, 156);

            var matrix = t.GetTranslationsMatrix(srcPoint, dstPoint);
            matrix.Invert();
            var result = Point.Multiply(dstPoint, matrix);

            Assert.AreEqual(srcPoint.X, result.X, 1);
            Assert.AreEqual(srcPoint.Y, result.Y, 1);
        }
    }
}
