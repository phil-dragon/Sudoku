﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Symmetry
{
	/// <summary>
	/// Provides a usage of <b>symmetry</b> technique.
	/// </summary>
	public abstract class SymmetryTechniqueInfo : TechniqueInfo
	{
		/// <inheritdoc/>
		protected SymmetryTechniqueInfo(IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views)
			: base(conclusions, views)
		{
		}
	}
}
