using System.Collections.Generic;

public class CityDbStruct
{
	public string id = string.Empty;

	public string cityName = "N/A";

	public string description = string.Empty;

	public Element.Type element = Element.Type.Metal;

	public string iconSpriteName = string.Empty;

	public List<string> dungeonBosses = new List<string>();
}
