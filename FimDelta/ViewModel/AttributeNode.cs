using FimDelta.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FimDelta.ViewModel
{

	/// <summary>
	/// Represents an single attribute of an object
	/// </summary>
	public class AttributeNode : IIncludableNode, INotifyPropertyChanged, IDisposable
	{
		private readonly Delta delta;
		private readonly ImportObject obj;
		private readonly ImportChange attr;
		private ObjectNode[] children = null;
		private WeakReference parent = null;

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="obj"></param>
		/// <param name="attr"></param>
		public AttributeNode(Delta delta, ImportObject obj, ImportChange attr)
		{
			this.delta = delta;
			this.obj = obj;
			this.attr = attr;

			this.attr.PropertyChanged += SourcePropertyChanged;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~AttributeNode()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			attr.PropertyChanged -= SourcePropertyChanged;
			if (disposing)
				GC.SuppressFinalize(this);
		}

		private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsIncluded")
				UpdateInclude();
		}

		public ObjectNode Parent
		{
			get { return parent != null && parent.IsAlive ? (ObjectNode)parent.Target : null; }
			set { parent = new WeakReference(value); }
		}

		public string OperationName
		{
			get
			{
				if (attr.Operation == "None")
					return "Set";
				else
					return attr.Operation;
			}
		}

		public string AttributeName
		{
			get { return attr.AttributeName; }
		}

		public string AttributeValue
		{
			get { return attr.AttributeValue; }
		}

		public string DisplayTooltip
		{
			get
			{
				return DisplayValue == attr.AttributeValue ? null : attr.AttributeValue;
			}
		}

		public string DisplayValue
		{
			get
			{
				if (attr.IsReference)
				{
					return attr.ReferenceValue;
				}
				if (string.IsNullOrEmpty(attr.AttributeValue))
				{
					if (attr.Operation == "None" || attr.Operation == "Set")
						return "(empty)";
					else
						return attr.AttributeValue;
				}

				string s = attr.AttributeValue.Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "");

				if (s.Length > 150)
				{
					s = s.Substring(0, 146) + " ...";
				}

				return s;
			}
		}

		public IEnumerable<INode> ChildNodes
		{
			get
			{
				if (string.IsNullOrEmpty(attr.AttributeValue)) return null;

				if (children == null)
				{
					children = delta.Objects
							 .Where(x => attr.AttributeValue.Equals(x.Id, StringComparison.OrdinalIgnoreCase))
							 .Select(x => new ObjectNode(delta, x))
							 .ToArray();
				}

				return children;
			}
		}

		private bool inIncludeLoop = false;

		public bool? Include
		{
			get
			{
				if (!obj.IsIncluded)
					return false;
				return attr.IsIncluded;
			}
			set
			{
				inIncludeLoop = true;

				try
				{
					attr.IsIncluded = value.GetValueOrDefault();
					if (attr.IsIncluded)
						obj.IsIncluded = true;

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

		public void UpdateInclude()
		{
			if (inIncludeLoop) return;

			OnPropertyChanged("Include");

			if (Parent != null)
				Parent.UpdateInclude();
		}

		protected void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

	}
}
