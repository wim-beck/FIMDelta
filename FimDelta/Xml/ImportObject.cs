using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FimDelta.Xml
{
	[Serializable]
	public class ImportObject : INotifyPropertyChanged
	{
		public string Id { get; private set; }

		public string ResourceType { get; private set; }
		
		public string ResourceName { get; private set; }

		public DeltaState State { get; set; }

		public ImportChange[] Changes { get; set; }

		public XElement XmlRepresentation { get; private set; }

		private bool isIncluded = true;

		[XmlIgnore]
		public bool IsIncluded
		{
			get { return isIncluded; }
			set
			{
				isIncluded = value;
				OnPropertyChanged("IsIncluded");
			}
		}

		internal bool NeedsInclude()
		{
			return IsIncluded || (Changes != null && Changes.Any(x => x.IsIncluded));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ImportObject() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="obj"></param>
		public ImportObject(XElement obj)
		{
			XmlRepresentation = obj;
			Id = obj.Attribute("id").Value;
			ResourceType = obj.Attribute("resourceType").Value;
			ResourceName = obj.Attribute("resourceName").Value;
			Changes = obj.Element("AttributeOperations").Elements("AttributeOperation").Select(x => new ImportChange(x)).ToArray();
			State = getState(obj.Attribute("operation").Value);
		}

		private DeltaState getState(string input)
		{
			if (input.Equals("Add"))
			{
				return DeltaState.Add;
			}
			else if (input.Equals("Delete"))
			{
				return DeltaState.Delete;
			}
			else if (input.Equals("Update"))
			{
				return DeltaState.Update;
			}
			throw new Exception("Invalid delta state");
		}

		/// <summary>
		/// Event listener.
		/// </summary>
		/// <param name="property"></param>
		protected void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		/// <summary>
		/// Remove deselected properties from xml element.
		/// </summary>
		public void Clean()
		{
			foreach (ImportChange change in Changes)
			{
				if (!change.IsIncluded)
				{
					change.XmlRepresentation.Remove();
				}
			}
		}
	}
}
