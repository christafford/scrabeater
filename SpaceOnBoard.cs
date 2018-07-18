using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrabeater
{
	public class SpaceOnBoard
	{
		public enum Type
		{
			Nothing,
			DoubleLetter,
			TripleLetter,
			DoubleWord,
			TripleWord,
			Start,
			Used
		}

		public char? Letter;
		public Type SpotType;
	}
}
