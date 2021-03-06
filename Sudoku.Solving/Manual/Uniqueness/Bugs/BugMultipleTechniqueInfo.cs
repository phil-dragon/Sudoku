﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using Sudoku.Windows;

namespace Sudoku.Solving.Manual.Uniqueness.Bugs
{
	/// <summary>
	/// Provides a usage of <b>BUG + n</b> technique.
	/// </summary>
	public sealed class BugMultipleTechniqueInfo : UniquenessTechniqueInfo
	{
		/// <summary>
		/// The table of extra difficulty values.
		/// </summary>
		private static readonly decimal[] DifficultyExtra =
		{
			.1M, .2M, .2M, .3M, .3M, .3M, .4M, .4M, .4M, .4M,
			.5M, .5M, .5M, .5M, .5M, .6M, .6M, .6M
		};


		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="candidates">All candidates.</param>
		public BugMultipleTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views, IReadOnlyList<int> candidates)
			: base(conclusions, views) => Candidates = candidates;


		/// <summary>
		/// Indicates all candidates used.
		/// </summary>
		public IReadOnlyList<int> Candidates { get; }

		/// <inheritdoc/>
		public override string Name => $"{Resources.GetValue("Bug")} + {Candidates.Count}";

		/// <inheritdoc/>
		public override decimal Difficulty => 5.7M + DifficultyExtra[Candidates.Count - 1];

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode => TechniqueCode.BugMultiple;


		/// <inheritdoc/>
		public override string ToString()
		{
			string candsStr = new CandidateCollection(Candidates).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: True candidates: {candsStr} => {elimStr}";
		}
	}
}
