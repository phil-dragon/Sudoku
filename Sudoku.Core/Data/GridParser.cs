﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Sudoku.Extensions;
using static Sudoku.Data.CellStatus;
using static Sudoku.Data.GridParsingOption;
using Sudoku.Constants;

namespace Sudoku.Data
{
	/// <summary>
	/// Encapsulates a grid parser.
	/// </summary>
	[DebuggerStepThrough]
	internal sealed class GridParser
	{
		/// <summary>
		/// Initializes an instance with parsing data.
		/// </summary>
		/// <param name="parsingValue">The string to parse.</param>
		public GridParser(string parsingValue) => ParsingValue = parsingValue;

		/// <summary>
		/// Initializes an instance with parsing data and a bool value
		/// indicating whether the parsing operation should use compatible mode.
		/// </summary>
		/// <param name="parsingValue">The string to parse.</param>
		/// <param name="compatibleFirst">
		/// Indicates whether the parsing operation should use compatible mode to check
		/// PM grid. See <see cref="CompatibleFirst"/> to learn more.
		/// </param>
		/// <seealso cref="CompatibleFirst"/>
		public GridParser(string parsingValue, bool compatibleFirst) =>
			(ParsingValue, CompatibleFirst) = (parsingValue, compatibleFirst);


		/// <summary>
		/// The value to parse.
		/// </summary>
		public string ParsingValue { get; private set; }

		/// <summary>
		/// Indicates whether the parser will change the execution order of PM grid.
		/// If the value is <see langword="true"/>, the parser will check compatible one
		/// first, and then check recommended parsing plan ('<c>&lt;d&gt;</c>' and '<c>*d*</c>').
		/// </summary>
		public bool CompatibleFirst { get; }


		/// <summary>
		/// To parse the value.
		/// </summary>
		/// <returns>The grid.</returns>
		/// <exception cref="ArgumentException">Throws when failed to parse.</exception>
		public Grid Parse() =>
			OnParsingSimpleTable()
				?? OnParsingSimpleMultilineGrid()
				?? (CompatibleFirst ? OnParsingPencilMarked(true) : OnParsingPencilMarked(false))
				?? (CompatibleFirst ? OnParsingPencilMarked(false) : OnParsingPencilMarked(true))
				?? OnParsingSusser()
				?? OnParsingExcel()
				?? (CompatibleFirst ? OnParsingSukaku(true) : OnParsingSukaku(false))
				?? (CompatibleFirst ? OnParsingSukaku(false) : OnParsingSukaku(true))
				?? throw Throwings.ParsingError<Grid>(nameof(ParsingValue));

		/// <summary>
		/// To parse the value with a specified grid parsing type.
		/// </summary>
		/// <param name="gridParsingOption">A specified parsing type.</param>
		/// <returns>The grid.</returns>
		/// <exception cref="ArgumentException">
		/// Throws when failed to parse.
		/// </exception>
		public Grid Parse(GridParsingOption gridParsingOption) =>
			new Dictionary<GridParsingOption, Func<Grid?>>
			{
				[Susser] = OnParsingSusser,
				[Table] = OnParsingSimpleMultilineGrid,
				[PencilMarked] = () => OnParsingPencilMarked(false),
				[PencilMarkedTreatSingleAsGiven] = () => OnParsingPencilMarked(true),
				[SimpleTable] = OnParsingSimpleTable,
				[Sukaku] = () => OnParsingSukaku(false),
				[SukakuSingleLine] = () => OnParsingSukaku(true),
				[Excel] = OnParsingExcel
			}[gridParsingOption]() ?? throw Throwings.ParsingError<Grid>(nameof(ParsingValue));

		/// <summary>
		/// Parse the value using multi-line simple grid (without any candidates).
		/// </summary>
		/// <returns>The result.</returns>
		private Grid? OnParsingSimpleMultilineGrid()
		{
			string[] matches = ParsingValue.MatchAll(RegularExpressions.DigitOrEmptyCell);
			int length = matches.Length;
			if (length is not (81 or 85))
			{
				// Subtle grid outline will bring 2 '.'s on first line of the grid.
				return null;
			}

			var result = Grid.Empty.Clone();
			for (int i = 0; i < 81; i++)
			{
				string currentMatch = matches[length - 81 + i];
				switch (currentMatch.Length)
				{
					case 1:
					{
						char match = currentMatch[0];
						if (match is not ('.' or '0'))
						{
							result[i] = match - '1';
							result.SetStatus(i, Given);
						}

						break;
					}
					case 2:
					{
						char match = currentMatch[1];
						if (match is '.' or '0')
						{
							// '+0' or '+.'? Invalid combination.
							return null;
						}

						result[i] = match - '1';
						result.SetStatus(i, Modifiable);

						break;
					}
					default:
					{
						// The sub-match contains more than 2 characters or empty string,
						// which is invalid.
						return null;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Parse the Excel format.
		/// </summary>
		/// <returns>The result.</returns>
		private Grid? OnParsingExcel()
		{
			if (!ParsingValue.Contains('\t'))
			{
				return null;
			}

			string[] values = ParsingValue.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			if (values.Length != 9)
			{
				return null;
			}

			var sb = new StringBuilder();
			foreach (string value in values)
			{
				string[] digitStrings = value.Split(new[] { '\t' });
				foreach (string digitString in digitStrings)
				{
					sb.Append(string.IsNullOrEmpty(digitString) ? '.' : digitString[0]);
				}
			}

			return Grid.Parse(sb.ToString());
		}

		/// <summary>
		/// Parse the PM grid.
		/// </summary>
		/// <param name="treatSingleValueAsGiven">
		/// The value indicating whether the parsing should treat
		/// the modifiable values as given ones.
		/// </param>
		/// <returns>The result.</returns>
		private Grid? OnParsingPencilMarked(bool treatSingleValueAsGiven)
		{
			// Older regular expression pattern:
			// string[] matches = ParsingValue.MatchAll(RegularExpressions.PmGridUnit_Old);
			string[] matches = ParsingValue.MatchAll(RegularExpressions.PmGridUnit);
			if (matches.Length != 81)
			{
				return null;
			}

			var result = Grid.Empty.Clone();
			for (int offset = 0; offset < 81; offset++)
			{
				string s = matches[offset].Reserve(RegularExpressions.Digit);
				int length = s.Length;
				if (length > 9)
				{
					// More than 9 characters.
					return null;
				}

				if (treatSingleValueAsGiven)
				{
					// This options means that all characters matched will
					// contain only digit characters.
					// Check the length is 1 or not.
					// The string has only one character, which means that
					// the digit character is the given of the cell.
					if (length == 1)
					{
						// To assign the value, and to trigger the event
						// to modify all information of peers.
						result[offset] = s[0] - '1';
						result.SetStatus(offset, Given);
					}
					else
					{
						bool[] series = DefaultCheckingArray;
						foreach (char c in s)
						{
							series[c - '1'] = false;
						}
						for (int digit = 0; digit < 9; digit++)
						{
							result[offset, digit] = series[digit];
						}
					}
				}
				else if (s.Contains('<'))
				{
					// All values will be treated as normal characters:
					// '<digit>', '*digit*' and 'candidates'.

					// Givens.
					if (length == 3)
					{
						char c = s[1];
						if (c is >= '1' and <= '9')
						{
							result[offset] = c - '1';
							result.SetStatus(offset, Given);
						}
						else
						{
							// Illegal characters found.
							return null;
						}
					}
					else
					{
						// The length is not 3.
						return null;
					}
				}
				else if (s.Contains('*'))
				{
					// Modifiables.
					if (length == 3)
					{
						char c = s[1];
						if (c is >= '1' and <= '9')
						{
							result[offset] = c - '1';
							result.SetStatus(offset, Modifiable);
						}
						else
						{
							// Illegal characters found.
							return null;
						}
					}
					else
					{
						// The length is not 3.
						return null;
					}
				}
				else if (s.SatisfyPattern(RegularExpressions.PmGridCandidates))
				{
					// Candidates.
					// Here do not need to check the length of the string,
					// and also all characters are digit characters.
					bool[] series = DefaultCheckingArray;
					foreach (char c in s)
					{
						series[c - '1'] = false;
					}
					for (int digit = 0; digit < 9; digit++)
					{
						result[offset, digit] = series[digit];
					}
				}
				else
				{
					// All conditions cannot match.
					return null;
				}
			}

			return result;
		}

		/// <summary>
		/// Parse the simple table format string (Sudoku explainer format).
		/// </summary>
		/// <returns>The grid.</returns>
		private Grid? OnParsingSimpleTable()
		{
			string? match = ParsingValue.Match(RegularExpressions.SimpleTable);
			if (match is null)
			{
				return null;
			}

			// Remove all '\r' and '\n'-s.
			var sb = new StringBuilder();
			foreach (char c in from EachChar in match where EachChar is not ('\r' or '\n') select EachChar)
			{
				sb.Append(c);
			}

			ParsingValue = sb.ToString();
			return OnParsingSusser();
		}

		/// <summary>
		/// Parse the susser format string.
		/// </summary>
		/// <returns>The result.</returns>
		private Grid? OnParsingSusser()
		{
			string? match = ParsingValue.Match(RegularExpressions.Susser);
			if (match is not { Length: <= 405 })
			{
				return null;
			}

			// Step 1: fills all digits.
			var result = Grid.Empty.Clone();
			int i = 0, length = match.Length;
			for (int realPos = 0; i < length && match[i] != ':'; realPos++)
			{
				char c = match[i];
				switch (c)
				{
					case '+':
					{
						// Plus sign means the character after it is a digit,
						// which is modifiable value in the grid in its corresponding position.
						if (i < length - 1)
						{
							char nextChar = match[i + 1];
							if (nextChar is >= '1' and <= '9')
							{
								// Set value.
								// Note that the subtracter is character '1', not '0'.
								result[realPos] = nextChar - '1';

								// Add 2 on iteration variable to skip 2 characters
								// (A plus sign '+' and a digit).
								i += 2;
							}
							else
							{
								// Why isn't the character a digit character?
								// Throws an exception to report this case.
								//throw new ArgumentException(
								//	message: $"Argument cannot be parsed and converted to target type {typeof(Grid)}.",
								//	innerException: new ArgumentException(
								//		message: "The value after the specified argument is not a digit.",
								//		paramName: nameof(i)));
								return null;
							}
						}
						else
						{
							return null;
						}

						break;
					}
					case '.' or '0':
					{
						// A placeholder.
						// Do nothing but only move 1 step forward.
						i++;

						break;
					}
					case >= '1' and <= '9':
					{
						// Is a digit character.
						// Digits are representing given values in the grid.
						// Not the plus sign, but a placeholder '0' or '.'.
						// Set value.
						result[realPos] = c - '1';

						// Set the cell status as 'CellStatus.Given'.
						// If the code below does not make sense to you,
						// you can see the comments in method 'OnParsingSusser(string)'
						// to know the meaning also.
						result.SetStatus(realPos, Given);

						// Finally moves 1 step forward.
						i++;

						break;
					}
					default:
					{
						// Other invalid characters. Throws an exception.
						//throw Throwing.ParsingError<Grid>(nameof(ParsingValue));
						return null;
					}
				}
			}

			// Step 2: eliminates candidates if exist.
			// If we have met the colon sign ':', this loop would not be executed.
			if (match.Match(RegularExpressions.ExtendedSusserEliminations) is string elimMatch)
			{
				string[] elimBlocks = elimMatch.MatchAll(RegularExpressions.ThreeDigitsCandidate);
				foreach (string elimBlock in elimBlocks)
				{
					// Set the candidate true value to eliminate the candidate.
					result[(elimBlock[1] - '1') * 9 + elimBlock[2] - '1', elimBlock[0] - '1'] = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Parse the sukaku format string.
		/// </summary>
		/// <returns>The result.</returns>
		private Grid? OnParsingSukaku(bool compatibleFirst)
		{
			if (compatibleFirst)
			{
				if (ParsingValue.Length < 729)
				{
					return null;
				}

				var result = Grid.Empty.Clone();
				for (int i = 0; i < 729; i++)
				{
					char c = ParsingValue[i];
					if (c is not (>= '0' and <= '9' or '.'))
					{
						return null;
					}

					if (c is '0' or '.')
					{
						result[i / 9, i % 9] = true;
					}
				}

				return result;
			}
			else
			{
				// You cannot use 'is string[] matches', but 'is var matches'.
				// Don't ask me why.
				if (ParsingValue.MatchAll(RegularExpressions.PmGridCandidatesUnit) is var matches
					&& matches is { Length: not 81 })
				{
					return null;
				}

				#region Obsolete code
				//string[] matches = ParsingValue.MatchAll(RegularExpressions.PmGridCandidatesUnit);
				//if (matches.Length != 81)
				//{
				//	return null;
				//}
				#endregion

				var result = Grid.Empty.Clone();
				for (int offset = 0; offset < 81; offset++)
				{
					string s = matches[offset].Reserve(RegularExpressions.Digit);
					if (s.Length > 9)
					{
						// More than 9 characters.
						return null;
					}

					bool[] series = DefaultCheckingArray;
					foreach (char c in s)
					{
						series[c - '1'] = false;
					}
					for (int digit = 0; digit < 9; digit++)
					{
						result[offset, digit] = series[digit];
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Get an array of default values in checking.
		/// </summary>
		/// <returns>The array of <see cref="bool"/> values.</returns>
		/// <remarks>
		/// Here must use method instead of property or field.
		/// Because the return value should be mutable.
		/// </remarks>
		private static bool[] DefaultCheckingArray => new[] { true, true, true, true, true, true, true, true, true };
	}
}
