namespace Server.Models
{
	public sealed class BottleInfo : IObjectModel
	{
		public uint id;

		public string name;

		public float multiplier;

		public int durationMin;

		public int durationMax;

		public int durationFadein;

		public int durationFadeout;

		public int dpsMin;

		public int dpsMax;
	}
}
