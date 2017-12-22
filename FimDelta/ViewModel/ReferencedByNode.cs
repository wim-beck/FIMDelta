using FimDelta.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FimDelta.ViewModel
{
	/// <summary>
	/// Represents list of nodes referencing parent object node (for tracking references)
	/// </summary>
	public class ReferencedByNode : INode
	{
		private readonly Delta delta;
		private readonly ImportObject obj;
		private ObjectNode[] children = null;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="obj"></param>
		public ReferencedByNode(Delta delta, ImportObject obj)
		{
			this.delta = delta;
			this.obj = obj;
		}

		/// <summary>
		/// Property for display name.
		/// </summary>
		public string DisplayName
		{
			get { return "Referenced by"; }
		}

		/// <summary>
		/// Property to get all childnodes
		/// </summary>
		public IEnumerable<INode> ChildNodes
		{
			get
			{
				if (delta == null) return null;
				if (obj.State == DeltaState.Delete) return null;

				if (children == null)
				{
					children = delta.Objects
						 .Where(x => x.Changes != null &&
										 x.Changes.Any(y => y.AttributeValue != null &&
																  y.AttributeValue.IndexOf(obj.Id, StringComparison.OrdinalIgnoreCase) >= 0))
						 .Select(x => new ObjectNode(delta, x))
						 .ToArray();
				}

				return children;
			}
		}
	}
}
