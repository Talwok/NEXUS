using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NEXUS.Growth.Models;
using NEXUS.Parsers.CSEG;
using NEXUS.Parsers.CSEG.Models.Elements;

namespace NEXUS.Growth.Helpers;

public static class ElementsHelper
{
    private static string ElementsFolderName = "Assets\\Elements";

    /// <summary>
    /// Gets elements
    /// </summary>
    /// <returns>Get all elements from Elements folder</returns>
    public static IEnumerable<ElementConfig> GetElements()
    {
        var elementsDir = Path.Combine(Environment.CurrentDirectory, ElementsFolderName);

        if (!Directory.Exists(elementsDir)) yield break;

        var fileNames = Directory.GetFiles(elementsDir);

        foreach (var fileName in fileNames)
        {
            yield return ElementConfigParser.Parse(Path.Combine(elementsDir, fileName));
        }
    }
}