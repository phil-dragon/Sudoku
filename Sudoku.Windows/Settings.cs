﻿using System;
using System.Linq;

namespace Sudoku.Windows
{
	/// <summary>
	/// To encapsulates a series of setting options for <see cref="MainWindow"/> utility.
	/// </summary>
	[Serializable]
	public sealed partial class Settings : ICloneable<Settings>
	{
		/// <summary>
		/// To provides a default setting instance.
		/// </summary>
		[NonSerialized]
		public static readonly Settings DefaultSetting = new();


		/// <summary>
		/// To cover all settings.
		/// </summary>
		/// <param name="newSetting">The new settings instance.</param>
		public void CoverBy(Settings newSetting)
		{
			foreach (var property in from Property in GetType().GetProperties() where Property.CanWrite select Property)
			{
				property.SetValue(this, property.GetValue(newSetting));
			}
		}

		/// <inheritdoc/>
		public Settings Clone()
		{
			var resultInstance = new Settings();
			foreach (var property in GetType().GetProperties())
			{
				property.SetValue(resultInstance, property.GetValue(this));
			}

			return resultInstance;
		}
	}
}
