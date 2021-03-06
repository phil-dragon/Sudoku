﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Uniqueness.Extended
{
	/// <summary>
	/// Provides a usage of <b>extended rectangle</b> (XR) type 2 technique.
	/// </summary>
	public sealed class XrType2TechniqueInfo : XrTechniqueInfo
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="cells">All cells.</param>
		/// <param name="digits">All digits.</param>
		/// <param name="extraDigit">The extra digit.</param>
		public XrType2TechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			GridMap cells, short digits, int extraDigit) : base(conclusions, views, cells, digits) =>
			ExtraDigit = extraDigit;


		/// <summary>
		/// Indicates the extra digit.
		/// </summary>
		public int ExtraDigit { get; }

		/// <inheritdoc/>
		public override decimal Difficulty => 4.6M + DifficultyExtra[Cells.Count];

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode => TechniqueCode.XrType2;


		/// <inheritdoc/>
		protected override string GetAdditional() => $"extra digit {ExtraDigit + 1}";
	}
}
