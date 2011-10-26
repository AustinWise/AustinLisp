using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AustinLisp
{
    static class Extensions
    {
        public static void Add<TKey, TVal>(this Dictionary<TKey, TVal> dic, TKey[] keys, TVal val)
        {
            foreach (var k in keys)
            {
                dic.Add(k, val);
            }
        }
    }
}
