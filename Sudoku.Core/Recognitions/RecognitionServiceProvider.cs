﻿#if SUDOKU_RECOGNIZING
using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sudoku.Data;

namespace Sudoku.Recognitions
{
	/// <summary>
	/// Define a sudoku recognition service provider.
	/// </summary>
	public sealed class RecognitionServiceProvider : IDisposable
	{
		/// <summary>
		/// Indicates the internal recognition service provider.
		/// </summary>
		private readonly InternalServiceProvider _recognizingServiceProvider;


		/// <include file='..\GlobalDocComments.xml' path='comments/defaultConstructor'/>
		public RecognitionServiceProvider()
		{
			_recognizingServiceProvider = new();
			_recognizingServiceProvider.InitTesseractAsync($@"{Directory.GetCurrentDirectory()}\tessdata").Wait();
		}


		/// <summary>
		/// Indicates whether the OCR tool has already initialized.
		/// </summary>
		public bool ToolIsInitialized => _recognizingServiceProvider.Initialized;


		/// <inheritdoc/>
		public void Dispose() => _recognizingServiceProvider.Dispose();

		/// <summary>
		/// Recognize the image.
		/// </summary>
		/// <param name="image">The image.</param>
		/// <returns>The grid.</returns>
		/// <exception cref="RecognizingException">
		/// Throws when the tool has not initialized yet.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Grid Recorgnize(Bitmap image)
		{
			if (ToolIsInitialized)
			{
				using var gridRecognizer = new GridRecognizer(image);
				return _recognizingServiceProvider.RecognizeDigits(gridRecognizer.Recognize());
			}

			throw new RecognizingException(
				"The recognizer has not initialized.",
				new NullReferenceException("The tool is current null."));
		}

		/// <summary>
		/// Recognize the image asynchronizedly.
		/// </summary>
		/// <param name="image">The image.</param>
		/// <returns>The task.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<Grid> RecorgnizeAsync(Bitmap image) => await Task.Run(() => Recorgnize(image));
	}
}
#endif