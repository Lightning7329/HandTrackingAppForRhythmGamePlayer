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
        public int max_sec = 1800;	// = 30 minutes

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
        public RunMode runMode { get; private set; } = RunMode.NotConnected;

        public enum RunMode
        {
            NotConnected = -1,
            Standby = 0,
            Ready = 1,
            Calibrating = 2
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
            runMode = RunMode.NotConnected;

            Tg_rdy.isOn = false;
        }

        void Update()
        {
            if (runMode == RunMode.NotConnected || runMode == RunMode.Standby) return;

            if (Imu.flg_setdat)
            {
                if (leapHandModel.isDetected)
                {
                    Imu.Pmc.n0 = 99;
                    Imu.Pmc.p0 = hand.transform.localPosition;
                }
                imuHandModel.Draw(Imu.Pim);
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
                /* Imu.Connectメソッドの中でStatメソッドが呼ばれていてその結果が返るから
			     * このImu.Stat()がfalseを返すことはないのでは？ */
                if (Imu.Stat())
                {
                    max_sec = (max_sec < 60) ? 60 : max_sec;

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
                    runMode = RunMode.NotConnected;
                    Im_rdy.color = Color.magenta;
                    Debug.Log("Err... " + gameObject.name + " is not ready !!");
                }
            }
            Debug.Log("RunMode: " + runMode);
        }

        void OnBtn_Ready(bool flg)
        {
            if (runMode == RunMode.NotConnected) return;

            Imu.Ready(flg);
            statusPanel.FpsVolt(Imu.fps, Imu.vbt);
            runMode = flg ? RunMode.Ready : RunMode.Standby;
            Im_rdy.color = (leapHandModel.isDetected) ? Color.cyan : Color.green;
            handSetting.Active(flg);
        }

        public void Calib()
        {
            if (runMode == RunMode.NotConnected || 
                runMode == RunMode.Calibrating) 
                return;

            /* キャリブレーション用のSS_DATクラスのオブジェクトを用意 */
            SS_DAT calibrationData = new SS_DAT();
            long max_data = 30 * (long)Imu.fps;     //最大フレーム数
            long max_key = 3;                       //最大キーフレーム数
            calibrationData.Init(max_data, Imu.now_sens, max_key, Imu.stat, Imu.fps);

            /* キャリブレーション */
            runMode = RunMode.Calibrating;
            Im_rdy.color = Color.yellow;
            StartCoroutine(Calibration(calibrationData));
            runMode = RunMode.Ready;
        }

        private IEnumerator Calibration(SS_DAT data)
        {
            long hs = data.GetNowRec();                 //開始フレーム
            long dh = (long)(cal_sec * (int)Imu.fps);   //キャリブレーション用フレーム数
            long he = hs + dh;                          //終了フレーム
            /* dataに記録 */
            data.SetRecFlg(true);
            Imu.Rec(true);
            while (data.GetNowRec() < he) yield return (null);
            data.SetRecFlg(false);
            Imu.Rec(false);

            /* 記録したDatを元にキャリブレーション */
            imuHandModel.Calibrate(hs, dh, data.Pim);
            data.SetNowRec(he);
        }

        public void End()
        {
            Imu.Ready(false);
            Imu.Close();
        }
    }
}
