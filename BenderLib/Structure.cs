namespace BenderLib
{
	using System.Collections.Generic;

    public class Structure
    {
        /// <summary>
        /// Gets or Sets name of this structure
        /// </summary>
		public string Name { get; set; }

        /// <summary>
        /// Gets or Sets ordered list of elements in this structure
        /// </summary>
		public IList<Element> Elements { get; set; }
    }
}
