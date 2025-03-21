namespace NEXUS.MDT.Misc;

public class ValidationNotEqualError : Exception
{
    public ValidationNotEqualError(byte[] expected, byte[] actual, KaitaiStream io, string path)
        : base($"Validation failed: expected {BitConverter.ToString(expected)}, but got {BitConverter.ToString(actual)} at {path}")
    {
    }
}