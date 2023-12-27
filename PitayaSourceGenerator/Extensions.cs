using System;
using System.Collections.Generic;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal static class Extensions
    {
        public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
        {
            foreach (TSource? item in source)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        public static MainReturnTypeEnum ToMainReturnTypeEnum(this string returnType)
        {
            return returnType switch
            {
                "void" => MainReturnTypeEnum.Void,
                "int" => MainReturnTypeEnum.Int,
                "System.Threading.Tasks.Task" => MainReturnTypeEnum.Task,
                "System.Threading.Tasks.Task<int>" => MainReturnTypeEnum.TaskInt,
                _ => throw new ArgumentException($"Unexpected return type '{returnType}'."),
            };
        }
    }
}
