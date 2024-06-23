using System;
using MSCLoader;
using UnityEngine;

namespace OldCarSounds.Stuff
{
	public class SatsumaOcs : MonoBehaviour
	{
		private void Start()
		{
			
		}

		private void Update()
		{
			if (ModLoader.GetCurrentScene() == (CurrentScene)1)
			{
				if (OldCarSounds.KeySoundSelectionSettings.Value.ToString() != "0")
				{
					if (OldCarSounds.KeySoundSelectionSettings.Value.ToString() == "1" || OldCarSounds.KeySoundSelectionSettings.Value.ToString() == "2")
					{
						if (_carKeysIn.GetComponent<AudioSource>().isPlaying)
						{
							_carKeysIn.GetComponent<AudioSource>().Stop();
						}
						if (_carKeysOut.GetComponent<AudioSource>().isPlaying)
						{
							_carKeysOut.GetComponent<AudioSource>().Stop();
						}
					}
					if (OldCarSounds.KeySoundSelectionSettings.Value.ToString() == "2" && _carKeysSwitch.GetComponent<AudioSource>().isPlaying)
					{
						_carKeysSwitch.GetComponent<AudioSource>().Stop();
					}
				}
				if ((bool)OldCarSounds.DisableKnobSoundsSettings.Value && _dashButtonAudio.isPlaying)
				{
					_dashButtonAudio.Stop();
				}
				if ((bool)OldCarSounds.DisableFootSoundsSettings.Value)
				{
					foreach (AudioSource audioSource in _walkingSoundsParent.GetComponentsInChildren<AudioSource>())
					{
						if (audioSource.isPlaying)
						{
							audioSource.Stop();
						}
					}
				}
				if ((bool)OldCarSounds.DisableDoorSoundsSettings.Value)
				{
					if (_openDoorSound.GetComponent<AudioSource>().isPlaying)
					{
						_openDoorSound.GetComponent<AudioSource>().Stop();
					}
					if (_openHoodSound.GetComponent<AudioSource>().isPlaying)
					{
						_openHoodSound.GetComponent<AudioSource>().Stop();
					}
					if (_openTrunkSound.GetComponent<AudioSource>().isPlaying)
					{
						_openTrunkSound.GetComponent<AudioSource>().Stop();
					}
					if (_closeDoorSound.GetComponent<AudioSource>().isPlaying)
					{
						_closeDoorSound.GetComponent<AudioSource>().Stop();
					}
					if (_closeHoodSound.GetComponent<AudioSource>().isPlaying)
					{
						_closeHoodSound.GetComponent<AudioSource>().Stop();
					}
					if (_closeTrunkSound.GetComponent<AudioSource>().isPlaying)
					{
						_closeTrunkSound.GetComponent<AudioSource>().Stop();
					}
				}
				if ((bool)OldCarSounds.OldDelaySettings.Value)
				{
					_drivetrain.revLimiterTime = 0.1f;
				}
			}
			foreach (AudioSource audioSource2 in GetComponentsInChildren<AudioSource>())
			{
				if (!(audioSource2 == null) && !(audioSource2.clip == null) && audioSource2.clip.name == "cooldown_tick")
				{
					audioSource2.Stop();
				}
			}
		}

		public static GameObject knobLights;

		public static GameObject knobWipers;

		public static GameObject knobHazards;

		public static GameObject knobChoke;

		public static GameObject triggerHazard;

		public static GameObject triggerChoke;

		public static GameObject triggerButtonWiper;

		public static GameObject triggerLightModes;

		public static RadioCore radioCoreInstance;

		public static GameObject powerKnob;

		public static GameObject volumeKnob;

		public static GameObject switchKnob;

		private Drivetrain _drivetrain;

		private SoundController _soundController;

		private GameObject _carKeysIn;

		private GameObject _carKeysOut;

		private GameObject _carKeysSwitch;

		private GameObject _closeDoorSound;

		private GameObject _closeHoodSound;

		private GameObject _closeTrunkSound;

		private AudioSource _dashButtonAudio;

		private GameObject _openDoorSound;

		private GameObject _openHoodSound;

		private GameObject _openTrunkSound;

		private GameObject _radio1Instantiated;

		private GameObject _walkingSoundsParent;
	}
}
