using System.Collections;

public class GooglePaymentTransaction
{
	public string signedData;

	public string signature;

	public static GooglePaymentTransaction transactionFromJson(string json)
	{
		return transactionFromHashtable(json.hashtableFromJson());
	}

	public static GooglePaymentTransaction transactionFromHashtable(Hashtable ht)
	{
		GooglePaymentTransaction googlePaymentTransaction = new GooglePaymentTransaction();
		if (ht.ContainsKey("signed_data"))
		{
			googlePaymentTransaction.signedData = ht["signed_data"].ToString();
		}
		if (ht.ContainsKey("signature"))
		{
			googlePaymentTransaction.signature = ht["signature"].ToString();
		}
		return googlePaymentTransaction;
	}

	public override string ToString()
	{
		return string.Format("<GooglePaymentTransaction> signedData: {0}, signature: {1}", signedData, signature);
	}
}
