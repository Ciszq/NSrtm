using System.Collections.Generic;
using NSrtm.Core.Pgm.BiCubicInterpolation;
using Xunit;
using KellermanSoftware.CompareNetObjects;

namespace NSrtm.Core.xTests
{
    public class Spline2DCalculatorTests
    {
        [Theory]
        [MemberData("ExternalData")]
        public void BicubicSplineCalculatorReturnSimilarResultToExternalData(List<List<double>> values, double step)
        {
            var spline = Spline2DCalculator.GetBiCubicSpline(values, step);
            var result = spline(0.55, 0.7);
            AssertDeep.Equal(result, 29.5, config => config.DoublePrecision = 1e-1 );

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
