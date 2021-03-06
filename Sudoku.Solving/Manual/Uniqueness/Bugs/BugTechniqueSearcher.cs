﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;
using Sudoku.Solving.Annotations;
using Sudoku.Solving.Checking;
using static Sudoku.Data.ConclusionType;

namespace Sudoku.Solving.Manual.Uniqueness.Bugs
{
	/// <summary>
	/// Encapsulates a <b>bivalue universal grave</b> (BUG) technique searcher.
	/// </summary>
	[TechniqueDisplay(nameof(TechniqueCode.BugType1))]
	[SearcherProperty(56)]
	public sealed partial class BugTechniqueSearcher : UniquenessTechniqueSearcher
	{
		/// <summary>
		/// Indicates whether the searcher should call the extended BUG checker
		/// to find all true candidates.
		/// </summary>
		private readonly bool _extended;


		/// <summary>
		/// Initializes an instance with the region maps.
		/// </summary>
		/// <param name="extended">
		/// A <see cref="bool"/> value indicating whether the searcher should call
		/// the extended BUG checker to search for all true candidates no matter how
		/// difficult searching.
		/// </param>
		public BugTechniqueSearcher(bool extended) => _extended = extended;


		/// <inheritdoc/>
		public override void GetAll(IList<TechniqueInfo> accumulator, Grid grid)
		{
			var trueCandidates = new BugChecker(grid).TrueCandidates;
			switch (trueCandidates.Count)
			{
				case 0:
				{
					return;
				}
				case 1:
				{
					// BUG + 1 found.
					accumulator.Add(
						new BugType1TechniqueInfo(
							conclusions: new[] { new Conclusion(Assignment, trueCandidates[0]) },
							views: new[] { new View(new[] { (0, trueCandidates[0]) }) }));
					break;
				}
				default:
				{
					if (CheckSingleDigit(trueCandidates))
					{
						CheckType2(accumulator, trueCandidates);
					}
					else
					{
						if (_extended)
						{
							CheckMultiple(accumulator, grid, trueCandidates);
							CheckXz(accumulator, grid, trueCandidates);
						}

						CheckType3Naked(accumulator, grid, trueCandidates);
						CheckType4(accumulator, grid, trueCandidates);
					}

					break;
				}
			}
		}

		partial void CheckType2(IList<TechniqueInfo> accumulator, IReadOnlyList<int> trueCandidates);

		partial void CheckType3Naked(IList<TechniqueInfo> accumulator, Grid grid, IReadOnlyList<int> trueCandidates);

		partial void CheckType4(IList<TechniqueInfo> accumulator, Grid grid, IReadOnlyList<int> trueCandidates);

		partial void CheckMultiple(IList<TechniqueInfo> accumulator, Grid grid, IReadOnlyList<int> trueCandidates);

		partial void CheckXz(IList<TechniqueInfo> accumulator, Grid grid, IReadOnlyList<int> trueCandidates);
	}
}
