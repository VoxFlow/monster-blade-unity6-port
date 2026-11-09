using UnityEngine;

public class ComboBox
{
	private static bool forceToUnShow;

	private static int useControlID = -1;

	private bool isClickedComboButton;

	private int selectedItemIndex;

	private Rect rect;

	private GUIContent buttonContent;

	private GUIContent[] listContent;

	private string buttonStyle;

	private string boxStyle;

	private GUIStyle listStyle;

	public int LastIndex
	{
		get
		{
			if (listContent != null)
			{
				return listContent.Length - 1;
			}
			return 0;
		}
	}

	public int SelectedItemIndex
	{
		get
		{
			return selectedItemIndex;
		}
		set
		{
			selectedItemIndex = value;
			buttonContent = listContent[selectedItemIndex];
		}
	}

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		buttonStyle = "button";
		boxStyle = "box";
		this.listStyle = listStyle;
	}

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle)
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = buttonStyle;
		this.boxStyle = boxStyle;
		this.listStyle = listStyle;
	}

	public int Show()
	{
		if (forceToUnShow)
		{
			forceToUnShow = false;
			isClickedComboButton = false;
		}
		bool flag = false;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		EventType typeForControl = Event.current.GetTypeForControl(controlID);
		if (typeForControl == EventType.MouseUp && isClickedComboButton)
		{
			flag = true;
		}
		if (GUI.Button(rect, buttonContent, buttonStyle))
		{
			if (useControlID == -1)
			{
				useControlID = controlID;
				isClickedComboButton = false;
			}
			if (useControlID != controlID)
			{
				forceToUnShow = true;
				useControlID = controlID;
			}
			isClickedComboButton = true;
		}
		if (isClickedComboButton)
		{
			Rect position = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1f), rect.width * (float)(Mathf.FloorToInt((float)listContent.Length / 15f) + 1), listStyle.CalcHeight(listContent[0], 1f) * (float)((listContent.Length <= 15) ? listContent.Length : 15));
			GUI.Box(position, string.Empty, boxStyle);
			int num = GUI.SelectionGrid(position, selectedItemIndex, listContent, Mathf.FloorToInt((float)listContent.Length / 15f) + 1, listStyle);
			if (num != selectedItemIndex)
			{
				selectedItemIndex = num;
				buttonContent = listContent[selectedItemIndex];
			}
		}
		if (flag)
		{
			isClickedComboButton = false;
		}
		return selectedItemIndex;
	}
}
