using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace KW_Mocap
{
    public class GeneralManager : MonoBehaviour
    {
        public static GeneralManager I = null;

        void Awake()
        {
            if (I == null)
            {
                I = this;
                DontDestroyOnLoad(this.gameObject); //シーンの移動で破棄されない
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        void Start()
        {
            //WorldTimer.DisplayFrameCount();
            //WorldTimer.Run();
        }
    }
}
