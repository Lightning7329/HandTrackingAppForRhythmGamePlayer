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
        [SerializeField] private RunMode _runMode = RunMode.NotConnected;
        [SerializeField] private CalibrationLevel _calibrationLevel = CalibrationLevel.DoingNothing;

        private SS_DAT data = new SS_DAT();
        private SS_IMU Imu = new SS_IMU();
        private SS_STAT statusPanel = new SS_STAT();
        private IMUHandModel imuHandModel;
        private HandSetting handSetting;
        private LeapHandModel leapHandModel;
        private InputField I_net;
        private Button B_net;
        private Toggle Tg_rdy;
        private Image Im_rdy;
        public RunMode runMode
        { 
            get => _runMode;
            private set => _runMode = value;
        }
        public CalibrationLevel calibrationLevel
        {
            get => _calibrationLevel;
            private set => _calibrationLevel = value;
        }

        public enum RunMode
        {
            NotConnected = -1,
            Standby = 0,
            Ready = 1,
            Calibrating = 2
        }

        public enum CalibrationLevel
        {
            DoingNothing = 0,
            Recording = 1,
            Calculating = 2
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
                data.Rec(Imu.Pim, Imu.Pmc);
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

        public void Ready() { OnBtn_Ready(true); }

        public void Calib()
        {
            if (runMode == RunMode.NotConnected || 
                calibrationLevel != CalibrationLevel.DoingNothing)
            {
                //yield break;
                return;
            }
            Debug.Log("Calibration Starts");    //Clear

            /* キャリブレーション用のSS_DATクラスのオブジェクトを用意 */
            SS_DAT calibrationData = new SS_DAT();
            long max_data = 30 * (long)Imu.fps;     //最大フレーム数
            long max_key = 3;                       //最大キーフレーム数
            calibrationData.Init(max_data, Imu.now_sens, max_key, Imu.stat, Imu.fps);

            /* キャリブレーション */
            runMode = RunMode.Calibrating;
            Im_rdy.color = Color.yellow;
            calibrationLevel = CalibrationLevel.Recording;
            //yield return Calibration(calibrationData);
            StartCoroutine(Calibration());
            runMode = RunMode.Ready;
            Debug.Log("Caribration Finished");
        }

        private IEnumerator Calibration()
        {
            Debug.Log("Coroutine Starts");
            long hs = data.GetNowRec();                 //開始フレーム
            long dh = (long)(cal_sec * (int)Imu.fps);   //キャリブレーション用フレーム数
            long he = hs + dh;                          //終了フレーム
            Debug.Log($"hs:{hs} hd:{dh} he:{he}");      //hs:0 hd:150 he:150
            /* dataに記録 */
            data.SetRecFlg(true);
            Imu.Rec(true);
            while (data.GetNowRec() < he) yield return null;
            data.SetRecFlg(false);
            Imu.Rec(false);

            /* 記録したDatを元にキャリブレーション */
            calibrationLevel = CalibrationLevel.Calculating;
            imuHandModel.Calibrate(hs, dh, data.Pim);
            data.SetNowRec(he);
            calibrationLevel = CalibrationLevel.DoingNothing;
            Debug.Log("Coroutine Finished");
        }

        public void End()
        {
            Imu.Ready(false);
            Imu.Close();
        }
    }
}
