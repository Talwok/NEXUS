namespace NEXUS.Fractal.Models;

public enum FrameSourceType
{
    /// <summary>
    /// Formats: jpeg, jpg, png, bmp.
    /// </summary>
    Image,
    /// <summary>
    /// Formats: bcr.
    /// </summary>
    DigitalSurf,
    /// <summary>
    /// Formats: mdt.
    /// </summary>
    NtMdtMda, NtMdtScanned, NtMdtSpectroscopy
}