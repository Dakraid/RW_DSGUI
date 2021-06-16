using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DSGUI {
    public static class DSGUI_Functions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool OptimizedNullOrEmpty<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null)
                return true;

            return enumerable is ICollection collection ? collection.Count == 0 : !enumerable.Any();
        }
    }
}