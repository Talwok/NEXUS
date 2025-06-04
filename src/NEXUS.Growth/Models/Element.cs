using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NEXUS.Growth.Models;

public class Element
{
    /// <summary>
    /// Title of element
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Element name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Element atomic mass 
    /// </summary>
    public string Mass { get; set; }
    /// <summary>
    /// Element atomic diameter
    /// </summary>
    public string Diameter { get; set; }
    /// <summary>
    /// Element lattice type
    /// </summary>
    public string LatticeType { get; set; }
    /// <summary>
    /// Element lattice parameter
    /// </summary>
    public string LatticeParameter { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Z1_r { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Z1_delta { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string A { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string E { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string p { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string q { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string r0 { get; set; }
    /// <summary>
    /// Cut radius
    /// </summary>
    public string r_cut { get; set; }

    public string FileName { get; set; }
}

public static class ElementsHelper
{
    private static string ElementsFolderName = "Assets\\Elements";

    private static string GlobalSection = "Global";
    private static string TightBindingSection = "TightBinding";
    
    /// <summary>
    /// Gets elements
    /// </summary>
    /// <returns>Get all elements from Elements folder</returns>
    public static IEnumerable<Element> GetElements()
    {
        var elementsDir = Path.Combine(Environment.CurrentDirectory, ElementsFolderName);
        
        if (!Directory.Exists(elementsDir)) yield break;
        
        var fileNames = Directory.GetFiles(elementsDir);
        
        foreach (var fileName in fileNames)
        {
            var currentFileDict = ParseElementFile(Path.Combine(elementsDir, fileName));
            yield return new Element
            {
                FileName = Path.GetFileNameWithoutExtension(fileName),
                Title = currentFileDict[GlobalSection][nameof(Element.Title)],
                Name = currentFileDict[GlobalSection][nameof(Element.Name)],
                Mass = currentFileDict[GlobalSection][nameof(Element.Mass)],
                Diameter = currentFileDict[GlobalSection][nameof(Element.Diameter)],
                LatticeType = currentFileDict[GlobalSection][nameof(Element.LatticeType)],
                LatticeParameter = currentFileDict[GlobalSection][nameof(Element.LatticeParameter)],
                Z1_r = currentFileDict[GlobalSection][nameof(Element.Z1_r)],
                Z1_delta = currentFileDict[GlobalSection][nameof(Element.Z1_delta)],
                A = currentFileDict[TightBindingSection][nameof(Element.A)],
                E = currentFileDict[TightBindingSection][nameof(Element.E)],
                p = currentFileDict[TightBindingSection][nameof(Element.p)],
                q = currentFileDict[TightBindingSection][nameof(Element.q)],
                r0 = currentFileDict[TightBindingSection][nameof(Element.r0)],
                r_cut = currentFileDict[TightBindingSection][nameof(Element.r_cut)]
            };
        }
    }
    /// <summary>
    /// Parsing .conf file to a dictionary
    /// </summary>
    /// <param name="filePath">Path to .conf file</param>
    /// <returns>Dictionary with all parsed elements</returns>
    private static Dictionary<string, Dictionary<string, string>> ParseElementFile(string filePath)
    {
        var config = new Dictionary<string, Dictionary<string, string>>();

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim().Split('#').FirstOrDefault();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            if (trimmedLine.Contains('='))
            {
                var keyValue = trimmedLine.Split('=', 2);
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                if (key.Contains('.'))
                {
                    var parts = key.Split('.', 2);
                    string section = parts[0].Trim();
                    string subKey = parts[1].Trim();

                    if (!config.ContainsKey(section))
                    {
                        config[section] = new Dictionary<string, string>();
                    }

                    config[section][subKey] = value;
                }
            }
        }
        return config;
    }
}