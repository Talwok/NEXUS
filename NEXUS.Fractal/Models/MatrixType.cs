namespace NEXUS.Fractal.Models;

public enum MatrixType
{
    // Laplacian Matrixes
    Laplacian3x3,
    Laplacian5x5,
    
    // Uniform Matrixes
    Uniform3x3,
    Uniform5x5,
    
    // Sharpen Matrixes
    Sharpen3x3,
    
    // Highpass Matrixes
    HighPass5x5Type1,
    HighPass5x5Type2,
    
    // Gaussian Matrixes
    Gaussian3x3_085,
    Gaussian3x3_0391,
    Gaussian5x5_0625,
    Gaussian5x5_10,
    
    // Sobel Matrixes
    Sobel3x3,
    Sobel3x3Horizontal,
    Sobel3x3Vertical,
    
    // Prewitt Matrixes
    Prewitt3x3,
    Prewitt3x3Horizontal,
    Prewitt3x3Vertical,
    
    // Kirsch Matrixes
    Kirsch3x3,
    Kirsch3x3Horizontal,
    Kirsch3x3Vertical,
    
    // Methods
    Inverse
}