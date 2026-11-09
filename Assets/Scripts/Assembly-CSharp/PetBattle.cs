using UnityEngine;

public class PetBattle : MonoBehaviour
{
	[SerializeField]
	private Vector3 m_targetOffsetFromPlayer = new Vector3(1f, 0f, 0.5f);

	public float speed = 2f;

	public float rotSpeed = 160f;

	public Player m_playerRef;

	public Monster m_monsterRef;

	public bool tryToPlaySkillAnim;

	public bool isPlayingSkillAnim;

	private NBAnimation m_nbAnim;

	private Transform m_cachedTransform;
}
