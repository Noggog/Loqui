﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System
{
    /// <summary>
    /// Extension class for checking and retrieving nullable values with one access.
    /// </summary>
    public static class NullableExt
    {
        /// <summary>
        /// Retrieves a nullable class field if it exists.
        /// This helps provide options for ideal Overlay usage where properties are only accessed once.
        /// </summary>
        /// <param name="source">Field to retrieve</param>
        /// <param name="item">Non-null result if field exists</param>
        /// <returns>True if field exists, and item contains a non-null value</returns>
        /// <typeparam name="T">Item type being retrieved</typeparam>
        public static bool TryGet<T>(this T? source, [MaybeNullWhen(false)] out T item)
            where T : class
        {
            if (source == null)
            {
                item = default!;
                return false;
            }
            item = source;
            return true;
        }

        /// <summary>
        /// Retrieves a nullable struct field if it exists.
        /// This helps provide options for ideal Overlay usage where properties are only accessed once.
        /// </summary>
        /// <param name="source">Field to retrieve</param>
        /// <param name="item">Non-null result if field exists</param>
        /// <returns>True if field exists, and item contains a non-null value</returns>
        /// <typeparam name="T">Item type being retrieved</typeparam>
        public static bool TryGet<T>(this Nullable<T> source, [MaybeNullWhen(false)] out T item)
            where T : struct
        {
            if (source == null)
            {
                item = default!;
                return false;
            }
            item = source.Value;
            return true;
        }
    }
}
