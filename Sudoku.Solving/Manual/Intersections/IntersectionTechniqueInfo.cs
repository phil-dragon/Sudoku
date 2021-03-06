﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Intersections
{
	/// <summary>
	/// Provides a usage of <b>intersection</b> technique.
	/// </summary>
	public abstract class IntersectionTechniqueInfo : TechniqueInfo
	{
		/// <inheritdoc/>
		protected IntersectionTechniqueInfo(IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views)
			: base(conclusions, views)
		{
		}
	}
}
