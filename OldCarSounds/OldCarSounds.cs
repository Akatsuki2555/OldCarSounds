using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HutongGames.PlayMaker;
using MSCLoader;
using OldCarSounds.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldCarSounds {
	public class OldCarSounds : Mod {
		public override string ID => "OldCarSounds";

		public override string Name => "Old Car Sounds";

		public override string Author => "MLDKYT";

		public override string Version => "1.6-pre";

		public override bool UseAssetsFolder => true;

		public override void OnNewGame() {
			if (!LoadGameOnMenu) {
				return;
			}

			LoadGameOnMenu = false;
			Application.LoadLevel(0b11);
		}

		public override void OnLoad() {
			if (File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(this), "log.log"))) {
				File.Delete(Path.Combine(ModLoader.GetModSettingsFolder(this), "log.log"));
			}

			PrintF("Starting Loading of OldCarSounds...", "load");
			PrintF("Loading AssetBundle", "load");
			Stream manifestResourceStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream("OldCarSounds.Resources.oldsound.unity3d");
			if (manifestResourceStream != null) {
				byte[] array = new byte[manifestResourceStream.Length];
				_ = manifestResourceStream.Read(array, 0b0, array.Length);
				AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(array);
				if (EngineSoundsTypeSettings.Value.ToString() == "2") {
					PrintF("Loading audio files from old builds...", "load");
					Clip2 = assetBundle.LoadAsset<AudioClip>("idle_sisa");
					Clip1 = assetBundle.LoadAsset<AudioClip>("idle");
				}

				if ((bool)AssembleSounds.Value) {
					PrintF("Loading audio files for assembly sounds...", "load");
					Clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
				}

				_noSel = assetBundle.LoadAsset<Material>("nosel");
				if ((bool)OldRadioSongsSettings.Value) {
					PrintF("Loading radio songs...", "load");
					Radio1 = assetBundle.LoadAsset("oldradio") as GameObject;
					if (ModLoader.CheckSteam()) {
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("mustamies"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("oldradiosong"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song2"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song3"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song4"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song5"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song6"));
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song7"));
					}
					else {
						RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("pirate"));
					}

					string path = Path.Combine(ModLoader.GetModAssetsFolder(this), "radiosongs");
					if (File.Exists(path)) {
						foreach (string str in Directory.GetFiles(path)) {
							PrintF("Loading: " + str);
							WWW www = new WWW("file:///" + str);
							RadioCore.Clips.Add(www.GetAudioClip(true, false));
						}
					}
				}

				if ((bool)OldDashTexturesSettings.Value) {
					PrintF("Loading black material for dashboard");
					Material1 = assetBundle.LoadAsset<Material>("black");
				}

				if ((bool)SelectionSelectionSettings.Value) {
					PrintF("Loading selection material");
					SelMaterial = assetBundle.LoadAsset<Material>("selection");
				}

				PrintF("Unloading AssetBundle", "load");
				assetBundle.Unload(false);
			}
			else {
				PrintF("Error while loading mod resources. Your copy of OCS is corrupted.", "ERR", true);
			}

			Satsuma = GameObject.Find("SATSUMA(557kg, 248)");
			PrintF("Adding component for Satsuma", "load");
			Satsuma.AddComponent<SatsumaOcs>();
			if ((bool)OldRpmGaugeSettings.Value) {
				GameObject gameObject = Object.FindObjectsOfType<GameObject>()
					.First(sdf => sdf.name.ToLower().Contains("rpm gauge"));
				if (gameObject == null) {
					ModConsole.Print("Error while loading Old RPM gauge");
					goto rpmFinish;
				}

				gameObject.SetActive(true);
				gameObject.AddComponent<RPMGauge>();
			}

			rpmFinish:
			PrintF("Starting stopwatch for info text", "load");
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
			PrintF("Fully loaded!", "load", true);
		}

		public override void ModSettings() {
			AssembleSounds = new Settings("assembleSounds", "Assemble Sounds", false);
			DisableDoorSoundsSettings = new Settings("doorSounds", "Disable Door Sounds", false);
			DisableFootSoundsSettings = new Settings("footSounds", "Disable Foot Sounds", false);
			DisableKnobSoundsSettings = new Settings("knobSounds", "Disable Knob Sounds", false);
			OldDashTexturesSettings = new Settings("oldDash", "Old Dashboard", false);
			InfoTextSettings = new Settings("info", "Information Text", false);
			OldRadioSongsSettings = new Settings("radio", "Old Radio", false);
			ShiftDelaySelectionSettings = new Settings("shiftDelay", "Shift Delay Selection", 0b0);
			KeySoundSelectionSettings = new Settings("keySound", "Key Sound Selection", 0b0);
			SelectionSelectionSettings = new Settings("selection", "Green selections", false);
			EngineSoundsTypeSettings = new Settings("sounds", "Engine sound type", 0b0);
			OldRpmGaugeSettings = new Settings("rpmgauge", "Old RPM Gauge", false);
			OldDelaySettings = new Settings("oldrev", "Old engine revving", false);
			
			Settings.AddHeader(this, "Sounds");
			Settings.AddSlider(this, EngineSoundsTypeSettings, 0b0, 0b10, new[] {
				"No engine sound change",
				"Lower pitch (2016)",
				"Old alpha (2014)"
			});
			Settings.AddSlider(this, KeySoundSelectionSettings, 0b0, 0b10, new[] {
				"No change",
				"Old key sounds (2016)",
				"No key sounds (2014)"
			});
			Settings.AddCheckBox(this, AssembleSounds);
			Settings.AddCheckBox(this, OldRadioSongsSettings);
			Settings.AddCheckBox(this, DisableDoorSoundsSettings);
			Settings.AddCheckBox(this, DisableFootSoundsSettings);
			Settings.AddCheckBox(this, DisableKnobSoundsSettings);
			Settings.AddHeader(this, "Visual");
			Settings.AddSlider(this, ShiftDelaySelectionSettings, 0b0, 0b10, new[] {
				"No change",
				"Build 172",
				"No delay"
			});
			Settings.AddCheckBox(this, SelectionSelectionSettings);
			Settings.AddCheckBox(this, OldDashTexturesSettings);
			Settings.AddCheckBox(this, InfoTextSettings);
			Settings.AddCheckBox(this, OldRpmGaugeSettings);
			Settings.AddHeader(this, "Useful links");
			Settings.AddButton(this, "mod-immortality", "If you want immortality, click here to get the GodMode mod.",
				() => Process.Start("https://www.nexusmods.com/mysummercar/mods/1301"));
		}

		public override void OnGUI() {
			if (ModLoader.GetCurrentScene() != (CurrentScene)0b1) {
				return;
			}

			if (!(bool)InfoTextSettings.Value) {
				return;
			}

			float num = (float)Math.Round(1f / Time.unscaledDeltaTime, 0b10);
			float value = FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value;
			GUI.Label(new Rect(0f, 00f, 1_000f, 20f), $"OldCarSounds v{Version}");
			GUI.Label(new Rect(0f, 20f, 1_000f, 20f), $"FPS: {num:0.0}");
			GUI.Label(new Rect(0f, 40f, 1_000f, 20f), $"Wrench size: {value * 0b1010:00}");
			GUI.Label(new Rect(0f, 60f, 1_000f, 20f),
				$"Lake run current time: {_stopwatch.Elapsed.Minutes:00}:{_stopwatch.Elapsed.Seconds:00}:{_stopwatch.Elapsed.Milliseconds:000}");
			GUI.Label(new Rect(0f, 80f, 1_000f, 20f), "Lake run last time: ");
		}

		public override void Update() {
			if (Camera.main == null) {
				return;
			}

			foreach (RaycastHit raycastHit in
			         Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f)) {
				if (!(SatsumaOcs.powerKnob == null)) {
					if (raycastHit.collider.gameObject.name == SatsumaOcs.powerKnob.name) {
						if (!(bool)SelectionSelectionSettings.Value) {
							FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
							FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
							break;
						}

						SatsumaOcs.powerKnob.GetComponent<Renderer>().material = SelMaterial;
						break;
					}

					SatsumaOcs.powerKnob.GetComponent<Renderer>().material = _noSel;
				}

				if (!(SatsumaOcs.volumeKnob == null)) {
					if (raycastHit.collider.gameObject.name == SatsumaOcs.volumeKnob.name) {
						if (!(bool)SelectionSelectionSettings.Value) {
							FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
							FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
						}
						else {
							SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = SelMaterial;
						}

						float axis = Input.GetAxis("Mouse ScrollWheel");
						if (axis >= 0.01f) {
							SatsumaOcs.radioCoreInstance.IncreaseVolume();
						}

						if (axis <= -0.01f) {
							SatsumaOcs.radioCoreInstance.DecreaseVolume();
						}

						break;
					}

					SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = _noSel;
				}

				if (!(SatsumaOcs.switchKnob == null)) {
					if (raycastHit.collider.gameObject.name == SatsumaOcs.switchKnob.name) {
						if ((bool)SelectionSelectionSettings.Value) {
							SatsumaOcs.switchKnob.GetComponent<Renderer>().material = SelMaterial;
							break;
						}

						FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next";
						FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
						break;
					}

					SatsumaOcs.switchKnob.GetComponent<Renderer>().material = _noSel;
				}

				if (!(SatsumaOcs.knobChoke == null) && !(SatsumaOcs.triggerChoke == null)) {
					if (raycastHit.collider.gameObject == SatsumaOcs.triggerChoke) {
						if ((bool)SelectionSelectionSettings.Value) {
							SatsumaOcs.knobChoke.GetComponent<Renderer>().material = SelMaterial;
							FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
							FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
						}

						break;
					}

					SatsumaOcs.knobChoke.GetComponent<Renderer>().material = Material1;
				}

				if (!(SatsumaOcs.knobHazards == null) && !(SatsumaOcs.triggerHazard == null)) {
					if (raycastHit.collider.gameObject == SatsumaOcs.triggerHazard) {
						if ((bool)SelectionSelectionSettings.Value) {
							SatsumaOcs.knobHazards.GetComponent<Renderer>().material = SelMaterial;
							FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
							FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
						}

						break;
					}

					SatsumaOcs.knobHazards.GetComponent<Renderer>().material = Material1;
				}

				if (!(SatsumaOcs.knobLights == null) && !(SatsumaOcs.triggerLightModes == null)) {
					if (raycastHit.collider.gameObject == SatsumaOcs.triggerLightModes) {
						if ((bool)SelectionSelectionSettings.Value) {
							SatsumaOcs.knobLights.GetComponent<Renderer>().material = SelMaterial;
							FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
							FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
						}

						break;
					}

					SatsumaOcs.knobLights.GetComponent<Renderer>().material = Material1;
				}

				if (SatsumaOcs.knobWipers == null || SatsumaOcs.triggerButtonWiper == null)
					continue;
				if (raycastHit.collider.gameObject == SatsumaOcs.triggerButtonWiper) {
					if ((bool)SelectionSelectionSettings.Value) {
						SatsumaOcs.knobWipers.GetComponent<Renderer>().material = SelMaterial;
						FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
						FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
					}

					break;
				}

				SatsumaOcs.knobWipers.GetComponent<Renderer>().material = Material1;
			}

			if (Input.GetMouseButtonDown(0b0)) {
				foreach (RaycastHit raycastHit2 in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),
					         3f)) {
					if (SatsumaOcs.radioCoreInstance == null) continue;
					string name = raycastHit2.collider.gameObject.name;
					switch (name) {
						case "trigger_ocs_power1" when SatsumaOcs.radioCoreInstance.on:
							SatsumaOcs.radioCoreInstance.DisableRadio();
							break;
						case "trigger_ocs_power1":
							SatsumaOcs.radioCoreInstance.EnableRadio();
							break;
						case "trigger_ocs_switch1":
							SatsumaOcs.radioCoreInstance.NextClip();
							break;
					}
				}
			}
		}

		public static void PrintF(string text, string module = "SYSTEM", bool console = false) {
			try {
				StreamWriter streamWriter =
					new StreamWriter(
						Path.Combine(
							ModLoader.GetModSettingsFolder(ModLoader.LoadedMods.First(x => x.ID == "OldCarSounds")),
							"log.log"), true);
				StringBuilder stringBuilder = new StringBuilder().Append(DateTime.Now.ToString("G")).Append(" [")
					.Append(module.ToUpper()).Append("]: ").Append(text);
				streamWriter.WriteLine(stringBuilder.ToString());
				streamWriter.Close();
				
				if (!console) return;
				string a = module.ToUpper();
				switch (a) {
					case "ERROR":
					case "ERR":
						ModConsole.Error(stringBuilder.ToString());
						break;
					case "WARN":
					case "WARNING":
						ModConsole.Warning(stringBuilder.ToString());
						break;
					default:
						ModConsole.Print(stringBuilder.ToString());
						break;
				}
			}
			catch (Exception) {
				// ignored
			}
		}

		public static string GameObjectPath(GameObject go) {
			string text = "";
			GameObject gameObject = go;
			for (;;) {
				text = gameObject.name + "/" + text;
				if (gameObject.transform.parent == null) {
					break;
				}

				gameObject = gameObject.transform.parent.gameObject;
			}

			return text;
		}

		public static Settings AssembleSounds;

		public static Settings OldRadioSongsSettings;

		public static Settings OldDashTexturesSettings;

		public static Settings InfoTextSettings;

		public static Settings DisableKnobSoundsSettings;

		public static Settings DisableDoorSoundsSettings;

		public static Settings DisableFootSoundsSettings;

		public static Settings OldRpmGaugeSettings;

		public static Settings ShiftDelaySelectionSettings;

		public static Settings KeySoundSelectionSettings;

		public static Settings SelectionSelectionSettings;

		public static Settings EngineSoundsTypeSettings;

		public static Settings OldDelaySettings;

		public static AudioClip Clip1;

		public static AudioClip Clip2;

		public static AudioClip Clip3;

		public static GameObject Radio1;

		public static GameObject Satsuma;

		public static Material Material1;

		public static Material SelMaterial;

		public static bool LoadGameOnMenu;

		private Stopwatch _stopwatch;

		private Material _noSel;
	}
}