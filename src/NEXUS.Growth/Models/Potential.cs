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

