using System;
using System.Collections.Generic;
using NEXUS.Growth.Models;

namespace NEXUS.Growth.Helpers;

public static class ProcessHelper
{
    public static Process GetProcess(string processName)
    {
        switch (processName)
        {
            case "epitaxial":
                return Process.EpitaxialGrowth;
            case "evolution":
                return Process.SystemEvolution;
            default:
                throw new ArgumentOutOfRangeException(nameof(processName), processName, null);
        }
    }

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