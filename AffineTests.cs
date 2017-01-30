using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WpfApp
{
    [TestFixture]
    public class AffineTests
    {
        [Test]
        public void Test()
        {
            var srcPoints = new List<List<int>>
            {
                new List<int> {1,1},
                new List<int> {1,2},
                new List<int> {2,2},
                new List<int> {2,1},
            };
            var dstPoints = new List<List<int>>
            {
                new List<int> {4,4},
                new List<int> {6,6},
                new List<int> {8,4},
                new List<int> {6,2},
            };
            Affine a = new Affine(srcPoints, dstPoints);
            var res = a.TransformPoint(new []{1, 1});
        }
    }
}
