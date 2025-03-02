using System.IO;

namespace NEXUS.Growth.Models;

public static class ViewerHelper
{
    public static string ViewerFileExtension = "*.sss";
    
    public static bool IsFileValid(string? filePath)
    {
        return !string.IsNullOrEmpty(filePath) 
               && File.Exists(filePath) 
               && string.Equals(
                   Path.GetExtension(filePath), 
                   Path.GetExtension(ViewerFileExtension));
    }
}