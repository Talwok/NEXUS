using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.Services
{
    public class CalculationService(
        ChartService chartService,
        ImageService imageService,
        InfoService infoService)
        : ServiceBase
    {
        [Reactive]
        public bool IsCalculating { get; set; }
        
        public void Run(Calculation calculation)
        {
            IsCalculating = true;
            
            if (imageService is { SelectedImages: {} selectedImageVMs })
            {
                if (selectedImageVMs.Count == 0)
                {
                    infoService.AppendMessage(new InfoMessageViewModel
                    {
                        Title = "Расчёт ФР",
                        Message = "Не выбраны изображения для проведения расчёта",
                        Icon = MaterialIconKind.Error,
                        Severity = InfoBarSeverity.Error
                    });
                    IsCalculating = false;
                    return;
                }

                switch (calculation)
                {
                    case Calculation.BoxCounting:
                        Dispatcher.UIThread.Invoke(chartService.Clear);
                        foreach (var imageVm in selectedImageVMs.ToArray())
                        {
                            using var image = Image.Load<Rgba32>(imageVm.Path);
                            var values = EnclosingBoxes(image, 1, 100);
                            Dispatcher.UIThread.Invoke(() => chartService.Add(imageVm, values));   
                        }
                        break;
                    case Calculation.Triangulation:
                        Dispatcher.UIThread.Invoke(chartService.Clear);
                        foreach (var imageVm in selectedImageVMs)
                        {
                            using var image = Image.Load<Rgba32>(imageVm.Path);
                            var values = Triangulation(image, 1, 100);
                            Dispatcher.UIThread.Invoke(() => chartService.Add(imageVm, values));   
                        }
                        break;
                    default:
                        infoService.AppendMessage(new InfoMessageViewModel
                        {
                            Title = "Расчёт ФР",
                            Message = "Выбранный метод расчёта не существует",
                            Icon = MaterialIconKind.Error,
                            Severity = InfoBarSeverity.Error
                        });
                        IsCalculating = false;
                        return;
                }
            }
            
            IsCalculating = false;
        }

        #region Dimension Methods

        private IDictionary<double, double> EnclosingBoxes(Image<Rgba32> image, int startSize,
            int finishSize, int step = 1)
        {
            int width = image.Width;
            int height = image.Height;

            int darkPixelsCount = 0;

            var pixelRow = image.DangerousGetPixelRowMemory(0).Span;
            foreach (var pixel in pixelRow)
            {
                if (pixel is { R: < 127, G: < 127, B: < 127 })
                {
                    darkPixelsCount++;
                }
            }

            bool colorCount = (image.Width * image.Height / 2) < darkPixelsCount;
            IDictionary<double, double> values = new Dictionary<double, double>();

            for (int b = startSize; b <= finishSize; b += step)
            {
                int hCount = height / b;
                int wCount = width / b;
                bool[] filledBoxes = new bool[(wCount + (width > wCount * b ? 1 : 0)) *
                                              (hCount + (height > hCount * b ? 1 : 0))];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = image[x, y];
                        if ((!colorCount && pixel.R < 127 && pixel.G < 127 && pixel.B < 127) ||
                            (colorCount && pixel.R > 127 && pixel.G > 127 && pixel.B > 127))
                        {
                            filledBoxes[(y / b) * wCount + (x / b)] = true;
                        }
                    }
                }

                int a = filledBoxes.Count(f => f);
                if (a > 0)
                    values.Add(Math.Log10(1d / b), Math.Log10(a));
            }

            return values;
        }

        private IDictionary<double, double> Triangulation(Image<Rgba32> image, int startSize,
            int finishSize, int step = 2)
        {
            IDictionary<double, double> values = new Dictionary<double, double>();
            int width = image.Width;
            int height = image.Height;

            for (int i = startSize; i < finishSize; i += step)
            {
                int bitmapHeight = height - (height % i);
                int bitmapWidth = width - (width % i);
                double surfaceArea = 0;
                double diag = i * Math.Sqrt(2);

                for (int y = 0; y < bitmapHeight; y += i)
                {
                    for (int x = 0; x < bitmapWidth; x += i)
                    {
                        if (x + i < bitmapWidth && y + i < bitmapHeight)
                        {
                            double a = image[x, y].B;
                            double b = image[x + i, y].B;
                            double c = image[x, y + i].B;
                            double d = image[x + i, y + i].B;
                            double e = 0.25 * (a + b + c + d);

                            double w = Math.Sqrt(((b - a) * (b - a)) + i * i);
                            double xDist = Math.Sqrt(((c - b) * (c - b)) + i * i);
                            double yDist = Math.Sqrt(((d - c) * (d - c)) + i * i);
                            double z = Math.Sqrt(((a - d) * (a - d)) + i * i);

                            double o = Math.Sqrt(((a - e) * (a - e)) + diag * diag);
                            double p = Math.Sqrt(((b - e) * (b - e)) + diag * diag);
                            double q = Math.Sqrt(((c - e) * (c - e)) + diag * diag);
                            double r = Math.Sqrt(((d - e) * (d - e)) + diag * diag);

                            double sa = 0.5 * (w + p + o);
                            double sb = 0.5 * (xDist + p + q);
                            double sc = 0.5 * (yDist + q + r);
                            double sd = 0.5 * (z + o + r);

                            double aa = Math.Sqrt(Math.Abs(sa * (sa - w) * (sa - p) * (sa - o)));
                            double ab = Math.Sqrt(Math.Abs(sb * (sb - xDist) * (sb - p) * (sb - q)));
                            double ac = Math.Sqrt(Math.Abs(sc * (sc - yDist) * (sc - q) * (sc - r)));
                            double ad = Math.Sqrt(Math.Abs(sd * (sd - z) * (sd - o) * (sd - r)));

                            surfaceArea += aa + ab + ac + ad;
                        }
                    }
                }

                values.Add(Math.Log10(surfaceArea), Math.Log10((double)height / i * 4));
            }

            return values;
        }

        #endregion
    }
}