using UnityEngine;

public class PetEventCell : NBUIGridCell<DungeonManager.DungeonStruct>
{
	public UILabel labelDungeonName;

	public UISprite spriteTextCleared;

	public UISprite spriteTextNew;

	public UILabel labelStamina;

	public UISprite spriteBg;

	public Component eventTarget;

	public UISprite[] bgSprites;

	private bool m_hidden;

	private UIWidget[] m_children;

	public bool hidden
	{
		get
		{
			return m_hidden;
		}
		set
		{
			m_hidden = value;
			if (m_children == null)
			{
				m_children = MBHier.GetComponentsInChildren<UIWidget>(this);
			}
			UIWidget[] children = m_children;
			foreach (UIWidget uIWidget in children)
			{
				uIWidget.enabled = !m_hidden;
			}
		}
	}

	public override DungeonManager.DungeonStruct data
	{
		set
		{
			m_data = value;
			Reset();
			labelDungeonName.text = m_data.dungeon.id;
			if (m_data.cleared)
			{
				spriteTextCleared.enabled = true;
			}
			else if (!m_data.visited)
			{
				spriteTextNew.enabled = true;
			}
			int element = (int)m_data.dungeon.element;
			if (element < bgSprites.Length)
			{
				spriteBg.spriteName = bgSprites[element].spriteName;
			}
		}
	}

	private void Reset()
	{
		labelDungeonName.text = "Unknown";
		spriteTextCleared.enabled = false;
		spriteTextNew.enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnClick()
	{
		if ((bool)eventTarget)
		{
			PetEventCellEvent petEventCellEvent = new PetEventCellEvent();
			m_data.visited = true;
			petEventCellEvent.dungeonData = m_data;
			eventTarget.SendMessage("OnPetEventCellEvent", petEventCellEvent, SendMessageOptions.DontRequireReceiver);
		}
	}
}
