using Models;
using UnityEngine;

public class PetSetup : Singleton<PetSetup>
{
	public static readonly string PET_SETUP_DB_NAME = "PetSetup";

	private static readonly string PET_PREFAB = "PREFAB";

	private static readonly string PET_NAME = "PET_NAME";

	private static readonly string PET_IMAGE = "PET_ICON";

	private static readonly string PET_ELEMENT = "PET_ELEMENT";

	private static readonly string FRAME_START_NAME = "frame_pet_";

	private GameObject m_pet;

	private NBBinaryFormat m_petSetupCsv;

	private NBBinaryFormat petSetupCsv
	{
		get
		{
			if (m_petSetupCsv == null)
			{
				ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
				if (null == instance)
				{
					return null;
				}
				m_petSetupCsv = instance.GetParametersFor(PET_SETUP_DB_NAME);
				if (m_petSetupCsv == null)
				{
					return null;
				}
			}
			return m_petSetupCsv;
		}
	}

	private PetSetup()
	{
	}

	public string GetPetPrefabPath(string p_id)
	{
		string result = string.Empty;
		if (petSetupCsv != null)
		{
			result = petSetupCsv.GetData(p_id, PET_PREFAB);
		}
		return result;
	}

	public string GetImage(string p_id)
	{
		string result = string.Empty;
		if (petSetupCsv != null)
		{
			result = petSetupCsv.GetData(p_id, PET_IMAGE);
		}
		return result;
	}

	public PetDetails GetPetDetails(string p_id)
	{
		PetDetails p_pet = new PetDetails(p_id);
		GetExtraInfo(ref p_pet);
		return p_pet;
	}

	public void GetExtraInfo(ref PetDetails p_pet)
	{
		if (p_pet != null && !(string.Empty == p_pet.m_petId) && petSetupCsv != null)
		{
			string data = petSetupCsv.GetData(p_pet.m_petId, PET_NAME);
			string petPrefabPath = GetPetPrefabPath(p_pet.m_petId);
			string image = GetImage(p_pet.m_petId);
			string data2 = petSetupCsv.GetData(p_pet.m_petId, PET_ELEMENT);
			string p_frameName = FRAME_START_NAME + data2.ToLower();
			p_pet.SetExtraInfo(petPrefabPath, data, image, p_frameName);
		}
	}
}
