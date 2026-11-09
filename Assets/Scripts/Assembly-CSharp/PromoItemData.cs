public class PromoItemData
{
	public int Id;

	public int PromoId;

	public string ItemId = string.Empty;

	public override string ToString()
	{
		return string.Format("PromoItemData Id: {0}, PromoId: {1}, ItemId: {2}", Id, PromoId, ItemId);
	}
}
