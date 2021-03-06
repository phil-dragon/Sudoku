﻿using System.Windows.Controls;

namespace Sudoku.Windows
{
	partial class MainWindow
	{
		private void TabControlInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ReferenceEquals(_tabControlInfo.SelectedItem, _tabItemDrawing))
			{
				_currentViewIndex = -1;
				_currentTechniqueInfo = null;
				_currentPainter.View = null;

				UpdateImageGrid();
			}
		}
	}
}
