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
            // Assert
            Transformation t = new Transformation();
            var sourceVector = new MyVector(146, 93);
            var destVector = new MyVector(208, 156);


            // Act
            var matrix = t.GetTranslationsMatrix(sourceVector, destVector);
            var inverse = t.InvertMatrix(matrix);
            var resultVector = t.MultiplyMatrixWithVector(inverse, destVector);

            Assert.AreEqual(sourceVector.X, resultVector.X);
            Assert.AreEqual(sourceVector.Y, resultVector.Y);
        }
    }
}
