using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SS_KinetrackIII;

namespace KW_Mocap
{
    public class MotionCaptureController : MonoBehaviour
    {
        [SerializeField] GameObject hand;
        public int cal_sec = 3;
        public bool isCalibrating = false;
        public int max_sec = 1800;	// = 30 minutes

        private SS_DAT Dat = new SS_DAT();
        private SS_IMU Imu = new SS_IMU();
        private SS_STAT statusPanel = new SS_STAT();
        private IMUHandModel imuHandModel;
        private HandSetting handSetting;
        private LeapHandModel leapHandModel;
        private InputField I_net;
        private Button B_net;
        private Toggle Tg_rdy;
        private Image Im_rdy;
        private int run_mode = -1;	// -1 NRY / 0 STBY / 1 RDY / 2 REC,CAL
        private RunMode runMode = RunMode.NotReady;

        public enum RunMode
        {
            NotReady = -1,
            Standby = 0,
            Ready = 1,
            Recording = 2
        }


        void Start()
        {
            Tg_rdy = this.transform.Find("Tgl").GetComponent<Toggle>();
            Im_rdy = this.transform.Find("Tgl/Background/Checkmark").GetComponent<Image>();
            B_net = this.transform.Find("Btn").GetComponent<Button>();
            I_net = this.transform.Find("I_net").GetComponent<InputField>();
            B_net.onClick.AddListener(OnBtn_Connect);
            Tg_rdy.onValueChanged.AddListener(OnBtn_Ready);
            
            handSetting = hand.GetComponent<HandSetting>();
            leapHandModel = hand.GetComponent<LeapHandModel>();
            statusPanel.Init(this.gameObject, hand.name);

            /* 最後に使用されたIPアドレスを入力 */
            string last_ip = PlayerPrefs.GetString(gameObject.name + "last_ip");
            if (last_ip.Length > 10) I_net.text = last_ip;
            //run_mode = -1;
            runMode = RunMode.NotReady;

            Tg_rdy.isOn = false;
        }

        void Update()
        {
            if (runMode == RunMode.NotReady || runMode == RunMode.Standby) return;

            if (Imu.flg_setdat)
            {
                if (leapHandModel.isDetected)
                {
                    Imu.Pmc.n0 = 99;
                    Imu.Pmc.p0 = hand.transform.localPosition;
                }
                Dat.Rec(Imu.Pim, Imu.Pmc);
                //flg_newdat = true;
                Imu.flg_setdat = false;
            }
        }

        void OnBtn_Connect()
        {
            if (Imu.flg_ready)
            {
                Imu.Close();
                B_net.GetComponentInChildren<Text>().text = "Connect";
            }
            else if (I_net.text.Length < 7)
            {
                I_net.text = "Wrn...Set IPaddress";
            }
            else if (Imu.Connect(I_net.text))
            {
                if (Imu.Stat())
                {
                    max_sec = (max_sec < 60) ? 60 : max_sec;
                    long max_data = max_sec * (long)Imu.fps;    //最大フレーム数
                    long max_key = max_sec / 30 + 2;            //最大キーフレーム数
                    Dat.Init(max_data, Imu.now_sens, max_key, Imu.stat, Imu.fps);
                    imuHandModel = new IMUHandModel(hand.name, Imu.now_sens, Imu.stat, handSetting.joints);
                    statusPanel.Stat(Imu.stat);
                    statusPanel.FpsVolt(Imu.fps, Imu.vbt);
                    statusPanel.Frame(string.Format("{0:.00}", max_sec));
                    //run_mode = 0;
                    runMode = RunMode.Standby;
                    Im_rdy.color = (leapHandModel.isDetected) ? Color.blue : Color.white;
                    PlayerPrefs.SetString((gameObject.name + "last_ip"), I_net.text);
                    B_net.GetComponentInChildren<Text>().text = "Close";
                }
                else
                {
                    Imu.Close();
                    //run_mode = -1;
                    runMode = RunMode.NotReady;
                    Im_rdy.color = Color.magenta;
                    Debug.Log("Err... " + gameObject.name + " is not ready !!");
                }
            }
        }

        void OnBtn_Ready(bool flg)
        {
            if (runMode == RunMode.NotReady) return;

            Imu.Ready(flg);
            statusPanel.FpsVolt(Imu.fps, Imu.vbt);
            runMode = flg ? RunMode.Ready : RunMode.Standby;
            Im_rdy.color = (leapHandModel.isDetected) ? Color.cyan : Color.green;
            handSetting.Active(flg);
        }

        public void Calib()
        {
            if ((run_mode < 0) || isCalibrating) return;
            
            run_mode = 2;
            Im_rdy.color = Color.yellow;
            isCalibrating = true;
            StartCoroutine("Calibration");
            isCalibrating = false;
        }

        private IEnumerator Calibration()
        {
            long hs = Dat.GetNowRec();
            long dh = (long)(cal_sec * (int)Imu.fps);
            long he = hs + dh;
            /* Datに記録 */
            Dat.SetRecFlg(true);
            Imu.Rec(true);
            while (Dat.GetNowRec() < he) yield return (null);
            Dat.SetRecFlg(false);
            Imu.Rec(false);

            /* 記録したDatを元にキャリブレーション */
            imuHandModel.Calibrate(hs, dh, Dat.Pim);
            Dat.SetNowRec(he);
        }
    }
}
