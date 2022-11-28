using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetQuality : MonoBehaviour
{
    enum IMAGE_QUALITY{
        VERY_LOW,
        LOW,
        MID,
        HIGH,
        VERY_HIGH,
        ULTRA,
    };
    
    // Start is called before the first frame update
    void Start()
    {
		QualitySettings.SetQualityLevel((int)IMAGE_QUALITY.HIGH, true);        
		Screen.SetResolution(1920, 1080, false/* full screen */);
		// Screen.SetResolution(624, 624, false/* full screen */);
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
