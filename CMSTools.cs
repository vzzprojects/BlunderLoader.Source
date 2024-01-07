using System;
using FG.Common.CMS;

namespace BlunderLoader
{
	// Token: 0x02000005 RID: 5
	public static class CMSTools
	{
		// Token: 0x06000033 RID: 51 RVA: 0x000045CC File Offset: 0x000027CC
		public static void NewLocalisedString(string key, string text)
		{
			if (!CMSLoader.Instance._localisedStrings.ContainsString(key))
			{
				CMSLoader.Instance._localisedStrings._localisedStrings.Add(key, text);
				return;
			}
			CMSLoader.Instance._localisedStrings._localisedStrings[key] = text;
		}
	}
}
