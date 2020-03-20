﻿using System.Collections.Generic;
using Sudoku.Drawing;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Manual.Intersections
{
	/// <summary>
	/// Provides a usage of <b>locked candidates</b> (LC) technique.
	/// </summary>
	public sealed class LcTechniqueInfo : IntersectionTechniqueInfo
	{
		/// <summary>
		/// Initializes an instance with information.
		/// </summary>
		/// <param name="conclusions">The conclusions.</param>
		/// <param name="views">The views.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="baseSet">The base set.</param>
		/// <param name="coverSet">The cover set.</param>
		public LcTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int digit, int baseSet, int coverSet)
			: base(conclusions, views) =>
			(Digit, BaseSet, CoverSet) = (digit, baseSet, coverSet);


		/// <inheritdoc/>
		public override string Name => BaseSet < 9 ? "Pointing" : "Claiming";

		/// <inheritdoc/>
		public override decimal Difficulty => BaseSet < 9 ? 2.6M : 2.8M;

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Moderate;

		/// <summary>
		/// Indicates the digit.
		/// </summary>
		public int Digit { get; }

		/// <summary>
		/// Indicates the base set.
		/// </summary>
		public int BaseSet { get; }

		/// <summary>
		/// Indicates the cover set.
		/// </summary>
		public int CoverSet { get; }


		/// <inheritdoc/>
		public override string ToString()
		{
			int value = Digit + 1;
			string baseSetStr = RegionUtils.ToString(BaseSet);
			string coverSetStr = RegionUtils.ToString(CoverSet);
			string elimStr = ConclusionCollection.ToString(Conclusions);
			return $@"{Name}: {value} in {baseSetStr}\{coverSetStr} => {elimStr}";
		}
	}
}