using System.Collections.Generic;

namespace Models
{
	public class PatchInfo
	{
		public string version = string.Empty;

		public List<PatchFileInfo> files = new List<PatchFileInfo>();
	}
}
