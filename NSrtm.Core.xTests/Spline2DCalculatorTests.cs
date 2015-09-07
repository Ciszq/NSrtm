using System.Collections.Generic;
using NSrtm.Core.Pgm.BiCubicInterpolation;
using Xunit;

namespace NSrtm.Core.xTests
{
    public class Spline2DCalculatorTests
    {
        [Theory]
        [MemberData("ExternalData")]
        public void BicubicSplineCalculatorReturnSimilarResultToExternalData(List<List<double>> values, double step)
        {
            var spline = Spline2DCalculator.GetBiCubicSpline(values, step);
            var result = spline(2.05, 2.2);
            Assert.Equal(result, 29.5);
        }

        #region Static Members

        public static IReadOnlyList<object[]> ExternalData
        {
            get
            {
                return new List<object[]>
                       {
                           new object[]
                           {
                               new List<List<double>>
                               {
                                   new List<double> {33, 33, 33, 33},
                                   new List<double> {31, 30, 31, 32},
                                   new List<double> {28, 29, 29, 30},
                                   new List<double> {26, 27, 28, 29},
                               },
                               1
                           }
                       };
            }
        }

        #endregion
    }
}
