using UnityEngine;

public class CharacterDamageDisplay : MonoBehaviour
{
	public string m_referencePointGameObject;

	public string m_damageValueHUD;

	public Transform m_attachRoot;

	private GameObject m_damagePoint;

	protected DamageValueContainer m_damageValue;

	protected virtual void Awake()
	{
		if (SingletonComponent<BattleManager>.Instance != null)
		{
			base.enabled = true;
		}
		else
		{
			base.enabled = false;
		}
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
	}

	protected virtual void Start()
	{
		GameObject gameObject = GameObject.Find(m_damageValueHUD);
		if (gameObject == null)
		{
			Debug.LogError("Cannot find " + m_damageValueHUD + " game-object in the scene.");
			return;
		}
		m_damageValue = MBHier.GetComponentInChildren<DamageValueContainer>(gameObject);
		if (m_damageValue == null)
		{
			Debug.LogError("There's no 'DamageValueContainer' component attached to the game-object: " + gameObject);
			return;
		}
		m_damagePoint = Object.Instantiate(GameObject.Find(m_referencePointGameObject)) as GameObject;
		Camera battleCamera = null;
		GameObject gameObject2 = GameObject.Find("Camera_Root");
		if (gameObject2 != null)
		{
			BattleCamera component = gameObject2.GetComponent<BattleCamera>();
			if ((bool)component && (bool)component.MainCameraNode)
			{
				battleCamera = component.MainCameraNode.GetComponent<Camera>();
			}
		}
		GameObject gameObject3 = GameObject.Find("Battle HUD Camera");
		m_damageValue.BattleCamera = battleCamera;
		m_damageValue.UiCam = ((!(gameObject3 != null)) ? null : gameObject3.GetComponent<Camera>());
		m_damageValue.ReferencePosition = m_damagePoint.transform;
		m_damagePoint.transform.position += base.transform.position;
		if (null == m_attachRoot)
		{
			m_damagePoint.transform.parent = base.transform;
		}
		else
		{
			m_damagePoint.transform.parent = m_attachRoot;
		}
	}

	public virtual void ReStart()
	{
		if (m_damageValue == null)
		{
			Start();
		}
	}

	protected virtual void OnDestroy()
	{
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
	}

	protected virtual void OnHit(DamageData p_damage)
	{
		DisplayDamageValue(p_damage);
	}

	protected virtual void OnHeal(float p_hp)
	{
		DamageData damageData = new DamageData();
		damageData.rawDamage = p_hp;
		damageData.finalDamage = p_hp;
		damageData.attacker = null;
		damageData.defender = null;
		damageData.damageType = DamageType.Heal;
		DisplayDamageValue(damageData);
	}

	public void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		Dbg.Log("CharacterDamageDisplay.OnCutsceneStart");
		if (m_damageValue != null)
		{
			m_damageValue.HideAllDamageValue();
		}
		else
		{
			Dbg.Log("CharacterDamageDisplay.OnCutsceneStart:  m_damageValue is null!!!!!!!");
		}
	}

	public virtual void DisplayDamageValue(DamageData p_damage)
	{
		bool flag = m_damageValue != null;
		if (flag & ((p_damage.damageType != DamageType.Block && p_damage.damageType != DamageType.None) || p_damage.finalDamage != 0f))
		{
			NormalDamageValueContainer normalDamageValueContainer = (NormalDamageValueContainer)m_damageValue;
			Element.Type p_element = ((p_damage.damageType == DamageType.Heal) ? Element.Type.Max : p_damage.attacker.AttackElement);
			normalDamageValueContainer.DisplayDamage((int)p_damage.rawDamage, (int)p_damage.finalDamage, p_damage.damageType, p_element);
		}
	}
}
