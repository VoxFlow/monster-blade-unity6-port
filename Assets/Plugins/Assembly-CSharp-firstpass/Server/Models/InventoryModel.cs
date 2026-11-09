namespace Server.Models
{
	public sealed class InventoryModel : IObjectModel
	{
		public RodInventoryModel[] rods;

		public BottleInventoryModel[] bottles;

		public BaitInventoryModel[] baits;

		public EnergyRechargerModel[] rechargers;

		public BattlePointRechargerModel[] bpPotions;

		public HpPotionModel[] hpPotions;

		public MonsterCollectionModel[] monsters;

		public RodCollectionModel[] collection;
	}
}
