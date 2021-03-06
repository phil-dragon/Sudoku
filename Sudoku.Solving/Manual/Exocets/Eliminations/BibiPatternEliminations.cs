﻿#pragma warning disable CA1815

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Extensions;

namespace Sudoku.Solving.Manual.Exocets.Eliminations
{
	/// <summary>
	/// Indicates the Bi-bi pattern eliminations.
	/// </summary>
	public struct BibiPatternEliminations : IEnumerable<Conclusion>
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="IEliminations"]'/>
		public BibiPatternEliminations(IList<Conclusion> conclusions) => Conclusions = conclusions;


		/// <include file='SolvingDocComments.xml' path='comments/property[@name="Count" and @type="IEliminations"]'/>
		public readonly int Count => Conclusions?.Count ?? 0;

		/// <include file='SolvingDocComments.xml' path='comments/property[@name="Conclusions" and @type="IEliminations"]'/>
		public IList<Conclusion>? Conclusions { readonly get; private set; }


		/// <include file='SolvingDocComments.xml' path='comments/method[@name="Add" and @type="IEliminations"]'/>
		public void Add(Conclusion conclusion) =>
			(Conclusions ??= new List<Conclusion>()).AddIfDoesNotContain(conclusion);

		/// <include file='SolvingDocComments.xml' path='comments/method[@name="AddRange" and @type="IEliminations"]'/>
		public void AddRange(IEnumerable<Conclusion> conclusions) =>
			(Conclusions ??= new List<Conclusion>()).AddRange(conclusions, true);

		/// <include file='SolvingDocComments.xml' path='comments/method[@name="Merge" and @type="IEliminations"]'/>
		public readonly BibiPatternEliminations Merge(params BibiPatternEliminations?[] eliminations)
		{
			var result = new BibiPatternEliminations();
			foreach (var instance in eliminations)
			{
				if (instance is null)
				{
					continue;
				}

				result.AddRange(instance);
			}

			return result;
		}

		/// <inheritdoc/>
		public readonly IEnumerator<Conclusion> GetEnumerator() =>
			(Conclusions ?? Array.Empty<Conclusion>()).GetEnumerator();


		/// <inheritdoc cref="object.ToString"/>
		public override readonly string? ToString() =>
			Conclusions is null ? null : $"  * Bi-bi pattern eliminations: {new ConclusionCollection(Conclusions).ToString()}";

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		/// <include file='SolvingDocComments.xml' path='comments/method[@name="MergeAll" and @type="IEliminations"]'/>
		public static BibiPatternEliminations MergeAll(IEnumerable<BibiPatternEliminations> list)
		{
			var result = new BibiPatternEliminations();
			foreach (var z in list)
			{
				if (z.Conclusions is null)
				{
					continue;
				}

				result.AddRange(z);
			}

			return result;
		}

		/// <include file='SolvingDocComments.xml' path='comments/method[@name="MergeAll" and @type="IEliminations"]'/>
		public static BibiPatternEliminations MergeAll(params BibiPatternEliminations[] list) =>
			MergeAll(list.AsEnumerable());
	}
}
