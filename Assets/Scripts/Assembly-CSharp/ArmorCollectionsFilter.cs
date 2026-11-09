using System.Collections.Generic;
using System.Linq;

public abstract class ArmorCollectionsFilter
{
	public class ElementSets : ArmorCollectionsFilter
	{
		private Element.Type m_element;

		public ElementSets(Element.Type p_element)
		{
			m_element = p_element;
		}

		public override bool Accept(ArmorCollectionsCell p_cell)
		{
			return p_cell.data.elementType == m_element;
		}
	}

	public class NewSets : ArmorCollectionsFilter
	{
		public override bool Accept(ArmorCollectionsCell p_cell)
		{
			return p_cell.data.isNew;
		}
	}

	public class CompletedSets : ArmorCollectionsFilter
	{
		public override bool Accept(ArmorCollectionsCell p_cell)
		{
			return p_cell.data.isCompleted;
		}
	}

	public class AllSets : ArmorCollectionsFilter
	{
		public override List<ArmorCollectionsCell> Filter(List<ArmorCollectionsCell> p_cells)
		{
			return p_cells.ToList();
		}

		public override bool Accept(ArmorCollectionsCell p_cell)
		{
			return true;
		}
	}

	public virtual List<ArmorCollectionsCell> Filter(List<ArmorCollectionsCell> p_cells)
	{
		return p_cells.Where((ArmorCollectionsCell cell) => Accept(cell)).ToList();
	}

	public abstract bool Accept(ArmorCollectionsCell p_cell);
}
