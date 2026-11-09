using UnityEngine;

public class WolfSpecialFx : MonsterSpecialFx
{
	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		GetTexture(0);
		GetTexture(1);
	}

	private void OnAnimBegin(string p_anim)
	{
		if (p_anim.Contains("Idle") && p_anim != "Idle_Exhaust")
		{
			PlaySoundFx("wolf_idle");
		}
		else if (p_anim.Contains("Attack"))
		{
			PlaySoundFx("wolf_pre_attack");
			PlaySoundFx("monsterAttackClaw1");
		}
	}

	private Texture GetTexture(int p_index)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor("MonsterSetup");
		if (parametersFor == null)
		{
			return null;
		}
		string text = parametersFor.GetData(base.gameObject.name, "DIFFUSE_MAP");
		if (p_index == 1)
		{
			text = text.Remove(text.LastIndexOf("_T01"));
			text += "_T02";
		}
		Texture texture = Resources.Load(text) as Texture;
		if (texture == null)
		{
			Debug.LogError("Invalid texture path: " + text);
		}
		return texture;
	}

	public void SwapTexture(int p_index)
	{
		Texture texture = GetTexture(p_index);
		if (!(texture == null))
		{
			SkinnedMeshRenderer[] renderers = m_renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				skinnedMeshRenderer.material.SetTexture("_MainTex", texture);
			}
		}
	}
}
