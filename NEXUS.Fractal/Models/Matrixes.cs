using System;
using System.Collections.Generic;

namespace NEXUS.Fractal.Models;

public static class Matrixes
{
    public static IEnumerable<double[,]> GetMatrixes(MatrixType type)
    {
        switch (type)
        {
            case MatrixType.Laplacian3x3:
                return [Laplacian3x3];
            case MatrixType.Laplacian5x5:
                return [Laplacian5x5];
            case MatrixType.Uniform3x3:
                return [Uniform3x3];
            case MatrixType.Uniform5x5:
                return [Uniform5x5];
            case MatrixType.Sharpen3x3:
                return [Sharpen3x3];
            case MatrixType.HighPass5x5Type1:
                return [HighPass5x5Type1];
            case MatrixType.HighPass5x5Type2:
                return [HighPass5x5Type2];
            case MatrixType.Gaussian3x3_085:
                return [Gaussian3x3_085];
            case MatrixType.Gaussian3x3_0391:
                return [Gaussian3x3_0391];
            case MatrixType.Gaussian5x5_0625:
                return [Gaussian5x5_0625];
            case MatrixType.Gaussian5x5_10:
                return [Gaussian5x5_10];
            case MatrixType.Sobel3x3:
                return [Sobel3x3Horizontal, Sobel3x3Vertical];
            case MatrixType.Sobel3x3Horizontal:
                return [Sobel3x3Horizontal];
            case MatrixType.Sobel3x3Vertical:
                return [Sobel3x3Vertical];
            case MatrixType.Prewitt3x3:
                return [Prewitt3x3Horizontal, Prewitt3x3Vertical];
            case MatrixType.Prewitt3x3Horizontal:
                return [Prewitt3x3Horizontal];
            case MatrixType.Prewitt3x3Vertical:
                return [Prewitt3x3Vertical];
            case MatrixType.Kirsch3x3:
                return [Kirsch3x3Horizontal, Kirsch3x3Vertical];
            case MatrixType.Kirsch3x3Horizontal:
                return [Kirsch3x3Horizontal];
            case MatrixType.Kirsch3x3Vertical:
                return [Kirsch3x3Vertical];
            case MatrixType.Inverse:
                return [Inverse];
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    #region Laplacian Matrixes

    public static double[,] Laplacian3x3
    {
        get
        {
            return new double[,]
            {
                { -1, -1, -1, },
                { -1, 8, -1, },
                { -1, -1, -1, },
            };
        }
    }

    public static double[,] Laplacian5x5
    {
        get
        {
            return new double[,]
            {
                { -1, -3, -4, -3, -1, },
                { -3, 0, 6, 0, -3, },
                { -4, 6, 24, 6, -4, },
                { -3, 0, 6, 0, -3, },
                { -1, -3, -4, -3, -1 }
            };
        }
    }

    #endregion

    #region Uniform Matrixes

    public static double[,] Uniform3x3
    {
        get
        {
            //return new double[,]
            //{ { 1, 1, 1, },
            //  { 1, 1, 1, },
            //  { 1, 1, 1, }, };

            return new double[,]
            {
                { 0, 1, 0, },
                { 1, 1, 1, },
                { 0, 1, 0, },
            };
        }
    }

    public static double[,] Uniform5x5
    {
        get
        {
            //return new double[,]
            //{ { 1, 1, 1, 1, 1, },
            //  { 1, 1, 1, 1, 1, },
            //  { 1, 1, 1, 1, 1, },
            //  { 1, 1, 1, 1, 1, },
            //  { 1, 1, 1, 1, 1  } };

            return new double[,]
            {
                { 0, 1, 1, 1, 0, },
                { 1, 1, 1, 1, 1, },
                { 1, 1, 1, 1, 1, },
                { 1, 1, 1, 1, 1, },
                { 0, 1, 1, 1, 0 }
            };
        }
    }

    #endregion

    #region Sharpen Matrixes

    public static double[,] Sharpen3x3
    {
        get
        {
            return new double[,]
            {
                { -1, -1, -1, },
                { -1, 16, -1, },
                { -1, -1, -1, },
            };
        }
    }

    #endregion

    #region Highpass Matrixes

    public static double[,] HighPass5x5Type1
    {
        get
        {
            return new double[,]
            {
                { -1, -1, -1, -1, -1, },
                { -1, -1, -1, -1, -1, },
                { -1, -1, 24, -1, -1, },
                { -1, -1, -1, -1, -1, },
                { -1, -1, -1, -1, -1 }
            };
        }
    }

    public static double[,] HighPass5x5Type2
    {
        get
        {
            return new double[,]
            {
                { -2, -2, -2, -2, -2, },
                { -2, -3, -3, -3, -2, },
                { -2, -3, 57, -3, -2, },
                { -2, -3, -3, -3, -2, },
                { -2, -2, -2, -2, -2 }
            };
        }
    }

    #endregion

    #region Gaussian Matrixes

    public static double[,] Gaussian3x3_085
    {
        get
        {
            return new double[,]
            {
                { 1, 2, 1, },
                { 2, 4, 2, },
                { 1, 2, 1, }
            };
        }
    }

    public static double[,] Gaussian3x3_0391
    {
        get
        {
            return new double[,]
            {
                { 1, 4, 1, },
                { 4, 12, 4, },
                { 1, 4, 1, }
            };
        }
    }

    public static double[,] Gaussian5x5_0625
    {
        get
        {
            return new double[,]
            {
                { 1, 2, 3, 2, 1 },
                { 2, 7, 11, 7, 2 },
                { 3, 11, 17, 11, 3 },
                { 2, 7, 11, 7, 2 },
                { 1, 2, 3, 2, 1 },
            };
        }
    }

    public static double[,] Gaussian5x5_10
    {
        get
        {
            return new double[,]
            {
                { 2, 7, 12, 7, 2 },
                { 7, 31, 52, 31, 7 },
                { 12, 52, 127, 52, 12 },
                { 7, 31, 52, 31, 7 },
                { 2, 7, 12, 7, 2 },
            };
        }
    }

    #endregion

    #region Sobel Matrixes

    public static double[,] Sobel3x3Horizontal
    {
        get
        {
            return new double[,]
            {
                { -1, 0, 1, },
                { -2, 0, 2, },
                { -1, 0, 1, },
            };
        }
    }

    public static double[,] Sobel3x3Vertical
    {
        get
        {
            return new double[,]
            {
                { 1, 2, 1, },
                { 0, 0, 0, },
                { -1, -2, -1, },
            };
        }
    }

    #endregion

    #region Prewitt Matrixes

    public static double[,] Prewitt3x3Horizontal
    {
        get
        {
            return new double[,]
            {
                { -1, 0, 1, },
                { -1, 0, 1, },
                { -1, 0, 1, },
            };
        }
    }

    public static double[,] Prewitt3x3Vertical
    {
        get
        {
            return new double[,]
            {
                { 1, 1, 1, },
                { 0, 0, 0, },
                { -1, -1, -1, },
            };
        }
    }

    #endregion

    #region Kirsch Matrixes

    public static double[,] Kirsch3x3Horizontal
    {
        get
        {
            return new double[,]
            {
                { 5, 5, 5, },
                { -3, 0, -3, },
                { -3, -3, -3, },
            };
        }
    }

    public static double[,] Kirsch3x3Vertical
    {
        get
        {
            return new double[,]
            {
                { 5, -3, -3, },
                { 5, 0, -3, },
                { 5, -3, -3, },
            };
        }
    }

    #endregion

    #region Methods

    public static double[,] Inverse
    {
        get
        {
            return new double[,]
            {
                { 0, 0, 0, },
                { 0, -1, 0, },
                { 0, 0, 0, },
            };
        }
    }

    #endregion
}