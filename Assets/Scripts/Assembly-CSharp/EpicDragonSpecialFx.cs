using UnityEngine;

public class EpicDragonSpecialFx : MonsterSpecialFx
{
	[SerializeField]
	private SelfDestroy m_shockWaveFxPrefab;

	private void PlayShockWaveFx()
	{
		if (null != m_shockWaveFxPrefab)
		{
			SelfDestroy selfDestroy = Object.Instantiate(m_shockWaveFxPrefab) as SelfDestroy;
			selfDestroy.gameObject.transform.AttachTo(base.transform, TransformAttachMode.PARENT_ORIGIN);
		}
	}
}
