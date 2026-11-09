using System.Collections.Generic;
using Models;

public class HuntReward
{
	public int m_rewardIgc;

	public List<ServerInventoryItem> m_invItems = new List<ServerInventoryItem>();

	public void Clear()
	{
		m_rewardIgc = 0;
		m_invItems.Clear();
	}
}
