using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

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
                    A[i, 0] = 1;       // Константа
                    A[i, 1] = x;       // x
                    A[i, 2] = y;       // y
                    A[i, 3] = x * x;   // x²
                    A[i, 4] = y * y;   // y²
                    A[i, 5] = x * y;    // xy
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
            var height = inputData.GetLength(0);
            var width = inputData.GetLength(1);

            var aligned = new float[height, width];
            Array.Copy(inputData, aligned, inputData.Length);

            for (int y = 0; y < height; y += windowSize)
            {
                for (int x = 0; x < width; x += windowSize)
                {
                    int xEnd = Math.Min(x + windowSize, width);
                    int yEnd = Math.Min(y + windowSize, height);

                    // Вычисляем среднее в окне
                    float mean = 0;
                    int count = 0;

                    for (int iy = y; iy < yEnd; iy++)
                    {
                        for (int ix = x; ix < xEnd; ix++)
                        {
                            mean += aligned[iy, ix];
                            count++;
                        }
                    }

                    if (count > 0)
                        mean /= count;

                    // Вычитаем среднее
                    for (int iy = y; iy < yEnd; iy++)
                    {
                        for (int ix = x; ix < xEnd; ix++)
                        {
                            aligned[iy, ix] -= mean;
                        }
                    }
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
    }
}