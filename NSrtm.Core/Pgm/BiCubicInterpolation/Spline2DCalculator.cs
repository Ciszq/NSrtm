using System;
using System.Collections.Generic;
using System.Linq;

namespace NSrtm.Core.Pgm.BiCubicInterpolation
{
    public class Spline2DCalculator
    {
        /// <summary>
        ///     Finding derivatives from 2-d function values - https://en.wikipedia.org/wiki/Bicubic_interpolation
        ///     using centered differencing formula (5.4)
        ///     http://www2.math.umd.edu/~dlevy/classes/amsc466/lecture-notes/differentiation-chap.pdf
        /// </summary>
        /// <param name="values"></param>
        /// <param name="step"></param>
        /// <returns>Array with derivatives</returns>
        private static List<double> firstDerivativesCalculator(
            IReadOnlyList<double> values,
            double step)
        {
            var length = values.Count;
            var derivative = new List<double>();
            for (int i = 1; i < length - 1; i++)
            {
                derivative[i - 1] = centeredDifferencingFormula(values[i - 1], values[i + 1], step);
            }
            return derivative;
        }

        private static double centeredDifferencingFormula(double previous, double next, double step)
        {
            return (previous - next) / 2 * step;
        }

        #region Static Members

        private static readonly int[,] linearEquationCoefficients =

        {
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-3, 3, 0, 0, -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {2, -2, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, -3, 3, 0, 0, -2, -1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 2, -2, 0, 0, 1, 1, 0, 0},
            {-3, 0, 3, 0, 0, 0, 0, 0, -2, 0, -1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, -3, 0, 3, 0, 0, 0, 0, 0, -2, 0, -1, 0},
            {9, -9, -9, 9, 6, 3, -6, -3, 6, -6, 3, -3, 4, 2, 2, 1},
            {-6, 6, 6, -6, -3, -3, 3, 3, -4, 4, -2, 2, -2, -2, -1, -1},
            {2, 0, -2, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 2, 0, -2, 0, 0, 0, 0, 0, 1, 0, 1, 0},
            {-6, 6, 6, -6, -4, -2, 4, 2, -3, 3, -3, 3, -2, -1, -2, -1},
            {4, -4, -4, 4, 2, 2, -2, -2, 2, -2, 2, -2, 1, 1, 1, 1}
        };

        /// <summary>
        ///     This subroutine builds bicubic func spline.
        /// </summary>
        /// <param name="values">function values, array[0..3, 0..3]</param>
        /// <param name="step">Distance between the nodes</param>
        /// <returns>Func with spline interpolant</returns>
        public static Func<double, double, double> GetBiCubicSpline(IReadOnlyList<IReadOnlyList<double>> values, double step)
        {
            if (values.Count != 4 || values.Any(value => value.Count != 4))
            {
                throw new ArgumentException("values", "Bicubic interpolation considers 16 pixels(4×4)");
            }

            var firstDerivativeX = values.Select(row => firstDerivativesCalculator(row, step))
                                         .ToList()
                                         .AsReadOnly();

            var firstDerivativeY = values.Select((t, i) => values.Select(element => element[i]))
                                         .Select(column => firstDerivativesCalculator(column.ToList(), step))
                                         .ToList()
                                         .AsReadOnly();

            var crossDerivative = firstDerivativeY.Select(row => firstDerivativesCalculator(row, step))
                                                  .ToList();

            var x = new List<double>
                    {
                        values[1][1],
                        values[1][2],
                        values[2][1],
                        values[2][2],
                        firstDerivativeX[1][0],
                        firstDerivativeX[1][1],
                        firstDerivativeX[2][0],
                        firstDerivativeX[2][1],
                        firstDerivativeY[1][0],
                        firstDerivativeY[1][1],
                        firstDerivativeY[2][0],
                        firstDerivativeY[2][1],
                        crossDerivative[0][0],
                        crossDerivative[0][1],
                        crossDerivative[1][0],
                        crossDerivative[1][1]
                    };

            var coefficients = new List<double>();

            for (int i = 0; i < linearEquationCoefficients.GetLength(0); i++)
            {
                double coefficient = x.Select((t, j) => linearEquationCoefficients[i, j] * t)
                                      .Sum();
                coefficients.Add(coefficient);
            }

            return ((xPos, yPos) =>
                    {
                        double result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 4; i++)
                            {
                                result += coefficients[i * 4 + j] * Math.Pow(xPos, i) * Math.Pow(yPos, j);
                            }
                        }
                        return result;
                    });
        }

        #endregion
    }
}
