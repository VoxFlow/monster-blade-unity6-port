namespace Server.Models
{
	public sealed class RodInfo : IObjectModel
	{
		public uint id;

		public string name;

		public string family;

		public int rodRarity;

		public int tier;

		public int tierMax;

		public int EvolutionMaterial01;

		public int EvolutionMaterial02;

		public int EvolutionMaterial03;

		public int EvolutionMaterial04;

		public int elemental;

		public string attackZoneName;

		public int energyCost;

		public int speedMin;

		public int speedMax;

		public int attackMin;

		public int attackMax;

		public int normalAttackIncrementMin;

		public int normalAttackIncrementMax;

		public int criticalMin;

		public int criticalMax;

		public int criticalAttackIncrementMin;

		public int criticalAttackIncrementMax;

		public int defense;

		public int defenseIncrement;

		public int baseSellingPrice;

		public int xpGiveAmount;
	}
}
