﻿#if CSHARP_9_PREVIEW
#pragma warning disable CS1591
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sudoku.Data;
using Sudoku.DocComments;

namespace Sudoku.Drawing
{
	/// <summary>
	/// Encapsulates a view when displaying the information on forms.
	/// Different with <see cref="View"/>, this data structure can add and remove the items
	/// in the current collection.
	/// </summary>
	/// <seealso cref="View"/>
	[DebuggerStepThrough]
	public sealed record MutableView(
		ICollection<(int Id, int CellOffset)>? CellOffsets,
		ICollection<(int Id, int CandidateOffset)>? CandidateOffsets,
		ICollection<(int Id, int RegionOffset)>? RegionOffsets,
		ICollection<Link>? Links)
	{
		/// <inheritdoc cref="DefaultConstructor"/>
		/// <remarks>
		/// The constructor is equivalent to code '<c>new MutableView(null, null, null, null)</c>'.
		/// </remarks>
		public MutableView()
			: this(new List<(int, int)>(), new List<(int, int)>(), new List<(int, int)>(), new List<Link>())
		{
		}


		/// <summary>
		/// Add the cell into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="cell">The cell.</param>
		public void AddCell(int id, int cell) => CellOffsets?.Add((id, cell));

		/// <summary>
		/// Add the candidate into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="candidate">The cell.</param>
		public void AddCandidate(int id, int candidate) => CandidateOffsets?.Add((id, candidate));

		/// <summary>
		/// Add the region into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="region">The region.</param>
		public void AddRegion(int id, int region) => RegionOffsets?.Add((id, region));

		/// <summary>
		/// Add the link into the list.
		/// </summary>
		/// <param name="inference">The link.</param>
		public void AddLink(Link inference) => Links?.Add(inference);

		/// <summary>
		/// Remove the cell from the list.
		/// </summary>
		/// <param name="cell">The cell.</param>
		public void RemoveCell(int cell) => (CellOffsets as List<(int, int Cell)>)?.RemoveAll(p => p.Cell == cell);

		/// <summary>
		/// Remove the candidate from the list.
		/// </summary>
		/// <param name="candidate">The candidate.</param>
		public void RemoveCandidate(int candidate) =>
			(CandidateOffsets as List<(int, int Candidate)>)?.RemoveAll(p => p.Candidate == candidate);

		/// <summary>
		/// Remove the region from the list.
		/// </summary>
		/// <param name="region">The region.</param>
		public void RemoveRegion(int region) =>
			(RegionOffsets as List<(int, int Region)>)?.RemoveAll(p => p.Region == region);

		/// <summary>
		/// Remove the link from the list.
		/// </summary>
		/// <param name="link">The link.</param>
		public void RemoveLink(Link link) => Links?.Remove(link);

		/// <summary>
		/// Clear all elements.
		/// </summary>
		public void Clear()
		{
			CellOffsets?.Clear();
			CandidateOffsets?.Clear();
			RegionOffsets?.Clear();
			Links?.Clear();
		}

		/// <summary>
		/// Indicates whether the specified list contains the cell.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsCell(int cell) => CellOffsets?.Any(p => p.CellOffset == cell) ?? false;

		/// <summary>
		/// Indicates whether the specified list contains the candidate.
		/// </summary>
		/// <param name="candidate">The candidate.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsCandidate(int candidate) => CandidateOffsets?.Any(p => p.CandidateOffset == candidate) ?? false;

		/// <summary>
		/// Indicates whether the specified list contains the region.
		/// </summary>
		/// <param name="region">The region.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsRegion(int region) => RegionOffsets?.Any(p => p.RegionOffset == region) ?? false;

		/// <summary>
		/// Indicates whether the specified list contains the link.
		/// </summary>
		/// <param name="inference">The link.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsLink(Link inference) => Links?.Contains(inference) ?? false;
	}

#if false
	/// <summary>
	/// Encapsulates a view when displaying the information on forms.
	/// Different with <see cref="View"/>, this data structure can add and remove the items
	/// in the current collection.
	/// </summary>
	/// <seealso cref="View"/>
	[DebuggerStepThrough]
	public sealed class MutableView
	{
		/// <include file='...\GlobalDocComments.xml' path='comments/defaultConstructor'/>
		/// <remarks>
		/// The constructor is equivalent to code '<c>new MutableView(null, null, null, null)</c>'.
		/// </remarks>
		public MutableView()
		{
		}

		/// <summary>
		/// Initializes an instance with information.
		/// </summary>
		/// <param name="cellOffsets">
		/// The list of pairs of identifier and cell offset.
		/// </param>
		/// <param name="candidateOffsets">
		/// The list of pairs of identifier and candidate offset.
		/// </param>
		/// <param name="regionOffsets">
		/// The list of pairs of identifier and region offset.
		/// </param>
		/// <param name="links">The list of links.</param>
		public MutableView(
			ICollection<(int, int)> cellOffsets, ICollection<(int, int)> candidateOffsets,
			ICollection<(int, int)> regionOffsets, ICollection<Link> links) =>
			(CellOffsets, CandidateOffsets, RegionOffsets, Links) = (cellOffsets, candidateOffsets, regionOffsets, links);


		/// <summary>
		/// The cell offsets.
		/// </summary>
		public ICollection<(int, int _cell)> CellOffsets { get; } = new List<(int, int)>();

		/// <summary>
		/// The candidate offsets.
		/// </summary>
		public ICollection<(int, int _candidate)> CandidateOffsets { get; } = new List<(int, int)>();

		/// <summary>
		/// The region offsets.
		/// </summary>
		public ICollection<(int, int _region)> RegionOffsets { get; } = new List<(int, int)>();

		/// <summary>
		/// The links.
		/// </summary>
		public ICollection<Link> Links { get; } = new List<Link>();


		/// <summary>
		/// Add the cell into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="cell">The cell.</param>
		public void AddCell(int id, int cell) => CellOffsets.Add((id, cell));

		/// <summary>
		/// Add the candidate into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="candidate">The cell.</param>
		public void AddCandidate(int id, int candidate) => CandidateOffsets.Add((id, candidate));

		/// <summary>
		/// Add the region into the list.
		/// </summary>
		/// <param name="id">The color ID.</param>
		/// <param name="region">The region.</param>
		public void AddRegion(int id, int region) => RegionOffsets.Add((id, region));

		/// <summary>
		/// Add the link into the list.
		/// </summary>
		/// <param name="inference">The link.</param>
		public void AddLink(Link inference) => Links.Add(inference);

		/// <summary>
		/// Remove the cell from the list.
		/// </summary>
		/// <param name="cell">The cell.</param>
		public void RemoveCell(int cell) => ((List<(int, int _cell)>)CellOffsets).RemoveAll(p => p._cell == cell);

		/// <summary>
		/// Remove the candidate from the list.
		/// </summary>
		/// <param name="candidate">The candidate.</param>
		public void RemoveCandidate(int candidate) =>
			((List<(int, int _candidate)>)CandidateOffsets).RemoveAll(p => p._candidate == candidate);

		/// <summary>
		/// Remove the region from the list.
		/// </summary>
		/// <param name="region">The region.</param>
		public void RemoveRegion(int region) =>
			((List<(int, int _region)>)RegionOffsets).RemoveAll(p => p._region == region);

		/// <summary>
		/// Remove the link from the list.
		/// </summary>
		/// <param name="link">The link.</param>
		public void RemoveLink(Link link) => Links.Remove(link);

		/// <summary>
		/// Clear all elements.
		/// </summary>
		public void Clear()
		{
			CellOffsets.Clear();
			CandidateOffsets.Clear();
			RegionOffsets.Clear();
			Links.Clear();
		}

		/// <summary>
		/// Indicates whether the specified list contains the cell.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsCell(int cell) => CellOffsets.Any(p => p._cell == cell);

		/// <summary>
		/// Indicates whether the specified list contains the candidate.
		/// </summary>
		/// <param name="candidate">The candidate.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsCandidate(int candidate) => CandidateOffsets.Any(p => p._candidate == candidate);

		/// <summary>
		/// Indicates whether the specified list contains the region.
		/// </summary>
		/// <param name="cell">The region.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsRegion(int region) => RegionOffsets.Any(p => p._region == region);

		/// <summary>
		/// Indicates whether the specified list contains the link.
		/// </summary>
		/// <param name="cell">The link.</param>
		/// <returns>A <see cref="bool"/> value.</returns>
		public bool ContainsLink(Link inference) => Links.Contains(inference);
	}
#endif
}
