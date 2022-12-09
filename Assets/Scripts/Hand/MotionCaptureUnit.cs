using UnityEngine;
using UnityEngine.UI;
using SS_KinetrackIII;

namespace KW_Mocap
{
    public class MotionCaptureUnit : MonoBehaviour
    {
        [SerializeField] GameObject hand;
        public int max_sec = 1800;	// = 30 minutes

        private SS_DAT Dat = new SS_DAT();
        private SS_IMU Imu = new SS_IMU();
        private SS_STAT statusPanel = new SS_STAT();
        private HandSetting handSetting;
        private InputField I_net;
        private Button B_net;
        private Toggle Tg_rdy;
        private Image Im_rdy;
        private int run_mode = -1;	// -1 NRY / 0 STBY / 1 RDY / 2 REC,CAL


        void Start()
        {
            Tg_rdy = this.transform.Find("Tgl").GetComponent<Toggle>();
            Im_rdy = this.transform.Find("Tgl/Background/Checkmark").GetComponent<Image>();
            B_net = this.transform.Find("Btn").GetComponent<Button>();
            I_net = this.transform.Find("I_net").GetComponent<InputField>();
            B_net.onClick.AddListener(OnBtn_Connect);
            Tg_rdy.onValueChanged.AddListener(OnBtn_Ready);

            handSetting = hand.GetComponent<HandSetting>();
            statusPanel.Init(this.gameObject, hand.name);

            /* �Ō�Ɏg�p���ꂽIP�A�h���X����� */
            string last_ip = PlayerPrefs.GetString(gameObject.name + "last_ip");
            if (last_ip.Length > 10) I_net.text = last_ip;
            run_mode = -1;
            Tg_rdy.isOn = false;
        }

        void Update()
        {

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
                    long max_data = max_sec * (long)Imu.fps;    //�ő�t���[����
                    long max_key = max_sec / 30 + 2;            //�ő�L�[�t���[����
                    Dat.Init(max_data, Imu.now_sens, max_key, Imu.stat, Imu.fps);
                    handSetting.Init(Imu.now_sens, Imu.stat);
                    statusPanel.Stat(Imu.stat);
                    statusPanel.FpsVolt(Imu.fps, Imu.vbt);
                    statusPanel.Frame(string.Format("{0:.00}", max_sec));
                    run_mode = 0;
                    Im_rdy.color = (hand.GetComponent<LeapHandModel>().isDetected) ? Color.blue : Color.white;
                    PlayerPrefs.SetString((gameObject.name + "last_ip"), I_net.text);
                    B_net.GetComponentInChildren<Text>().text = "Close";
                }
                else
                {
                    Imu.Close();
                    run_mode = -1;
                    Im_rdy.color = Color.magenta;
                    Debug.Log("Err... " + gameObject.name + " is not ready !!");
                }
            }
        }

        void OnBtn_Ready(bool flg)
        {
            if (run_mode >= 0)
            {
                Imu.Ready(flg);
                statusPanel.FpsVolt(Imu.fps, Imu.vbt);
                run_mode = (flg) ? 1 : 0;
                Im_rdy.color = (hand.GetComponent<LeapHandModel>().isDetected) ? Color.cyan : Color.green;
                handSetting.Active(flg);
            }
        }
    }
}
