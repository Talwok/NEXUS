using System;
using System.Collections.Generic;

namespace NEXUS.Growth.Models;

/// <summary>
/// Process
/// </summary>
public enum Process
{
    /// <summary>
    /// 
    /// </summary>
    EpitaxialGrowth,
    /// <summary>
    /// 
    /// </summary>
    SystemEvolution
}

public static class ProcessExtensions
{
    public static string ToOptionsString(this Process process)
    {
        switch (process)
        {
            case Process.EpitaxialGrowth:
                return "epitaxial";
            case Process.SystemEvolution:
                return "evolution";
            default:
                throw new ArgumentOutOfRangeException(nameof(process), process, null);
        }
    }
    
    public static Dictionary<Process, string> GetDictionary()
    {
        return new Dictionary<Process, string>
        {
            { Process.EpitaxialGrowth, "Эпитаксиальный рост" },
            { Process.SystemEvolution, "Эволюция системы" },
        };
    }
}