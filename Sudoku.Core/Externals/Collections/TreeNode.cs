﻿using System.Collections.Generic;

namespace System.Collections
{
	/// <summary>
	/// Encapsulates a tree node.
	/// </summary>
	public sealed class TreeNode : IComparable<TreeNode>
	{
		/// <summary>
		/// Indicates the current ID.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Indicates the parent ID of this instance.
		/// </summary>
		public int ParentId { get; set; }

		/// <summary>
		/// Indicates the content.
		/// </summary>
		public object? Content { get; set; }
		
		/// <summary>
		/// Indicates its children nodes.
		/// </summary>
		public ICollection<TreeNode> Children { get; set; } = new List<TreeNode>();


		/// <inheritdoc/>
		public int CompareTo(TreeNode other) => Id.CompareTo(other.Id);

		/// <inheritdoc/>
		public override string ToString() =>
			$"{{Id = {Id}, ParentId = {ParentId}, Content = {Content}, ChildrenCount = {Children.Count}}}";
	}
}
