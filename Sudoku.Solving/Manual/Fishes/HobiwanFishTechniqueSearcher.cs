﻿using System.Collections.Generic;
using System.Linq;
using Sudoku.Data;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using Sudoku.Solving.Manual.LastResorts;
using static System.Algorithms;
using static System.Math;
using static Sudoku.Constants.Processings;
using static Sudoku.Constants.RegionLabel;
using static Sudoku.Data.ConclusionType;

namespace Sudoku.Solving.Manual.Fishes
{
	/// <summary>
	/// Encapsulates a <b>Hobiwan's fish</b> technique searcher.
	/// </summary>
	[TechniqueDisplay(nameof(TechniqueCode.FrankenSwordfish))]
	[SearcherProperty(80, IsEnabled = false, DisabledReason = DisabledReason.TooSlow)]
	public sealed class HobiwanFishTechniqueSearcher : FishTechniqueSearcher
	{
		/// <summary>
		/// Indicates the maximum number of exo-fins will be found.
		/// </summary>
		private readonly int _exofinCount;

		/// <summary>
		/// Indicates the maximum number of endo-fins will be found.
		/// </summary>
		private readonly int _endofinCount;

		/// <summary>
		/// Indicates the maximum size will be found. The maximum value supporting is 7.
		/// </summary>
		private readonly int _size;

		/// <summary>
		/// Indicates whether the puzzle will check POM first.
		/// If so and the digit does not have any eliminations, this digit
		/// will be skipped rather than do empty and useless loops.
		/// </summary>
		private readonly bool _checkPom;


		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <param name="exofinCount">The maximum number of exo-fins.</param>
		/// <param name="endofinCount">The maximum number of endo-fins.</param>
		/// <param name="checkPom">
		/// Indicates whether the puzzle will check POM first.
		/// </param>
		public HobiwanFishTechniqueSearcher(int size, int exofinCount, int endofinCount, bool checkPom) =>
			(_size, _exofinCount, _endofinCount, _checkPom) = (size, exofinCount, endofinCount, checkPom);


		/// <inheritdoc/>
		public override void GetAll(IList<TechniqueInfo> accumulator, Grid grid)
		{
			for (int size = 2; size <= _size; size++)
			{
				GetAll(accumulator, grid, size);
			}
		}

		/// <summary>
		/// Accumulate all technique information instances into the specified accumulator by size.
		/// </summary>
		/// <param name="accumulator">The accumulator.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="size">The size to iterate on.</param>
		private void GetAll(IList<TechniqueInfo> accumulator, Grid grid, int size)
		{
			var bag = new List<TechniqueInfo>();
			var conclusionList = new GridMap[9];
			if (_checkPom)
			{
				var searcher = new PomTechniqueSearcher();
				searcher.GetAll(bag, grid);

				foreach (var info in bag)
				{
					foreach (var (_, cell, digit) in info.Conclusions)
					{
						conclusionList[digit].AddAnyway(cell);
					}
				}
			}

			for (int digit = 0; digit < 9; digit++)
			{
				var candMap = CandMaps[digit];
				if (_checkPom && conclusionList[digit].IsEmpty || candMap.RowMask.CountSet() <= size)
				{
					// This digit does not contain any conclusions or
					// No available fish can be found.
					continue;
				}

				var globalElimMap = conclusionList[digit];
				int mask = candMap.RowMask << 9 | candMap.ColumnMask << 18 | (int)candMap.BlockMask;
				var baseSetsList = mask.GetAllSets().ToArray().GetSubsets(size);

				// Iterate on each combination.
				foreach (int[] baseSets in baseSetsList)
				{
					var baseRegionMap = new RegionMap(baseSets);
					var baseSetsMap = GridMap.Empty;
					foreach (int baseSet in baseSets)
					{
						baseSetsMap |= RegionMaps[baseSet];
					}

					var endoFinsMap = GridMap.Empty;
					var tempMap = GridMap.Empty;
					for (int i = 0; i < size; i++)
					{
						var baseSetMap = RegionMaps[baseSets[i]];
						if (i != 0)
						{
							endoFinsMap |= baseSetMap & tempMap;
						}

						tempMap |= baseSetMap;
					}
					endoFinsMap &= candMap;

					if (endoFinsMap.Count > _endofinCount)
					{
						continue;
					}

					var elimEndoFinsMap = GridMap.Empty;
					if (endoFinsMap.IsNotEmpty)
					{
						elimEndoFinsMap = endoFinsMap.PeerIntersection & candMap;
						if (elimEndoFinsMap.IsEmpty)
						{
							continue;
						}
					}

					baseSetsMap &= candMap;

					mask = candMap.RowMask << 9 | candMap.ColumnMask << 18 | (int)candMap.BlockMask;
					var coverCombinations = new RegionMap(mask);
					foreach (int region in mask.GetAllSets())
					{
						if (baseRegionMap[region])
						{
							mask &= ~(1 << region);
						}
					}

					// Gather the cover sets that contains the eliminations.
					foreach (int cell in globalElimMap)
					{
						mask &= ~(1 << GetRegion(cell, Row));
						mask &= ~(1 << GetRegion(cell, Column));
						mask &= ~(1 << GetRegion(cell, Block));
					}

					// Then 'mask' contains the regions that eliminations don't lie on.
					var coverCombinationsDoNotContainElim = new RegionMap(mask);
					var coverCombinationsContainElim = coverCombinations - coverCombinationsDoNotContainElim;
					for (int internalSize = 1;
						internalSize <= Min(coverCombinationsContainElim.Count, size);
						internalSize++)
					{
						foreach (int[] comb in coverCombinationsContainElim.ToArray().GetSubsets(internalSize))
						{
							foreach (int[] comb2 in
								coverCombinationsDoNotContainElim.ToArray().GetSubsets(size - internalSize))
							{
								var coverRegionMap = new RegionMap(comb) | new RegionMap(comb2);
								var coverSets = coverRegionMap.ToArray();
								var coverSetMap = GridMap.Empty;
								foreach (int coverSet in coverSets)
								{
									coverSetMap |= RegionMaps[coverSet];
								}

								if (_checkPom && !coverSetMap.Overlaps(globalElimMap))
								{
									// God view: The cover set combination must contain the eliminations
									// that found before.
									continue;
								}

								if ((baseRegionMap.Mask & 0x3FFFF, coverRegionMap.Mask & 0x7FC01FF) is (0, 0)
									|| (baseRegionMap.Mask & 0x7FC01FF, coverRegionMap.Mask & 0x3FFFF) is (0, 0))
								{
									// Basic fish.
									continue;
								}

								var exoFinsMap = baseSetsMap - coverSetMap - endoFinsMap;
								if (exoFinsMap.Count > _exofinCount)
								{
									continue;
								}

								var elimExoFinsMap = GridMap.Empty;
								if (exoFinsMap.IsNotEmpty)
								{
									elimExoFinsMap = exoFinsMap.PeerIntersection & candMap;
									if (elimExoFinsMap.IsEmpty)
									{
										continue;
									}
								}

								var elimMap = (coverSetMap & candMap) - baseSetsMap;
								if (exoFinsMap.IsNotEmpty)
								{
									elimMap &= elimExoFinsMap;
								}
								if (endoFinsMap.IsNotEmpty)
								{
									elimMap &= elimEndoFinsMap;
								}

								GridMap one = GridMap.Empty, two = GridMap.Empty;
								for (int i = 0; i < coverSets.Length; i++)
								{
									var z = RegionMaps[coverSets[i]];
									if (i > 0)
									{
										two |= one & z;
									}

									one |= z;
								}

								two &= candMap & baseSetsMap;
								if (endoFinsMap.IsNotEmpty)
								{
									two &= elimEndoFinsMap;
								}
								if (exoFinsMap.IsNotEmpty)
								{
									two &= elimExoFinsMap;
								}

								if (elimMap.IsEmpty && two.IsEmpty)
								{
									continue;
								}

								var conclusions = new List<Conclusion>();
								foreach (int cell in elimMap)
								{
									// Normal eliminations.
									conclusions.Add(new(Elimination, cell, digit));
								}
								foreach (int cell in two)
								{
									// Cannibalisms.
									conclusions.Add(new(Elimination, cell, digit));
								}

								var regionOffsets = new List<(int, int)>();
								foreach (int baseSet in baseSets)
								{
									regionOffsets.Add((0, baseSet));
								}
								foreach (int coverSet in coverSets)
								{
									regionOffsets.Add((1, coverSet));
								}

								var candidateOffsets = new List<(int, int)>();
								foreach (int cell in exoFinsMap)
								{
									candidateOffsets.Add((1, cell * 9 + digit));
								}
								foreach (int cell in endoFinsMap)
								{
									candidateOffsets.Add((2, cell * 9 + digit));
								}
								foreach (int cell in (baseSetsMap & candMap) - exoFinsMap - endoFinsMap)
								{
									candidateOffsets.Add((0, cell * 9 + digit));
								}

								bool isSashimi = false;
								foreach (int baseSet in baseSets)
								{
									if ((RegionMaps[baseSet] - ((endoFinsMap | exoFinsMap) & candMap)).Count == 1)
									{
										isSashimi = true;
										break;
									}
								}

								accumulator.Add(
									new HobiwanFishTechniqueInfo(
										conclusions,
										views: new[] { new View(null, candidateOffsets, regionOffsets, null) },
										digit,
										baseSets,
										coverSets,
										exofins: exoFinsMap,
										endofins: endoFinsMap,
										isSashimi: (exoFinsMap | endoFinsMap).IsEmpty ? (bool?)null : isSashimi));
							}
						}
					}
				}
			}
		}
	}
}
