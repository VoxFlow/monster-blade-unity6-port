using UnityEngine;

public class GachaOptionPanel : StandardMenuPanel
{
	protected BackButtonMapping m_backButtonMapper;

	protected TutorialManager m_tutorialManager;

	protected override void Awake()
	{
		base.Awake();
		m_backButtonMapper = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
		m_tutorialManager = SingletonComponent<TutorialManager>.Instance;
	}

	protected override void Update()
	{
		base.Update();
		if ((!(m_tutorialManager != null) || !m_tutorialManager.enabled || !m_tutorialManager.IsAnyTutorialExisting()) && m_backButtonMapper != null && parentScreen.backButton != null)
		{
			if (base.GetComponent<Animation>().isPlaying && m_backButtonMapper.enabled)
			{
				m_backButtonMapper.enabled = false;
				parentScreen.backButton.SetActiveRecursively(false);
			}
			else if (!base.GetComponent<Animation>().isPlaying && !m_backButtonMapper.enabled)
			{
				m_backButtonMapper.enabled = true;
				parentScreen.backButton.SetActiveRecursively(true);
			}
		}
	}
}
