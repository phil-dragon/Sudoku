﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Sudoku.Data.CellStatus;

namespace Sudoku.Data.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="Grid"/>.
	/// </summary>
	/// <seealso cref="Grid"/>
	[DebuggerStepThrough]
	public static class GridEx
	{
		/// <summary>
		/// <para>
		/// Indicates whether the specified grid contains the digit in the specified cell.
		/// </para>
		/// <para>
		/// The return value will be <see langword="true"/> if and only if
		/// the cell is empty and contains that digit.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="cellOffset">The cell offset.</param>
		/// <param name="digit">The digit.</param>
		/// <returns>
		/// A <see cref="bool"/>? value indicating that.
		/// </returns>
		/// <remarks>
		/// The cases of the return value are below:
		/// <list type="table">
		/// <item>
		/// <term><c><see langword="true"/></c></term>
		/// <description>
		/// The cell is an empty cell <b>and</b> contains the specified digit.
		/// </description>
		/// </item>
		/// <item>
		/// <term><c><see langword="false"/></c></term>
		/// <description>
		/// The cell is an empty cell <b>but doesn't</b> contain the specified digit.
		/// </description>
		/// </item>
		/// <item>
		/// <term><c><see langword="null"/></c></term>
		/// <description>The cell is <b>not</b> an empty cell.</description>
		/// </item>
		/// </list>
		/// </remarks>
		/// <example>
		/// Note that the method will return a <see cref="bool"/>?, so you should use the code
		/// <code>grid.Exists(candidate) is true</code>
		/// or
		/// <code>grid.Exists(candidate) == true</code>
		/// to decide whether a condition is true.
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool? Exists(this Grid @this, int cellOffset, int digit) =>
			@this.GetStatus(cellOffset) == Empty ? !@this[cellOffset, digit] : (bool?)null;
	}
}
