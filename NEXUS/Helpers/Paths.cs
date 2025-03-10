namespace NEXUS.Helpers;

public static class Paths
{
    public static string AppName = "NEXUS";
    
    public static string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string RoamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
}