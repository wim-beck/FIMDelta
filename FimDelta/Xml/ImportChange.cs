using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace FimDelta.Xml
{
	[Serializable]
	public class ImportChange : INotifyPropertyChanged
	{
		public string Operation { get; set; }

		public string AttributeName { get; set; }

		public string AttributeValue { get; set; }

		public bool IsReference { get; private set; }

		public string ReferenceValue { get; private set; }

		public string ReferenceType { get; private set; }

		public bool FullyResolved { get; private set; }

		public string Locale { get; set; }

		private bool isIncluded = true;

		public XElement XmlRepresentation { get; private set; }

		public bool IsIncluded
		{
			get { return isIncluded; }
			set
			{
				isIncluded = value;
				OnPropertyChanged("IsIncluded");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ImportChange() { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="change"></param>
		public ImportChange(XElement change)
		{
			XmlRepresentation = change;
			Operation = change.Attribute("operation").Value;
			AttributeName = change.Attribute("name").Value;
			AttributeValue = change.Value;

			if (change.Attribute("type") != null)
			{
				if (change.Attribute("type").Value.Equals("xmlref"))
				{
					ReferenceType = "xmlref";
					IsReference = true;
					FullyResolved = true;
					ReferenceValue = AttributeValue.Split('#')[1];
				}
				else if (change.Attribute("type").Value.Equals("ref"))
				{
					ReferenceType = "ref";
					IsReference = true;
					FullyResolved = false;
					ReferenceValue = AttributeValue.Split('|')[2];
				}
			}
		}

		/// <summary>
		/// Event listener.
		/// </summary>
		/// <param name="property"></param>
		protected void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
