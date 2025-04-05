using System;
using Silk.NET.Vulkan;

namespace ModelRendering.VulkanDemo;

public static class VulkanExtensions
{
    public static void ThrowOnError(this Result result)
    {
        if (result != Result.Success) throw new Exception($"Unexpected API error \"{result}\".");
    }
}