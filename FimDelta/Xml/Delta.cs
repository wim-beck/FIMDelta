using System.Linq;
using System.Xml.Linq;

namespace FimDelta.Xml
{
	public class Delta
	{
		private XDocument xmlDocument;
		public ImportObject[] Objects { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Delta() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="deltaFile"></param>
		public Delta(string deltaFile)
		{
			xmlDocument = XDocument.Load(deltaFile);
			Objects = xmlDocument.Root.Element("Operations").Elements("ResourceOperation").Select(x => new ImportObject(x)).ToArray();
		}

		/// <summary>
		/// Save the delta.
		/// </summary>
		/// <param name="outFile"></param>
		public void Save(string outFile)
		{
			foreach (ImportObject obj in Objects)
			{
				if (!obj.NeedsInclude())
				{
					obj.XmlRepresentation.Remove();
				}
				else
				{
					obj.Clean();
				}
			}
			xmlDocument.Save(outFile);
		}

	}
}
