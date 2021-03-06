﻿using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Documents;
using Sudoku.Windows.Constants;

namespace Sudoku.Windows
{
	/// <summary>
	/// Interaction logic for <c>ThankForWindow.xaml</c>.
	/// </summary>
	public partial class SpecialThanksWindow : Window
	{
		/// <include file='..\GlobalDocComments.xml' path='comments/defaultConstructor'/>
		public SpecialThanksWindow() => InitializeComponent();


		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Hyperlink link)
			{
				try
				{
					Process.Start(link.NavigateUri.AbsoluteUri);
				}
				catch (Exception ex)
				{
					Messagings.ShowExceptionMessage(ex);
				}
			}
		}
	}
}
