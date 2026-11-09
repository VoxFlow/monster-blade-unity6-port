using Models;
using UnityEngine;

public class PetInventoryCell : NBUIGridCell<PetDetails>
{
	[SerializeField]
	private UISprite m_frameSprite;

	[SerializeField]
	private UISprite m_iconSprite;

	public override PetDetails data
	{
		set
		{
			m_data = value;
			if (m_data != null)
			{
				SetFrameSprite(m_data.PetFrameName);
				SetIconSprite(m_data.PetIconPath);
			}
		}
	}

	private void SetFrameSprite(string p_spriteName)
	{
		if (null != m_frameSprite)
		{
			m_frameSprite.spriteName = p_spriteName;
		}
	}

	private void SetIconSprite(string p_spriteName)
	{
		if (null != m_iconSprite)
		{
			m_iconSprite.spriteName = p_spriteName;
		}
	}
}
