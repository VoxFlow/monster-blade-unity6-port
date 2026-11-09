using System;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneSetup : MonoBehaviour
{
	private class FxData
	{
		public string path;

		public GameObject gameObject;
	}

	public GameObject m_playerObject;

	public GameObject m_enemyObject;

	public GameObject m_friendObject;

	private Vector3 m_enemyDefaultScale = Vector3.one;

	private Camera[] m_cutsceneCameras;

	private Dictionary<string, List<CutsceneAnimSq>> m_cachedScripts = new Dictionary<string, List<CutsceneAnimSq>>();

	private Dictionary<string, List<FxData>> m_playerEffects = new Dictionary<string, List<FxData>>();

	private Dictionary<string, List<FxData>> m_enemyEffects = new Dictionary<string, List<FxData>>();

	private Dictionary<string, List<FxData>> m_friendEffects = new Dictionary<string, List<FxData>>();

	private List<Transform> m_lookAtTargets = new List<Transform>();

	private void Awake()
	{
		base.enabled = false;
		if (m_playerObject != null || m_enemyObject != null || m_friendObject != null)
		{
			PrepareCutsceneData();
		}
		Messenger.AddListener(CutsceneMessage.CUTSCENE_PLAYER_BEGIN, OnCutsceneStartPlaying);
		Messenger.AddListener(CutsceneMessage.CUTSCENE_PLAYER_FINISH, OnCutsceneFinishPlaying);
		Messenger<GameObject>.AddListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
		Messenger<GameObject>.AddListener(CutsceneMessage.CUTSCENE_EVENT_DISABLED, OnCutsceneDisabled);
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener(CutsceneMessage.CUTSCENE_PLAYER_BEGIN, OnCutsceneStartPlaying);
		Messenger.RemoveListener(CutsceneMessage.CUTSCENE_PLAYER_FINISH, OnCutsceneFinishPlaying);
		Messenger<GameObject>.RemoveListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
		Messenger<GameObject>.RemoveListener(CutsceneMessage.CUTSCENE_EVENT_DISABLED, OnCutsceneDisabled);
	}

	private void OnCutsceneStartPlaying()
	{
		if (m_playerObject != null)
		{
			m_playerObject.active = false;
			ShowCharacter(m_playerObject, false);
		}
		if (m_enemyObject != null)
		{
			m_enemyDefaultScale = m_enemyObject.transform.localScale;
			m_enemyObject.active = false;
			ShowCharacter(m_enemyObject, false);
		}
		if (m_friendObject != null)
		{
			m_friendObject.active = false;
			ShowCharacter(m_friendObject, false);
		}
	}

	private void OnCutsceneFinishPlaying()
	{
		foreach (KeyValuePair<string, List<CutsceneAnimSq>> cachedScript in m_cachedScripts)
		{
			RemovesCachedScripts(cachedScript.Value);
		}
		m_cachedScripts.Clear();
		ClearCachedEffects(m_playerEffects);
		ClearCachedEffects(m_enemyEffects);
		ClearCachedEffects(m_friendEffects);
		if (m_playerObject != null)
		{
			m_playerObject.transform.parent = null;
			m_playerObject.active = true;
			ShowCharacter(m_playerObject, true);
		}
		if (m_enemyObject != null)
		{
			m_enemyObject.transform.parent = null;
			m_enemyObject.transform.localScale = m_enemyDefaultScale;
			m_enemyObject.active = true;
			ShowCharacter(m_enemyObject, true);
		}
		if (m_friendObject != null)
		{
			m_friendObject.transform.parent = null;
		}
		foreach (Transform lookAtTarget in m_lookAtTargets)
		{
			if (lookAtTarget != null)
			{
				UnityEngine.Object.Destroy(lookAtTarget.gameObject);
			}
		}
	}

	private void OnCutsceneEnabled(GameObject p_obj)
	{
		if (!base.gameObject.active)
		{
			return;
		}
		Transform transform = null;
		if (m_playerObject != null)
		{
			transform = p_obj.transform.FindInHierarchy(CompareTag, "Player");
			AttachCharacter(m_playerObject, p_obj, transform, m_playerEffects);
		}
		if (m_enemyObject != null)
		{
			transform = p_obj.transform.FindInHierarchy(CompareTag, "Enemy");
			AttachCharacter(m_enemyObject, p_obj, transform, m_enemyEffects);
		}
		if (!(m_friendObject != null))
		{
			return;
		}
		transform = p_obj.transform.FindInHierarchy(CompareTag, "Ally");
		AttachCharacter(m_friendObject, p_obj, transform, m_friendEffects);
		BattleScreenUI instance = SingletonComponent<BattleScreenUI>.Instance;
		GameObject gameObject = GameObject.Find("Battle HUD Camera");
		if (instance != null && gameObject != null && gameObject.GetComponent<Camera>() != null && instance.FriendLabel != null && m_cutsceneCameras != null)
		{
			instance.FriendLabel.BattleCamera = Array.Find(m_cutsceneCameras, (Camera cam) => cam.enabled && cam.gameObject.active);
			instance.FriendLabel.UiCam = gameObject.GetComponent<Camera>();
		}
	}

	private void OnCutsceneDisabled(GameObject p_obj)
	{
		if (base.gameObject.active)
		{
			DetachCharacter(m_playerObject, p_obj, m_playerEffects);
			DetachCharacter(m_enemyObject, p_obj, m_enemyEffects);
			DetachCharacter(m_friendObject, p_obj, m_friendEffects);
			List<CutsceneAnimSq> value = null;
			if (m_cachedScripts.TryGetValue(p_obj.name, out value))
			{
				RemovesCachedScripts(value);
				m_cachedScripts.Remove(p_obj.name);
			}
		}
	}

	public void PrepareCutsceneData()
	{
		Func<Transform, object, bool> p_function = (Transform current, object obj) => current.gameObject.GetComponent((Type)obj) != null;
		Transform[] array = base.transform.FindChildrenInHierarchy(p_function, typeof(Camera));
		int num = -257;
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			transform.GetComponent<Camera>().cullingMask &= num;
			LookAt component = transform.gameObject.GetComponent<LookAt>();
			if (component != null)
			{
				Transform target = component.target;
				if (target.root.gameObject != m_playerObject && target.root.gameObject != m_enemyObject && target.root.gameObject != m_friendObject)
				{
					if (component.targetTagName == "Player")
					{
						if (m_playerObject != null)
						{
							AttachNode(component.target, m_playerObject.transform, component.targetPath);
						}
					}
					else if (component.targetTagName == "Enemy")
					{
						if (m_enemyObject != null)
						{
							AttachNode(component.target, m_enemyObject.transform, component.targetPath);
						}
					}
					else if (component.targetTagName == "Ally" && m_friendObject != null)
					{
						AttachNode(component.target, m_friendObject.transform, component.targetPath);
					}
					if (target.parent != null)
					{
						m_lookAtTargets.Add(target);
					}
				}
			}
			string[] array3 = new string[3] { "AudioListener", "GUILayer", "FlareLayer" };
			string[] array4 = array3;
			foreach (string type in array4)
			{
				Component component2 = transform.gameObject.GetComponent(type);
				if (component2 != null)
				{
					UnityEngine.Object.Destroy(component2);
				}
			}
		}
		m_cutsceneCameras = MBHier.GetComponentsInChildren<Camera>(base.transform);
		PrepareCharacter("Player", m_playerEffects);
		PrepareCharacter("Enemy", m_enemyEffects);
		PrepareCharacter("Ally", m_friendEffects);
	}

	private void PrepareCharacter(string p_tag, Dictionary<string, List<FxData>> p_effects)
	{
		Transform[] array = base.transform.FindChildrenInHierarchy(CompareTag, p_tag);
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			Animation component = transform.gameObject.GetComponent<Animation>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			PrepareEffectDict(p_effects, transform);
			for (int j = 0; j < transform.childCount; j++)
			{
				UnityEngine.Object.Destroy(transform.GetChild(j).gameObject);
			}
		}
	}

	private void AttachCharacter(GameObject p_character, GameObject p_root, Transform p_transform, Dictionary<string, List<FxData>> p_effectDict)
	{
		if (!(p_transform != null))
		{
			return;
		}
		string key = p_root.gameObject.name;
		p_character.transform.parent = p_transform;
		p_character.transform.localPosition = Vector3.zero;
		p_character.transform.localRotation = Quaternion.identity;
		p_character.SetActiveRecursively(true);
		ShowCharacter(p_character, true);
		CutsceneAnimSq component = p_character.GetComponent<CutsceneAnimSq>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		component = p_transform.gameObject.GetComponent<CutsceneAnimSq>();
		if (component != null)
		{
			CutsceneAnimSq cutsceneAnimSq = (CutsceneAnimSq)p_character.AddComponent(component.GetType());
			cutsceneAnimSq.CopyPlayList(component);
			cutsceneAnimSq.Play();
			List<CutsceneAnimSq> list = null;
			if (m_cachedScripts.ContainsKey(key))
			{
				list = m_cachedScripts[key];
			}
			else
			{
				list = new List<CutsceneAnimSq>();
				m_cachedScripts.Add(key, list);
			}
			UnityEngine.Object.Destroy(component);
			list.Add(cutsceneAnimSq);
		}
		List<FxData> value = null;
		if (p_effectDict.TryGetValue(key, out value))
		{
			AttachParticleFx(value, p_character.transform);
		}
	}

	private void DetachCharacter(GameObject p_character, GameObject p_root, Dictionary<string, List<FxData>> p_effects)
	{
		List<FxData> value = null;
		if (p_character != null && p_character.transform.IsChildOf(p_root.transform))
		{
			if (p_effects.TryGetValue(p_root.name, out value))
			{
				RemovesCachedEffects(value);
				p_effects.Remove(p_root.name);
			}
			p_character.transform.parent = null;
			ShowCharacter(p_character, false);
		}
	}

	private void RemovesCachedScripts(List<CutsceneAnimSq> p_scripts)
	{
		foreach (CutsceneAnimSq p_script in p_scripts)
		{
			if (p_script != null)
			{
				UnityEngine.Object.Destroy(p_script);
			}
		}
	}

	private void ClearCachedEffects(Dictionary<string, List<FxData>> p_effectDict)
	{
		foreach (KeyValuePair<string, List<FxData>> item in p_effectDict)
		{
			RemovesCachedEffects(item.Value);
		}
		p_effectDict.Clear();
	}

	private void RemovesCachedEffects(List<FxData> p_effects)
	{
		foreach (FxData p_effect in p_effects)
		{
			if (p_effect != null && p_effect.gameObject != null)
			{
				UnityEngine.Object.Destroy(p_effect.gameObject);
			}
		}
	}

	private void PrepareEffectDict(Dictionary<string, List<FxData>> p_dict, Transform p_target)
	{
		Transform[] array = p_target.FindChildrenInHierarchy(CompareTag, "FX");
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			FxData fxData = new FxData();
			fxData.gameObject = transform.gameObject;
			fxData.path = transform.GetPathBetween(p_target);
			string text = transform.GetPathBetween(transform.root);
			if (text != string.Empty)
			{
				int num = text.IndexOf('/');
				if (num > 0)
				{
					text = text.Remove(text.IndexOf('/'));
				}
				List<FxData> list = null;
				if (p_dict.ContainsKey(text))
				{
					list = p_dict[text];
				}
				else
				{
					list = new List<FxData>();
					p_dict.Add(text, list);
				}
				list.Add(fxData);
			}
			Vector3 localPosition = transform.localPosition;
			Quaternion localRotation = transform.localRotation;
			transform.parent = null;
			transform.localPosition = localPosition;
			transform.localRotation = localRotation;
			transform.gameObject.SetActiveRecursively(false);
		}
	}

	private void AttachParticleFx(List<FxData> p_effects, Transform p_target)
	{
		foreach (FxData p_effect in p_effects)
		{
			if (p_effect != null)
			{
				AttachNode(p_effect.gameObject.transform, p_target, p_effect.path);
				p_effect.gameObject.SetActiveRecursively(true);
			}
		}
	}

	private void ShowCharacter(GameObject p_character, bool p_show)
	{
		SkinnedMeshRenderer[] componentsInChildren = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(p_character);
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (skinnedMeshRenderer != null)
			{
				skinnedMeshRenderer.enabled = p_show;
			}
		}
		ShadowProjector componentInChildren = MBHier.GetComponentInChildren<ShadowProjector>(p_character);
		if (componentInChildren != null)
		{
			componentInChildren.enabled = p_show;
		}
		ParticleSystemController[] componentsInChildren2 = MBHier.GetComponentsInChildren<ParticleSystemController>(p_character);
		ParticleSystemController[] array2 = componentsInChildren2;
		foreach (ParticleSystemController particleSystemController in array2)
		{
			particleSystemController.Stop();
		}
	}

	private void AttachNode(Transform p_node, Transform p_root, string p_path)
	{
		if (p_root != null)
		{
			Transform parent = ((!string.IsNullOrEmpty(p_path)) ? p_root.Find(p_path) : p_root);
			Vector3 localPosition = p_node.localPosition;
			Quaternion localRotation = p_node.localRotation;
			p_node.parent = parent;
			p_node.localPosition = localPosition;
			p_node.localRotation = localRotation;
		}
	}

	private Camera GetActiveCamera()
	{
		return Array.Find(m_cutsceneCameras, (Camera cam) => cam.enabled && cam.gameObject.active);
	}

	private bool CompareTag(Transform p_current, object p_tag)
	{
		return p_current.tag == (string)p_tag;
	}
}
