using FimDelta.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FimDelta.ViewModel
{
	/// <summary>
	/// Represents a single changed object in object tree
	/// </summary>
	public class ObjectNode : IIncludableNode, INotifyPropertyChanged, IDisposable
	{
		private readonly Delta delta;
		private readonly ImportObject obj;
		private readonly AttributeNode[] children;
		private WeakReference parent = null;
		private string displayName = null;

		public event PropertyChangedEventHandler PropertyChanged;

		public IIncludableNode Parent
		{
			get { return parent != null && parent.IsAlive ? (IIncludableNode)parent.Target : null; }
			set { parent = new WeakReference(value); }
		}

		public string State
		{
			get { return obj.State.ToString(); }
		}

		public string ObjectType
		{
			get { return obj.ObjectType; }
		}

		public string DisplayName
		{
			get
			{
				if (displayName != null)
					return displayName;

				if (obj.Changes != null)
				{
					var c = obj.Changes.FirstOrDefault(x => x.AttributeName == "DisplayName");
					if (c != null)
						return displayName = c.AttributeValue;
				}

				throw new Exception("Display name not found");
			}
		}

		public IEnumerable<INode> ChildNodes
		{
			get
			{
				List<INode> list = new List<INode>();

				var refdBy = new ReferencedByNode(delta, obj);
				if (refdBy.ChildNodes != null && refdBy.ChildNodes.Any())
					list.Add(refdBy);

				if (children != null)
					list.AddRange(children);

				return list;
			}
		}

		private bool inIncludeLoop = false;

		public bool? Include
		{
			get
			{
				if (!obj.IsIncluded) return false;

				if (obj.Changes != null)
				{
					bool hasIncluded = false, hasExcluded = false;
					foreach (var ch in obj.Changes)
					{
						if (ch.IsIncluded)
							hasIncluded = true;
						else
							hasExcluded = true;

						if (hasIncluded && hasExcluded) return null;
					}
					return hasIncluded;
				}
				return obj.IsIncluded;
			}
			set
			{
				inIncludeLoop = true;

				try
				{
					obj.IsIncluded = value.GetValueOrDefault();
					if (obj.Changes != null)
						foreach (var ch in obj.Changes)
							ch.IsIncluded = obj.IsIncluded;

					if (children != null)
						foreach (var ch in children)
							ch.UpdateInclude();
				}
				finally
				{
					inIncludeLoop = false;
				}

				UpdateInclude();
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="obj"></param>
		public ObjectNode(Delta delta, ImportObject obj)
		{
			this.delta = delta;
			this.obj = obj;

			this.obj.PropertyChanged += new PropertyChangedEventHandler(SourcePropertyChanged);

			children = null;
			if (obj.Changes != null)
				children = obj.Changes.Select(a => new AttributeNode(delta, obj, a) { Parent = this }).ToArray();
		}

		~ObjectNode()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			obj.PropertyChanged -= SourcePropertyChanged;
			if (disposing)
				GC.SuppressFinalize(this);
		}

		private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsIncluded")
				UpdateInclude();
		}

		public void UpdateInclude()
		{
			if (inIncludeLoop) return;

			OnPropertyChanged("Include");

			var p = Parent;
			if (p != null)
				p.UpdateInclude();
		}

		protected void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

	}
}
