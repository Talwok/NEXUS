using System.Collections.Generic;
using System.Linq;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Extensions;

public static class FrameExtensions
{
    public static IEnumerable<FrameViewModel> BuildTree(this IEnumerable<FrameModel> frames)
    {
        var frameModels = frames.ToArray();
        var lookup = frameModels.ToDictionary(f => f.Id, f => new FrameViewModel(f));
        var rootNodes = new List<FrameViewModel>();
    
        foreach (var frame in frameModels)
        {
            if (frame.ParentId == null)
            {
                rootNodes.Add(lookup[frame.Id]);
            }
            else if (lookup.TryGetValue(frame.ParentId.Value, out var parentNode))
            {
                parentNode.Children.Add(lookup[frame.Id]);
            }
        }
    
        return rootNodes;
    }
    
    public static IEnumerable<FrameModel> FlattenTree(this IEnumerable<FrameViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node.GetModel();
        
            foreach (var child in FlattenTree(node.Children))
            {
                yield return child;
            }
        }
    }
}