﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using Sudoku.Solving.Checking;
using Sudoku.Solving.Manual.Chaining;
using static Sudoku.Data.ConclusionType;
using static Sudoku.Solving.Constants.Processings;

namespace Sudoku.Solving.Manual.Uniqueness.Bugs
{
	/// <summary>
	/// Encapsulates a <b>bivalue universal grave multiple</b> (BUG + n) with forcing chains
	/// technique searcher.
	/// </summary>
	[TechniqueDisplay(nameof(TechniqueCode.BugMultipleFc))]
	[SearcherProperty(60)]
	public sealed class BugMultipleWithFcTechniqueSearcher : UniquenessTechniqueSearcher
	{
		/// <inheritdoc/>
		public override void GetAll(IList<TechniqueInfo> accumulator, Grid grid)
		{
			var trueCandidates = new BugChecker(grid).TrueCandidates;
			if (trueCandidates.Count <= 1)
			{
				return;
			}

			CheckMultipleWithForcingChains(accumulator, grid, trueCandidates);
		}

		/// <summary>
		/// Check BUG + n with forcing chains.
		/// </summary>
		/// <param name="accumulator">The result list.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		private void CheckMultipleWithForcingChains(
			IList<TechniqueInfo> accumulator, Grid grid, IReadOnlyList<int> trueCandidates)
		{
			var tempAccumulator = new List<BugMultipleWithFcTechniqueInfo>();

			// Prepare storage and accumulator for cell eliminations.
			var valueToOn = new Dictionary<int, Set<Node>>();
			var valueToOff = new Dictionary<int, Set<Node>>();
			Set<Node>? cellToOn = null, cellToOff = null;
			foreach (int candidate in trueCandidates)
			{
				int cell = candidate / 9, digit = candidate % 9;
				var onToOn = new Set<Node>();
				var onToOff = new Set<Node>();

				onToOn.Add(new(cell, digit, true));
				DoChaining(grid, onToOn, onToOff);

				// Collect results for cell chaining.
				valueToOn.Add(candidate, onToOn);
				valueToOff.Add(candidate, onToOff);
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

			// Do cell eliminations.
			if (cellToOn is not null)
			{
				foreach (var p in cellToOn)
				{
					var hint = CreateEliminationHint(trueCandidates, p, valueToOn);
					if (hint is not null)
					{
						tempAccumulator.Add(hint);
					}
				}
			}
			if (cellToOff is not null)
			{
				foreach (var p in cellToOff)
				{
					var hint = CreateEliminationHint(trueCandidates, p, valueToOff);
					if (hint is not null)
					{
						tempAccumulator.Add(hint);
					}
				}
			}

			tempAccumulator.Sort((i1, i2) => i1.Complexity.CompareTo(i2.Complexity));
			accumulator.AddRange(tempAccumulator);
		}

		/// <summary>
		/// Do chaining. This method is only called by
		/// <see cref="CheckMultipleWithForcingChains(IList{TechniqueInfo}, Grid, IReadOnlyList{int})"/>.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="toOn">All nodes to on.</param>
		/// <param name="toOff">All nodes to off.</param>
		/// <returns>The result nodes.</returns>
		/// <seealso cref="CheckMultipleWithForcingChains(IList{TechniqueInfo}, Grid, IReadOnlyList{int})"/>
		private static Node[]? DoChaining(Grid grid, ISet<Node> toOn, ISet<Node> toOff)
		{
			var pendingOn = new Set<Node>(toOn);
			var pendingOff = new Set<Node>(toOff);
			while (pendingOn.Count != 0 || pendingOff.Count != 0)
			{
				if (pendingOn.Count != 0)
				{
					var p = pendingOn.Remove();

					var makeOff = ChainingTechniqueSearcher.GetOnToOff(grid, p, true);
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

					var makeOn = ChainingTechniqueSearcher.GetOffToOn(grid, p, true, true);

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
			}

			return null;
		}

		/// <summary>
		/// Create the elimination hint. This method is only called by
		/// <see cref="CheckMultipleWithForcingChains(IList{TechniqueInfo}, Grid, IReadOnlyList{int})"/>.
		/// </summary>
		/// <param name="trueCandidates">The true candidates.</param>
		/// <param name="target">The target node.</param>
		/// <param name="outcomes">All outcomes.</param>
		/// <returns>The result information instance.</returns>
		/// <seealso cref="CheckMultipleWithForcingChains(IList{TechniqueInfo}, Grid, IReadOnlyList{int})"/>
		private static BugMultipleWithFcTechniqueInfo? CreateEliminationHint(
			IReadOnlyList<int> trueCandidates, Node target, IReadOnlyDictionary<int, Set<Node>> outcomes)
		{
			// Build removable nodes.
			var conclusions = new List<Conclusion>
			{
				new(target.IsOn ? Assignment : Elimination, target.Cell, target.Digit)
			};

			// Build chains.
			var chains = new Dictionary<int, Node>();
			foreach (int candidate in trueCandidates)
			{
				// Get the node that contains the same cell, digit and isOn property.
				var valueTarget = outcomes[candidate][target];
				chains.Add(candidate, valueTarget);
			}

			// Get views.
			var views = new List<View>();
			var globalCandidates = new List<(int, int)>();
			var globalLinks = new List<Link>();
			foreach (var (candidate, node) in chains)
			{
				var candidateOffsets = new List<(int, int)>(GetCandidateOffsets(node)) { (2, candidate) };
				var links = new List<Link>(GetLinks(node, true));
				views.Add(new(null, candidateOffsets, null, links));
				globalCandidates.AddRange(candidateOffsets);
				globalLinks.AddRange(links);
			}

			views.Insert(0, new(null, globalCandidates, null, globalLinks));

			return new BugMultipleWithFcTechniqueInfo(conclusions, views, candidates: trueCandidates, chains);
		}
	}
}
