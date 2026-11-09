using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CutsceneMaterialSwitchSq : CutsceneSq
{
	public Material[] m_materials;

	public override bool CanPlay()
	{
		return base.CanPlay() && m_materials != null && m_materials.Length == m_sequenceTime.Length;
	}

	protected override void PlayNextSequence(int p_index)
	{
		base.PlayNextSequence(p_index);
		if (m_materials[p_index] != null)
		{
			base.GetComponent<Renderer>().material = m_materials[p_index];
		}
	}
}
