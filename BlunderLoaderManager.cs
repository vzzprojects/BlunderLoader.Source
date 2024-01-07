using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.IL2CPP.Utils.Collections;
using BlunderLoader.Fixes;
using Events;
using FG.Common;
using FG.Common.Character;
using FG.Common.Character.MotorSystem;
using FG.Common.CMS;
using FG.Common.Definition;
using FG.Common.Loadables;
using FGClient;
using FGClient.OfflinePlayground;
using FGClient.UI;
using FGClient.UI.Core;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Levels.DoorDash;
using Levels.Obstacles;
using Levels.PixelPerfect;
using Levels.Progression;
using Levels.Rollout;
using Levels.ScoreZone;
using Levels.TimeAttack;
using Levels.TipToe;
using LiveOps.Collections;
using MPG.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BlunderLoader
{
	// Token: 0x02000004 RID: 4
	public class BlunderLoaderManager : MonoBehaviour
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x00002067 File Offset: 0x00000267
		private string version
		{
			get
			{
				return "1.7.6";
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4 RVA: 0x0000206E File Offset: 0x0000026E
		private string baseUrl
		{
			get
			{
				return "http://loaderfiles.c1.biz/";
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002078 File Offset: 0x00000278
		public BlunderLoaderManager(IntPtr handle)
			: base(handle)
		{
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000020E4 File Offset: 0x000002E4
		// (set) Token: 0x06000007 RID: 7 RVA: 0x000020EC File Offset: 0x000002EC
		private bool CanLoadUI { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000020F5 File Offset: 0x000002F5
		private string username
		{
			get
			{
				return IdentityProviderUtils.GetEosOrGeneratedName();
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000009 RID: 9 RVA: 0x000020FC File Offset: 0x000002FC
		// (set) Token: 0x0600000A RID: 10 RVA: 0x00002104 File Offset: 0x00000304
		public bool localPlay { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000B RID: 11 RVA: 0x00002110 File Offset: 0x00000310
		private Sprite loadingBackground
		{
			get
			{
				Texture2D texture2D = LoaderTools.DownloadTexture2D("https://raw.githubusercontent.com/mattiFG/LoaderFiles/main/gradient.jpg", Screen.width, Screen.height);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
				sprite.name = "BlunderBackground";
				return sprite;
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000216F File Offset: 0x0000036F
		public void Awake()
		{
			BlunderLoaderManager.Instance = this;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002178 File Offset: 0x00000378
		public void Start()
		{
			this.globalGameStateClient = Singleton<GlobalGameStateClient>.Instance;
			base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.Setup()));
			base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.CreateList()));
			this.loadingMetaData = new ScreenMetaData();
			this.loadingMetaData.Transition = 3;
			this.loadingMetaData.ScreenStack = 2;
			base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.LoaderSplash()));
			LoaderData.keys = new KeyCode[5];
			LoaderData.keys[0] = 282;
			LoaderData.keys[1] = 283;
			LoaderData.keys[2] = 114;
			LoaderData.keys[3] = 99;
			LoaderData.keys[4] = 116;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002227 File Offset: 0x00000427
		public void HandleMPGNetObject(MPGNetObject mpgNetObject)
		{
			mpgNetObject.NetID = Singleton<GlobalGameStateClient>.Instance.NetObjectManager.GetNextNetID();
			Singleton<GlobalGameStateClient>.Instance.NetObjectManager.RegisterNetObject(mpgNetObject);
			mpgNetObject.AreAnimationsNetworkControlled = false;
			mpgNetObject.SyncTransform = false;
			mpgNetObject.SyncScale = false;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002263 File Offset: 0x00000463
		private IEnumerator Setup()
		{
			yield return new WaitForFixedUpdate();
			while (!CMSLoader.Instance)
			{
				yield return null;
			}
			while (CMSLoader.Instance.State != 2)
			{
				yield return null;
			}
			if (CMSLoader.Instance._roundsSO != null)
			{
				this.roundsSO = CMSLoader.Instance._roundsSO;
				this.localisedStrings = CMSLoader.Instance._localisedStrings;
			}
			this.gameStateMachine = Singleton<GlobalGameStateClient>.Instance._gameStateMachine;
			while (!Object.FindObjectOfType<MainMenuManager>().IsOnMainMenu)
			{
				yield return null;
			}
			ModalMessageData modalMessageData = new ModalMessageData
			{
				Title = "BlunderLoader - matti",
				Message = "Hey " + this.username + ", welcome to using BlunderLoader! \\nThis loader is full of features, discover them using it!\\nHave fun,\\nmatti",
				ModalType = 0,
				OkButtonType = 2,
				LocaliseMessage = 1,
				LocaliseTitle = 1,
				OnCloseButtonPressed = new Action<bool>(this.SecondMessage)
			};
			Singleton<PopupManager>.Instance.Show(2, modalMessageData, null);
			yield break;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002274 File Offset: 0x00000474
		private void SecondMessage(bool _bool)
		{
			if (_bool)
			{
				ModalMessageData modalMessageData = new ModalMessageData
				{
					Title = "WARNING!",
					Message = "There are currently some problems with wormholes. For example, if you see them all the same color, there may be a problem and you may need to restart the game to fix it.\nCurrently, the first load is a bit broken. You need to enter the round id, click Local play, empty the text box and press Local play again. After that you can also disable the round loader if you prefer.\nI also remind you to never give BlunderLoader to anyone, not even if them tell you that are authorized to have it, always ask matti for confirmation first.",
					ModalType = 0,
					OkButtonType = 1,
					LocaliseTitle = 1,
					LocaliseMessage = 1,
					OnCloseButtonPressed = new Action<bool>(this.<SecondMessage>g__SetVisible|87_0)
				};
				Singleton<PopupManager>.Instance.Show(2, modalMessageData, null);
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000022E1 File Offset: 0x000004E1
		private IEnumerator CreateList()
		{
			yield return new WaitForFixedUpdate();
			while (this.roundsSO == null)
			{
				yield return null;
			}
			this.CreateRoundList();
			yield break;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000022F0 File Offset: 0x000004F0
		private IEnumerator LocalPlay()
		{
			this.startingPositions = null;
			this.respawn = null;
			yield return new WaitForFixedUpdate();
			if (!this.isPlayerAlreadyLoaded)
			{
				this.LoadPlayer();
				this.isPlayerAlreadyLoaded = true;
			}
			this.player = Object.Instantiate<GameObject>(this.playerGameObject);
			this.mpg = this.player.AddComponent<MPGNetObject>();
			this.mpg.spawnObjectType_ = 0;
			this.mpg.NetID = Singleton<GlobalGameStateClient>.Instance.NetObjectManager.GetNextNetID();
			this.fallGuysCharacterController = this.player.GetComponent<FallGuysCharacterController>();
			this.offlinePlaygroundManager = this.player.AddComponent<OfflinePlaygroundManager>();
			this.offlinePlaygroundManager.enabled = false;
			this.offlinePlaygroundManager._cameraDirector = this.cameraDirector;
			this.offlinePlaygroundManager._fallGuysCharacter = this.fallGuysCharacterController;
			this.fallGuysCharacterControllerInput = this.player.AddComponent<FallGuysCharacterControllerInput>();
			this.offlinePlaygroundManager._playerInput = this.fallGuysCharacterControllerInput;
			this.offlinePlaygroundManager.enabled = true;
			this.offlinePlaygroundManager.Awake();
			this.RandomizeCheckpoint();
			this.player.transform.position = this.respawn.transform.position + new Vector3(0f, 1f, 0f);
			this.player.transform.rotation = this.respawn.transform.rotation;
			this.respawn.transform.position = this.player.transform.position;
			FallGuysCameraAvoidence fallGuysCameraAvoidence = Object.FindObjectOfType<FallGuysCameraAvoidence>();
			this.fallGuysCharacterController._cameraTransform = fallGuysCameraAvoidence.gameObject.transform;
			SpeedBoostManager speedBoostManager = this.fallGuysCharacterController.SpeedBoostManager;
			speedBoostManager._isAuthoritative = true;
			speedBoostManager._resyncToClients = true;
			speedBoostManager._currentSpeedBoost._isValid = true;
			speedBoostManager._currentSpeedBoost._serverVerified = true;
			speedBoostManager._characterController = this.fallGuysCharacterController;
			this.player.tag = "Player";
			yield return new WaitForEndOfFrame();
			this.offlinePlaygroundManager.SetupPlayerUpdateManagerAndRegister(this.fallGuysCharacterController, true);
			yield return new WaitForEndOfFrame();
			yield break;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000022FF File Offset: 0x000004FF
		private IEnumerator SelectGameStateAfter(InGameUiManager.InGameState inGameState, float timer, bool isStart)
		{
			yield return new WaitForFixedUpdate();
			float counter = 0f;
			while (counter < timer)
			{
				counter += Time.deltaTime;
				yield return null;
			}
			if (this.inGameUiManager != null)
			{
				if (LoaderData.EnableUiLocalPlay)
				{
					this.inGameUiManager.SwitchToState(inGameState);
				}
				else
				{
					Object.Destroy(this.inGameUiManager.gameObject);
				}
			}
			if (Object.FindObjectOfType<GameplayTimerViewModel>() != null)
			{
				Object.FindObjectOfType<GameplayTimerViewModel>().gameObject.SetActive(false);
			}
			if (Object.FindObjectOfType<GameplayInstructionsViewModel>() != null)
			{
				GameplayInstructionsViewModel gameplayInstructionsViewModel = Object.FindObjectOfType<GameplayInstructionsViewModel>();
				gameplayInstructionsViewModel.ObjectiveText = this.currentRound.GameRules.ObjectiveText;
				if (this.currentRound.GameRules.ObjectiveHintText != null)
				{
					gameplayInstructionsViewModel.ObjectiveHint = this.currentRound.GameRules.ObjectiveHintText;
				}
			}
			if (isStart)
			{
				this.OnLevelStarted();
			}
			yield return null;
			yield break;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002323 File Offset: 0x00000523
		private void LoadPlayer()
		{
			this.playerGameObject = this.globalGameStateClient.CommonConfig.fallGuyAssetRef.LoadAsset().WaitForCompletion();
			this.playerGameObject.GetComponent<MotorAgent>()._motorFunctionsConfig = 2;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002358 File Offset: 0x00000558
		private void PowerUp()
		{
			PowerupState powerupState = new PowerupState();
			RollingBallPowerup rollingBallPowerup = Resources.FindObjectsOfTypeAll<RollingBallPowerup>()[0];
			powerupState.Init(rollingBallPowerup.ID, 0, -1f, true);
			MotorFunctionPowerup powerupMotorFunction = this.fallGuysCharacterController.PowerupMotorFunction;
			powerupMotorFunction.EquipPowerup_Local(rollingBallPowerup, powerupState);
			powerupMotorFunction.ConfirmPowerupEquip_Local();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000023A4 File Offset: 0x000005A4
		private void ImageLoader(string imageFileName)
		{
			try
			{
				string text = Path.Combine(Paths.PluginPath, "BlunderLoader\\Media\\" + imageFileName);
				if (File.Exists(text))
				{
					byte[] array = File.ReadAllBytes(text);
					this.imageTexture = new Texture2D(2, 2);
					if (ImageConversion.LoadImage(this.imageTexture, array))
					{
						GameObject gameObject = new GameObject(imageFileName);
						SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
						this.imageSprite = Sprite.Create(this.imageTexture, new Rect(0f, 0f, (float)this.imageTexture.width, (float)this.imageTexture.height), new Vector2(0.5f, 0.5f), 100f, 0U, 1);
						spriteRenderer.sprite = this.imageSprite;
						if (this.localPlay && this.player != null)
						{
							gameObject.transform.position = this.player.transform.position;
						}
						else
						{
							gameObject.transform.position = new Vector3(0f, 0f, 0f);
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000024D8 File Offset: 0x000006D8
		private void FixedUpdate()
		{
			if (this.localPlay && this.clientGameManager != null)
			{
				this.clientGameManager.FixedUpdate(Time.fixedDeltaTime);
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000024FC File Offset: 0x000006FC
		public void Update()
		{
			if (this.localPlay && this.player.transform.position.y < this.playerEliminationVolume.transform.position.y)
			{
				this.Respawn();
			}
			if (this.waitingForKey)
			{
				using (IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						KeyCode keyCode = (KeyCode)obj;
						if (Input.GetKeyDown(keyCode))
						{
							if (this.keyToChange >= 0 && this.keyToChange < LoaderData.keys.Length)
							{
								LoaderData.keys[this.keyToChange] = keyCode;
							}
							this.keyToChange = -1;
							this.waitingForKey = false;
						}
					}
					goto IL_1B6;
				}
			}
			if (Input.GetKeyDown(LoaderData.keys[1]))
			{
				if (this.UI)
				{
					this.UI = false;
				}
				else
				{
					this.UI = true;
				}
			}
			if (Input.GetKeyDown(LoaderData.keys[0]))
			{
				if (!Cursor.visible)
				{
					Cursor.visible = true;
					Cursor.lockState = 0;
				}
				else
				{
					Cursor.visible = false;
					Cursor.lockState = 1;
				}
			}
			if (this.localPlay && Input.GetKeyDown(LoaderData.keys[3]))
			{
				this.respawn.transform.position = this.player.transform.position;
				this.respawn.transform.rotation = this.player.transform.rotation;
				this.fallGuysCharacterController.CharacterEventSystem.RaiseEvent<VfxCheckpointEvent>(FGEventFactory.GetVfxCheckpointEvent());
			}
			if (this.localPlay && Input.GetKeyDown(LoaderData.keys[2]))
			{
				this.Respawn();
			}
			if (this.localPlay && Input.GetKeyDown(LoaderData.keys[4]))
			{
				this.RandomizeCheckpoint();
			}
			IL_1B6:
			if (this.localPlay && this.clientGameManager.CurrentGameSession.CurrentSessionState == 3 && LoaderData.UseSimulationTime)
			{
				this.clientGameManager.CurrentGameSession._networkFixedTimeManager._smoothedExtrapolatedRemoteTimeStep += Time.deltaTime;
				this.clientGameManager.CurrentGameSession._networkFixedTimeManager.SmoothedExtrapolatedRemoteTime += Time.deltaTime;
			}
			if (this.localPlay && this.clientGameManager.CurrentGameSession.CurrentSessionState == 3)
			{
				this.clientGameManager.CurrentGameSession._sessionClock += Time.time;
				this.clientGameManager.CurrentGameSession._discreteSessionClock += Time.deltaTime;
			}
			if (this.localPlay && this.clientGameManager.CurrentGameSession.CurrentSessionState == 3 && this.cameraDirector != null)
			{
				this.cameraDirector.HandlePlayerCameraControls();
			}
			if (this.localPlay && this.clientGameManager != null)
			{
				this.clientGameManager.Update(Time.deltaTime);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000027DC File Offset: 0x000009DC
		private void LoadAllBundles()
		{
			string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/aa~/StandaloneWindows64/", "*.bundle", 1);
			List<string> list = new List<string>();
			foreach (ILoadableAsset loadableAsset in Singleton<LoadableAssetManager>.Instance._loadingAssets)
			{
				list.Add(loadableAsset.Name);
			}
			foreach (string text in files)
			{
				if (!list.Contains(text))
				{
					try
					{
						base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.LoadBundleAndObjects(text)));
					}
					catch (Exception ex)
					{
						Debug.Log("BlunderLoader | Error loading assetbundle: " + ex.Message);
					}
				}
			}
			string text2 = Path.Combine(Paths.PluginPath, "BlunderLoader\\All objects.txt");
			File.WriteAllLines(text2, this.allBundleObjectsString);
			Debug.Log("BlunderLoader | Object list saved in: " + text2);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000028D8 File Offset: 0x00000AD8
		private IEnumerator LoadBundleAndObjects(string path)
		{
			AssetBundleCreateRequest _bundle = AssetBundle.LoadFromFileAsync(path);
			yield return _bundle;
			AssetBundle assetBundle = _bundle.assetBundle;
			AssetBundleRequest _assets = assetBundle.LoadAllAssetsAsync();
			yield return _assets;
			using (IEnumerator<Object> enumerator = _assets.allAssets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Object @object = enumerator.Current;
					CollectionExtensions.AddItem<Object>(this.allBundleObjects, @object);
					CollectionExtensions.AddItem<string>(this.allBundleObjectsString, @object.name);
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000028F0 File Offset: 0x00000AF0
		private void SetupUI()
		{
			if (this.inGameUiManager != null)
			{
				Object.Destroy(this.inGameUiManager.gameObject);
			}
			if (this.loading != null)
			{
				Object.Destroy(this.loading.gameObject);
			}
			if (LoaderData.UseLoadingScreenLocalPlay)
			{
				this.loading = Singleton<UIManager>.Instance.ShowScreen<LoadingGameScreenViewModel>(this.loadingMetaData);
				this.loading._round = this.currentRound;
				this.loading.UpdateLoadingScreenData();
				if (LoaderData.UseBlunderBranding)
				{
					try
					{
						Image component = this.loading.transform.Find("Generic_UI_CurrentSeasonBackground_Container").gameObject.transform.Find("Generic_UI_SeasonS10Background_Canvas_Variant").gameObject.transform.Find("Mask").gameObject.transform.Find("Backdrop").gameObject.GetComponent<Image>();
						component.m_Sprite = this.loadingBackground;
						component.OnCanvasHierarchyChanged();
					}
					catch (Exception ex)
					{
						Debug.LogError("BlunderLoader | Error loading screen customization: " + ex.Message);
					}
				}
			}
			this.inGameUiManager = Singleton<UIManager>.Instance.ShowScreen<InGameUiManager>(null);
			this.inGameIntroCameraState = this.inGameUiManager._inGameIntroCameraState;
			this.inGameIntroCameraState._introCamBannerUI.gameObject.GetComponent<Prefab_UI_Intro_Overlay>()._round = this.currentRound;
			this.inGameIntroCameraState._introCamBannerUI.gameObject.GetComponent<Prefab_UI_Intro_Overlay>().UpdateIntroOverlayData();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002A78 File Offset: 0x00000C78
		public void AddRound(string id, string roundLocKey, string roundName, string descLocKey, string description, string roundScene)
		{
			try
			{
				if (Utility.IsNullOrWhiteSpace(roundLocKey))
				{
					roundLocKey = roundName;
				}
				if (Utility.IsNullOrWhiteSpace(descLocKey))
				{
					descLocKey = description;
				}
				CMSTools.NewLocalisedString(roundLocKey, roundName);
				CMSTools.NewLocalisedString(descLocKey, description);
				LocalisedString localisedString = new LocalisedString();
				localisedString.Id = roundLocKey;
				localisedString.Text = roundName;
				Round round = new Round
				{
					Id = id,
					DisplayName = localisedString,
					SceneData = new RoundSceneData
					{
						PrimeLevel = new PrimeLevels
						{
							Id = roundScene,
							SceneName = roundScene
						}
					}
				};
				CMSLoader.Instance._roundsSO.Rounds.Add(id, round);
				BlunderLoaderManager.Instance.roundsSO.Rounds.Add(id, round);
				Debug.Log("BlunderLoader | CMS Tools | Added custom round: " + id);
			}
			catch (Exception ex)
			{
				Debug.LogError("BlunderLoader | CMS Tools | Error while adding custom round " + id + ": " + ex.Message);
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002B74 File Offset: 0x00000D74
		private IEnumerator LoadLevel()
		{
			this.hasToShowFrogsPopup = false;
			if (LoaderData.UseLoadingScreenLocalPlay)
			{
				if (LoaderData.EnableServerThings)
				{
					this.server = true;
				}
				else
				{
					this.server = false;
				}
				yield return new WaitForFixedUpdate();
				this.SetRound(this.sceneToLoad, 0, false, true);
				yield return new WaitForEndOfFrame();
				this.SetupUI();
				float counter = 0f;
				while (counter < 1f)
				{
					counter += Time.deltaTime;
					yield return null;
				}
				yield return new WaitForEndOfFrame();
				Singleton<CPUBoost>.Instance.StartFastLoad();
				this.SetRound(this.sceneToLoad, 0, true, true);
				float counterA = 0f;
				while (counterA < 6f)
				{
					counterA += Time.deltaTime;
					yield return null;
				}
				yield return new WaitForEndOfFrame();
				this.Variations();
				Debug.Log("BlunderLoader | Variations should be activated");
				float counterB = 0f;
				while (counterB < 3f)
				{
					counterB += Time.deltaTime;
					yield return null;
				}
				yield return new WaitForEndOfFrame();
				this.FixObstacles(this.localPlay);
				Debug.Log("BlunderLoader | Fixing obstacles");
				float counterC = 0f;
				while (counterC < 1f)
				{
					counterC += Time.deltaTime;
					yield return null;
				}
				Singleton<CPUBoost>.Instance.EndFastLoad();
				yield return new WaitForEndOfFrame();
				this.loading.DoFadeOut();
			}
			try
			{
				this.HandleClientGameManager();
			}
			catch (Exception ex)
			{
				Debug.Log("BlunderLoader | Error handling game manager: " + ex.Message);
			}
			yield return new WaitForEndOfFrame();
			this.server = false;
			this.cameraDirector = Object.FindObjectOfType<CameraDirector>();
			this.introCamsDuration = 5f;
			yield return new WaitForEndOfFrame();
			base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.LocalPlay()));
			yield return new WaitForEndOfFrame();
			try
			{
				if (this.cameraDirector != null && this.cameraDirector.HasValidIntroCameraSequence)
				{
					this.introCamsDuration = this.cameraDirector.IntroCamerasDuration;
					this.cameraDirector.UseIntroCameras(null, 1f);
					Broadcaster.Instance.Broadcast<OnLocalPlayerInitialized>(new OnLocalPlayerInitialized(this.fallGuysCharacterController, 0U));
				}
			}
			catch
			{
			}
			this.offlinePlaygroundManager.enabled = true;
			this.offlinePlaygroundManager.Start();
			base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.SelectGameStateAfter(1, this.introCamsDuration, true)));
			GameStateEvents.IntroCameraSequenceStartedEvent introCameraSequenceStartedEvent = new GameStateEvents.IntroCameraSequenceStartedEvent();
			this.inGameUiManager.HandleIntroCamsStarted(introCameraSequenceStartedEvent);
			Debug.Log("BlunderLoader | Intro UI");
			yield break;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002B84 File Offset: 0x00000D84
		private void OnLevelStarted()
		{
			try
			{
				if (Object.FindObjectOfType<PixelPerfectManager>())
				{
					PixelPerfectManager pixelPerfectManager = Object.FindObjectOfType<PixelPerfectManager>();
					pixelPerfectManager.Init();
					pixelPerfectManager.BeginGame();
				}
			}
			catch
			{
			}
			if (this.hasToShowFrogsPopup)
			{
				ModalMessageData modalMessageData = new ModalMessageData
				{
					Title = "SORRY...",
					Message = "This round features frogs or expanding spheres that have been disabled due to a bug that made them very fast. Working to fix.",
					ModalType = 0,
					OkButtonType = 1,
					LocaliseTitle = 1,
					LocaliseMessage = 1
				};
				Singleton<PopupManager>.Instance.Show(1, modalMessageData, null);
			}
			this.cameraDirector.AddCloseCameraTarget(this.fallGuysCharacterController.CachedGameObject, true);
			this.cameraDirector.UseCloseShot();
			Broadcaster.Instance.Broadcast<GameStateEvents.IntroCameraSequenceEndedEvent>(new GameStateEvents.IntroCameraSequenceEndedEvent());
			Singleton<RewiredManager>.Instance.SetActiveMap(0, 0, false, false);
			this.fallGuysCharacterControllerInput.AcceptInput = true;
			this.clientPlayerUpdateManager.GameIsStarting();
			this.fallGuysCharacterControllerInput.SetPlayerIndex(0);
			try
			{
				this.PowerUp();
				this.fallGuysCharacterController.SpeedBoostManager._characterController = this.fallGuysCharacterController;
				if (LoaderData.EnableServerThings)
				{
					this.server = true;
				}
				Singleton<GlobalGameStateClient>.Instance.ClientPlayerManager.ClearAllPlayers();
				this.clientGameManager.SetReady(5, null, null);
				this.clientGameManager.FinishPreparationPhase();
				this.clientGameManager.AddPlayerCollidersToMap(this.fallGuysCharacterController);
				if (CMSLoader.Instance.State == 2)
				{
					string name = Singleton<GlobalGameStateClient>.Instance.PlayerProfile.name;
				}
				this.clientGameManager.AddPlayerIdentification(Singleton<GlobalGameStateClient>.Instance.PlayerProfile.name, Singleton<GlobalGameStateClient>.Instance._platformAccountID, this.fallGuysCharacterController, true, null, 0U);
				this.clientGameManager.SetReady(6, null, null);
				this.clientGameManager.CurrentGameSession.CurrentSessionState = 3;
				this.clientGameManager.CurrentGameSession.OnSwitchToPlayingState();
				if (this.CurrentNetObject != null)
				{
					Singleton<GlobalGameStateClient>.Instance.NetObjectManager.RemoveNetObject(this.CurrentNetObject);
				}
				NetworkPlayerDataClient networkPlayerDataClient = new NetworkPlayerDataClient();
				networkPlayerDataClient.completedLevel = false;
				networkPlayerDataClient.realPlayer = true;
				networkPlayerDataClient.isLocalPlayer = true;
				networkPlayerDataClient.playerKey = "0";
				networkPlayerDataClient.isParticipant = true;
				networkPlayerDataClient.fgcc = this.fallGuysCharacterController;
				networkPlayerDataClient.inputIndex = 0;
				this.fallGuysCharacterController.SetupOnClient(Singleton<GlobalGameStateClient>.Instance.NetObjectManager, this.mpg);
				Singleton<GlobalGameStateClient>.Instance.NetObjectManager.RegisterNetObject(this.mpg);
				this.clientGameManager.CurrentGameSession._defaultRoundLength = 3600f;
				this.clientGameManager.CurrentGameSession.SetEndRoundTime(3600f);
				this.CurrentNetObject = this.mpg;
				this.clientGameManager.FinishPreparationPhase();
			}
			catch (Exception ex)
			{
				Debug.Log("BlunderLoader | Error starting game: " + ex.Message);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002E64 File Offset: 0x00001064
		private void Respawn()
		{
			Quaternion quaternion = default(Quaternion);
			this.cameraDirector.OnRecenterAndSnapCameraNextFrameRequested();
			this.fallGuysCharacterController.TeleportMotorFunction.RequestTeleport(this.respawn.transform.position, quaternion);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002EA5 File Offset: 0x000010A5
		private IEnumerator WatermarkDownload()
		{
			yield return new WaitForFixedUpdate();
			while (!Object.FindObjectOfType<MainMenuManager>())
			{
				yield return null;
			}
			this.logo = LoaderTools.DownloadTexture2D("https://raw.githubusercontent.com/mattiFG/LoaderFiles/main/mod%20logo.png", 385, 459);
			this.rawImage = new RawImage();
			this.rawImage.texture = this.logo;
			this.rawImage.rectTransform.sizeDelta = new Vector2(385f, 459f);
			Color color = this.rawImage.color;
			color.a = 0.6f;
			this.rawImage.color = color;
			this.rawImage.rectTransform.anchorMin = Vector2.zero;
			this.rawImage.rectTransform.anchorMax = Vector2.zero;
			this.rawImage.rectTransform.pivot = Vector2.zero;
			this.rawImage.rectTransform.anchoredPosition = Vector2.zero;
			yield break;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002EB4 File Offset: 0x000010B4
		private void FixObstacles(bool isLocalPlay)
		{
			try
			{
				Object.FindObjectOfType<TimeAttackManager>().gameObject.SetActive(false);
			}
			catch
			{
			}
			try
			{
				if (Object.FindObjectOfType<LivesSystemManager>() != null)
				{
					Object.FindObjectOfType<LivesSystemManager>().StartCoroutine(Object.FindObjectOfType<LivesSystemManager>().SetupWhenReady());
				}
			}
			catch
			{
			}
			try
			{
				foreach (GameSystem gameSystem in Object.FindObjectsOfType<GameSystem>())
				{
					if (!gameSystem.GetComponent<TimeAttackManager>())
					{
						gameSystem.Init(Singleton<GlobalGameStateClient>.Instance.NetObjectManager, null, null, Singleton<GlobalGameStateClient>.Instance.CommonConfig);
					}
				}
			}
			catch
			{
			}
			try
			{
				foreach (RolloutManager rolloutManager in Object.FindObjectsOfType<RolloutManager>())
				{
					if (rolloutManager.gameObject.active && !this.rollOff_fix)
					{
						rolloutManager.gameObject.GetComponent<MPGNetObjectPossessable>()._areAnimationsNetworkControlled = false;
						rolloutManager._IsInitialised_k__BackingField = true;
						Random random = new Random();
						rolloutManager.InstantiateRing(1, random.Next(0, 5));
						rolloutManager.InstantiateRing(0, random.Next(0, 5));
						this.rollOff_fix = true;
					}
				}
			}
			catch
			{
			}
			try
			{
				foreach (SeededRandomisablesManager seededRandomisablesManager in Object.FindObjectsOfType<SeededRandomisablesManager>())
				{
					seededRandomisablesManager.RollSeededRandomisables();
				}
			}
			catch
			{
			}
			try
			{
				foreach (COMMON_Thruster common_Thruster in Object.FindObjectsOfType<COMMON_Thruster>())
				{
					common_Thruster._lodLevelWantsPhysicsActive = true;
				}
			}
			catch
			{
			}
			try
			{
				foreach (COMMON_Button common_Button in Object.FindObjectsOfType<COMMON_Button>())
				{
					common_Button.enabled = true;
				}
			}
			catch
			{
			}
			if (LoaderData.UseRespawningTilesFix)
			{
				try
				{
					foreach (COMMON_RespawningTile common_RespawningTile in Object.FindObjectsOfType<COMMON_RespawningTile>())
					{
						RespawningTile respawningTile = common_RespawningTile.gameObject.transform.Find("Trigger").gameObject.AddComponent<RespawningTile>();
						respawningTile.enabled = true;
						respawningTile.tile = common_RespawningTile;
					}
				}
				catch
				{
				}
			}
			try
			{
				foreach (ScoreZoneManager scoreZoneManager in Object.FindObjectsOfType<ScoreZoneManager>())
				{
					scoreZoneManager.ActivateInitialZones();
				}
			}
			catch
			{
			}
			try
			{
				foreach (CheckpointZonePositions checkpointZonePositions in Object.FindObjectsOfType<CheckpointZonePositions>())
				{
					if (!this.localPlay)
					{
						LevelEditorCheckpointZone levelEditorCheckpointZone = checkpointZonePositions.gameObject.AddComponent<LevelEditorCheckpointZone>();
						levelEditorCheckpointZone.enabled = true;
						levelEditorCheckpointZone._boxCollider = checkpointZonePositions.gameObject.GetComponent<BoxCollider>();
					}
				}
			}
			catch
			{
			}
			try
			{
				foreach (TipToe_Platform tipToe_Platform in Object.FindObjectsOfType<TipToe_Platform>())
				{
					if (Random.value < 0.5f)
					{
						tipToe_Platform._isFakePlatform = false;
					}
					else
					{
						tipToe_Platform._isFakePlatform = true;
					}
				}
			}
			catch
			{
			}
			try
			{
				foreach (COMMON_FakeDoorRandomiser common_FakeDoorRandomiser in Object.FindObjectsOfType<COMMON_FakeDoorRandomiser>())
				{
					common_FakeDoorRandomiser.InitializeServerSideData();
					common_FakeDoorRandomiser.CreateBreakableDoors();
				}
			}
			catch
			{
			}
			if (isLocalPlay)
			{
				try
				{
					this.playerEliminationVolume = new GameObject();
					this.playerEliminationVolume.name = "respawn";
					if (Object.FindObjectOfType<COMMON_PlayerEliminationVolume>() != null)
					{
						this.playerEliminationVolume.transform.position = new Vector3(0f, Object.FindObjectOfType<COMMON_PlayerEliminationVolume>().gameObject.transform.position.y, 0f);
					}
					else
					{
						this.playerEliminationVolume.transform.position = new Vector3(0f, -70f, 0f);
					}
				}
				catch (Exception ex)
				{
					Debug.Log("BlunderLoader | Error: " + ex.Message);
				}
				try
				{
					foreach (COMMON_GravityModifierVolume common_GravityModifierVolume in Object.FindObjectsOfType<COMMON_GravityModifierVolume>())
					{
						common_GravityModifierVolume.gameObject.GetComponent<Collider>().enabled = true;
					}
				}
				catch
				{
				}
				try
				{
					foreach (COMMON_ReversibleConveyorBelt common_ReversibleConveyorBelt in Object.FindObjectsOfType<COMMON_ReversibleConveyorBelt>())
					{
					}
				}
				catch
				{
				}
				try
				{
					foreach (COMMON_Rotator common_Rotator in Object.FindObjectsOfType<COMMON_Rotator>())
					{
						common_Rotator._syncToServer = false;
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000360C File Offset: 0x0000180C
		private void Variations()
		{
			LevelSwitchablesManager levelSwitchablesManager = Object.FindObjectOfType<LevelSwitchablesManager>();
			if (levelSwitchablesManager != null && this.currentRound.SetSwitchers != null)
			{
				levelSwitchablesManager.Init(this.currentRound.SetSwitchers, true, null);
			}
			try
			{
				if (this.currentRound.CollectableSpawn != null)
				{
					CollectableManager[] array = Object.FindObjectsOfType<CollectableManager>();
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Init(true, this.currentRound.CollectableSpawn);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003698 File Offset: 0x00001898
		private void SetRound(string levelId, LoadSceneMode mode, bool isToLoad, bool isLocalPlay)
		{
			this.roundHasData = false;
			if (levelId != null && levelId.StartsWith("round"))
			{
				if (this.roundsSO.Rounds.ContainsKey(levelId))
				{
					this.currentRound = this.roundsSO.Rounds[levelId];
					int num = Random.Range(1, int.MaxValue);
					this.clientGameManager.RandomSeed = num;
					this.roundHasData = true;
					if (isLocalPlay)
					{
						NetworkGameData.SetGameOptionsFromRoundData(this.currentRound, null);
						Singleton<GlobalGameStateClient>.Instance.GameStateView.CurrentGameLevelName = levelId;
					}
				}
			}
			else if (levelId.StartsWith("FallGuy"))
			{
				int num2 = Random.Range(1, int.MaxValue);
				this.clientGameManager.RandomSeed = num2;
				if (isLocalPlay)
				{
					foreach (Round round in CMSLoader.Instance.CMSData.Rounds.Values)
					{
						if (round.SceneData != null && round.SceneData.PrimeLevel != null && round.SceneData.PrimeLevel.SceneName == this.sceneToLoad)
						{
							this.currentRound = round;
							NetworkGameData.SetGameOptionsFromRoundData(this.currentRound, null);
							Singleton<GlobalGameStateClient>.Instance.GameStateView.CurrentGameLevelName = this.currentRound.Id;
							this.roundHasData = true;
						}
					}
				}
				if (!this.roundHasData)
				{
					this.currentRound = null;
				}
			}
			Debug.Log("BlunderLoader | Round: " + this.currentRound.Id + " with scene: " + this.currentRound.SceneData.PrimeLevel.SceneName);
			if (isToLoad)
			{
				if (LoaderData.EnableServerThings)
				{
					this.server = true;
				}
				else
				{
					this.server = false;
				}
				if (levelId.StartsWith("FallGuy"))
				{
					if (Enumerable.Contains<string>(LoaderData.secretScenes, levelId))
					{
						SceneManager.LoadSceneAsync(levelId, 0);
					}
					else
					{
						Addressables.LoadScene(levelId, mode, true, 0);
					}
					this.currentScene = levelId;
				}
				else
				{
					if (Enumerable.Contains<string>(LoaderData.secretScenes, this.currentRound.SceneData.PrimeLevel.SceneName))
					{
						if (this.currentRound.SceneData.PrimeLevel.SceneName == "fallguy_obstacles_symphony_2")
						{
							SceneManager.LoadSceneAsync("FallGuy_Obstacles_Symphony_2", 0);
						}
						else
						{
							SceneManager.LoadSceneAsync(this.currentRound.SceneData.PrimeLevel.SceneName, 0);
						}
					}
					else
					{
						Addressables.LoadScene(this.currentRound.SceneData.PrimeLevel.SceneName, mode, true, 0);
					}
					this.currentScene = this.currentRound.SceneData.PrimeLevel.SceneName;
				}
				this.server = false;
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000394C File Offset: 0x00001B4C
		private void HandleClientGameManager()
		{
			Debug.Log("BlunderLoader | Handling Local play.");
			try
			{
				if (this.clientGameManager != null)
				{
					this.clientGameManager.Shutdown();
				}
				ClientPlayerManager clientPlayerManager = new ClientPlayerManager();
				CharacterVfxSO vfxprefabs = Singleton<GlobalGameStateClient>.Instance.CommonConfig.VFXPrefabs;
				MPGNetObjectManager netObjectManager = Singleton<GlobalGameStateClient>.Instance.NetObjectManager;
				if (netObjectManager != null && vfxprefabs != null && clientPlayerManager != null)
				{
					this.clientGameManager = new ClientGameManager(0, clientPlayerManager, vfxprefabs, netObjectManager, true);
					Singleton<GlobalGameStateClient>.Instance.GameStateView.SetClientGameManager(this.clientGameManager);
					Debug.Log("BlunderLoader | Game ready.");
					this.clientGameManager.SetReady(4, null, null);
					this.clientGameManager._readinessState = 6;
					this.clientGameManager._cameraDirector = this.cameraDirector;
					this.clientGameManager._commonConfig = Singleton<GlobalGameStateClient>.Instance.CommonConfig;
					this.clientGameManager.StartPreparationPhase();
				}
			}
			catch (Exception ex)
			{
				Debug.Log("BlunderLoader | HandleLocalPlay error: " + ex.Message);
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003A60 File Offset: 0x00001C60
		private void CreateRoundList()
		{
			foreach (KeyValuePair<string, Round> keyValuePair in this.roundsSO.Rounds)
			{
				Round value = keyValuePair.Value;
				if (!value.Id.Contains("wle"))
				{
					string text = string.Concat(new string[]
					{
						value.DisplayName,
						" - ",
						value.SceneData.GetSceneName(),
						" - ",
						value.Id
					});
					this.loadableRounds.Add(text);
				}
			}
			string text2 = Path.Combine(Paths.PluginPath, "BlunderLoader\\Loadable rounds.txt");
			File.WriteAllLines(text2, this.loadableRounds);
			Debug.Log("BlunderLoader | Rounds saved in: " + text2);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003B24 File Offset: 0x00001D24
		public void OnTriggerCheckpoint(CheckpointZone checkpointZone)
		{
			if (checkpointZone != this.lastCheckpoint)
			{
				this.fallGuysCharacterController.CharacterEventSystem.RaiseEvent<VfxCheckpointEvent>(FGEventFactory.GetVfxCheckpointEvent());
				this.respawn.transform.position = this.player.transform.position;
				this.respawn.transform.rotation = this.player.transform.rotation;
				this.lastCheckpoint = checkpointZone;
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003B9B File Offset: 0x00001D9B
		private IEnumerator LoaderSplash()
		{
			while (!Object.FindObjectOfType<MainMenuManager>())
			{
				yield return null;
			}
			yield return new WaitForFixedUpdate();
			if ((Object)Object.FindObjectOfType<BootSplashScreenViewModel>())
			{
				BootSplashScreenViewModel bootSplashScreenViewModel = Object.FindObjectOfType<BootSplashScreenViewModel>();
				Texture2D texture2D = LoaderTools.DownloadTexture2D("https://raw.githubusercontent.com/mattiFG/LoaderFiles/main/BlunderLoaderSplash.jpg", 1920, 1080);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
				sprite.name = "BlunderLoaderSplash";
				bootSplashScreenViewModel._slides.Add(sprite);
			}
			yield break;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003BA4 File Offset: 0x00001DA4
		private void RandomizeCheckpoint()
		{
			if (this.startingPositions == null)
			{
				this.startingPositions = Object.FindObjectsOfType<MultiplayerStartingPosition>();
			}
			if (this.respawn == null)
			{
				this.respawn = GameObject.CreatePrimitive(3);
				if (!LoaderData.CheckpointCollider)
				{
					this.respawn.gameObject.GetComponent<Collider>().enabled = false;
				}
				Renderer component = this.respawn.GetComponent<Renderer>();
				if (LoaderData.InvisibleCheckpoint)
				{
					component.enabled = false;
				}
				else
				{
					component.material.color = new Color((float)LoaderData.Red, (float)LoaderData.Green, (float)LoaderData.Blue);
				}
				this.respawn.name = "checkpoint";
			}
			int num = Random.Range(0, this.startingPositions.Length);
			this.respawn.transform.position = this.startingPositions[num].gameObject.transform.position;
			this.respawn.transform.rotation = this.startingPositions[num].gameObject.transform.rotation;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003CAB File Offset: 0x00001EAB
		public IEnumerator LoadBundleAndScene(string bundleName, string sceneName, LoadSceneParameters loadSceneParameters)
		{
			string text = Path.Combine(new string[] { Application.streamingAssetsPath + "/aa~/StandaloneWindows64/" + bundleName });
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(text);
			yield return request;
			AssetBundle assetBundle = request.assetBundle;
			SceneManager.LoadScene(sceneName, loadSceneParameters);
			yield break;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003CC8 File Offset: 0x00001EC8
		private void LoadOnlyBundle(string bundleName)
		{
			AssetBundle.LoadFromFileAsync(Path.Combine(new string[] { Application.streamingAssetsPath + "/aa~/StandaloneWindows64/" + bundleName }));
			Debug.Log("BlunderLoader | Loaded bundle: " + bundleName);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003D03 File Offset: 0x00001F03
		private IEnumerator AssetBundleLoader(string name, bool spawn)
		{
			string text = Path.Combine(Paths.PluginPath, "BlunderLoader\\Media\\" + name);
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(text);
			yield return request;
			AssetBundle assetBundle = request.assetBundle;
			if (assetBundle != null && spawn)
			{
				AssetBundleRequest request_ = assetBundle.LoadAllAssetsAsync();
				yield return request_;
				GameObject[] array = (GameObject[])request_.allAssets;
				for (int i = 0; i < array.Length; i++)
				{
					Object.Instantiate<GameObject>(array[i], Vector3.zero, Quaternion.identity);
				}
				request_ = null;
			}
			yield break;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003D1C File Offset: 0x00001F1C
		public void OnGUI()
		{
			if (this.CanLoadUI && this.UI)
			{
				if (this.uiState == BlunderLoaderManager.UIState.Main)
				{
					this.LoaderGUI();
				}
				if (this.uiState == BlunderLoaderManager.UIState.Settings)
				{
					this.SettingsGUI();
				}
				if (this.uiState == BlunderLoaderManager.UIState.MediaTools)
				{
					this.MediaGUI();
				}
				if (this.uiState == BlunderLoaderManager.UIState.CMSTools_Main)
				{
					this.CMSToolsMainUI();
				}
				if (this.uiState == BlunderLoaderManager.UIState.CMSTools_Round)
				{
					this.CMSToolsRoundAdd();
				}
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003D84 File Offset: 0x00001F84
		public void LoaderGUI()
		{
			this.sceneToLoad = GUI.TextField(new Rect(LoaderData.x, LoaderData.y, LoaderData.width, LoaderData.height), this.sceneToLoad);
			if (LoaderData.UseLoadingScreenLocalPlay)
			{
				if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 25f, LoaderData.width, LoaderData.height), "Local play"))
				{
					this.sceneToLoad = this.sceneToLoad.ToLower();
					this.localPlay = true;
					this.server = false;
					base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.LoadLevel()));
					this.rollOff_fix = false;
					if (this.clientGameManager != null)
					{
						this.clientGameManager.CurrentGameSession.CurrentSessionState = 1;
					}
					if (LoaderData.HideUiWhenPressingLocalPlay)
					{
						this.UI = false;
					}
				}
				if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 50f, LoaderData.width, LoaderData.height), "Additive"))
				{
					this.actualLevelName = this.sceneToLoad.ToLower();
					this.SetRound(this.actualLevelName, 1, true, false);
				}
			}
			else
			{
				if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 25f, LoaderData.width, LoaderData.height), "Single"))
				{
					this.localPlay = true;
					if (this.clientGameManager != null)
					{
						this.clientGameManager.CurrentGameSession.CurrentSessionState = 1;
					}
					this.SetRound(this.sceneToLoad, 0, true, true);
					this.SetupUI();
				}
				if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 50f, LoaderData.width, LoaderData.height), "Additive"))
				{
					this.SetRound(this.sceneToLoad, 0, true, true);
				}
				if (this.roundHasData && GUI.Button(new Rect(LoaderData.x, LoaderData.y + 75f, LoaderData.width, LoaderData.height), "Variations"))
				{
					this.Variations();
				}
			}
			if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 125f, LoaderData.width, LoaderData.height), "MediaTools"))
			{
				this.uiState = BlunderLoaderManager.UIState.MediaTools;
			}
			if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 175f, LoaderData.width, LoaderData.height), "Leave"))
			{
				Addressables.LoadScene("MainMenu", 0, true, 0);
				if (this.clientGameManager != null)
				{
					this.clientGameManager.CurrentGameSession.CurrentSessionState = 1;
				}
			}
			if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 200f, LoaderData.width, LoaderData.height), "Settings"))
			{
				this.uiState = BlunderLoaderManager.UIState.Settings;
			}
			if (GUI.Button(new Rect(LoaderData.x, LoaderData.y + 225f, LoaderData.width, LoaderData.height), "Load bundle"))
			{
				this.LoadOnlyBundle(this.sceneToLoad);
			}
			if (!LoaderData.UseLoadingScreenLocalPlay && GUI.Button(new Rect(LoaderData.x, LoaderData.y + 250f, LoaderData.width, LoaderData.height), "Spawn"))
			{
				this.clientGameManager.CurrentGameSession.CurrentSessionState = 1;
				this.localPlay = true;
				this.FixObstacles(true);
				base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.LoadLevel()));
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000040CC File Offset: 0x000022CC
		private void SettingsGUI()
		{
			GUILayout.Label("Settings", null);
			LoaderData.UseLoadingScreenLocalPlay = GUILayout.Toggle(LoaderData.UseLoadingScreenLocalPlay, "Use round loader", null);
			if (LoaderData.UseLoadingScreenLocalPlay)
			{
				LoaderData.EnableUiLocalPlay = GUILayout.Toggle(LoaderData.EnableUiLocalPlay, "Enable in game UI (top left)", null);
			}
			LoaderData.CheckpointCollider = GUILayout.Toggle(LoaderData.CheckpointCollider, "Checkpoint collider", null);
			LoaderData.InvisibleCheckpoint = GUILayout.Toggle(LoaderData.InvisibleCheckpoint, "Invisible checkpoint", null);
			if (LoaderData.UseLoadingScreenLocalPlay)
			{
				LoaderData.EnableServerThings = GUILayout.Toggle(LoaderData.EnableServerThings, "Enable server things (sometimes unstable)", null);
			}
			LoaderData.UseBlunderBranding = GUILayout.Toggle(LoaderData.UseBlunderBranding, "Use BlunderLoader branding", null);
			if (GUILayout.Button("Edit cursor key", null))
			{
				this.keyToChange = 0;
				this.waitingForKey = true;
			}
			if (GUILayout.Button("Edit UI key", null))
			{
				this.keyToChange = 1;
				this.waitingForKey = true;
			}
			if (GUILayout.Button("Edit respawn key", null))
			{
				this.keyToChange = 2;
				this.waitingForKey = true;
			}
			if (GUILayout.Button("Edit checkpoint key", null))
			{
				this.keyToChange = 3;
				this.waitingForKey = true;
			}
			if (GUILayout.Button("Edit reset checkpoint key", null))
			{
				this.keyToChange = 4;
				this.waitingForKey = true;
			}
			if (GUILayout.Button("Close settings", null))
			{
				this.uiState = BlunderLoaderManager.UIState.Main;
			}
			this.red = GUI.TextField(new Rect(10f, 360f, 100f, 20f), this.red);
			this.green = GUI.TextField(new Rect(10f, 400f, 100f, 20f), this.green);
			this.blue = GUI.TextField(new Rect(10f, 440f, 100f, 20f), this.blue);
			LoaderData.Red = int.Parse(this.red);
			LoaderData.Green = int.Parse(this.green);
			LoaderData.Blue = int.Parse(this.blue);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000042BC File Offset: 0x000024BC
		private void MediaGUI()
		{
			GUILayout.Label("MediaTools", null);
			if (GUILayout.Button("Load image", null))
			{
				this.ImageLoader(this.sceneToLoad);
				this.uiState = BlunderLoaderManager.UIState.Main;
			}
			if (GUILayout.Button("Back", null))
			{
				this.uiState = BlunderLoaderManager.UIState.Main;
			}
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004308 File Offset: 0x00002508
		private void CMSToolsMainUI()
		{
			GUILayout.Label("CMSTools Menu", null);
			if (GUILayout.Button("Add round", null))
			{
				this.uiState = BlunderLoaderManager.UIState.CMSTools_Round;
			}
			if (GUILayout.Button("Add show", null))
			{
				this.uiState = BlunderLoaderManager.UIState.CMSTools_Show;
			}
			if (GUILayout.Button("Add Live event", null))
			{
				this.uiState = BlunderLoaderManager.UIState.CMSTools_LiveEvent;
			}
			if (GUILayout.Button("Close CMSTools", null))
			{
				this.uiState = BlunderLoaderManager.UIState.Main;
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004370 File Offset: 0x00002570
		private void CMSToolsRoundAdd()
		{
			GUILayout.Label("CMSTools - Add round", null);
			this.roundId = GUI.TextField(new Rect(LoaderData.x, LoaderData.y, LoaderData.width, LoaderData.height), this.roundId);
			GUI.Label(new Rect(LoaderData.x + LoaderData.width, LoaderData.y, LoaderData.width, LoaderData.height), "Round ID");
			this.roundName = GUI.TextField(new Rect(LoaderData.x, LoaderData.y + 30f, LoaderData.width, LoaderData.height), this.roundName);
			GUI.Label(new Rect(LoaderData.x + LoaderData.width, LoaderData.y + 30f, LoaderData.width, LoaderData.height), "Round name");
			this.roundScene = GUI.TextField(new Rect(LoaderData.x, LoaderData.y + 60f, LoaderData.width, LoaderData.height), this.roundScene);
			GUI.Label(new Rect(LoaderData.x + LoaderData.width, LoaderData.y + 60f, LoaderData.width, LoaderData.height), "Round scene");
			this.description = GUI.TextField(new Rect(LoaderData.x, LoaderData.y + 90f, LoaderData.width, LoaderData.height), this.description);
			GUI.Label(new Rect(LoaderData.x + LoaderData.width, LoaderData.y + 90f, LoaderData.width, LoaderData.height), "Round description");
			this.roundImage = GUI.TextField(new Rect(LoaderData.x, LoaderData.y + 120f, LoaderData.width, LoaderData.height), this.roundImage);
			GUI.Label(new Rect(LoaderData.x + LoaderData.width, LoaderData.y + 120f, LoaderData.width, LoaderData.height), "Round image from Media folder");
			if (GUI.Button(new Rect(LoaderData.x + LoaderData.width, LoaderData.y + 150f, LoaderData.width, LoaderData.height), "Add round"))
			{
				this.uiState = BlunderLoaderManager.UIState.CMSTools_Main;
				this.AddRound(this.roundId, this.roundLocKey, this.roundName, this.descLocKey, this.description, this.roundScene);
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000045B9 File Offset: 0x000027B9
		[CompilerGenerated]
		private void <SecondMessage>g__SetVisible|87_0(bool _bool)
		{
			this.CanLoadUI = true;
			this.UI = true;
		}

		// Token: 0x04000002 RID: 2
		public static BlunderLoaderManager Instance;

		// Token: 0x04000004 RID: 4
		public bool UI;

		// Token: 0x04000005 RID: 5
		private string sceneToLoad = "";

		// Token: 0x04000006 RID: 6
		public Main manager;

		// Token: 0x04000007 RID: 7
		private Texture2D imageTexture;

		// Token: 0x04000008 RID: 8
		private Sprite imageSprite;

		// Token: 0x04000009 RID: 9
		private VideoPlayer videoPlayer;

		// Token: 0x0400000A RID: 10
		private Texture2D videoTexture;

		// Token: 0x0400000B RID: 11
		private string currentScene;

		// Token: 0x0400000C RID: 12
		public Texture2D logo;

		// Token: 0x0400000D RID: 13
		public RawImage rawImage;

		// Token: 0x0400000F RID: 15
		public bool server;

		// Token: 0x04000010 RID: 16
		public RoundsSO roundsSO;

		// Token: 0x04000011 RID: 17
		private LocalisedStrings localisedStrings;

		// Token: 0x04000012 RID: 18
		private string actualLevelName;

		// Token: 0x04000013 RID: 19
		private Round currentRound;

		// Token: 0x04000014 RID: 20
		private List<string> loadableRounds = new List<string>();

		// Token: 0x04000015 RID: 21
		private List<Round> rounds = new List<Round>();

		// Token: 0x04000016 RID: 22
		private GlobalGameStateClient globalGameStateClient;

		// Token: 0x04000017 RID: 23
		private bool isPlayerAlreadyLoaded;

		// Token: 0x04000018 RID: 24
		private MultiplayerStartingPosition[] startingPositions;

		// Token: 0x04000019 RID: 25
		private CameraDirector cameraDirector;

		// Token: 0x0400001A RID: 26
		private float introCamsDuration;

		// Token: 0x0400001B RID: 27
		private GameObject playerGameObject;

		// Token: 0x0400001C RID: 28
		public GameObject playerEliminationVolume;

		// Token: 0x0400001D RID: 29
		private GameObject player;

		// Token: 0x0400001E RID: 30
		private GameObject respawn;

		// Token: 0x0400001F RID: 31
		private GameStateMachine gameStateMachine;

		// Token: 0x04000020 RID: 32
		private ClientGameManager clientGameManager;

		// Token: 0x04000021 RID: 33
		private FallGuysCharacterController fallGuysCharacterController;

		// Token: 0x04000022 RID: 34
		private FallGuysCharacterControllerInput fallGuysCharacterControllerInput;

		// Token: 0x04000023 RID: 35
		public ClientPlayerUpdateManager clientPlayerUpdateManager;

		// Token: 0x04000024 RID: 36
		private CheckpointZone lastCheckpoint;

		// Token: 0x04000025 RID: 37
		private OfflinePlaygroundManager offlinePlaygroundManager;

		// Token: 0x04000026 RID: 38
		private MPGNetObject CurrentNetObject;

		// Token: 0x04000027 RID: 39
		private MPGNetObject mpg;

		// Token: 0x04000028 RID: 40
		private bool roundHasData;

		// Token: 0x04000029 RID: 41
		private BlunderLoaderManager.UIState uiState;

		// Token: 0x0400002A RID: 42
		private InGameUiManager inGameUiManager;

		// Token: 0x0400002B RID: 43
		private ScreenMetaData loadingMetaData;

		// Token: 0x0400002C RID: 44
		private LoadingGameScreenViewModel loading;

		// Token: 0x0400002D RID: 45
		private InGameIntroCameraState inGameIntroCameraState;

		// Token: 0x0400002E RID: 46
		private Vector2 scrollPosition;

		// Token: 0x0400002F RID: 47
		private GUIStyle verticalScrollbarStyle;

		// Token: 0x04000030 RID: 48
		private bool roundSelector;

		// Token: 0x04000031 RID: 49
		private string red = "255";

		// Token: 0x04000032 RID: 50
		private string green = "0";

		// Token: 0x04000033 RID: 51
		private string blue = "0";

		// Token: 0x04000034 RID: 52
		private AssetBundle[] allBundles;

		// Token: 0x04000035 RID: 53
		private Object[] allBundleObjects;

		// Token: 0x04000036 RID: 54
		private string[] allBundleObjectsString;

		// Token: 0x04000037 RID: 55
		private IPowerupPickup powerupPickup;

		// Token: 0x04000038 RID: 56
		private bool hasToShowFrogsPopup;

		// Token: 0x04000039 RID: 57
		private int keyToChange;

		// Token: 0x0400003A RID: 58
		private bool waitingForKey;

		// Token: 0x0400003B RID: 59
		private bool rollOff_fix;

		// Token: 0x0400003C RID: 60
		private AssetBundle assetBundle;

		// Token: 0x0400003D RID: 61
		private Object[] loadedAssets;

		// Token: 0x0400003E RID: 62
		private float loadProgress;

		// Token: 0x0400003F RID: 63
		private string roundId = "BlunderRound";

		// Token: 0x04000040 RID: 64
		private string roundName = "BlunderRound";

		// Token: 0x04000041 RID: 65
		private string roundImage;

		// Token: 0x04000042 RID: 66
		private string roundScene;

		// Token: 0x04000043 RID: 67
		private string roundLocKey;

		// Token: 0x04000044 RID: 68
		private string descLocKey;

		// Token: 0x04000045 RID: 69
		private string description;

		// Token: 0x02000013 RID: 19
		private enum UIState
		{
			// Token: 0x0400006A RID: 106
			Main,
			// Token: 0x0400006B RID: 107
			Settings,
			// Token: 0x0400006C RID: 108
			MediaTools,
			// Token: 0x0400006D RID: 109
			CMSTools_Main,
			// Token: 0x0400006E RID: 110
			CMSTools_Round,
			// Token: 0x0400006F RID: 111
			CMSTools_Show,
			// Token: 0x04000070 RID: 112
			CMSTools_LiveEvent
		}
	}
}
