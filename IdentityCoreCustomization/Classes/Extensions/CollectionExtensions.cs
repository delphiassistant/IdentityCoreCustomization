using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityCoreCustomization.Classes.Extensions
{
    /// <summary>
    /// Placeholder for collection-related extension types.
    /// </summary>
    /// <remarks>
    /// This class intentionally contains no members. Use <see cref="ListExtensions"/> for list-related helpers.
    /// </remarks>
    public class CollectionExtensions
    {
    }

    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/>.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Determines whether two lists contain the same set of elements, ignoring order and duplicate counts.
        /// </summary>
        /// <typeparam name="T">
        /// The element type. Must implement <see cref="IEquatable{T}"/> to support value-based equality comparisons.
        /// </typeparam>
        /// <param name="list">The first list to compare.</param>
        /// <param name="other">The second list to compare with <paramref name="list"/>.</param>
        /// <returns>
        /// <see langword="true"/> if both lists contain the same unique elements (set equality); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// - Element order is ignored.
        /// - Duplicate counts are ignored (e.g., [A, A] and [A] are considered equal).
        /// - Passing <paramref name="other"/> as <see langword="null"/> will result in an <see cref="ArgumentNullException"/>.
        /// - Invoking this extension on a <see langword="null"/> <paramref name="list"/> reference will result in a <see cref="NullReferenceException"/>.
        /// </remarks>
        public static bool HaveSameElements<T>(this List<T> list, List<T> other) where T : IEquatable<T>
        {
            if (list.Except(other).Any())
                return false;
            if (other.Except(list).Any())
                return false;
            return true;
        }
    }
}