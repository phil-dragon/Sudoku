﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;
using Sudoku.Solving.Manual.Exocets.Eliminations;

namespace Sudoku.Solving.Manual.Exocets
{
	/// <summary>
	/// Provides a usage of <b>junior exocet</b> (JE) technique.
	/// </summary>
	public sealed class JuniorExocetTechniqueInfo : ExocetTechniqueInfo
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="exocet">The exocet.</param>
		/// <param name="digits">All digits.</param>
		/// <param name="lockedMemberQ">The locked member Q.</param>
		/// <param name="lockedMemberR">The locked member R.</param>
		/// <param name="targetEliminations">The target eliminations.</param>
		/// <param name="mirrorEliminations">The mirror eliminations.</param>
		/// <param name="bibiEliminations">The Bi-bi pattern eliminations.</param>
		/// <param name="targetPairEliminations">The target pair eliminations.</param>
		/// <param name="swordfishEliminations">The swordfish pattern eliminations.</param>
		public JuniorExocetTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views, Pattern exocet,
			IEnumerable<int> digits, IEnumerable<int>? lockedMemberQ, IEnumerable<int>? lockedMemberR,
			TargetEliminations targetEliminations, MirrorEliminations mirrorEliminations,
			BibiPatternEliminations bibiEliminations, TargetPairEliminations targetPairEliminations,
			SwordfishEliminations swordfishEliminations)
			: base(
				  conclusions, views, exocet, digits, TechniqueCode.Je,
				  lockedMemberQ, lockedMemberR, targetEliminations, mirrorEliminations,
				  bibiEliminations, targetPairEliminations, swordfishEliminations, default, default)
		{
		}


		/// <inheritdoc/>
		public override decimal Difficulty =>
			9.4M
			+ (MirrorEliminations.Conclusions is null ? 0 : .1M)
			+ (BibiEliminations.Conclusions is null ? 0 : .3M)
			+ (TargetPairEliminations.Conclusions is null ? 0 : .1M)
			+ (SwordfishEliminations.Conclusions is null ? 0 : .2M);


		/// <inheritdoc/>
		protected override string? GetAdditional() => null;
	}
}
