using System.Reflection;

namespace NEXUS.Helpers;

public class ClassAttributeCheck
{
    private static bool HasAttribute<T, TA>() where T : class where TA : Attribute
    {
        return typeof(T).GetCustomAttribute<TA>(inherit: true) != null;
    }
}