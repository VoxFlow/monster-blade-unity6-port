using UnityEngine;

public class SocialScreen : NBUIMenuScreen
{
	public enum Choice
	{
		FRIEND_LIST = 0,
		FRIEND_REQUEST = 1,
		GIFT_A_MYSTERY_BOX = 2,
		MESSAGE_BOX = 3
	}

	public enum ScreenState
	{
		HOME = 0,
		FRIENDS_LIST = 1,
		FRIENDS_REQUEST = 2,
		GIFT_MYSTERY_BOX = 3,
		MESSAGE_BOX = 4,
		CHARACTER_INFO = 5,
		STRANGERS_LIST = 6,
		FRIENDS_SEARCH = 7
	}

	private ScreenState _currState;

	private FriendCell _cellSelected;

	public ScreenState currState
	{
		get
		{
			return _currState;
		}
		set
		{
			_currState = value;
			if (_currState != ScreenState.HOME)
			{
				backButton.SetActiveRecursively(true);
				if (backButton.transform.localPosition.y > -100f || backButton.transform.localPosition.y < -102f)
				{
					Vector3 localPosition = backButton.transform.localPosition;
					localPosition.y = -101f;
					backButton.transform.localPosition = localPosition;
					Dbg.LogWarn("The back button position is incorrect, move it to proper position");
				}
			}
			else
			{
				backButton.SetActiveRecursively(false);
			}
		}
	}

	public override void Start()
	{
		PushState(ScreenState.HOME);
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length)
		{
			PushPanel(menuPanels[(int)nextState]);
			currState = nextState;
		}
	}

	private void PopState()
	{
		PopPanel();
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	private void OnButtonClick(GameObject source)
	{
		UIButtonMessage component = source.GetComponent<UIButtonMessage>();
		switch ((Choice)component.id)
		{
		case Choice.FRIEND_LIST:
			OnFriendListSelected();
			break;
		case Choice.FRIEND_REQUEST:
			OnFriendRequestSelected();
			break;
		case Choice.GIFT_A_MYSTERY_BOX:
			OnGiftSelected();
			break;
		case Choice.MESSAGE_BOX:
			OnMessageBoxSelected();
			break;
		}
	}

	private void OnFriendListSelected()
	{
		PushState(ScreenState.FRIENDS_LIST);
	}

	private void OnFriendRequestSelected()
	{
		PushState(ScreenState.FRIENDS_REQUEST);
	}

	private void OnGiftSelected()
	{
		Dbg.LogWarn("Gift Mystery Box");
	}

	private void OnMessageBoxSelected()
	{
		Dbg.LogWarn("Message Box");
	}

	private void OnBack()
	{
		PopState();
	}
}
