public class TutorialDataRow
{
	public string Id;

	public string Type;

	public string SubtitleKey;

	public string AssetPath;

	public string CallbackStr;

	public string NextId;

	public string MascotFacial;

	public string MascotAnim;

	public string Sound;

	public string Music;

	public string TriggerKey;

	public string Arg;

	public override string ToString()
	{
		return string.Format("TutorialDataRow Id:{0}, Type:{1}, SubtitleKey:{2}, AssetPath: {3}, CallbackStr:{4}, NextId:{5}, MascotFacial:{6}, Sound:{7}, Music:{8}", Id, Type, SubtitleKey, AssetPath, CallbackStr, NextId, MascotFacial, Sound, Music);
	}
}
