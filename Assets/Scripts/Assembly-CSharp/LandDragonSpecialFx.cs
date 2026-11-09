using UnityEngine;

public class LandDragonSpecialFx : MonsterSpecialFx
{
	public GameObject m_fxGroundDigPrefab;

	public GameObject m_fxGroundShockWavePrefab;

	public GameObject m_fxGroundShockWaveSmallPrefab;

	public GameObject m_fxGroundTremorPrefab;

	public GameObject m_fxSandSinkingPrefab;

	public GameObject m_fxFullRakePrefab;

	public GameObject m_fxFootDustPrefab;

	public ParticleSystemController m_fxGroundSnap;

	public ParticleSystemController m_fxGroundTrail;

	private ParticleSystemController m_fxSandSinking;

	private void OnEmerge()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			PlaySnapFx();
			if (m_fxGroundTrail != null)
			{
				m_fxGroundTrail.Stop();
			}
		}
	}

	private void OnEmergeDown()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			GameObject gameObject = Object.Instantiate(m_fxGroundShockWaveSmallPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = base.transform.position;
			}
			ShakeCamera("Emerge");
		}
	}

	private void OnStumbleDown()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			GameObject gameObject = Object.Instantiate(m_fxGroundShockWaveSmallPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = base.transform.position;
			}
			ShakeCamera("Drop");
		}
	}

	private void OnSlam()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			GameObject gameObject = Object.Instantiate(m_fxGroundShockWavePrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = base.transform.position;
			}
			ShakeCamera("Slam");
		}
	}

	private void OnSnap()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			PlaySnapFx();
			if (m_fxGroundTrail != null)
			{
				m_fxGroundTrail.Stop();
			}
		}
	}

	private void OnRake()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene && m_fxFullRakePrefab != null)
		{
			GameObject gameObject = Object.Instantiate(m_fxFullRakePrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = Vector3.forward * -8f;
				gameObject.transform.LookAt(base.transform.TransformPoint(Vector3.forward));
			}
		}
	}

	private void OnDive()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			GameObject gameObject = Object.Instantiate(m_fxGroundDigPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = base.transform.position - Vector3.forward * 6.5f;
			}
			ShakeCamera("Emerge");
		}
	}

	public void SetSignActive(bool active, Vector3 position)
	{
		if (active)
		{
			position.y = 0.7f;
			GameObject gameObject = Object.Instantiate(m_fxSandSinkingPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = position;
			}
		}
	}

	public void SetTrailActive(bool active)
	{
		if (m_fxGroundTrail != null)
		{
			m_fxGroundTrail.Play();
		}
	}

	public void SetTrailPosition(Vector3 position)
	{
		if (m_fxGroundTrail != null)
		{
			position.y = 0.1f;
			m_fxGroundTrail.transform.position = position;
		}
	}

	private void ShakeEmerge()
	{
		Shake(1f, new Vector3(0.2f, 0.2f, 0.4f));
	}

	private void Shake(float duration, Vector3 amplitude, float frequency = 20f)
	{
		CameraShakeData cameraShakeData = new CameraShakeData();
		if (m_battleCamera != null && cameraShakeData != null)
		{
			cameraShakeData.duration = duration;
			cameraShakeData.frequency = 20f;
			cameraShakeData.amplitude = new Vector3(0.1f, 0.1f, 0.2f);
			m_battleCamera.ShakeCamera(cameraShakeData);
		}
	}

	private void PlaySnapFx()
	{
		if (m_fxGroundSnap != null)
		{
			m_fxGroundSnap.transform.position = base.transform.position + Vector3.forward * 3f;
			m_fxGroundSnap.Play();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.GetComponent<Animation>() != null && m_monster != null && m_monster.CurrentState == Monster.State.Cutscene)
		{
			if (base.GetComponent<Animation>().IsPlaying(LandDragon.RAKE_ATK_ANIM) || base.GetComponent<Animation>().IsPlaying(LandDragon.SNAP_ATK_ANIM) || base.GetComponent<Animation>().IsPlaying("Intro"))
			{
				m_monster.EnableShadow(false);
			}
			else
			{
				m_monster.EnableShadow(m_monster.IsVisibile());
			}
		}
	}
}
