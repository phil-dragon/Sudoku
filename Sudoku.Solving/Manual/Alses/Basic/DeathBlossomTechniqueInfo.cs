﻿using System.Collections.Generic;
using System.Text;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using Sudoku.Extensions;

namespace Sudoku.Solving.Manual.Alses.Basic
{
	/// <summary>
	/// Provides a usage of <b>death blossom</b> technique.
	/// </summary>
	public sealed class DeathBlossomTechniqueInfo : AlsTechniqueInfo
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="pivot">The pivot cell.</param>
		/// <param name="alses">All ALSes used.</param>
		public DeathBlossomTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int pivot, IReadOnlyDictionary<int, Als> alses) : base(conclusions, views) =>
			(Pivot, Alses) = (pivot, alses);


		/// <summary>
		/// Indicates how many petals used.
		/// </summary>
		public int PetalsCount => Alses.Count;

		/// <summary>
		/// Indicates the pivot cell.
		/// </summary>
		public int Pivot { get; }

		/// <summary>
		/// Indicates all ALSes used sorted by digit.
		/// </summary>
		public IReadOnlyDictionary<int, Als> Alses { get; }

		/// <inheritdoc/>
		public override decimal Difficulty => 8.0M + PetalsCount * .1M;

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Nightmare;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode => TechniqueCode.DeathBlossom;


		/// <inheritdoc/>
		public override string ToString()
		{
			string pivotStr = new CellCollection(Pivot).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: Cell {pivotStr} - {g(this)} => {elimStr}";

			static string g(DeathBlossomTechniqueInfo @this)
			{
				const string separator = ", ";
				var sb = new StringBuilder();
				foreach (var (digit, als) in @this.Alses)
				{
					sb.Append($"{digit + 1} - {als}{separator}");
				}

				return sb.RemoveFromEnd(separator.Length).ToString();
			}
		}
	}
}
