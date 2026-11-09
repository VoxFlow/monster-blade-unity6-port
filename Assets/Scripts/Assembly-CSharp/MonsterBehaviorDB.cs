using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviorDB : Singleton<MonsterBehaviorDB>
{
	private string m_behaviorDbKey = "Behavior";

	private string m_triggerDbKey = "CombatBehaviour";

	private string m_triggersColumn = "HP_TRIGGERS";

	private string m_behaviorsColumn = "BEHAVIOUR_SET";

	private Dictionary<string, MonsterBehavior> m_behaviors;

	protected MonsterBehaviorDB()
	{
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!(instance == null))
		{
			NBBinaryFormat parametersFor = instance.GetParametersFor(m_behaviorDbKey);
			int rowsCount = parametersFor.RowsCount;
			string[] array = parametersFor.RowsNames.ToArray();
			m_behaviors = new Dictionary<string, MonsterBehavior>();
			for (int i = 0; i < rowsCount; i++)
			{
				MonsterBehavior monsterBehavior = new MonsterBehavior
				{
					name = array[i]
				};
				Dbg.Log("Adding behavior: " + monsterBehavior.name);
				monsterBehavior.comboCount = parametersFor.GetDataGeneric<int>(array[i], "Combo_Count");
				monsterBehavior.hasAdvanceAttack = parametersFor.GetData(array[i], "Advanced") == "TRUE";
				monsterBehavior.hasSpecialAttack = parametersFor.GetData(array[i], "Special") == "TRUE";
				m_behaviors.Add(monsterBehavior.name, monsterBehavior);
			}
		}
	}

	public MonsterBehavior GetBehavior(string p_name)
	{
		MonsterBehavior result = null;
		if (m_behaviors.ContainsKey(p_name))
		{
			result = m_behaviors[p_name];
		}
		else
		{
			Debug.LogError("Invalid behavior: " + p_name);
		}
		return result;
	}

	public MonsterBehavior.Trigger[] GetTriggers(int p_level, bool p_isBoss)
	{
		string text = ((!p_isBoss) ? "Trash_" : "Boss_") + p_level;
		Dbg.Log("Retreiving behavior triggers of '" + text + "'");
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		int numberItemInCell = instance.GetNumberItemInCell(m_triggerDbKey, text, m_triggersColumn);
		Dbg.Log("Number of Triggers : " + numberItemInCell + " : " + m_triggerDbKey + " " + text + " " + m_triggersColumn);
		MonsterBehavior.Trigger[] array = new MonsterBehavior.Trigger[numberItemInCell];
		for (int i = 0; i < numberItemInCell; i++)
		{
			float p_parameter = float.Parse(instance.GetParameter(m_triggerDbKey, text, m_triggersColumn, i));
			string parameter = instance.GetParameter(m_triggerDbKey, text, m_behaviorsColumn, i);
			array[i] = new MonsterBehavior.Trigger(p_parameter, parameter);
		}
		MBSort.Sort(array);
		return array;
	}
}
