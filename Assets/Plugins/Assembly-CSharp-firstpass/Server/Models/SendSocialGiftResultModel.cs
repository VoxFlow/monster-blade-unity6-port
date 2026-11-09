using System.Collections.Generic;

namespace Server.Models
{
	public sealed class SendSocialGiftResultModel : IObjectModel
	{
		public int socialType;

		public List<uint> ids;

		public uint coolDown;
	}
}
