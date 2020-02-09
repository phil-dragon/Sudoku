﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Data.Extensions;
using Sudoku.Data.Meta;
using Sudoku.Solving.Extensions;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Checking
{
	/// <summary>
	/// Encapsulates a BUG technique checker.
	/// </summary>
	public sealed partial class BugChecker
	{
		/// <summary>
		/// The distribution of all empty cells.
		/// </summary>
		private readonly GridMap _emptyCellsDistribution;

		/// <summary>
		/// The distribution of all bivalue cells.
		/// </summary>
		private readonly GridMap _bivalueCellsDistribution;

		/// <summary>
		/// The distribution of all digits.
		/// </summary>
		private readonly GridMap[] _digitsDistributions;


		/// <summary>
		/// Initializes an instance with the specified grid.
		/// </summary>
		/// <param name="grid">The grid.</param>
		public BugChecker(Grid grid)
		{
			if (grid.IsUnique(out _))
			{
				Grid = grid;
				(_emptyCellsDistribution, _bivalueCellsDistribution, _digitsDistributions) = grid;
			}
			else
			{
				throw new ArgumentException(
					"The specified grid does not have a unique solution.", nameof(grid));
			}
		}

		/// <summary>
		/// Initializes an instance with the specified grid and all grid maps information.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="empty">The distribution of all empty cells.</param>
		/// <param name="bivalue">The distribution of all bivalue cells.</param>
		/// <param name="digits">The distribution of all single digits.</param>
		/// <remarks>
		/// The constructor is only called and used for reducting the redundant calculations.
		/// </remarks>
		internal BugChecker(Grid grid, GridMap empty, GridMap bivalue, GridMap[] digits)
		{
			if (grid.IsUnique(out _))
			{
				Grid = grid;
				(_emptyCellsDistribution, _bivalueCellsDistribution, _digitsDistributions) =
					(empty, bivalue, digits);
			}
			else
			{
				throw new ArgumentException(
					"The specified grid does not have a unique solution.", nameof(grid));
			}
		}


		/// <summary>
		/// Indicates the current grid is a BUG+n pattern.
		/// </summary>
		public bool IsBugPattern => GetAllTrueCandidates().Count != 0;

		/// <summary>
		/// The grid.
		/// </summary>
		public Grid Grid { get; }

		/// <summary>
		/// Indicates all true candidates (non-BUG candidates).
		/// </summary>
		public IReadOnlyList<int> TrueCandidates => GetAllTrueCandidates();


		/// <summary>
		/// Get all true candidates.
		/// </summary>
		/// <returns>All true candidates.</returns>
		private IReadOnlyList<int> GetAllTrueCandidates()
		{
			var allRegionsMap = GetAllRegionMaps();
			int[] array = _emptyCellsDistribution.ToArray();

			// Get the number of multivalue cells.
			int multivalueCellsCount = 0;
			foreach (int value in array)
			{
				int candidatesCount = Grid.GetCandidatesReversal(value).CountSet();
				if (candidatesCount == 1)
				{
					return Array.Empty<int>();
				}

				if (candidatesCount > 2)
				{
					multivalueCellsCount++;
				}
			}

			// Store all bivalue cells.
			var stack = new GridMap[multivalueCellsCount + 1, 9];
			if (_bivalueCellsDistribution.Count > 0)
			{
				int[] bivalueCells = _bivalueCellsDistribution.ToArray();
				foreach (int bivalueCell in bivalueCells)
				{
					int[] digits = Grid.GetCandidatesReversal(bivalueCell).GetAllSets().ToArray();
					for (int j = 0; j < 2; j++)
					{
						int digit = digits[j];
						ref var map = ref stack[0, digit];
						map[bivalueCell] = true;

						var (r, c, b) = CellUtils.GetRegion(bivalueCell);
						var span = (Span<int>)stackalloc[] { r + 9, c + 18, b };
						for (int k = 0; k < 3; k++)
						{
							if ((map & allRegionsMap[span[k]]).Count > 2)
							{
								return Array.Empty<int>();
							}
						}
					}
				}
			}

			// Store all multivalue cells.
			short mask = default;
			short[,] pairs = new short[multivalueCellsCount, 37];
			int[] multivalueCellsMap = (_emptyCellsDistribution - _bivalueCellsDistribution).ToArray();
			for (int i = 0; i < multivalueCellsMap.Length; i++)
			{
				mask = Grid.GetCandidatesReversal(multivalueCellsMap[i]);
				short[] list = GetAllCombinations(mask, 2);
				pairs[i, 0] = (short)list.Length;

				for (int z = 1; z <= list.Length; z++)
				{
					pairs[i, z] = list[z - 1];
				}
			}

			int pt = 1;
			int[] chosen = new int[multivalueCellsCount + 1];
			var resultMap = new GridMap[9];
			var result = new List<int>();
			do
			{
				int i;
				int ps = multivalueCellsMap[pt - 1];
				bool @continue = false;
				for (i = chosen[pt] + 1; i <= pairs[pt - 1, 0]; i++)
				{
					@continue = true;
					mask = pairs[pt - 1, i];
					for (int j = 1; j <= 2; j++)
					{
						var temp = stack[pt - 1, mask.GetSetBitIndex(j)];
						temp[ps] = true;
						var (r, c, b) = CellUtils.GetRegion(ps);
						var span = (Span<int>)stackalloc[] { b, r + 9, c + 18 };
						for (int k = 0; k < 3; k++)
						{
							if ((temp & allRegionsMap[span[k]]).Count > 2)
							{
								@continue = false;
								break;
							}
						}

						if (!@continue) break;
					}

					if (@continue) break;
				}

				if (@continue)
				{
					for (int z = 0; z < stack.GetLength(1); z++)
					{
						stack[pt, z] = stack[pt - 1, z];
					}

					chosen[pt] = i;
					var digits = mask.GetAllSets();
					stack[pt, digits.ElementAt(0)][ps] = true;
					stack[pt, digits.ElementAt(1)][ps] = true;
					if (pt == multivalueCellsCount)
					{
						for (int k = 0; k < 9; k++)
						{
							ref var map = ref resultMap[k];
							map = _digitsDistributions[k] - stack[pt, k];
							foreach (int cell in map.Offsets)
							{
								result.Add(cell * 9 + k);
							}
						}

						return result;
					}

					pt++;
				}
				else
				{
					chosen[pt] = 0;
					pt--;
				}
			} while (pt > 0);

			return result;
		}


		/// <summary>
		/// Get all combinations of a specified mask.
		/// </summary>
		/// <param name="mask">The mask.</param>
		/// <param name="oneCount">
		/// The number of <see langword="true"/> bits.
		/// </param>
		/// <returns>All combinations.</returns>
		private static short[] GetAllCombinations(short mask, int oneCount)
		{
			var result = new List<short>();
			foreach (short z in new BitEnumerator(9, oneCount))
			{
				if ((mask | z) == mask)
				{
					result.Add(z);
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Get all grid maps about all regions (all cells lie on
		/// specified region will be set <see langword="true"/>).
		/// </summary>
		/// <returns>The grid maps.</returns>
		private static GridMap[] GetAllRegionMaps()
		{
			var result = new GridMap[27];
			for (int region = 0; region < 27; region++)
			{
				foreach (int offset in GridMap.GetCellsIn(region))
				{
					result[region][offset] = true;
				}
			}

			return result;
		}
	}
}