// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class helper extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Performs a shallow clone of a List of type T.
        /// </summary>
        /// <typeparam name="T">The type of object the list contains.</typeparam>
        /// <param name="source">The List instance to clone.</param>
        /// <returns>A shallow clone of the provided List instance.</returns>
        public static List<T> Clone<T>(this List<T> source)
        {
            List<T> clonedList = new List<T>();

            clonedList.AddRange(source);

            return clonedList;
        }
    }
}
