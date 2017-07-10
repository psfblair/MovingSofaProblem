using System;
using System.Collections.Generic;

/*
The MIT License (MIT)

Copyright (c) 2013 Tom Jacques

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Functional.Option
{
    /// <summary>
    /// Extension methods for turning values into options
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a value to an Option&lt;T&gt;.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// Option&lt;T&gt;.None if value is null, otherwise an
        /// Option&lt;T&gt; whose value is set to <paramref name="value"/>.
        /// </returns>
        public static Option<T> ToOption<T>(this T value)
        {
            return Option.Create(value);
        }

        /// <summary>
        /// Converts a value to an Option&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// Option&lt;T&gt;.None if value is null, otherwise an
        /// Option&lt;T&gt; whose value is set to <paramref name="value"/>.
        /// </returns>
        public static Option<T> ToOption<T>(this Nullable<T> value)
            where T : struct
        {
            return Option.Create(value);
        }

        /// <summary>
        /// Flattens a sequence of Option&lt;T&gt; into one sequence of T
        /// elements where the value of the Option&lt;T&gt; was Some&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">A sequence of Option&lt;T&gt;.</param>
        /// <returns>
        /// A sequence of T elements where the value of the Option&lt;T&gt;
        /// was Some&lt;T&gt;.
        /// </returns>
        public static IEnumerable<T> Flatten<T>(
            this IEnumerable<Option<T>> source)
        {
            foreach (var option in source)
            {
                if (option.HasValue)
                {
                    yield return option.Value;
                }
            }
        }
    }
}