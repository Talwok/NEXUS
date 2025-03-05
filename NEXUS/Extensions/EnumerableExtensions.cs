using System.Collections;

namespace NEXUS.Extensions;

public static class EnumerableExtensions
{
    #region Generics

    public static T? FirstOrDefault<T>(this IEnumerable collection){
        return collection.OfType<T>().FirstOrDefault();
    }
    
    public static T First<T>(this IEnumerable collection){
        return collection.OfType<T>().First();
    }

    #endregion

}