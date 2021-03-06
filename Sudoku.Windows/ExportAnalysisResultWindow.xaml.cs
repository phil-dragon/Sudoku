﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Sudoku.Solving;

namespace Sudoku.Windows
{
	/// <summary>
	/// Interaction logic for ExportAnalysisResultWindow.xaml
	/// </summary>
	public partial class ExportAnalysisResultWindow : Window
	{
		/// <summary>
		/// The analysis result.
		/// </summary>
		private readonly AnalysisResult _analysisResult;

		/// <summary>
		/// The internal dictionary of all format characters.
		/// </summary>
		private readonly IDictionary<char, bool> _dic = new Dictionary<char, bool>
		{
			['-'] = false, // Show separators.
			['#'] = false, // Show step indices.
			['@'] = true, // Don't show eliminations.
			['?'] = false, // Show bottleneck.
			['!'] = false, // Show difficulty rating of each step.
			['.'] = true, // Don't show steps after bottleneck.
			['a'] = false, // Show attributes of this puzzle (if exists).
			['b'] = false, // Show magic cells.
			['d'] = false, // Show difficulty details.
			['l'] = true // Show technique steps.
		};


		public ExportAnalysisResultWindow(AnalysisResult analysisResult)
		{
			InitializeComponent();

			// Initialize controls.
			foreach (var control in _gridMain.Children.OfType<CheckBox>())
			{
				control.IsChecked = _dic[control.Tag.ToString()![0]];
			}

			_analysisResult = analysisResult;
		}


		private void ButtonExport_Click(object sender, RoutedEventArgs e)
		{
			var format = new StringBuilder();
			foreach (char key in from Pair in _dic where Pair.Value select Pair.Key)
			{
				format.Append(key);
			}

			_textBoxAnalysisResult.Text = _analysisResult.ToString(format.ToString());
		}

		private void CheckBoxShowSeparators_Click(object sender, RoutedEventArgs e) => _dic['-'] ^= true;

		private void CheckBoxShowStepIndices_Click(object sender, RoutedEventArgs e) => _dic['#'] ^= true;

		private void CheckBoxShowLogic_Click(object sender, RoutedEventArgs e) => _dic['@'] ^= true;

		private void CheckBoxShowBottleneck_Click(object sender, RoutedEventArgs e) => _dic['?'] ^= true;

		private void CheckBoxShowDifficulty_Click(object sender, RoutedEventArgs e) => _dic['!'] ^= true;

		private void CheckboxShowStepsAfterBottleneck_Click(object sender, RoutedEventArgs e) => _dic['.'] ^= true;

		private void CheckBoxShowAttributesOfPuzzle_Click(object sender, RoutedEventArgs e) => _dic['a'] ^= true;

		private void CheckBoxShowMagicCells_Click(object sender, RoutedEventArgs e) => _dic['b'] ^= true;

		private void CheckBoxShowDifficultyDetail_Click(object sender, RoutedEventArgs e) => _dic['d'] ^= true;

		private void CheckBoxShowTechniqueSteps_Click(object sender, RoutedEventArgs e) => _dic['l'] ^= true;
	}
}
