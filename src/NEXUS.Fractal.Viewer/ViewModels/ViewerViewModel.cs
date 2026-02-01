using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Core.Services.Project;
using NEXUS.Fractal.Core.ViewModels.Project;
using NEXUS.Fractal.Project.Data;
using Prism.Commands;
using Prism.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace NEXUS.Fractal.Viewer.ViewModels
{
    public partial class ViewerViewModel : ObservableBaseObject
    {
        [ObservableProperty]
        private ProjectEntityViewModel? _selectedProjectEntity;
        
        public ViewerViewModel(
            IEventAggregator eventAggregator, 
            ProjectService projectService,
            ColorTableService colorTableService)
        {
            ProjectService = projectService;
            ColorTableService = colorTableService;
            
            eventAggregator.GetEvent<PubSubEvent<SelectProjectEntityEventPayload>>()
                .Subscribe(OnSelectProjectEntity)
                .DisposeWith(Disposable);
            
            eventAggregator.GetEvent<PubSubEvent<RemoveProjectEntityEventPayload>>()
                .Subscribe(OnRemoveProjectEntity)
                .DisposeWith(Disposable);
            
            SelectionChangedCommand = new DelegateCommand(SelectionChanged);
        }

        private void OnRemoveProjectEntity(RemoveProjectEntityEventPayload payload)
        {
            var projectEntityViewModels = ProjectService.SelectedEntities.Where(x =>  payload.Ids.Contains(x.Id)).ToList();
            foreach (var projectEntityViewModel in projectEntityViewModels)
            {
                ProjectService.SelectedEntities.Remove(projectEntityViewModel);
            }
        }

        private void SelectionChanged()
        {
            if (SelectedProjectEntity is { Image: not null })
            {
                //SelectedProjectEntity.Image?.Dispose();
                SelectedProjectEntity.Image = null;
            }

            if (SelectedProjectEntity != null)
            {
                var heightmapData = ProjectService.GetHeightMapEntityData(SelectedProjectEntity.Id);

                SelectedProjectEntity.Image = GetImage(heightmapData);
            }
        }

        public ICommand SelectionChangedCommand { get; }

        public ProjectService ProjectService { get; }
        public ColorTableService ColorTableService { get; }
        
        private void OnSelectProjectEntity(SelectProjectEntityEventPayload payload)
        {
            if (SelectedProjectEntity is { Image: not null })
            {
                //SelectedProjectEntity.Image?.Dispose();
                SelectedProjectEntity.Image = null;
            }

            var heightmapData = ProjectService.GetHeightMapEntityData(payload.ProjectEntity.Id);
            
            SelectedProjectEntity = payload.ProjectEntity;

            SelectedProjectEntity.Image = GetImage(heightmapData);
        }
        
        private BitmapImage? GetImage(ProjectHeightmapEntityData? heightmapData)
        {
            if (heightmapData?.Data == null) 
                return null;
            
            var heightmap = Normalize(heightmapData.GetHeightmap());
            
            if (heightmap == null)
                return null;
            
            var (min, max) = GetMinMax(heightmap);
            
            var colorTable = ColorTableService.SelectedColorTable;
            
            var image = new Image<Rgba32>(heightmap.GetLength(1), heightmap.GetLength(0));
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        var value = heightmap[y, x];

                        if (colorTable != null)
                        {
                            int colorIndex;
                            if (value > max)
                            {
                                colorIndex = colorTable.Colors.Count - 1;
                            }
                            else if (value < min)
                            {
                                colorIndex = 0;
                            }
                            else
                            {
                                value = Normalize(value, min, max);
                                colorIndex = (int)(value * (colorTable.Colors.Count - 1));
                                colorIndex = Math.Clamp(colorIndex, 0, colorTable.Colors.Count - 1);
                            }
                            row[x] = new Rgba32(colorTable.Colors[colorIndex].Red, colorTable.Colors[colorIndex].Green,
                                colorTable.Colors[colorIndex].Blue);
                        }
                        else
                        {
                            row[x] = new Rgba32(value, value, value);
                        }
                    }
                }
            });
            
            var stream = new MemoryStream();
            image.SaveAsBmp(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            
            return bitmap;
        }

        private WriteableBitmap GetWritableBitmap(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            var wb = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null);

            var rect = new Int32Rect(0, 0, width, height);
            var data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            wb.WritePixels(rect, data.Scan0, data.Stride * height, data.Stride);
            bitmap.UnlockBits(data);
            
            return wb;
        }
        
        
        public static float Denormalize(float value, float min, float max)
            => value * (max - min) + min;

        public static float Normalize(float value, float min, float max)
            => (value - min) / (max - min);
        
        public static (float min, float max) GetMinMax(float[,] data)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            // Find min and max values in the array
            float min = data[0, 0];
            float max = data[0, 0];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (data[i, j] < min) min = data[i, j];
                    if (data[i, j] > max) max = data[i, j];
                }
            }

            return (min, max);
        }
        
        public static float[,] Normalize(float[,] data)
        {
            if (data.Length == 0)
                return data;

            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            var (min, max) = GetMinMax(data);

            // Normalize the data
            float[,] normalized = new float[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    normalized[i, j] = (data[i, j] - min) / (max - min);
                }
            }

            return normalized;
        }

    }
}