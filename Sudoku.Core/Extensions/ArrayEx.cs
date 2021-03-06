﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sudoku.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="Array"/>.
	/// </summary>
	/// <seealso cref="Array"/>
	[DebuggerStepThrough]
	public static class ArrayEx
	{
		/// <summary>
		/// Get all subsets from the specified number of the values to take.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The array.</param>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>All subsets.</returns>
		public static IEnumerable<T[]> GetSubsets<T>(this T[] @this, int count)
		{
			if (count == 0)
			{
				return Array.Empty<T[]>();
			}

			// Local function 'g' will capture variable 'result'.
			var result = new List<T[]>();
			g(@this.Length, count, count, new int[count]);

			return result;

			void g(int last, int count, int m, int[] b)
			{
				for (int i = last; i >= m; i--)
				{
					b[m - 1] = i - 1;
					if (m > 1)
					{
						g(i - 1, count, m - 1, b);
					}
					else
					{
						var temp = new T[count];
						for (int j = 0; j < b.Length; j++)
						{
							temp[j] = @this[b[j]];
						}

						result.Add(temp);
					}
				}
			}
		}
	}
}
