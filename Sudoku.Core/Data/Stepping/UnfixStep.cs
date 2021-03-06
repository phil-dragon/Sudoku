﻿namespace Sudoku.Data.Stepping
{
	/// <summary>
	/// Encapsulates an unfix step.
	/// </summary>
	public sealed class UnfixStep : Step
	{
		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="allCells">All cells.</param>
		public UnfixStep(GridMap allCells) => AllCells = allCells;


		/// <summary>
		/// Indicates all cells.
		/// </summary>
		public GridMap AllCells { get; }


		/// <inheritdoc/>
		public override void DoStepTo(UndoableGrid grid)
		{
			foreach (int cell in AllCells)
			{
				// To prevent the event re-invoke.
				ref short mask = ref grid._masks[cell];
				mask = (short)((int)CellStatus.Modifiable << 9 | mask & Grid.MaxCandidatesMask);
			}
		}

		/// <inheritdoc/>
		public override void UndoStepTo(UndoableGrid grid)
		{
			foreach (int cell in AllCells)
			{
				// To prevent the event re-invoke.
				ref short mask = ref grid._masks[cell];
				mask = (short)((int)CellStatus.Given << 9 | mask & Grid.MaxCandidatesMask);
			}
		}
	}
}
