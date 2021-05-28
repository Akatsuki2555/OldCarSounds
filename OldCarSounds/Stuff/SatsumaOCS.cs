using MSCLoader;
using UnityEngine;

namespace OldCarSounds.Stuff
{
    public class SatsumaOCS : MonoBehaviour
    {
        public static GameObject knobLights;
        public static GameObject knobWipers;
        public static GameObject knobHazards;
        public static GameObject knobChoke;
        public static GameObject triggerHazard;
        public static GameObject triggerChoke;
        public static GameObject triggerButtonWiper;
        public static GameObject triggerLightModes;
        public static RadioCore radioCoreInstance;
        public static GameObject PowerKnob;
        public static GameObject VolumeKnob;
        public static GameObject SwitchKnob;
        private Drivetrain _drivetrain;
        private SoundController _soundController;
        private GameObject carKeysIn;
        private GameObject carKeysOut;
        private GameObject carKeysSwitch;
        private GameObject closeDoorSound;
        private GameObject closeHoodSound;
        private GameObject closeTrunkSound;
        private AudioSource dashButtonAudio;
        private GameObject openDoorSound;
        private GameObject openHoodSound;
        private GameObject openTrunkSound;
        private GameObject radio1Instantiated;
        private GameObject walkingSoundsParent;

        private void Start()
        {
            _soundController = GetComponent<SoundController>();
            _drivetrain = GetComponent<Drivetrain>();
            // if the user selected "old alpha 2014" sounds
            if (OldCarSounds.engineSoundsType == 2)
            {
                // Change sounds.
                _soundController.engineNoThrottle = OldCarSounds.clip2;
                _soundController.engineThrottle = OldCarSounds.clip2;
                OldCarSounds.PrintF("Applied extra sound effects.", "load");
                // Change the pitches.
                OldCarSounds.PrintF("Loading audio pitch change...", "load");
                _soundController.engineThrottlePitchFactor = 0.95f;
                _soundController.engineNoThrottlePitchFactor = 0.4f;
                // Change more audio clips.
                gameObject.transform.GetChild(40).GetComponent<AudioSource>().clip = OldCarSounds.clip2;
                gameObject.transform.GetChild(41).GetComponent<AudioSource>().clip = OldCarSounds.clip2;
                OldCarSounds.PrintF("Loaded in engine sound effects.", "load");
                // Play these audio clips otherwise the sound will be quiet.
                // In Unity, when you change the clip, the sound will not play automatically.
                gameObject.transform.GetChild(40).GetComponent<AudioSource>().Play();
                gameObject.transform.GetChild(41).GetComponent<AudioSource>().Play();
                // Set exhaust sounds.
                gameObject.transform.Find("CarSimulation/Exhaust/FromMuffler").GetComponent<AudioSource>().clip = OldCarSounds.clip1;
                gameObject.transform.Find("CarSimulation/Exhaust/FromHeaders").GetComponent<AudioSource>().clip = OldCarSounds.clip1;
                gameObject.transform.Find("CarSimulation/Exhaust/FromPipe").GetComponent<AudioSource>().clip = OldCarSounds.clip1;
                gameObject.transform.Find("CarSimulation/Exhaust/FromEngine").GetComponent<AudioSource>().clip = OldCarSounds.clip1;
                OldCarSounds.PrintF("Applied exhaust sound to the exhaust.", "load");
            }

            // if the user selected "first release 2016" sounds
            if (OldCarSounds.engineSoundsType == 1)
                // adjust pitches
                _soundController.engineThrottlePitchFactor = 1.0f;

            OldCarSounds.PrintF("Applied sound effects for engine.", "load");

            OldCarSounds.PrintF("Loading old sounds for removing/attaching parts", "load");
            //Only apply assemble sound if the user enabled it.
            if (OldCarSounds.loadAssembleSound)
            {
                //Find the container of assemble and disassemble sounds.
                GameObject go1 = GameObject.Find("MasterAudio/CarBuilding");
                //Set the disassemble clip.
                go1.transform.Find("disassemble").GetComponent<AudioSource>().clip = OldCarSounds.clip3;
                //Set the assemble clip.
                go1.transform.Find("assemble").GetComponent<AudioSource>().clip = OldCarSounds.clip3;
            }


            // Load old dash texture  if the user has chosen to
            if (OldCarSounds.oldDashTextures)
            {
                OldCarSounds.PrintF("Loading old car textures...", "LOAD");
                // Create the old reflective material.
                Material theMaterial = OldCarSounds.material1;
                // Apply to dashboard.
                GameObject dashboardClone = GameObject.Find("dashboard(Clone)");
                dashboardClone.GetComponent<MeshRenderer>().material = theMaterial;
                // Apply to stock steering wheel.
                GameObject steeringWheelClone = GameObject.Find("stock steering wheel(Clone)");
                steeringWheelClone.GetComponent<MeshRenderer>().material = theMaterial;
                // Apply to dashboard meters.
                GameObject dashboardMetersClone = GameObject.Find("dashboard meters(Clone)");
                dashboardMetersClone.GetComponent<MeshRenderer>().material = theMaterial;
                // Define game object variables for knobs.
                triggerHazard = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Hazard").gameObject;
                triggerButtonWiper = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/ButtonWipers").gameObject;
                triggerChoke = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Choke").gameObject;
                triggerLightModes = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/LightModes").gameObject;
                knobChoke = dashboardMetersClone.transform.Find("Knobs/KnobChoke/knob").gameObject;
                knobChoke.GetComponent<Renderer>().material = theMaterial;
                knobHazards = dashboardMetersClone.transform.Find("Knobs/KnobHazards/knob").gameObject;
                knobHazards.GetComponent<Renderer>().material = theMaterial;
                knobWipers = dashboardMetersClone.transform.Find("Knobs/KnobWasher/knob").gameObject;
                knobWipers.GetComponent<Renderer>().material = theMaterial;
                knobLights = dashboardMetersClone.transform.Find("Knobs/KnobLights/knob").gameObject;
                knobLights.GetComponent<Renderer>().material = theMaterial;
            }

            OldCarSounds.PrintF("Adjusting drivetrain sounds...", "load");
            //Make drivetrain quieter.
            _soundController.transmissionVolume = 0.08f;
            _soundController.transmissionVolumeReverse = 0.08f;
            OldCarSounds.PrintF("Adjusting drivetrain done.", "load");

            OldCarSounds.PrintF("Adjusting shift delay...", "load");
            // Shift delay selection load
            if (OldCarSounds.shiftDelaySelection != 0)
            {
                if (OldCarSounds.shiftDelaySelection == 1)
                    // Old shift delay
                    _drivetrain.shiftTime = 0;

                if (OldCarSounds.shiftDelaySelection == 2)
                    // No shift delay
                    _drivetrain.shiftTime = 0.0000001f;
            }

            OldCarSounds.PrintF("Adjusting shift delay done.", "load");


            //If the user enabled the old radio
            if (OldCarSounds.oldRadioSongs)
            {
                OldCarSounds.PrintF("Loading old radio...", "LOAD");
                // Spawn old car radio
                OldCarSounds.PrintF("Spawning cube radio...", "load");
                radio1Instantiated = Instantiate(OldCarSounds.radio1);
                // Define knobs
                PowerKnob = radio1Instantiated.transform.Find("trigger_ocs_power1").gameObject;
                VolumeKnob = radio1Instantiated.transform.Find("trigger_ocs_volume1").gameObject;
                SwitchKnob = radio1Instantiated.transform.Find("trigger_ocs_switch1").gameObject;
                // Add a script to the radio
                OldCarSounds.PrintF("Adding script to cube radio...", "load");
                radioCoreInstance = radio1Instantiated.AddComponent<RadioCore>();
            }

            OldCarSounds.PrintF("Finished loading old radio.", "load");

            // Disable dashboard knob sounds when the user enables it.
            if (OldCarSounds.disableKnobSounds)
            {
                OldCarSounds.PrintF("Disabling knob sounds: Definition...", "load");
                // Define the audio source
                dashButtonAudio = GameObject.Find("CarFoley/dash_button").GetComponent<AudioSource>();
                OldCarSounds.PrintF("Disabling knob sounds: Definition done.", "load");
            }

            OldCarSounds.PrintF("Defining key sounds...", "load");
            // Define the key sounds
            carKeysIn = GameObject.Find("CarFoley/carkeys_in");
            carKeysOut = GameObject.Find("CarFoley/carkeys_out");
            carKeysSwitch = GameObject.Find("CarFoley/ignition_keys");

            // Define door sounds
            if (OldCarSounds.disableDoorSounds)
            {
                openDoorSound = GameObject.Find("CarFoley/open_door1");
                openHoodSound = GameObject.Find("CarFoley/open_hood1");
                openTrunkSound = GameObject.Find("CarFoley/open_trunk1");
                closeDoorSound = GameObject.Find("CarFoley/close_door1");
                closeHoodSound = GameObject.Find("CarFoley/close_hood1");
                closeTrunkSound = GameObject.Find("CarFoley/close_trunk1");
            }

            if (OldCarSounds.disableFootSounds) walkingSoundsParent = GameObject.Find("Walking");
        }

        private void Update()
        {
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                // If No engine overrev is enabled
                if (OldCarSounds.noEngineOverRev)
                    // Check if the RPM is high
                    if (_drivetrain.rpm > 7500)
                        // Set it slightly under the point.
                        _drivetrain.rpm = 7500;

                // Car key sounds
                if (OldCarSounds.keySoundSelection != 0)
                {
                    // Old key sounds (2016)
                    if (OldCarSounds.keySoundSelection == 1 || OldCarSounds.keySoundSelection == 2)
                    {
                        // Disable the carkeysin and carkeysout sounds
                        if (carKeysIn.GetComponent<AudioSource>().isPlaying) carKeysIn.GetComponent<AudioSource>().Stop();

                        if (carKeysOut.GetComponent<AudioSource>().isPlaying) carKeysOut.GetComponent<AudioSource>().Stop();
                    }

                    if (OldCarSounds.keySoundSelection == 2)
                        if (carKeysSwitch.GetComponent<AudioSource>().isPlaying)
                            carKeysSwitch.GetComponent<AudioSource>().Stop();
                }


                // If the user has chosen to
                if (OldCarSounds.disableKnobSounds)
                    // Disable the dashboard sound constantly.
                    if (dashButtonAudio.isPlaying)
                        dashButtonAudio.Stop();

                if (OldCarSounds.disableFootSounds)
                    foreach (AudioSource item in walkingSoundsParent.GetComponentsInChildren<AudioSource>())
                        if (item.isPlaying)
                            item.Stop();

                if (OldCarSounds.disableDoorSounds)
                {
                    if (openDoorSound.GetComponent<AudioSource>().isPlaying) openDoorSound.GetComponent<AudioSource>().Stop();
                    if (openHoodSound.GetComponent<AudioSource>().isPlaying) openHoodSound.GetComponent<AudioSource>().Stop();
                    if (openTrunkSound.GetComponent<AudioSource>().isPlaying) openTrunkSound.GetComponent<AudioSource>().Stop();
                    if (closeDoorSound.GetComponent<AudioSource>().isPlaying) closeDoorSound.GetComponent<AudioSource>().Stop();
                    if (closeHoodSound.GetComponent<AudioSource>().isPlaying) closeHoodSound.GetComponent<AudioSource>().Stop();
                    if (closeTrunkSound.GetComponent<AudioSource>().isPlaying) closeTrunkSound.GetComponent<AudioSource>().Stop();
                }

                if (OldCarSounds.oldDelay) _drivetrain.revLimiterTime = 0.1f;
            }
        }
    }
}