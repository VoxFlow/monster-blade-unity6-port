using System.Collections.Generic;

public class PlayerAttackParameterDB : Singleton<PlayerAttackParameterDB>
{
	public const string PARAMETER_FILE = "MaleAttack";

	public const string DAMAGE_WINDOW = "DAMAGE_WINDOW";

	public const string WIND_DOWN = "WIND_DOWN";

	public const string ATTACK_END = "COMBO_END";

	public const string DIRECTION = "DIRECTION";

	private Dictionary<string, PlayerAttackParameter> m_parameters = new Dictionary<string, PlayerAttackParameter>();

	public PlayerAttackParameter this[string p_animName]
	{
		get
		{
			if (p_animName == null)
			{
				return null;
			}
			PlayerAttackParameter value = null;
			if (!m_parameters.TryGetValue(p_animName, out value))
			{
				Dbg.LogWarn(p_animName + " is not available.");
			}
			return value;
		}
	}

	private PlayerAttackParameterDB()
	{
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!instance.IsDataInitialized)
		{
			instance.InitManager();
		}
		NBBinaryFormat parametersFor = instance.GetParametersFor("MaleAttack");
		foreach (string rowsName in parametersFor.RowsNames)
		{
			int numberItemInCell = instance.GetNumberItemInCell("MaleAttack", rowsName, "DIRECTION");
			List<PlayerAttack.Direction> list = new List<PlayerAttack.Direction>();
			List<float> list2 = new List<float>();
			List<float> list3 = new List<float>();
			float result = 0f;
			float result2 = 0f;
			for (int i = 0; i < numberItemInCell; i++)
			{
				string parameter = instance.GetParameter("MaleAttack", rowsName, "DIRECTION", i);
				PlayerAttack.Direction directionFromString = PlayerAttack.GetDirectionFromString(parameter);
				list.Add(directionFromString);
				if (float.TryParse(instance.GetParameter("MaleAttack", rowsName, "DAMAGE_WINDOW", i), out result2))
				{
					list2.Add(result2);
				}
				else
				{
					list2.Add(0f);
				}
				if (float.TryParse(instance.GetParameter("MaleAttack", rowsName, "WIND_DOWN", i), out result2))
				{
					list3.Add(result2);
				}
				else
				{
					list3.Add(0f);
				}
			}
			float.TryParse(instance.GetParameter("MaleAttack", rowsName, "COMBO_END"), out result);
			PlayerAttackParameter value = new PlayerAttackParameter(list.ToArray(), list2.ToArray(), list3.ToArray(), result);
			m_parameters.Add(rowsName, value);
		}
	}

	public int GetNumCombo(string p_animName)
	{
		PlayerAttackParameter playerAttackParameter = this[p_animName];
		if (playerAttackParameter == null)
		{
			return 0;
		}
		return playerAttackParameter.damageWindows.Length;
	}
}
