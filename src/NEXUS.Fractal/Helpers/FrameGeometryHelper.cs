using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;

namespace NEXUS.Fractal.Helpers
{
    public static class FrameGeometryHelper
    {
        /// <summary>
        /// Удаление линейного тренда (1-го порядка) методом наименьших квадратов
        /// </summary>
        public static float[,] RemoveLinearTrend(this float[,] inputData)
        {
            var height = inputData.GetLength(0);
            var width = inputData.GetLength(1);

            int totalPixels = height * width;

            // 1. Преобразуем входные данные в вектор-столбец
            var z = Vector<float>.Build.Dense(totalPixels);
            for (int y = 0, i = 0; y < height; y++)
            for (int x = 0; x < width; x++, i++)
                z[i] = inputData[y, x];

            // 2. Строим матрицу плана A (размер totalPixels × 3)
            var A = Matrix<float>.Build.Dense(totalPixels, 3);
            for (int y = 0, i = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, i++)
                {
                    A[i, 0] = 1; // Константа
                    A[i, 1] = x; // Коэффициент для x
                    A[i, 2] = y; // Коэффициент для y
                }
            }

            // 3. Решаем систему методом наименьших квадратов
            var coefficients = A.TransposeThisAndMultiply(A).Inverse() * A.TransposeThisAndMultiply(z);

            // 4. Вычисляем тренд и вычитаем его
            var result = new float[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float trend = coefficients[0] + coefficients[1] * x + coefficients[2] * y;
                    result[y, x] = inputData[y, x] - trend;
                }
            }

            return result;
        }

        /// <summary>
        /// Удаление квадратичного тренда (2-го порядка)
        /// </summary>
        public static float[,] RemoveQuadraticTrend(this float[,] inputData)
        {
            var height = inputData.GetLength(0);
            var width = inputData.GetLength(1);

            int totalPixels = height * width;

            // 1. Преобразуем входные данные в вектор-столбец
            var z = Vector<float>.Build.Dense(totalPixels);
            for (int y = 0, i = 0; y < height; y++)
            for (int x = 0; x < width; x++, i++)
                z[i] = inputData[y, x];

            // 2. Строим матрицу плана A (размер totalPixels × 6 для квадратичного тренда)
            var A = Matrix<float>.Build.Dense(totalPixels, 6);
            for (int y = 0, i = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, i++)
                {
                    A[i, 0] = 1; // Константа
                    A[i, 1] = x; // x
                    A[i, 2] = y; // y
                    A[i, 3] = x * x; // x²
                    A[i, 4] = y * y; // y²
                    A[i, 5] = x * y; // xy
                }
            }

            // 3. Решаем систему методом наименьших квадратов
            var coefficients = A.TransposeThisAndMultiply(A).Inverse() * A.TransposeThisAndMultiply(z);

            // 4. Вычисляем тренд и вычитаем его
            var result = new float[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float trend = coefficients[0] +
                                  coefficients[1] * x +
                                  coefficients[2] * y +
                                  coefficients[3] * x * x +
                                  coefficients[4] * y * y +
                                  coefficients[5] * x * y;
                    result[y, x] = inputData[y, x] - trend;
                }
            }

            return result;
        }

        /// <summary>
        /// Локальное выравнивание (скользящее окно + вычитание среднего)
        /// </summary>
        public static float[,] LocalAlignment(this float[,] inputData, int windowSize = 32)
        {
            int height = inputData.GetLength(0);
            int width = inputData.GetLength(1);

            // Создаем интегральное изображение с дополнительными границами
            float[,] integral = new float[height + 1, width + 1];

            // Построение интегрального изображения
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    integral[y + 1, x + 1] = inputData[y, x]
                                             + integral[y, x + 1]
                                             + integral[y + 1, x]
                                             - integral[y, x];
                }
            }

            float[,] aligned = new float[height, width];
            int halfWindow = windowSize / 2;

            // Обработка каждого пикселя с локальным окном
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Определяем границы окна с учетом краев изображения
                    int x1 = Math.Max(0, x - halfWindow);
                    int y1 = Math.Max(0, y - halfWindow);
                    int x2 = Math.Min(width - 1, x + halfWindow);
                    int y2 = Math.Min(height - 1, y + halfWindow);

                    int pixelCount = (x2 - x1 + 1) * (y2 - y1 + 1);

                    // Вычисляем сумму в окне через интегральное изображение
                    float windowSum = integral[y2 + 1, x2 + 1]
                                      - integral[y1, x2 + 1]
                                      - integral[y2 + 1, x1]
                                      + integral[y1, x1];

                    float localMean = windowSum / pixelCount;
                    aligned[y, x] = inputData[y, x] - localMean;
                }
            }

            return aligned;
        }

        /// <summary>
        /// Построение матрицы плана (design matrix) для полиномиальной регрессии
        /// </summary>
        private static Matrix<float> BuildDesignMatrix(int height, int width, int degree)
        {
            int pointsCount = height * width;
            int termsCount = (degree + 1) * (degree + 2) / 2; // Число коэффициентов для полинома степени `degree`

            var A = new DenseMatrix(pointsCount, termsCount);

            for (int y = 0, idx = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, idx++)
                {
                    if (degree >= 0) A[idx, 0] = 1; // Константа (1)
                    if (degree >= 1)
                    {
                        A[idx, 1] = x; // x
                        A[idx, 2] = y; // y
                    }

                    if (degree >= 2)
                    {
                        A[idx, 3] = x * x; // x²
                        A[idx, 4] = y * y; // y²
                        A[idx, 5] = x * y; // xy
                    }
                    // Можно добавить кубические члены, если нужно
                }
            }

            return A;
        }

        /// <summary>
        /// Метод наименьших квадратов (Ax ≈ b)
        /// </summary>
        private static Vector<float> LeastSquares(Matrix<float> a, Matrix<float> b)
        {
            // Решаем систему AᵀA x = Aᵀ b
            var AtA = a.TransposeThisAndMultiply(a);
            var Atb = a.TransposeThisAndMultiply(b);
            return AtA.Solve(Atb).Column(0);
        }

        public static float[,] RemoveHorizontalStripes(this float[,] input)
        {
            int w = input.GetLength(0), h = input.GetLength(1);
            var result = new float[w, h];

            for (int j = 0; j < h; j++)
            {
                var A = DenseMatrix.Create(w, 2, 0.0f);
                var b = DenseVector.Create(w, 0.0f);
                for (int i = 0; i < w; i++)
                {
                    A[i, 0] = i;
                    A[i, 1] = 1.0f;
                    b[i] = input[i, j];
                }

                Vector<float> p = (A.TransposeThisAndMultiply(A)).Cholesky().Solve(A.TransposeThisAndMultiply(b));
                double a = p[0], c = p[1];
                for (int i = 0; i < w; i++)
                {
                    double trend = a * i + c;
                    result[i, j] = (float)(input[i, j] - trend);
                }
            }

            return result;
        }

        public static float[,] RemoveVerticalStripes(this float[,] input)
        {
            int w = input.GetLength(0), h = input.GetLength(1);
            var result = new float[w, h];

            for (int i = 0; i < w; i++)
            {
                var A = DenseMatrix.Create(h, 2, 0.0f);
                var b = DenseVector.Create(h, 0.0f);
                for (int j = 0; j < h; j++)
                {
                    A[j, 0] = j;
                    A[j, 1] = 1.0f;
                    b[j] = input[i, j];
                }

                Vector<float> p = (A.TransposeThisAndMultiply(A)).Cholesky().Solve(A.TransposeThisAndMultiply(b));
                double a = p[0], c = p[1];
                for (int j = 0; j < h; j++)
                {
                    double trend = a * j + c;
                    result[i, j] = (float)(input[i, j] - trend);
                }
            }

            return result;
        }
    }
}