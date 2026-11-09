using UnityEngine;

public class PlayerPetrifiedLogic : PetrifiedLogic
{
	private GameObject m_cameraRoot;

	private Quaternion m_cameraDefaultRot = Quaternion.identity;

	private OrbitAroundTarget3D m_orbitScript;

	private float m_saveTime;

	private float m_saveTimePerTap = 0.125f;

	private float m_maxSaveTime = 5f;

	public override void Init()
	{
		base.Init();
		m_orbitScript = m_player.gameObject.GetComponent<OrbitAroundTarget3D>();
		m_shaker = m_player.gameObject.AddComponent<MonsterShakeScript>();
		if (null != m_shaker)
		{
			m_shaker.m_amplitude = 0.01f;
			m_shaker.m_frequency = (m_shaker.m_duration = 0.5f);
		}
		if (null != m_player)
		{
			base.charData = m_player.Data;
		}
		base.petrifyFx.shakeParticleRoot = m_player.transform;
		base.petrifyFx.targetRenderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(m_player.gameObject);
		if (null == m_cameraRoot)
		{
			m_cameraRoot = GameObject.Find("Camera_Root");
		}
	}

	public override void OnPetrified(float p_duration, float p_armourMod, Callback p_onBreak)
	{
		base.OnPetrified(p_duration, p_armourMod, p_onBreak);
		if (!(null == m_playerController) && !(null == m_player))
		{
			m_player.ChangeState(Player.State.Idle);
			m_player.GetComponent<Animation>().enabled = false;
			m_playerController.enabled = false;
			m_playerController.Pointer.enabled = true;
			if (m_orbitScript != null)
			{
				m_orbitScript.StopOrbit();
			}
			if (m_cameraRoot != null)
			{
				m_cameraDefaultRot = m_cameraRoot.transform.rotation;
			}
			SingletonComponent<BattleScreenUI>.Instance.ShowFocusRing(true, Vector3.zero);
			m_saveTime = 0f;
			base.enabled = true;
		}
	}

	public override void OnBreak()
	{
		base.OnBreak();
		if (!(null == m_playerController) && !(null == m_player))
		{
			m_playerController.enabled = true;
			m_player.GetComponent<Animation>().enabled = true;
			m_player.Idle();
			SingletonComponent<BattleScreenUI>.Instance.ShowFocusRing(false);
		}
	}

	public override void OnShattered()
	{
		base.OnShattered();
		m_player.GetComponent<Animation>().enabled = true;
		if (m_player.AnimPlayer.CurrentAnimName != "Death")
		{
			m_player.AnimPlayer.Play("Death");
		}
		m_player.fader.Fade(0.5f, 0f, 1.5f, null);
		SingletonComponent<SlowMotionSystem>.Instance.SlowTheTime(4f, 1.25f);
		SingletonComponent<BattleScreenUI>.Instance.ShowFocusRing(false);
	}

	protected override void OnUpdate()
	{
		if (base.petrified && !(null == m_playerController) && !(null == m_player))
		{
			if (m_playerController.enabled)
			{
				m_playerController.enabled = false;
			}
			if (m_player.GetComponent<Animation>().enabled)
			{
				m_player.GetComponent<Animation>().enabled = false;
			}
			if (m_playerController.Pointer.m_state == Pointer.State.Down && m_saveTime < m_maxSaveTime)
			{
				base.timer.ResetCounterTo(base.timer.Counter - m_saveTimePerTap);
				m_saveTime += m_saveTimePerTap;
				Shake();
			}
		}
	}

	private new void LateUpdate()
	{
		if (base.petrified && null != m_cameraRoot)
		{
			m_cameraRoot.transform.rotation = m_cameraDefaultRot;
		}
	}
}
