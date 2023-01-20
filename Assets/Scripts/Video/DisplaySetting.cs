using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class DisplaySetting : MonoBehaviour
    {
        protected Material displayMaterial = null;
        public Vector2 DisplaySize
        {
            get => new Vector2(this.transform.localScale.x, this.transform.localScale.y);
            set => this.transform.localScale = new Vector3(value.x, value.y, 1.0f);
        }

        protected void FitVideoAspect(float marginedAspectRatio, float targetAspectRatio)
        {
            if (displayMaterial == null) return;

            float xScale = targetAspectRatio / marginedAspectRatio;
            displayMaterial.mainTextureScale = new Vector2(xScale, 1.0f);

            float xOffset = 0.5f * (marginedAspectRatio - targetAspectRatio) / marginedAspectRatio;
            displayMaterial.mainTextureOffset = new Vector2(xOffset, 0.0f);
        }
    }
}
