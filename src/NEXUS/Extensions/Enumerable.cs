using System.Collections;

namespace NEXUS.Extensions;

public static class Enumerable
{
    #region Generics

    public static T? FirstOrDefault<T>(this IEnumerable collection){
        return collection.OfType<T>().FirstOrDefault();
    }
    
    public static T First<T>(this IEnumerable collection){
        return collection.OfType<T>().First();
    }

    public static int IndexOf<T>(this IList<T> collection, Func<T, bool> func)
    {
        foreach (var item in collection)
            if (func(item))
                return collection.IndexOf(item);
        
        return -1;
    }
    
    public static void Remove<T>(this ICollection<T> collection, Func<T, bool> func)
    {
        var filteredItem = collection.FirstOrDefault(func);
        if (filteredItem != null) 
            collection.Remove(filteredItem);
    }
    
    #endregion

}