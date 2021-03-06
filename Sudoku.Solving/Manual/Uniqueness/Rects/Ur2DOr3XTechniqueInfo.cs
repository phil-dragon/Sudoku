﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Uniqueness.Rects
{
	/// <summary>
	/// Provides a usage of <b>unique rectangle + 2D (or 3X)</b> or
	/// <b>avoidable rectangle + 2D (or 3X)</b> technique.
	/// </summary>
	public sealed class Ur2DOr3XTechniqueInfo : UrTechniqueInfo
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="typeCode">The type code.</param>
		/// <param name="digit1">The digit 1.</param>
		/// <param name="digit2">The digit 2.</param>
		/// <param name="cells">All cells.</param>
		/// <param name="x">The X digit.</param>
		/// <param name="y">The Y digit.</param>
		/// <param name="xyCell">The cell that only contains X and Y digit.</param>
		/// <param name="isAr">Indicates whether the specified structure forms an AR.</param>
		public Ur2DOr3XTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			UrTypeCode typeCode, int digit1, int digit2, int[] cells,
			int x, int y, int xyCell, bool isAr) : base(conclusions, views, typeCode, digit1, digit2, cells, isAr) =>
			(X, Y, XyCell) = (x, y, xyCell);


		/// <summary>
		/// Indicates the X digit.
		/// </summary>
		public int X { get; }

		/// <summary>
		/// Indicates the Y digit.
		/// </summary>
		public int Y { get; }

		/// <summary>
		/// Indicates the cell that only contains X and Y digit.
		/// </summary>
		public int XyCell { get; }


		/// <inheritdoc/>
		public override decimal Difficulty => 4.7M;

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;


		/// <inheritdoc/>
		protected override string GetAdditional()
		{
			string xyCellStr = new CellCollection(XyCell).ToString();
			return $"X = {X + 1}, Y = {Y + 1} and a bi-value cell {xyCellStr}";
		}
	}
}
