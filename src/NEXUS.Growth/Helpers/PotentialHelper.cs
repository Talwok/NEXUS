using System;
using System.Collections.Generic;
using NEXUS.Growth.Models;

namespace NEXUS.Growth.Helpers;

public static class PotentialHelper
{
    public static string ToOptionsString(this Potential potential)
    {
        switch (potential)
        {
            case Potential.LennardJones:
                return "Lenard-Jones";
            case Potential.StillingerWeber:
                return "Stillinger-Weber";
            case Potential.Tersoff:
                return "Tersoff";
            case Potential.Dzhugutov:
                return "Dzhugutov";
            case Potential.TightBinding:
                return "tight-binding";
            default:
                throw new ArgumentOutOfRangeException(nameof(potential), potential, null);
        }
    }

    public static Dictionary<Potential, string> GetDictionary()
    {
        return new Dictionary<Potential, string>
        {
            { Potential.LennardJones, "Леннарда-Джонса (Lennard-Jones)" },
            { Potential.StillingerWeber, "Стиллинджера-Вебера (Stillinger-Weber)" },
            { Potential.Tersoff, "Терсоффа (Tersoff)" },
            { Potential.Dzhugutov, "Джугутова (Dzhugutov)" },
            { Potential.TightBinding, "Сильной связи (Tight binding)" }
        };
    }

    public static Potential GetPotential(string potential)
    {
        switch (potential)
        {
            case "Lenard-Jones":
                return Potential.LennardJones;
            case "Stillinger-Weber":
                return Potential.StillingerWeber;
            case "Tersoff":
                return Potential.Tersoff;
            case "Dzhugutov":
                return Potential.Dzhugutov;
            case "tight-binding":
                return Potential.TightBinding;
            default:
                throw new ArgumentOutOfRangeException(nameof(potential), potential, null);
        }
    }
}