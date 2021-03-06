﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;

namespace Sudoku.Solving.Manual.Uniqueness.Extended
{
	/// <summary>
	/// Encapsulates an <b>extended rectangle</b> technique searcher.
	/// </summary>
	[TechniqueDisplay(nameof(TechniqueCode.XrType1))]
	[SearcherProperty(46)]
	public sealed partial class XrTechniqueSearcher : UniquenessTechniqueSearcher
	{
		/// <inheritdoc/>
		public override void GetAll(IList<TechniqueInfo> accumulator, Grid grid)
		{
			foreach (var (allCellsMap, pairs, size) in Combinations)
			{
				if ((EmptyMap & allCellsMap) != allCellsMap)
				{
					continue;
				}

				// Check each pair.
				// Ensures all pairs should contains same digits
				// and the kind of digits must be greater than 2.
				bool checkKindsFlag = true;
				foreach (var (l, r) in pairs)
				{
					short tempMask = (short)(grid.GetCandidateMask(l) & grid.GetCandidateMask(r));
					if (tempMask == 0 || tempMask.IsPowerOfTwo())
					{
						checkKindsFlag = false;
						break;
					}
				}
				if (!checkKindsFlag)
				{
					// Failed to check.
					continue;
				}

				// Check the mask of cells from two regions.
				short m1 = 0, m2 = 0;
				foreach (var (l, r) in pairs)
				{
					m1 |= grid.GetCandidateMask(l);
					m2 |= grid.GetCandidateMask(r);
				}

				short resultMask = (short)(m1 | m2);
				short normalDigits = 0, extraDigits = 0;
				foreach (int digit in resultMask.GetAllSets())
				{
					int count = 0;
					foreach (var (l, r) in pairs)
					{
						if (((grid.GetCandidateMask(l) & grid.GetCandidateMask(r)) >> digit & 1) != 0)
						{
							// Both two cells contain same digit.
							count++;
						}
					}

					(count >= 2 ? ref normalDigits : ref extraDigits) |= (short)(1 << digit);
				}

				if (normalDigits.CountSet() != size)
				{
					// The number of normal digits are not enough.
					continue;
				}

				if (resultMask.CountSet() == size + 1)
				{
					// Possible type 1 or 2 found.
					// Now check extra cells.
					int extraDigit = extraDigits.FindFirstSet();
					var extraCellsMap = allCellsMap & CandMaps[extraDigit];
					if (extraCellsMap.IsEmpty)
					{
						continue;
					}

					if (extraCellsMap.Count == 1)
					{
						CheckType1(accumulator, grid, allCellsMap, extraCellsMap, normalDigits, extraDigit);
					}

					CheckType2(accumulator, grid, allCellsMap, extraCellsMap, normalDigits, extraDigit);
				}
				else
				{
					var extraCellsMap = GridMap.Empty;
					foreach (int cell in allCellsMap)
					{
						foreach (int digit in extraDigits.GetAllSets())
						{
							if (!grid[cell, digit])
							{
								extraCellsMap.AddAnyway(cell);
								break;
							}
						}
					}

					if (!extraCellsMap.InOneRegion)
					{
						continue;
					}

					CheckType3Naked(accumulator, grid, allCellsMap, normalDigits, extraDigits, extraCellsMap);
					CheckType14(accumulator, grid, allCellsMap, normalDigits, extraCellsMap);
				}
			}
		}

		partial void CheckType1(
			IList<TechniqueInfo> accumulator, Grid grid, GridMap allCellsMap,
			GridMap extraCells, short normalDigits, int extraDigit);

		partial void CheckType2(
			IList<TechniqueInfo> accumulator, Grid grid, GridMap allCellsMap,
			GridMap extraCells, short normalDigits, int extraDigit);

		partial void CheckType3Naked(
			IList<TechniqueInfo> accumulator, Grid grid, GridMap allCellsMap,
			short normalDigits, short extraDigits, GridMap extraCellsMap);

		partial void CheckType14(
			IList<TechniqueInfo> accumulator, Grid grid, GridMap allCellsMap,
			short normalDigits, GridMap extraCellsMap);
	}
}
