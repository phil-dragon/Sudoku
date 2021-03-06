﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using static System.Math;

namespace Sudoku.Solving.Manual.Alses.Mslses
{
	/// <summary>
	/// Provides a usage of <b>almost locked sets net</b> technique.
	/// </summary>
	public sealed class AlsNetTechniqueInfo : MslsTechniqueInfo
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="cells">The cells.</param>
		public AlsNetTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views, GridMap cells)
			: base(conclusions, views) => Cells = cells;


		/// <summary>
		/// Indicates the cells used.
		/// </summary>
		public GridMap Cells { get; }

		/// <inheritdoc/>
		public override decimal Difficulty => 9.4M + (decimal)Floor((Sqrt(1 + 8 * Cells.Count) - 1) / 2) * .1M;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode => TechniqueCode.Msls;

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Nightmare;


		/// <inheritdoc/>
		public override string ToString()
		{
			string cellsStr = new CellCollection(Cells).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: {Cells.Count} cells {cellsStr} => {elimStr}";
		}
	}
}
