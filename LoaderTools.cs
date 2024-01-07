using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace BlunderLoader
{
	// Token: 0x02000007 RID: 7
	internal static class LoaderTools
	{
		// Token: 0x06000035 RID: 53 RVA: 0x0000473C File Offset: 0x0000293C
		public static Texture2D DownloadTexture2D(string url, int width, int height)
		{
			byte[] array = new WebClient().DownloadData(url);
			Texture2D texture2D = new Texture2D(width, height);
			if (ImageConversion.LoadImage(texture2D, array))
			{
				return texture2D;
			}
			return null;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004770 File Offset: 0x00002970
		public static Texture2D SpriteToTexture2D(Sprite sprite)
		{
			if (sprite == null)
			{
				return null;
			}
			Texture2D texture2D = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
			texture2D.SetPixels(sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height));
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00004800 File Offset: 0x00002A00
		public static Mesh MeshImporter(string path)
		{
			Mesh mesh = new Mesh();
			try
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					List<Vector3> list = new List<Vector3>();
					List<Vector3> list2 = new List<Vector3>();
					List<Vector2> list3 = new List<Vector2>();
					List<int> list4 = new List<int>();
					string text;
					while ((text = streamReader.ReadLine()) != null)
					{
						string[] array = text.Split(' ', 0);
						string text2 = array[0];
						if (!(text2 == "v"))
						{
							if (!(text2 == "vn"))
							{
								if (!(text2 == "vt"))
								{
									if (text2 == "f")
									{
										for (int i = 1; i < array.Length; i++)
										{
											string[] array2 = array[i].Split('/', 0);
											int num = int.Parse(array2[0]) - 1;
											int.Parse(array2[1]);
											int.Parse(array2[2]);
											list4.Add(num);
										}
									}
								}
								else
								{
									float num2 = float.Parse(array[1]);
									float num3 = float.Parse(array[2]);
									list3.Add(new Vector2(num2, num3));
								}
							}
							else
							{
								float num4 = float.Parse(array[1]);
								float num5 = float.Parse(array[2]);
								float num6 = float.Parse(array[3]);
								list2.Add(new Vector3(num4, num5, num6));
							}
						}
						else
						{
							float num7 = float.Parse(array[1]);
							float num8 = float.Parse(array[2]);
							float num9 = float.Parse(array[3]);
							list.Add(new Vector3(num7, num8, num9));
						}
					}
					mesh.vertices = list.ToArray();
					mesh.normals = list2.ToArray();
					mesh.uv = list3.ToArray();
					mesh.triangles = list4.ToArray();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error importing OBJ file: " + ex.Message);
			}
			return mesh;
		}
	}
}
