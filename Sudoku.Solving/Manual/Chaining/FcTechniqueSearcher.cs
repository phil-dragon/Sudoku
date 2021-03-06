﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using static Sudoku.Constants.Processings;
using static Sudoku.Constants.RegionLabel;
using static Sudoku.Data.ConclusionType;
using static Sudoku.Solving.Constants.Processings;

namespace Sudoku.Solving.Manual.Chaining
{
	/// <summary>
	/// Encapsulates an <b>forcing chains</b> (<b>FCs</b>) technique searcher.
	/// </summary>
	[TechniqueDisplay(nameof(TechniqueCode.RegionFc))]
	[SearcherProperty(80)]
	public sealed class FcTechniqueSearcher : ChainingTechniqueSearcher
	{
#if DYNAMIC_CHAINING
		/// <summary>
		/// Indicates the information.
		/// </summary>
		private readonly bool _nishio, _multiple, _dynamic;

		/// <summary>
		/// Indicates the level of the dynamic searching.
		/// </summary>
		private readonly int _level;

		/// <summary>
		/// The temporary grid for handling dynamic and multiple forcing chains.
		/// </summary>
		private Grid _tempGrid = null!;
#else
		/// <summary>
		/// Indicates the information.
		/// </summary>
		private readonly bool _multiple;
#endif


		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		public FcTechniqueSearcher() => _multiple = true;

#if DYNAMIC_CHAINING
		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="nishio">Indicates whether the current searcher searches nishio forcing chains.</param>
		/// <param name="multiple">Indicates whether the current searcher searches multiple forcing chains.</param>
		/// <param name="dynamic">Indicates whether the current searcher searches dynamic forcing chains.</param>
		/// <param name="level">Indicates the level of the dynamic searching.</param>
		public FcTechniqueSearcher(bool nishio, bool multiple, bool dynamic, int level) =>
			(_nishio, _multiple, _dynamic, _level) = (nishio, multiple, dynamic, level);
#endif


		/// <inheritdoc/>
		public override void GetAll(IList<TechniqueInfo> accumulator, Grid grid)
		{
			var tempAccumulator = new List<ChainingTechniqueInfo>();
			GetAll(tempAccumulator, grid);

			if (tempAccumulator.Count == 0)
			{
				return;
			}

			accumulator.AddRange(SortInfo(tempAccumulator));
		}

		/// <summary>
		/// Search for chains of each type.
		/// </summary>
		/// <param name="accumulator">The accumulator.</param>
		/// <param name="grid">Thr grid.</param>
		private void GetAll(IList<ChainingTechniqueInfo> accumulator, Grid grid)
		{
			// Iterate on all empty cells.
			foreach (int cell in EmptyMap)
			{
				short mask = grid.GetCandidateMask(cell);
				int count = mask.CountSet();
				if (count > 2)
				{
					// Prepare storage and accumulator for cell eliminations.
					var valueToOn = new Dictionary<int, Set<Node>>();
					var valueToOff = new Dictionary<int, Set<Node>>();
					Set<Node>? cellToOn = null, cellToOff = null;

					// Iterate on all candidates that aren't alone.
					foreach (int digit in mask.GetAllSets())
					{
						var pOn = new Node(cell, digit, true);
						var pOff = new Node(cell, digit, false);
						var onToOn = new Set<Node>();
						var onToOff = new Set<Node>();
#if DYNAMIC_CHAINING
						// Do binary chaining (same candidate either on or off).
						bool doDouble = count >= 3 && !_nishio && _dynamic, doContradiction = _dynamic || _nishio;
						DoBinaryChaining(accumulator, grid, pOn, pOff, onToOn, onToOff, doDouble, doContradiction);
#endif

#if DYNAMIC_CHAINING
						if (!_nishio)
#else
						if (true)
#endif
						{
							// Do region chaining.
#if !DYNAMIC_CHAINING
							onToOn.Add(pOn);
							DoChaining(grid, onToOn, onToOff);
#endif
							DoRegionChaining(accumulator, grid, cell, digit, onToOn, onToOff);
						}

						// Collect results for cell chaining.
						valueToOn.Add(digit, onToOn);
						valueToOff.Add(digit, onToOff);
						if (cellToOn is null/* || cellToOff is null*/)
						{
							cellToOn = new(onToOn);
							cellToOff = new(onToOff);
						}
						else
						{
							cellToOn &= onToOn;
							cellToOff = cellToOff! & onToOff;
						}
					}

#if DYNAMIC_CHAINING
					if (!_nishio)
#else
					if (true)
#endif
					{
						// Do cell eliminations.
						if (count == 2 || _multiple)
						{
							if (cellToOn is not null)
							{
								foreach (var p in cellToOn)
								{
									var hint = CreateCellEliminationHint(grid, cell, p, valueToOn);
									if (hint is not null)
									{
										accumulator.Add(hint);
									}
								}
							}
							if (cellToOff is not null)
							{
								foreach (var p in cellToOff)
								{
									var hint = CreateCellEliminationHint(grid, cell, p, valueToOff);
									if (hint is not null)
									{
										accumulator.Add(hint);
									}
								}
							}
						}
					}
				}
			}
		}

#if DYNAMIC_CHAINING
		private void DoBinaryChaining(
			IList<ChainingTechniqueInfo> accumulator, Grid grid, Node pOn, Node pOff,
			ISet<Node> onToOn, ISet<Node> onToOff, bool doDouble, bool doContradiction)
		{
			Node[]? absurdNodes;
			Set<Node> offToOn = new(), offToOff = new();

			// Circular forcing chains (hypothesis implying its negation)
			// are already covered by cell forcing chains, and are therefore not checked for.

			// Test o is currectly on.
			onToOn.Add(pOn);
			absurdNodes = DoChaining(grid, onToOn, onToOff);
			if (doContradiction && absurdNodes is not null)
			{
				// 'p' cannot hold its value, otherwise it would lead to a contradiction.
				var hint = CreateChainingOffHint(absurdNodes[0], absurdNodes[1], pOn, pOn, true);
				if (hint is not null)
				{
					accumulator.Add(hint);
				}
			}

			// Test p is currently off.
			offToOff.Add(pOff);
			if (doContradiction && absurdNodes is not null)
			{
				// 'p' must hold its value otherwise it would lead to a contradiction.
				var hint = CreateChainingOnHint(absurdNodes[0], absurdNodes[1], pOff, pOff, true);
				if (hint is not null)
				{
					accumulator.Add(hint);
				}
			}

			if (doDouble)
			{
				// Check nodes that must be on in both case.
				foreach (var pFromOn in onToOn)
				{
					if (offToOn.Contains(pFromOn))
					{
						var hint = CreateChainingOnHint(pFromOn, pOn, pFromOn, false);
						if (hint is not null)
						{
							accumulator.Add(hint);
						}
					}
				}

				// Check nodes that must be off in both case.
				foreach (var pFromOn in onToOff)
				{
					if (offToOff.Contains(pFromOn))
					{
						var hint = CreateChainingOffHint(pFromOn, pFromOn, pOff, pFromOn, false);
						if (hint is not null)
						{
							accumulator.Add(hint);
						}
					}
				}
			}
		}
#endif

		private void DoRegionChaining(
			IList<ChainingTechniqueInfo> accumulator, Grid grid, int cell, int digit,
			Set<Node> onToOn, Set<Node> onToOff)
		{
			for (var label = Block; label <= Column; label++)
			{
				int region = GetRegion(cell, label);
				var worthMap = CandMaps[digit] & RegionMaps[region];
				if (worthMap.Count == 2 || _multiple && worthMap.Count > 2)
				{
					// Determine whether we meet this region for the first time.
					if (worthMap.First == cell)
					{
						var posToOn = new Dictionary<int, Set<Node>>();
						var posToOff = new Dictionary<int, Set<Node>>();
						Set<Node> regionToOn = new(), regionToOff = new();

						// Iterate on node positions within the region.
						foreach (int otherCell in worthMap)
						{
							if (otherCell == cell)
							{
								posToOn.Add(otherCell, onToOn);
								posToOff.Add(otherCell, onToOff);
								regionToOn |= onToOn;
								regionToOff |= onToOff;
							}
							else
							{
								var other = new Node(otherCell, digit, true);
								Set<Node> otherToOn = new() { other }, otherToOff = new();

								DoChaining(grid, otherToOn, otherToOff);

								posToOn.Add(otherCell, otherToOn);
								posToOff.Add(otherCell, otherToOff);
								regionToOn &= otherToOn;
								regionToOff &= otherToOff;
							}
						}

						// Gather results.
						foreach (var p in regionToOn)
						{
							var hint = CreateRegionEliminationHint(region, digit, p, posToOn);
							if (hint is not null)
							{
								accumulator.Add(hint);
							}
						}
						foreach (var p in regionToOff)
						{
							var hint = CreateRegionEliminationHint(region, digit, p, posToOff);
							if (hint is not null)
							{
								accumulator.Add(hint);
							}
						}
					}
				}
			}
		}

		private Node[]? DoChaining(Grid grid, ISet<Node> toOn, ISet<Node> toOff)
		{
#if DYNAMIC_CHAINING
			_tempGrid = grid.Clone();
#endif

			var pendingOn = new Set<Node>(toOn);
			var pendingOff = new Set<Node>(toOff);
			while (pendingOn.Count != 0 || pendingOff.Count != 0)
			{
				if (pendingOn.Count != 0)
				{
					var p = pendingOn.Remove();

					var makeOff = GetOnToOff(
						grid,
						p,
#if DYNAMIC_CHAINING
						!_nishio);
#else
						true);
#endif
					foreach (var pOff in makeOff)
					{
						var pOn = new Node(pOff.Cell, pOff.Digit, true); // Conjugate
						if (toOn.Contains(pOn))
						{
							// Contradiction found.
							return new[] { pOn, pOff }; // Cannot be both on and off at the same time.
						}
						else if (!toOff.Contains(pOff))
						{
							// Not processed yet.
							toOff.Add(pOff);
							pendingOff.Add(pOff);
						}
					}
				}
				else
				{
					var p = pendingOff.Remove();

					var makeOn = GetOffToOn(
						grid,
						p,
#if DYNAMIC_CHAINING
						!_nishio,
#else
						true,
#endif
						true);
#if DYNAMIC_CHAINING
					if (_dynamic)
					{
						p.Off(grid);
					}
#endif

					foreach (var pOn in makeOn)
					{
						var pOff = new Node(pOn.Cell, pOn.Digit, false); // Conjugate.
						if (toOff.Contains(pOff))
						{
							// Contradiction found.
							return new[] { pOn, pOff }; // Cannot be both on and off at the same time.
						}
						else if (!toOn.Contains(pOn))
						{
							// Not processed yet.
							toOn.Add(pOn);
							pendingOn.Add(pOn);
						}
					}
				}

#if DYNAMIC_CHAINING
				if (pendingOn.Count != 0 && pendingOff.Count != 0 && _level > 0)
				{
					foreach (var pOff in GetAdvanceedNodes(grid, _tempGrid, toOff))
					{
						if (!toOff.Contains(pOff))
						{
							// Not processed yet.
							toOff.Add(pOff);
							pendingOff.Add(pOff);
						}
					}
				}
#endif
			}

#if DYNAMIC_CHAINING
			// Recover.
			grid = _tempGrid;
#endif

			return null;
		}

#if DYNAMIC_CHAINING
		private ChainingTechniqueInfo? CreateChainingOnHint(
			Node destOn, Node destOff, Node source, Node target, bool isAbsurd) =>
			new BinaryChainingTechniqueInfo(
				conclusions: new List<Conclusion> { new(Assignment, target._cell, target.Digit) },
				views: new[]
				{
					new View(
						cellOffsets: null,
						candidateOffsets: _,
						regionOffsets: null,
						links: _)
				},
				source,
				destOn,
				destOff,
				isAbsurd,
				isNishio: _nishio);

		private ChainingTechniqueInfo? CreateChainingOffHint(
			Node destOn, Node destOff, Node source, Node target, bool isAbsurd) =>
			new BinaryChainingTechniqueInfo(
				conclusions: new List<Conclusion> { new(Elimination, target._cell, target.Digit) },
				views: new[]
				{
					new View(
						cellOffsets: null,
						candidateOffsets: _,
						regionOffsets: null,
						links: _)
				},
				source,
				destOn,
				destOff,
				isAbsurd,
				isNishio: _nishio);
#endif

		private ChainingTechniqueInfo? CreateCellEliminationHint(
			Grid grid, int sourceCell, Node target, IReadOnlyDictionary<int, Set<Node>> outcomes)
		{
			// Build removable nodes.
			var conclusions = new List<Conclusion>
			{
				new(target.IsOn ? Assignment : Elimination, target.Cell, target.Digit)
			};

			// Build chains.
			var chains = new Dictionary<int, Node>();
			foreach (int digit in grid.GetCandidates(sourceCell))
			{
				// Get the node that contains the same cell, digit and isOn property.
				var valueTarget = outcomes[digit][target];
				chains.Add(digit, valueTarget);
			}

			// Get views.
			var views = new List<View>();
			var globalCandidates = new List<(int, int)>();
			var globalLinks = new List<Link>();
			foreach (var (digit, node) in chains)
			{
				var candidateOffsets = new List<(int, int)>(GetCandidateOffsets(node)) { (2, sourceCell * 9 + digit) };
				var links = GetLinks(node, true);
				views.Add(new(new[] { (0, sourceCell) }, candidateOffsets, null, links));
				globalCandidates.AddRange(candidateOffsets);
				globalLinks.AddRange(links);
			}

			// Insert the global view at head.
			views.Insert(0, new(new[] { (0, sourceCell) }, globalCandidates, null, globalLinks));

			return new CellChainingTechniqueInfo(conclusions, views, sourceCell, chains);
		}

		private ChainingTechniqueInfo? CreateRegionEliminationHint(
			int region, int digit, Node target, IDictionary<int, Set<Node>> outcomes)
		{
			// Build removable nodes.
			var conclusions = new List<Conclusion>
			{
				new(target.IsOn ? Assignment : Elimination, target.Cell, target.Digit)
			};

			// Build chains.
			var chains = new Dictionary<int, Node>();
			var map = RegionMaps[region] & CandMaps[digit];
			foreach (int cell in map)
			{
				// Get the node that contains the same cell, digit and isOn property.
				var posTarget = outcomes[cell][target];
				chains.Add(cell, posTarget);
			}

			// Get views.
			var views = new List<View>();
			var globalCandidates = new List<(int, int)>();
			var globalLinks = new List<Link>();
			foreach (var (cell, node) in chains)
			{
				var candidateOffsets = new List<(int, int)>(GetCandidateOffsets(node)) { (2, cell * 9 + digit) };
				var links = GetLinks(node, true);
				views.Add(new(null, candidateOffsets, new[] { (0, region) }, links));
				globalCandidates.AddRange(candidateOffsets);
				globalLinks.AddRange(links);
			}

			views.Insert(0, new(null, globalCandidates, new[] { (0, region) }, globalLinks));

			return new RegionChainingTechniqueInfo(conclusions, views, region, digit, chains);
		}
	}
}
