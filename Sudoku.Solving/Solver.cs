﻿using System.Threading.Tasks;
using Sudoku.Data.Meta;

namespace Sudoku.Solving
{
	/// <summary>
	/// Indicates an instance used for solving a sudoku puzzle.
	/// </summary>
	public abstract class Solver
	{
		/// <summary>
		/// The name of this solver.
		/// </summary>
		public abstract string SolverName { get; }


		/// <summary>
		/// Solves the specified puzzle.
		/// </summary>
		/// <param name="grid">The puzzle.</param>
		/// <returns>
		/// An <see cref="AnalysisResult"/> displaying all information of solving.
		/// </returns>
		public abstract AnalysisResult Solve(Grid grid);

		/// <summary>
		/// Solves the specified puzzle asynchronizedly.
		/// </summary>
		/// <param name="grid">The puzzle.</param>
		/// <param name="continueOnCapturedContext">
		/// <c>true</c> to attempt to marshal the continuation back to
		/// the original context captured; otherwise, <c>false</c>.
		/// </param>
		/// <returns>The solving task.</returns>
		public virtual async Task<AnalysisResult> SolveAsync(Grid grid, bool continueOnCapturedContext) =>
			await Task.Run(() => Solve(grid)).ConfigureAwait(continueOnCapturedContext);
	}
}
