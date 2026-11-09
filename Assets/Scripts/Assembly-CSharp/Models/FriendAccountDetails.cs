namespace Models
{
	public class FriendAccountDetails
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string name = string.Empty;

		public bool isFriend = true;

		public int coolDown = -1;

		public CharacterAccountDetails character;

		public FriendAccountDetails()
		{
			character = new CharacterAccountDetails();
		}
	}
}
