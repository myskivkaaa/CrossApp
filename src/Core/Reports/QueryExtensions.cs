// додаткове 7
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class QueryExtensions
{
    // Власний універсальний метод-розширення для отримання топ-N елементів за довільним критерієм
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source, 
        Func<T, TKey> keySelector, 
        int n) 
    {
        return source.OrderByDescending(keySelector).Take(n);
    }
}