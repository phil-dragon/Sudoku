﻿using System;
using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.DocComments;
using Sudoku.Extensions;
using Sudoku.Solving.Manual.Exocets.Eliminations;
using static Sudoku.Constants.Processings;
using static Sudoku.Constants.RegionLabel;
using static Sudoku.Data.CellStatus;
using static Sudoku.Data.ConclusionType;

namespace Sudoku.Solving.Manual.Exocets
{
	/// <summary>
	/// <para>
	/// Encapsulates an <b>exocet</b> technique searcher. The pattern will be like:
	/// <code>
	/// .-------.-------.-------.
	/// | B B E | E . . | E . . |
	/// | . . E | Q . . | R . . |
	/// | . . E | Q . . | R . . |
	/// :-------+-------+-------:
	/// | . . S | S . . | S . . |
	/// | . . S | S . . | S . . |
	/// | . . S | S . . | S . . |
	/// :-------+-------+-------:
	/// | . . S | S . . | S . . |
	/// | . . S | S . . | S . . |
	/// | . . S | S . . | S . . |
	/// '-------'-------'-------'
	/// </code>
	/// Where:
	/// <list type="table">
	/// <item><term>B</term><description>Base Cells.</description></item>
	/// <item><term>Q</term><description>1st Object Pair (Target cells pair 1).</description></item>
	/// <item><term>R</term><description>2nd Object Pair (Target cells pair 2).</description></item>
	/// <item><term>S</term><description>Cross-line Cells.</description></item>
	/// <item><term>E</term><description>Escape Cells.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// In the data structure, all letters will be used as the same one in this exemplar.
	/// In addition, if senior exocet, one of two target cells will lie on cross-line cells,
	/// and the lines of two target cells lying on cannot contain any base digits.
	/// </para>
	/// </summary>
	public abstract class ExocetTechniqueSearcher : TechniqueSearcher
	{
		/// <summary>
		/// Indicates all patterns.
		/// </summary>
		protected static readonly Pattern[] Patterns = new Pattern[1458];


		/// <summary>
		/// The cross line cells iterator.
		/// </summary>
		private static readonly int[,] SIter = { { 3, 4, 5, 6, 7, 8 }, { 0, 1, 2, 6, 7, 8 }, { 0, 1, 2, 3, 4, 5 } };

		/// <summary>
		/// The base cells iterator.
		/// </summary>
		private static readonly int[,] BIter =
		{
			{0, 1}, {0, 2}, {1, 2}, {9, 10}, {9, 11}, {10, 11}, {18, 19}, {18, 20}, {19, 20},
			{0, 9}, {0, 18}, {9, 18}, {1, 10}, {1, 19}, {10, 19}, {2, 11}, {2, 20}, {11, 20}
		};

		/// <summary>
		/// The Q or R cells iterator.
		/// </summary>
		private static readonly int[,] RqIter =
		{
			{9, 18}, {10, 19}, {11, 20}, {0, 18}, {1, 19}, {2, 20}, {0, 9}, {1, 10}, {2, 11},
			{1, 2}, {10, 11}, {19, 20}, {0, 2}, {9, 11}, {18, 20}, {0, 1}, {9, 10}, {18, 19}
		};

		/// <summary>
		/// The mirror list.
		/// </summary>
		private static readonly int[,] M =
		{
			{10, 11, 19, 20}, {9, 11, 18, 20}, {9, 10, 18, 19}, {1, 2, 19, 20}, {0, 2, 18, 20},
			{0, 1, 18, 19}, {1, 2, 10, 11}, {0, 2, 9, 11}, {0, 1, 9, 10}, {10, 19, 11, 20},
			{1, 19, 2, 20}, {1, 10, 2, 11}, {9, 18, 11, 20}, {0, 18, 2, 20}, {0, 9, 2, 11},
			{9, 18, 10, 19}, {0, 18, 1, 19}, {0, 9, 1, 10}
		};

		/// <summary>
		/// The base list.
		/// </summary>
		private static readonly int[] B = { 0, 3, 6, 27, 30, 33, 54, 57, 60, 0, 27, 54, 3, 30, 57, 6, 33, 60 };

		/// <summary>
		/// The combinations for base list <see cref="B"/>.
		/// </summary>
		/// <seealso cref="B"/>
		private static readonly int[,] BC =
		{
			{1, 2}, {0, 2}, {0, 1}, {4, 5}, {3, 5}, {3, 4}, {7, 8}, {6, 8}, {6, 7},
			{3, 6}, {0, 6}, {0, 3}, {4, 7}, {1, 7}, {1, 4}, {5, 8}, {2, 8}, {2, 5}
		};


		/// <summary>
		/// Indicates whether the searcher will find advanced eliminations.
		/// </summary>
		protected readonly bool _checkAdvanced;


		/// <summary>
		/// Initializes an instance with the specified region maps.
		/// </summary>
		/// <param name="checkAdvanced">
		/// Indicates whether the searcher will find advanced eliminations.
		/// </param>
		protected ExocetTechniqueSearcher(bool checkAdvanced) => _checkAdvanced = checkAdvanced;


		/// <inheritdoc cref="StaticConstructor"/>
		static ExocetTechniqueSearcher()
		{
			var t = (stackalloc int[3]);
			var crossline = (stackalloc int[25]); // Only use [7]..[24].
			int n = 0;
			for (int i = 0; i < 18; i++)
			{
				for (int z = i / 9 * 9, j = z; j < z + 9; j++)
				{
					for (int y = j / 3 * 3, k = y; k < y + 3; k++)
					{
						for (int l = y; l < y + 3; l++)
						{
							ref var exocet = ref Patterns[n];
							var (b1, b2) = (B[i] + BIter[j, 0], B[i] + BIter[j, 1]);
							var (tq1, tr1) = (B[BC[i, 0]] + RqIter[k, 0], B[BC[i, 1]] + RqIter[l, 0]);

							int index = 6, x = i / 3 % 3;
							int m = (m = i < 9 ? b1 % 9 + b2 % 9 : b1 / 9 + b2 / 9) switch
							{
								< 4 => 3 - m,
								< 13 => 12 - m,
								_ => 21 - m
							};
							(t[0], t[1], t[2]) = i < 9 ? (m, tq1 % 9, tr1 % 9) : (m, tq1 / 9, tr1 / 9);

							for (int a = 0, r = default, c = default; a < 3; a++)
							{
								(i < 9 ? ref c : ref r) = t[a];
								for (int b = 0; b < 6; b++)
								{
									(i < 9 ? ref r : ref c) = SIter[x, b];

									crossline[++index] = r * 9 + c;
								}
							}

							exocet = new(
								b1,
								b2,
								tq1,
								B[BC[i, 0]] + RqIter[k, 1],
								tr1,
								B[BC[i, 1]] + RqIter[l, 1],
								new(crossline[7..]),
								new() { B[BC[i, 1]] + M[l, 2], B[BC[i, 1]] + M[l, 3] },
								new() { B[BC[i, 1]] + M[l, 0], B[BC[i, 1]] + M[l, 1] },
								new() { B[BC[i, 0]] + M[k, 2], B[BC[i, 0]] + M[k, 3] },
								new() { B[BC[i, 0]] + M[k, 0], B[BC[i, 0]] + M[k, 1] });

							n++;
						}
					}
				}
			}
		}

		/// <summary>
		/// Check mirror eliminations.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="target">The target cell.</param>
		/// <param name="target2">
		/// The another target cell that is adjacent with <paramref name="target"/>.
		/// </param>
		/// <param name="lockedNonTarget">The locked member that is non-target digits.</param>
		/// <param name="baseCandidateMask">The base candidate mask.</param>
		/// <param name="mirror">The mirror map.</param>
		/// <param name="x">The x.</param>
		/// <param name="onlyOne">The only one cell.</param>
		/// <param name="cellOffsets">The cell offsets.</param>
		/// <param name="candidateOffsets">The candidate offsets.</param>
		/// <returns>All mirror eliminations.</returns>
		protected (TargetEliminations, MirrorEliminations) CheckMirror(
			Grid grid, int target, int target2, short lockedNonTarget, short baseCandidateMask,
			GridMap mirror, int x, int onlyOne, IList<(int, int)> cellOffsets, IList<(int, int)> candidateOffsets)
		{
			var targetElims = new TargetEliminations();
			var mirrorElims = new MirrorEliminations();
			var playground = (Span<int>)stackalloc[] { mirror.First, mirror.SetAt(1) };
			short mirrorCandidatesMask = (short)(
				grid.GetCandidateMask(playground[0]) | grid.GetCandidateMask(playground[1]));
			short commonBase = (short)(mirrorCandidatesMask & baseCandidateMask & grid.GetCandidateMask(target));
			short targetElimination = (short)(grid.GetCandidateMask(target) & ~(short)(commonBase | lockedNonTarget));
			if (targetElimination != 0 && grid.GetStatus(target) != Empty ^ grid.GetStatus(target2) != Empty)
			{
				foreach (int digit in targetElimination.GetAllSets())
				{
					targetElims.Add(new(Elimination, target, digit));
				}
			}

			if (_checkAdvanced)
			{
				var regions = (stackalloc int[2]);
				short m1 = (short)(grid.GetCandidateMask(playground[0]) & baseCandidateMask);
				short m2 = (short)(grid.GetCandidateMask(playground[1]) & baseCandidateMask);
				if ((m1, m2) is (0, not 0) or (not 0, 0))
				{
					int p = playground[m1 == 0 ? 1 : 0];
					short candidateMask = (short)(grid.GetCandidateMask(p) & ~commonBase);
					if (candidateMask != 0 && grid.GetStatus(target) != Empty ^ grid.GetStatus(target2) != Empty)
					{
						cellOffsets.Add((3, playground[0]));
						cellOffsets.Add((3, playground[1]));
						foreach (int digit in candidateMask.GetAllSets())
						{
							mirrorElims.Add(new(Elimination, p, digit));
						}
					}

					return (targetElims, mirrorElims);
				}

				short nonBase = (short)(mirrorCandidatesMask & ~baseCandidateMask);
				int r1 = GetRegion(playground[0], Row);
				(regions[0], regions[1]) = (
					GetRegion(playground[0], Block),
					r1 == GetRegion(playground[1], Row) ? r1 : GetRegion(playground[0], Column));
				short locked = default;
				foreach (short mask in GetCombinations(nonBase))
				{
					for (int i = 0; i < 2; i++)
					{
						int count = 0;
						for (int j = 0; j < 9; j++)
						{
							int p = RegionCells[regions[i]][j];
							if (p == playground[0] || p == playground[1] || p == onlyOne)
							{
								continue;
							}

							if ((grid.GetCandidateMask(p) & mask) != 0)
							{
								count++;
							}
						}

						if (count == mask.CountSet() - 1)
						{
							for (int j = 0; j < 9; j++)
							{
								int p = RegionCells[regions[i]][j];
								if ((grid.GetCandidateMask(p) & mask) == 0
									|| grid.GetStatus(p) != Empty || p == onlyOne
									|| p == playground[0] || p == playground[1]
									|| (grid.GetCandidateMask(p) & ~mask) == 0)
								{
									continue;
								}
							}

							locked = mask;
							break;
						}
					}

					if (locked != 0)
					{
						// Here you should use '|' operator rather than '||'.
						// Operator '||' will not execute the second method if the first condition is true.
						if (g(playground, 0) | g(playground, 1))
						{
							cellOffsets.Add((3, playground[0]));
							cellOffsets.Add((3, playground[1]));
						}

						short mask1 = grid.GetCandidateMask(playground[0]);
						short mask2 = grid.GetCandidateMask(playground[1]);
						if (locked.CountSet() == 1 && (mask1 & locked) != 0 ^ (mask2 & locked) != 0)
						{
							short candidateMask = (short)(
								~(
									grid.GetCandidateMask(
										playground[(mask1 & locked) != 0 ? 1 : 0]
									) & grid.GetCandidateMask(target) & baseCandidateMask
								) & grid.GetCandidateMask(target) & baseCandidateMask);
							if (candidateMask != 0)
							{
								foreach (int digit in candidateMask.GetAllSets())
								{
									mirrorElims.Add(new(Elimination, target, digit));
								}
							}
						}

						break;

						// Gathering.
						bool g(Span<int> playground, int i)
						{
							short candidateMask = (short)(
								grid.GetCandidateMask(playground[i]) & ~(baseCandidateMask | locked));
							if (candidateMask != 0)
							{
								foreach (int digit in locked.GetAllSets())
								{
									if (grid.Exists(playground[i], digit) is true)
									{
										candidateOffsets.Add((1, playground[i] * 9 + digit));
									}
								}

								foreach (int digit in candidateMask.GetAllSets())
								{
									mirrorElims.Add(new(Elimination, playground[i], digit));
								}

								return true;
							}

							return false;
						}
					}
				}
			}

			return (targetElims, mirrorElims);
		}


		/// <summary>
		/// Get all combinations that contains all set bits from the specified number.
		/// </summary>
		/// <param name="seed">The specified number.</param>
		/// <returns>All combinations.</returns>
		protected static IEnumerable<short> GetCombinations(short seed)
		{
			for (int i = 0; i < 9; i++)
			{
				foreach (short mask in new BitCombinationGenerator(9, i))
				{
					if ((mask & seed) == mask)
					{
						yield return mask;
					}
				}
			}
		}
	}
}
