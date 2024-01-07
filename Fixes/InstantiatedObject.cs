using System;
using Levels.Obstacles;
using UnityEngine;

namespace BlunderLoader.Fixes
{
	// Token: 0x0200000A RID: 10
	public class InstantiatedObject : MonoBehaviour
	{
		// Token: 0x0600003A RID: 58 RVA: 0x00004BAC File Offset: 0x00002DAC
		private void Update()
		{
			if (this._killzone != null)
			{
				if (base.gameObject.transform.position.y < this._killzone.position.y)
				{
					this._manager._currentInstances.Remove(base.gameObject);
					Object.Destroy(base.gameObject);
					return;
				}
			}
			else if (base.gameObject.transform.position.y < -75f)
			{
				this._manager._currentInstances.Remove(base.gameObject);
				Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x0400005E RID: 94
		public Transform _killzone;

		// Token: 0x0400005F RID: 95
		public COMMON_PrefabSpawnerBase _manager;
	}
}
