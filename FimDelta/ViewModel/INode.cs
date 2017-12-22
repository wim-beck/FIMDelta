using System.Collections.Generic;

namespace FimDelta.ViewModel
{
	public interface INode
	{
		IEnumerable<INode> ChildNodes { get; }
	}

	public interface IIncludableNode : INode
	{
		bool? Include { get; set; }
		void UpdateInclude();
	}
}
