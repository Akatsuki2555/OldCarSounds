﻿using UnityEngine;

namespace OldCarSounds.Stuff {

    public class RPMGauge : MonoBehaviour {

        private void Start() {
            if ((bool)OldCarSounds.OldRpmGaugeSettings.Value) {
                GameObject o = transform.FindChild("Pivot/needle").gameObject;
                o.transform.localScale = new Vector3(0.64f, 1, 0.8f);
            }
        }
    }
}
