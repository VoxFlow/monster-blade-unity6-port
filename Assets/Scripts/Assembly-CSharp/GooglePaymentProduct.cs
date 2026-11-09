using System;

public class GooglePaymentProduct
{
	public string productIdentifier;

	public string publishedState;

	public string purchaseType;

	public bool autoTranslate;

	public string title;

	public string description;

	public bool autoFillPrice;

	public string price;

	public string currencySymbol;

	public string formattedPrice;

	public override string ToString()
	{
		return string.Format("<GooglePaymentProduct>\nID: {0}\nPublished State: {1}\nPurchase Type: {2}\nAuto Translate: {3}\nTitle: {4}\nDescription: {5}\nAuto Fill Price: {6}\nPrice: {7}\nFormatted Price: {8}", productIdentifier, publishedState, purchaseType, autoTranslate, title, description, autoFillPrice, price, formattedPrice);
	}

	public static GooglePaymentProduct productFromText(string text)
	{
		GooglePaymentProduct googlePaymentProduct = new GooglePaymentProduct();
		string[] array = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		googlePaymentProduct.productIdentifier = array[0];
		googlePaymentProduct.publishedState = array[1];
		googlePaymentProduct.purchaseType = array[2];
		googlePaymentProduct.autoTranslate = bool.Parse(array[3]);
		googlePaymentProduct.title = array[4];
		googlePaymentProduct.description = array[5];
		googlePaymentProduct.autoFillPrice = bool.Parse(array[6]);
		googlePaymentProduct.price = array[7];
		googlePaymentProduct.currencySymbol = array[8];
		googlePaymentProduct.formattedPrice = string.Format("{0}{1}", googlePaymentProduct.currencySymbol, googlePaymentProduct.price);
		return googlePaymentProduct;
	}
}
