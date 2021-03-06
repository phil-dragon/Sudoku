﻿#nullable disable warnings
#pragma warning disable CA1036

using System;
using System.Collections.Generic;
using Sudoku.Constants;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using Sudoku.Solving.Annotations;

namespace Sudoku.Solving.Manual.Uniqueness.Rects
{
	/// <summary>
	/// Provides a usage of <b>unique rectangle</b> (UR) or
	/// <b>avoidable rectangle</b> (AR) technique.
	/// </summary>
	public abstract class UrTechniqueInfo : UniquenessTechniqueInfo, IComparable<UrTechniqueInfo>
	{
		/// <include file='SolvingDocComments.xml' path='comments/constructor[@type="TechniqueInfo"]'/>
		/// <param name="typeCode">The type code.</param>
		/// <param name="digit1">The digit 1.</param>
		/// <param name="digit2">The digit 2.</param>
		/// <param name="cells">All cells.</param>
		/// <param name="isAr">Indicates whether the structure is an AR.</param>
		protected UrTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			UrTypeCode typeCode, int digit1, int digit2, int[] cells, bool isAr) : base(conclusions, views) =>
			(Digit1, Digit2, Cells, IsAr, TypeCode) = (digit1, digit2, cells, isAr, typeCode);


		/// <summary>
		/// Indicates the UR type code.
		/// </summary>
		public UrTypeCode TypeCode { get; }

		/// <summary>
		/// Indicates the digit 1.
		/// </summary>
		public int Digit1 { get; }

		/// <summary>
		/// Indicates the digit 2.
		/// </summary>
		public int Digit2 { get; }

		/// <summary>
		/// Indicates the cells.
		/// </summary>
		public int[] Cells { get; }

		/// <summary>
		/// Indicates the current structure is UR or AR.
		/// </summary>
		public bool IsAr { get; }

		/// <inheritdoc/>
		public sealed override string Name => base.Name;

		/// <inheritdoc/>
		public sealed override bool ShowDifficulty => true;

		/// <inheritdoc/>
		public sealed override TechniqueCode TechniqueCode =>
			AliasAttribute.Convert<UrTypeCode, TechniqueCode>(TypeCode)!.Value;

		/// <inheritdoc/>
		public override string ToString()
		{
			int d1 = Digit1 + 1;
			int d2 = Digit2 + 1;
			string cellsStr = new CellCollection(Cells).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			string? additional = GetAdditional();
			return
				$"{Name}: {d1}, {d2} in {cellsStr}{(additional is null ? string.Empty : $" with {additional}")} => " +
				$"{elimStr}";
		}

		/// <summary>
		/// Get additional string.
		/// </summary>
		/// <returns>The additional string.</returns>
		protected abstract string? GetAdditional();

		/// <inheritdoc/>
		int IComparable<UrTechniqueInfo>.CompareTo(UrTechniqueInfo other) =>
			Math.Sign(TypeCode.CompareTo(other.TypeCode)) switch
			{
				0 => new GridMap(Cells).CompareTo(other.Cells) switch
				{
					0 => Math.Sign((Digit1 * 9 + Digit2).CompareTo(other.Digit1 * 9 + other.Digit2)) switch
					{
						0 => TypeCode.CompareTo(other.TypeCode),
						1 => 1,
						-1 => -1,
						_ => throw Throwings.ImpossibleCase
					},
					1 => 1,
					-1 => -1,
					_ => throw Throwings.ImpossibleCase
				},
				1 => 1,
				-1 => -1,
				_ => throw Throwings.ImpossibleCase
			};
	}
}
