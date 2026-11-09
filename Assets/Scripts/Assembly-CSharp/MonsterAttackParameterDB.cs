using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackParameterDB
{
	private Dictionary<string, MonsterAttackParameter> m_parameters = new Dictionary<string, MonsterAttackParameter>();

	public MonsterAttackParameter this[string p_animName]
	{
		get
		{
			if (p_animName == null)
			{
				return null;
			}
			MonsterAttackParameter value = null;
			if (!m_parameters.TryGetValue(p_animName, out value))
			{
				Dbg.LogWarn(p_animName + " is not available.");
			}
			return value;
		}
	}

	public MonsterAttackParameterDB(string p_dbFile)
	{
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!instance.IsDataInitialized)
		{
			instance.InitManager();
		}
		NBBinaryFormat parametersFor = instance.GetParametersFor(p_dbFile);
		foreach (string rowsName in parametersFor.RowsNames)
		{
			int numberItemInCell = instance.GetNumberItemInCell(p_dbFile, rowsName, "ATTACK_TYPE");
			MonsterAttackParameter monsterAttackParameter = new MonsterAttackParameter();
			List<MonsterAttack.Type> list = new List<MonsterAttack.Type>();
			List<MonsterAttack.Zone> list2 = new List<MonsterAttack.Zone>();
			List<float> list3 = new List<float>();
			List<float> list4 = new List<float>();
			List<float> list5 = new List<float>();
			List<float> list6 = new List<float>();
			List<float> list7 = new List<float>();
			List<float> list8 = new List<float>();
			List<float> list9 = new List<float>();
			List<float> list10 = new List<float>();
			List<string> list11 = new List<string>();
			float result = 0f;
			for (int i = 0; i < numberItemInCell; i++)
			{
				string parameter = instance.GetParameter(p_dbFile, rowsName, "ATTACK_TYPE", i);
				string parameter2 = instance.GetParameter(p_dbFile, rowsName, "DIRECTION", i);
				MonsterAttack.Type typeFromString = MonsterAttack.GetTypeFromString(parameter);
				MonsterAttack.Zone zoneFromString = MonsterAttack.GetZoneFromString(parameter2);
				if (typeFromString == MonsterAttack.Type.None || zoneFromString == MonsterAttack.Zone.None)
				{
					Debug.LogError("Unknown attack Type: " + parameter + ", Zone: " + parameter2);
				}
				list.Add(typeFromString);
				list2.Add(zoneFromString);
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "WIND_UP", i), out result))
				{
					list3.Add(result);
				}
				else
				{
					list3.Add(0f);
				}
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "DAMAGE_WINDOW", i), out result))
				{
					list4.Add(result);
				}
				else
				{
					list4.Add(0f);
				}
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "WIND_DOWN", i), out result))
				{
					list5.Add(result);
				}
				else
				{
					list5.Add(0f);
				}
				list7.Add(0f);
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "DODGE_WINDOW_END", i), out result))
				{
					list8.Add(result);
				}
				else
				{
					list8.Add(0f);
				}
				list6.Add(list8[i] - DodgeSystem.perfectDodgePeriod);
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "START_TRACK", i), out result))
				{
					list9.Add(result);
				}
				else
				{
					list9.Add(0f);
				}
				if (float.TryParse(instance.GetParameter(p_dbFile, rowsName, "END_TRACK", i), out result))
				{
					list10.Add(result);
				}
				else
				{
					list10.Add(0f);
				}
				list11.Add(instance.GetParameter(p_dbFile, rowsName, "ATTACK_POINT", i));
			}
			monsterAttackParameter.types = list.ToArray();
			monsterAttackParameter.zones = list2.ToArray();
			monsterAttackParameter.windupWindows = list3.ToArray();
			monsterAttackParameter.damageWindows = list4.ToArray();
			monsterAttackParameter.windownWindows = list5.ToArray();
			monsterAttackParameter.perfectDodgeWindowStart = list6.ToArray();
			monsterAttackParameter.dodgeWindowStart = list7.ToArray();
			monsterAttackParameter.dodgeWindowEnd = list8.ToArray();
			monsterAttackParameter.startTrackingWindows = list9.ToArray();
			monsterAttackParameter.endTrackingWindows = list10.ToArray();
			monsterAttackParameter.attackPoints = list11.ToArray();
			monsterAttackParameter.comboCount = numberItemInCell;
			m_parameters.Add(rowsName, monsterAttackParameter);
		}
	}
}
