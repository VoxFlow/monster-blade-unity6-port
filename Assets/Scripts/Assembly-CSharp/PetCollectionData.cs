using System.Collections.Generic;

public class PetCollectionData
{
	public string m_rewardId = string.Empty;

	public List<string> m_members = new List<string>();

	public bool m_isHidden;

	public void Clear()
	{
		m_rewardId = string.Empty;
		m_members.Clear();
	}
}
