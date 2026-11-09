using System.Collections.Generic;

namespace Models
{
	public class MultipleSell
	{
		public static int okCode = 200;

		public static int missingCode = 603;

		public static int baseCode = 604;

		public CharacterAccountDetails characterDetails;

		public List<int> returnCode = new List<int>();
	}
}
