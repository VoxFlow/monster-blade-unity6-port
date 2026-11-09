namespace Models
{
	public class PetDetails : IEquipable
	{
		public string m_petId = string.Empty;

		public string m_petType = string.Empty;

		public string m_petElement = string.Empty;

		public string m_petRarity = string.Empty;

		public string m_petSkillName = string.Empty;

		public int m_skillLevelOffset;

		public string m_assetBundle = string.Empty;

		public string m_name = string.Empty;

		private int m_level = 1;

		public int m_nextLevelupExp = 1;

		public string m_petModelPath = string.Empty;

		public long m_lastHuntStartTime;

		public long m_assignedHuntConsumeTime;

		public string PetPrefabPath { get; private set; }

		public string PetIconPath { get; private set; }

		public string PetFrameName { get; private set; }

		public string Name { get; private set; }

		public int m_Level
		{
			get
			{
				int result = 150;
				int.TryParse(SingletonComponent<ParameterManager>.Instance.GetParameter("PetLeveling", m_level.ToString(), "XP_TO_LEVEL_UP"), out result);
				if (result <= 0)
				{
					result = 150;
				}
				m_nextLevelupExp = result;
				return m_level;
			}
			set
			{
				m_level = value;
				int result = 150;
				if (SingletonComponent<ParameterManager>.Instance.IsDataInitialized)
				{
					int.TryParse(SingletonComponent<ParameterManager>.Instance.GetParameter("PetLeveling", m_level.ToString(), "XP_TO_LEVEL_UP"), out result);
					if (result <= 0)
					{
						result = 150;
					}
					m_nextLevelupExp = result;
				}
			}
		}

		public PetDetails()
		{
		}

		public PetDetails(string p_id)
		{
			m_petId = p_id;
		}

		public void SetExtraInfo(string p_prefabPath, string p_name, string p_iconPath, string p_frameName)
		{
			PetPrefabPath = p_prefabPath;
			Name = p_name;
			PetIconPath = p_iconPath;
			PetFrameName = p_frameName;
		}

		public void Equip(IPlayer p_player)
		{
			if (p_player != null)
			{
				p_player.Equip(this);
			}
		}

		public bool IsEquipped(IPlayer p_player)
		{
			if (p_player == null)
			{
				return false;
			}
			return p_player.IsEquipped(this);
		}

		public override string ToString()
		{
			return string.Format("m_petId: {0}, m_petType: {1}, m_petElement: {2}, m_petRarity: {3}, m_skillLevelOffset: {4}, m_assetBundle: {5}, m_name: {6}, m_level: {7}", m_petId, m_petType, m_petElement, m_petRarity, m_skillLevelOffset, m_assetBundle, m_name, m_level);
		}
	}
}
