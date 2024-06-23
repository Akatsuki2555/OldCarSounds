using UnityEngine;

namespace OldCarSounds.Stuff
{
	public class RPMGauge : MonoBehaviour
	{
		private void Start()
		{
			if ((bool)OldCarSounds.OldRpmGaugeSettings.Value)
			{
				transform.FindChild("Pivot/needle").gameObject.transform.localScale = new Vector3(0.64f, 1f, 0.8f);
			}
		}
	}
}