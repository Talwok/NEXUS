using System;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NEXUS.Fractal.Models;

public class Filter
{
        public static class ConvolutionImageFilters1
        {
            public static Image<Rgba32> ConvolutionFilter(Image<Rgba32> sourceImage, double[,] xFilterMatrix, double[,] yFilterMatrix, double factor = 1, int bias = 0)
            {
                // Применяем свертку для X и Y матриц
                var tempImage = ApplyConvolution(sourceImage, xFilterMatrix, factor, bias);
                var resultImage = ApplyConvolution(tempImage, yFilterMatrix, factor, bias);

                return resultImage;
            }

            public static Image<Rgba32> ConvolutionFilter(Image<Rgba32> sourceImage, double[,] filterMatrix, double factor = 1, int bias = 0)
            {
                return ApplyConvolution(sourceImage, filterMatrix, factor, bias);
            }

            private static Image<Rgba32> ApplyConvolution(Image<Rgba32> sourceImage, double[,] filterMatrix, double factor, int bias)
            {
                int filterWidth = filterMatrix.GetLength(1);
                int filterHeight = filterMatrix.GetLength(0);
                int filterOffset = (filterWidth - 1) / 2;

                var resultImage = sourceImage.Clone();

                sourceImage.ProcessPixelRows(accessor =>
                {
                    for (int y = filterOffset; y < accessor.Height - filterOffset; y++)
                    {
                        var row = accessor.GetRowSpan(y);

                        for (int x = filterOffset; x < row.Length - filterOffset; x++)
                        {
                            double blue = 0.0, green = 0.0, red = 0.0;

                            for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                            {
                                for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                                {
                                    var pixel = accessor.GetRowSpan(y + filterY)[x + filterX];
                                    double filterValue = filterMatrix[filterY + filterOffset, filterX + filterOffset];

                                    blue += pixel.B * filterValue;
                                    green += pixel.G * filterValue;
                                    red += pixel.R * filterValue;
                                }
                            }

                            // Применяем фактор и смещение
                            blue = factor * blue + bias;
                            green = factor * green + bias;
                            red = factor * red + bias;

                            // Ограничиваем значения в диапазоне [0, 255]
                            blue = Math.Clamp(blue, 0, 255);
                            green = Math.Clamp(green, 0, 255);
                            red = Math.Clamp(red, 0, 255);

                            // Записываем результат
                            resultImage[x, y] = new Rgba32((byte)red, (byte)green, (byte)blue);
                        }
                    }
                });

                return resultImage;
            }
        }
        
    public static class Matrixes
    {
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

    /*public static class ImageFilters
    {
        public static Bitmap InvertColorMatrix(this Bitmap sourceBitmap)
        {
            Bitmap bmpDest = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            ColorMatrix clrMatrix = new ColorMatrix(new float[][]
            {
                new float[] { -1, 0, 0, 0, 0 },
                new float[] { 0, -1, 0, 0, 0 },
                new float[] { 0, 0, -1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 1, 1, 1, 0, 1 }
            });

            using (ImageAttributes attrImage = new ImageAttributes())
            {
                attrImage.SetColorMatrix(clrMatrix);

                using (Graphics g = Graphics.FromImage(bmpDest))
                {
                    g.DrawImage(sourceBitmap, new Rectangle(0, 0,
                            sourceBitmap.Width, sourceBitmap.Height), 0, 0,
                        sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel,
                        attrImage);
                }
            }

            return bmpDest;
        }

        public static Bitmap MakeGrayscale(this Bitmap sourceBitmap)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];

            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            pixelBuffer.CopyTo(resultBuffer, 0);

            sourceBitmap.UnlockBits(sourceData);

            float rgb = 0;
            for (int k = 0; k < pixelBuffer.Length; k += 4)
            {
                rgb = pixelBuffer[k] * 0.11f;
                rgb += pixelBuffer[k + 1] * 0.59f;
                rgb += pixelBuffer[k + 2] * 0.3f;

                resultBuffer[k] = (byte)rgb;
                resultBuffer[k + 1] = (byte)rgb;
                resultBuffer[k + 2] = (byte)rgb;
                resultBuffer[k + 3] = 255;
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        #region Softness Filters

        public static Bitmap Gaussian3x3Filter_085(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Gaussian3x3_085, 1.0 / 16.0, 0);
        }

        public static Bitmap Gaussian3x3Filter_0391(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Gaussian3x3_0391, 1.0 / 32.0, 0);
        }

        public static Bitmap Gaussian5x5Filter_0625(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Gaussian5x5_0625, 1.0 / 114.0, 0);
        }

        public static Bitmap Gaussian5x5Filter_10(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Gaussian5x5_10, 1.0 / 324.0, 0);
        }

        public static Bitmap Uniform3x3Filter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Uniform3x3, 1.0 / 8.0, 0);
        }

        public static Bitmap Uniform5x5Filter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Uniform5x5, 1.0 / 8.0, 0);
        }

        #endregion

        #region Sharpness Filters

        public static Bitmap Sharpen3x3Filter(this Bitmap sourceBitmap)
        {
            return sourceBitmap.ConvolutionFilter(Matrixes.Sharpen3x3, 0.5, 0);
        }

        #endregion

        #region Brightness Filters

        public static Bitmap Laplacian3x3Filter(this Bitmap sourceBitmap)
        {
            return sourceBitmap.ConvolutionFilter(Matrixes.Laplacian3x3, 3, 255);
        }

        public static Bitmap Laplacian5x5Filter(this Bitmap sourceBitmap)
        {
            return sourceBitmap.ConvolutionFilter(Matrixes.Laplacian5x5, 0.1, 0);
        }

        public static Bitmap Highpass5x5Filter1(this Bitmap sourceBitmap)
        {
            return sourceBitmap.ConvolutionFilter(Matrixes.HighPass5x5Type1, 0.1, 0);
        }

        public static Bitmap Highpass5x5Filter2(this Bitmap sourceBitmap)
        {
            return sourceBitmap.ConvolutionFilter(Matrixes.HighPass5x5Type2, 0.05, 0);
        }

        #endregion

        #region Complex Filters

        public static Bitmap Sobel3x3Filter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Sobel3x3Horizontal,
                Matrixes.Sobel3x3Vertical, 3, 255);
        }

        public static Bitmap Sobel3x3HorizontalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Sobel3x3Horizontal, 3, 255);
        }

        public static Bitmap Sobel3x3VerticalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Sobel3x3Vertical, 3, 255);
        }

        public static Bitmap Prewitt3x3Filter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Prewitt3x3Horizontal,
                Matrixes.Prewitt3x3Vertical, 3, 255);
        }

        public static Bitmap Prewitt3x3HorizontalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Prewitt3x3Horizontal, 3, 255);
        }

        public static Bitmap Prewitt3x3VerticalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Prewitt3x3Vertical, 3, 255);
        }

        public static Bitmap Kirsch3x3Filter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Kirsch3x3Horizontal,
                Matrixes.Kirsch3x3Vertical, 3, 255);
        }

        public static Bitmap Kirsch3x3HorizontalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Kirsch3x3Horizontal, 3, 255);
        }

        public static Bitmap Kirsch3x3VerticalFilter(this Bitmap sourceBitmap)
        {
            return ConvolutionImageFilters.ConvolutionFilter(sourceBitmap, Matrixes.Kirsch3x3Vertical, 3, 255);
        }

        #endregion
    }*/
}