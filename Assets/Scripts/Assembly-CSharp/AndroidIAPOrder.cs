using System.Collections.Generic;
using ServiceCalls;

public class AndroidIAPOrder
{
	public string notificationId { get; private set; }

	public string orderId { get; private set; }

	public string packageName { get; private set; }

	public string productId { get; private set; }

	public long purchaseTime { get; private set; }

	public int purchaseState { get; private set; }

	public string purchaseToken { get; private set; }

	public string originalJson { get; private set; }

	public AndroidIAPOrder(JSONObject p_json)
	{
		if (p_json != null)
		{
			notificationId = ServiceCallUtils.GetStringField(p_json, "notificationId", string.Empty);
			orderId = ServiceCallUtils.GetStringField(p_json, "orderId", string.Empty);
			packageName = ServiceCallUtils.GetStringField(p_json, "packageName", string.Empty);
			productId = ServiceCallUtils.GetStringField(p_json, "productId", string.Empty);
			purchaseTime = ((p_json.GetField("purchaseTime") != null) ? ((long)p_json.GetField("purchaseTime").n) : 0);
			purchaseState = ((p_json.GetField("purchaseState") != null) ? ((int)p_json.GetField("purchaseState").n) : 0);
			purchaseToken = ServiceCallUtils.GetStringField(p_json, "purchaseToken", string.Empty);
			originalJson = p_json.ToString();
		}
	}

	public static void GetOrdersFromJSON(string p_json, ref List<AndroidIAPOrder> p_iapOrders)
	{
		if (p_json == null || p_iapOrders == null)
		{
			Dbg.LogWarn("GetOrdersFromJSON cannot accept null parameters.");
			return;
		}
		p_iapOrders.Clear();
		string text = p_json.Replace("\\", string.Empty);
		string text2 = text.Replace("\"{", "{");
		string str = text2.Replace("}\"", "}");
		JSONObject jSONObject = new JSONObject(str);
		if (jSONObject != null)
		{
			Dbg.Log(jSONObject.ToString());
			AndroidIAPOrder androidIAPOrder = new AndroidIAPOrder(jSONObject);
			Dbg.Log("json order added to list " + androidIAPOrder.productId);
			p_iapOrders.Add(androidIAPOrder);
		}
		else
		{
			Dbg.LogWarn("Cannot create JSON object.");
		}
	}
}
