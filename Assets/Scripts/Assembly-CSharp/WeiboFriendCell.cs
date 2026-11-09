using UnityEngine;

public class WeiboFriendCell : NBUIGridCell<WeiboUser>
{
	[SerializeField]
	private UISprite m_defaultAvatar;

	[SerializeField]
	private UITexture m_friendPic;

	[SerializeField]
	private UILabel m_friendName;

	[SerializeField]
	private UIButtonMessage m_inviteButton;

	public override WeiboUser data
	{
		get
		{
			return base.data;
		}
		set
		{
			base.data = value;
			if (m_data == null)
			{
				return;
			}
			if (null != m_friendName)
			{
				m_friendName.text = m_data.name;
			}
			if (null != m_inviteButton)
			{
				UIButton component = m_inviteButton.gameObject.GetComponent<UIButton>();
				if (null != component)
				{
					component.SetButtonGrey(data.InviteSent);
				}
			}
		}
	}

	public void SetWBPicture(Texture2D p_texture)
	{
		if (null == p_texture)
		{
			return;
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (null == shader)
		{
			return;
		}
		Material material = new Material(shader);
		if (!(null == material) && !(null == m_friendPic))
		{
			if (null != m_defaultAvatar)
			{
				m_defaultAvatar.enabled = false;
			}
			material.mainTexture = p_texture;
			m_friendPic.material = material;
			m_friendPic.enabled = true;
			if (null != m_defaultAvatar)
			{
				m_friendPic.transform.localScale = m_defaultAvatar.transform.localScale;
			}
		}
	}

	public void SetTarget(GameObject p_target, string p_functionName)
	{
		if (null != m_inviteButton)
		{
			m_inviteButton.target = p_target;
			m_inviteButton.functionName = p_functionName;
		}
	}
}
