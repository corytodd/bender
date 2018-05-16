namespace BenderLib
{
	using System.Collections.Generic;

    public class Structure
    {
		public string Name { get; set; }

		public IList<Element> Elements { get; set; }
    }
}
