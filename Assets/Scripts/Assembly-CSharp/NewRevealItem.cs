using System.Collections;
using UnityEngine;

public class NewRevealItem : MonoBehaviour
{
	[SerializeField]
	private Camera m_newItemCamera;

	[SerializeField]
	private GameObject m_chestRootBone;

	[SerializeField]
	private GameObject[] m_chests;

	[SerializeField]
	private NewEggRevealItemController m_eggController;

	[SerializeField]
	private GameObject m_effectObj;

	[SerializeField]
	private GameObject m_groundObj;

	private int m_chestNeedToActive;

	private Animation m_animtion;

	private bool m_isShowing;

	private float dirtyTimer = 4f;

	private void OnEnable()
	{
		m_isShowing = true;
		dirtyTimer = 4f;
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.EnableMainMenuCameras(false);
		}
		if (m_newItemCamera != null)
		{
			m_newItemCamera.gameObject.SetActiveRecursively(true);
		}
		else
		{
			Dbg.LogWarn("Unable to find m_newItemCamera");
		}
		for (int i = 0; i < m_chests.Length; i++)
		{
			if (i != m_chestNeedToActive)
			{
				m_chests[i].gameObject.SetActiveRecursively(false);
				Dbg.Log("[Info] set chest : " + i + " to deactive");
			}
			else
			{
				m_chests[i].gameObject.SetActiveRecursively(true);
			}
		}
		m_effectObj.SetActiveRecursively(true);
		m_groundObj.SetActiveRecursively(true);
		Dbg.Log("The NewRevealItem game object enabled");
	}

	private void OnDisable()
	{
		m_chestNeedToActive = 0;
		m_isShowing = false;
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.EnableMainMenuCameras(true);
		}
		Dbg.Log("The NewRevealItem game object disable");
	}

	private void Start()
	{
		m_animtion = GetComponent<Animation>();
		if (m_animtion == null)
		{
			Dbg.LogWarn("NewRevealItem cannot get animtion, it might unable to control the animation!!");
		}
	}

	private void Update()
	{
		if (m_animtion.isPlaying && ((double)m_animtion["Anim_ItemRevealFlash"].normalizedTime > 0.99 || dirtyTimer <= 0f))
		{
			m_animtion.Stop("Anim_ItemRevealFlash");
			Dbg.LogWarn("Anim stuck!, try to deactive");
			StartCoroutine(DelayDeactive());
		}
		if ((!SingletonComponent<TutorialManager>.Instance.enabled || SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AllCombats)) && (MBInput.GetMouseButton(0) || (MBInput.touchCount > 0 && MBInput.touches[0].phase == TouchPhase.Ended)))
		{
			Dbg.Log("User taps the screen to skip");
			m_chestNeedToActive = 0;
			m_isShowing = false;
			m_newItemCamera.gameObject.SetActiveRecursively(false);
			base.gameObject.SetActiveRecursively(false);
		}
		dirtyTimer -= Time.deltaTime;
	}

	private IEnumerator DelayDeactive()
	{
		Dbg.LogWarn("DelayDeactive");
		yield return new MBWaitForSeconds(0.6f);   // PORT (CD-059)
		ResetParams();
		yield return new MBWaitForSeconds(0.4f);   // PORT (CD-059)
		m_newItemCamera.gameObject.SetActiveRecursively(false);
		base.gameObject.SetActiveRecursively(false);
	}

	public void ResetParams()
	{
		m_chestNeedToActive = 0;
		m_isShowing = false;
		dirtyTimer = 4f;
	}

	public void SetChest(int index = 0)
	{
		m_chestNeedToActive = index;
		m_eggController.gameObject.SetActiveRecursively(false);
	}

	public void EnableRareEgg(bool p_enable)
	{
		m_eggController.EnableRareEgg(p_enable);
	}

	public void EnableEggs(bool p_enable)
	{
		m_eggController.EnableEggs(p_enable);
	}

	public bool IsShowing()
	{
		return m_isShowing;
	}

	public void EnableAllChests(bool p_enable)
	{
		GameObject[] chests = m_chests;
		foreach (GameObject gameObject in chests)
		{
			gameObject.SetActiveRecursively(p_enable);
		}
		m_chestRootBone.SetActiveRecursively(p_enable);
	}
}
