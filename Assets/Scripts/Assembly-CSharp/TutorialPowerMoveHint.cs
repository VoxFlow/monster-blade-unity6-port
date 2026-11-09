using System.Collections;
using UnityEngine;

public class TutorialPowerMoveHint : MonoBehaviour
{
	private UIImageButton m_blockButton;

	private PlayerController m_controller;

	private Player m_player;

	private bool m_showing;

	private bool m_init;

	private float m_retryTime;

	private float m_retryDuration = 5f;

	private UILabel m_labelPrefab;

	private void OnDisable()
	{
		Stop();
	}

	private void Awake()
	{
		m_labelPrefab = (Resources.Load("UI/Prefabs/Generic/UILabel") as GameObject).GetComponent<UILabel>();
	}

	private void Start()
	{
		m_blockButton = SingletonComponent<BattleScreenUI>.Instance.m_blockButton;
		m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
		m_init = true;
		Show();
	}

	private void Show()
	{
		Pulse();
		m_showing = true;
	}

	private void Pulse()
	{
		Vector3 vector = Vector3.one * 2f;
		iTween.ScaleTo(m_blockButton.gameObject, iTween.Hash("scale", vector, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "onstart", "SpawnBlockText", "onstarttarget", base.gameObject));
		iTween.ScaleTo(m_blockButton.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.1f, "easetype", iTween.EaseType.easeOutExpo, "delay", 0.2f));
		iTween.ScaleTo(m_blockButton.gameObject, iTween.Hash("scale", vector, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "delay", 0.3f, "onstart", "SpawnBlockText", "onstarttarget", base.gameObject));
		iTween.ScaleTo(m_blockButton.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.1f, "easetype", iTween.EaseType.easeOutExpo, "onstart", "SpawnSlashText", "onstarttarget", base.gameObject, "oncomplete", "Slash", "oncompletetarget", base.gameObject, "delay", 0.5f));
	}

	private void SpawnBlockText()
	{
		UILabel uILabel = Object.Instantiate(m_labelPrefab) as UILabel;
		uILabel.text = "Block!";
		uILabel.transform.parent = UIPopupController.Instance.Panel;
		uILabel.transform.localPosition = Vector3.zero;
		uILabel.transform.localScale = new Vector3(70f, 70f, 1f);
		SelfDestroy selfDestroy = uILabel.gameObject.AddComponent<SelfDestroy>();
		selfDestroy.m_duration = 3f;
		TweenPosition.Begin(uILabel.gameObject, 3f, uILabel.transform.localPosition + Vector3.up * 500f);
		TweenColor.Begin(uILabel.gameObject, 3f, Color.clear);
	}

	private void SpawnSlashText()
	{
		UILabel uILabel = Object.Instantiate(m_labelPrefab) as UILabel;
		uILabel.text = "Slash!";
		uILabel.transform.parent = UIPopupController.Instance.Panel;
		uILabel.transform.localPosition = -Vector3.up * 10f;
		uILabel.transform.localScale = new Vector3(70f, 70f, 1f);
		SelfDestroy selfDestroy = uILabel.gameObject.AddComponent<SelfDestroy>();
		selfDestroy.m_duration = 3f;
		TweenPosition.Begin(uILabel.gameObject, 3f, uILabel.transform.localPosition + Vector3.up * 500f);
		TweenColor.Begin(uILabel.gameObject, 3f, Color.clear);
	}

	private void Slash()
	{
		StartCoroutine("DoWait", 6f);
	}

	private IEnumerator DoWait(float p_duration)
	{
		yield return new MBWaitForSeconds(p_duration);   // PORT (CD-059)
		Pulse();
	}

	private void Stop()
	{
		iTween.Stop(m_blockButton.gameObject);
		StopCoroutine("DoWait");
		m_showing = false;
	}

	private void Update()
	{
		if (m_player.IsBlocking())
		{
			m_retryTime = 0f;
			if (m_showing)
			{
				Stop();
			}
		}
		else if (!m_showing && m_init)
		{
			m_retryTime += Time.deltaTime;
			if (m_retryTime >= m_retryDuration)
			{
				Show();
			}
		}
	}
}
