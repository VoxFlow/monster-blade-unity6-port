using UnityEngine;

public class PopControllerTest : MonoBehaviour
{
	private void Awake()
	{
		UIPopupController instance = UIPopupController.Instance;
		if (instance != null)
		{
			instance.ShowOKPopupMSG("test", "test1");
			instance.ShowOKPopupMSG("test", "test2");
			instance.ShowOKPopupMSG("test", "test3");
			instance.ShowOKPopupMSG("test", "test4");
			instance.ShowMediumPopupMSG("testM", "testMedium1", null, null);
			instance.ShowMediumPopupMSG("testM", "testMedium2", null, null);
			instance.ShowOKPopupMSG("test", "test5");
			instance.ShowMediumPopupMSG("testM", "testMedium3", null, null);
			instance.ShowShopBagSlotsSuccess();
			instance.ShowDiamondShop(null, null);
			instance.ShowShopStaminaSuccess();
			instance.ShowShopBagSlots(null, null);
		}
	}
}
