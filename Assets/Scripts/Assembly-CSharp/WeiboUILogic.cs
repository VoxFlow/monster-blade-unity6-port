using System.Collections;
using NetDimension.Weibo.Entities.status;
using UnityEngine;

public class WeiboUILogic
{
	public delegate void PictureGot(string weibo_id);

	private bool m_bDownloading;

	public IEnumerator DownloadWeiboPictures(WeiboUser[] we_users, PictureGot callback)
	{
		if (we_users == null)
		{
			yield break;
		}
		foreach (WeiboUser usr in we_users)
		{
			m_bDownloading = FetchWeiboPicture(usr.uid, usr.imgUrl, callback);
			while (m_bDownloading)
			{
				yield return null;
			}
		}
	}

	public Texture2D GetWeiboTexture(string weibo_id)
	{
		if (weibo_id == null)
		{
			Dbg.LogError("[GetWeiboTexture] weibo_id is null");
			return null;
		}
		byte[] fileFromCacheOnly = NBCacheManager.Instance.GetFileFromCacheOnly("weibo", weibo_id + ".dat");
		if (fileFromCacheOnly == null)
		{
			Dbg.LogWarn("[GetWeiboTexture] picture does not exists in cache for " + weibo_id);
			return null;
		}
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
		if (null == texture2D)
		{
			Dbg.LogError("[GetWeiboTexture] failed to create texture object");
			return null;
		}
		if (!texture2D.LoadImage(fileFromCacheOnly))
		{
			Dbg.LogError("[GetWeiboTexture] failed to load image into Texture object for " + weibo_id);
			return null;
		}
		if (texture2D.width == 8 && texture2D.height == 8)
		{
			return null;
		}
		return texture2D;
	}

	public void SendInvitation(string name, SocialManager.SocialCallback p_callback = null)
	{
		string format = SingletonComponent<NBLocalization>.Instance.Get("weibo_invite_ms") + GLOBAL.NUBEE_ZHS_WEBSITE;
		string post_msg = string.Format(format, name);
		WeiboMod.DoAction(delegate(WeiboMod weibo)
		{
			Entity entity = weibo.Client.API.Statuses.Update(post_msg, 0f, 0f, string.Empty);
			if (entity == null)
			{
				Dbg.Log("[WEIBO] POST NOT");
			}
			else
			{
				if (p_callback != null)
				{
					p_callback(ServiceResponseCodes.SUCCESS);
				}
				Dbg.Log("[WEIBO] POSTED " + post_msg);
			}
		});
	}

	private void FriendPictureDownload(bool p_bSuccess, string p_szError, byte[] p_fileContent, object p_arg1, object p_arg2)
	{
		m_bDownloading = false;
		if (!p_bSuccess || p_fileContent == null)
		{
			Dbg.LogError("[WeiboUILogic::FriendPictureDownload] Unable to download friend weibo picture: " + ((p_szError == null) ? string.Empty : p_szError));
		}
		else if (p_arg1 != null)
		{
			PictureGot pictureGot = (PictureGot)p_arg1;
			string weibo_id = (string)p_arg2;
			pictureGot(weibo_id);
		}
	}

	private bool FetchWeiboPicture(string weibo_id, string url, PictureGot callback)
	{
		Dbg.Log("Downloading weibo picture : " + url);
		NBCacheManager.Instance.GetFile(url, "weibo", weibo_id + ".dat", null, FriendPictureDownload, callback, weibo_id, 10);
		return true;
	}
}
