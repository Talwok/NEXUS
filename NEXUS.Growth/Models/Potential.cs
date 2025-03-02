using System;
using System.Collections.Generic;

namespace NEXUS.Growth.Models;

/// <summary>
/// Potentials
/// </summary>
public enum Potential
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Lennard-Jones_potential
    /// </summary>
    LennardJones,
    /// <summary>
    /// https://en.wikipedia.org/wiki/Interatomic_potential
    /// </summary>
    StillingerWeber,
    /// <summary>
    /// https://en.wikipedia.org/wiki/Bond_order_potential
    /// </summary>
    Tersoff,
    /// <summary>
    /// https://arxiv.org/pdf/cond-mat/0003159
    /// </summary>
    Dzhugutov,
    /// <summary>
    /// https://en.wikipedia.org/wiki/Tight_binding
    /// </summary>
    TightBinding
}

public static class PotentialExtensions
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
}