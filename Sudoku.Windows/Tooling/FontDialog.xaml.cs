﻿using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sudoku.Drawing.Extensions;
using static System.Drawing.StringAlignment;
using static System.Drawing.Text.TextRenderingHint;
using static System.Windows.Input.Key;
using DFontStyle = System.Drawing.FontStyle;

namespace Sudoku.Windows.Tooling
{
	/// <summary>
	/// Interaction logic for <c>FontDialog.xaml</c>.
	/// </summary>
	public partial class FontDialog : Window, IDisposable
	{
		/// <summary>
		/// Indicates the sample text.
		/// </summary>
		private const string SampleText = "0123456789";


		/// <summary>
		/// The internal brush.
		/// </summary>
		private readonly Brush _brush = new SolidBrush(Color.Black);


		/// <summary>
		/// Indicates whether the specified instance has been already disposed.
		/// </summary>
		private bool _isDisposed;


		public FontDialog() => InitializeComponent();


		/// <summary>
		/// The finalizer of this class.
		/// </summary>
		~FontDialog() => Dispose(disposing: false);


		public Font SelectedFont { get; private set; } = null!;


		/// <inheritdoc/>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					// Dispose managed state (managed objects).
					_brush.Dispose();
				}

				// Free unmanaged resources (unmanaged objects) and override finalizer.
				// Set large fields to null.
				_isDisposed = true;
			}
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			_listBoxFonts.ItemsSource = from Font in new InstalledFontCollection().Families select Font.Name;
			_listBoxFonts.SelectedIndex = 0;

			SelectedFont = new(_listBoxFonts.Items[0].ToString(), (float)FontSize, DFontStyle.Regular);

			_textBoxSize.Text = FontSize.ToString();

			DrawString();
		}

		private void DrawString()
		{
			var bitmap = new Bitmap((int)_imagePreview.Width, (int)_imagePreview.Height);
			using var g = Graphics.FromImage(bitmap);
			g.TextRenderingHint = AntiAlias;
			g.DrawString(
				SampleText, SelectedFont, _brush, bitmap.Width >> 1, bitmap.Height >> 1,
				new()
			{ Alignment = Center, LineAlignment = Center });

			_imagePreview.Source = bitmap.ToImageSource();
		}


		private void ButtonApply_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;

			_brush.Dispose();
			Close();
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;

			_brush.Dispose();
			Close();
		}

		private void ListBoxStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// While initializing, 'SelectedFont' is null.
			if ((sender, SelectedFont) is (ListBox listBox, not null))
			{
				SelectedFont = new(SelectedFont, (DFontStyle)listBox.SelectedIndex);

				DrawString();
			}
		}

		private void ListBoxFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// While initializing, 'SelectedFont' is null.
			if ((sender, SelectedFont) is (ListBox listBox, not null))
			{
				SelectedFont = new(listBox.SelectedItem.ToString(), SelectedFont.Size, SelectedFont.Style);

				DrawString();
			}
		}

		private void TextBoxSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!double.TryParse(textBox.Text, out double value))
				{
					textBox.Text = "9";

					e.Handled = true;
					return;
				}

				SelectedFont = new(SelectedFont.Name, (float)value, SelectedFont.Style);

				DrawString();
			}
		}

		private void TextBoxSize_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			bool c() => (e.Key, Keyboard.Modifiers) is ( > D0 and <= D9 or > NumPad0 and <= NumPad9, ModifierKeys.None);
			if (sender is TextBox textBox)
			{
				if (textBox.Text == "0")
				{
					if (c())
					{
						textBox.Text = e.Key is > D0 and <= D9 ? (e.Key - D0).ToString() : (e.Key - NumPad0).ToString();
					}
				}
				else if (c())
				{
					textBox.Text = e.Key is >= D0 and <= D9 ? (e.Key - D0).ToString() : (e.Key - NumPad0).ToString();
				}
			}
		}
	}
}
