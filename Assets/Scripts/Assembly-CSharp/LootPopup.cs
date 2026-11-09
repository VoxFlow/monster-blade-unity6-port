using UnityEngine;

public class LootPopup : MonoBehaviour
{
	public delegate void Callback();

	public UISprite m_lootSprite;

	private Callback m_animEndCallback;

	private void Awake()
	{
		if (base.GetComponent<Animation>() == null)
		{
			Debug.LogError("Animation component is required.");
		}
		else
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public void ShowLoot(ItemEnum.Chest p_lootType, Callback p_endCallback)
	{
		bool flag = false;
		if (p_lootType == ItemEnum.Chest.NONE || p_lootType == ItemEnum.Chest.Max)
		{
			Debug.LogError("Invalid loot type");
			flag = true;
		}
		else if (m_lootSprite == null)
		{
			Debug.LogWarning("Loot sprite has not yet specified.");
			flag = true;
		}
		else
		{
			string[] array = new string[5] { "Chest_Lv1", "Chest_Lv2", "Chest_Lv3", "Chest_Lv4", "Chest_Lv5" };
			m_lootSprite.spriteName = array[(int)p_lootType];
			m_animEndCallback = p_endCallback;
			base.gameObject.SetActiveRecursively(true);
		}
		if (flag && p_endCallback != null)
		{
			p_endCallback();
		}
	}

	private void Update()
	{
		if (!base.GetComponent<Animation>().isPlaying)
		{
			m_animEndCallback();
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
