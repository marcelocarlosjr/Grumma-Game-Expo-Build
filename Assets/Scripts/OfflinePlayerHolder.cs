using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

public class OfflinePlayerHolder : MonoBehaviour
{
    public string PublicIP;
    public string FloridaPolyIP;

    bool isUsed = false;
    public bool IsTeleporting = false;

    public static int PreviousScene = 0;
    public static int PlayerPrefab = 0;

    public bool IsServer = false;

    public static string PName = "Default Player";

    [Header("Player Current Stats")]
    public float Health;
    public float Stamina;
    public float EXP;
    public float EXPToLevel;
    public int Level;

    [Header("Player Item Mods")]
    public float MoveSpeedMod = 0;
    public float HealthMod = 0;
    public float DamageMod = 0;
    public float HealthRegenerationMod = 0;
    public float AttackSpeedMod = 0;
    public float EXPMod = 0;
    public float StaminaMod = 0;

    [Header("Player Level Upgrades")]
    public float MoveSpeedUpgrade = 0;
    public float HealthUpgrade = 0;
    public float DamageUpgrade = 0;
    public float HealthRegenerationUpgrade = 0;
    public float AttackSpeedUpgrade = 0;
    public float EXPModUpgrade = 0;
    public float StaminaUpgrade = 0;

    public int item1ID;
    public int item2ID;
    public int item3ID;
    public int item4ID;
    public int item5ID;


    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.OnSceneSwitched;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += this.OnSceneSwitched;
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string s in args)
        {
            switch (s)
            {
                case "SERVER1":
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = args[0];
                    proc.StartInfo.Arguments = "SERVER2";
                    proc.Start();

                    System.Diagnostics.Process proc2 = new System.Diagnostics.Process();
                    proc2.StartInfo.FileName = args[0];
                    proc2.StartInfo.Arguments = "SERVER3";
                    proc2.Start();

                    IsServer = true;
                    SceneManager.LoadScene(1);
                    break;
                case "SERVER2":
                    IsServer = true;
                    SceneManager.LoadScene(2);
                    break;
                case "SERVER3":
                    IsServer = true;
                    SceneManager.LoadScene(3);
                    break;
            }
        }
    }

    public void OnSceneSwitched(Scene s, LoadSceneMode l)
    {
        if (s.buildIndex == 0 && isUsed)
        {
            Destroy(this.gameObject);
        }
        else if (s.buildIndex > 0)
        {
            isUsed = true;
        }

        NetworkCore MyCore = GameObject.FindObjectOfType<NetworkCore>();
        if (MyCore == null)
        {
            throw new System.Exception("There is no network core on this scene! " + s.buildIndex);
        }
        switch (s.buildIndex)
        {
            case 1:
                MyCore.PortNumber = 9001;
                break;
            case 2:
                MyCore.PortNumber = 9002;
                break;
            case 3:
                MyCore.PortNumber = 9003;
                break;
        }

        if (IsServer)
        {
            MyCore.UI_StartServer();
        }
        else
        {
            //MyCore.IP = "127.0.0.1";
            StartCoroutine(SlowAgentStart());
        }
    }

    public IEnumerator SlowAgentStart()
    {
        bool UsePublic = false;
        bool UseFlorida = false;

        string IP = "127.0.0.1";

        //Ping Public Ip address to see if we are external..........
        GenericNetworkCore.Logger("Trying Public IP Address: " + PublicIP.ToString());
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        System.Net.NetworkInformation.PingOptions po = new System.Net.NetworkInformation.PingOptions();
        po.DontFragment = true;
        string data = "HELLLLOOOOO!";
        byte[] buffer = ASCIIEncoding.ASCII.GetBytes(data);
        int timeout = 500;
        System.Net.NetworkInformation.PingReply pr = ping.Send(PublicIP, timeout, buffer, po);
        yield return new WaitForSeconds(1.5f);
        if (pr.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            GenericNetworkCore.Logger("The public IP responded with a roundtrip time of: " + pr.RoundtripTime);
            UsePublic = true;
            IP = PublicIP;
        }
        else
        {
            GenericNetworkCore.Logger("The public IP failed to respond");
            UsePublic = false;
        }
        //-------------------If not public, ping Florida Poly for internal access.
        if (!UsePublic)
        {
            pr = ping.Send(FloridaPolyIP, timeout, buffer, po);
            yield return new WaitForSeconds(1.5f);
            if (pr.Status.ToString() == "Success")
            {
                UseFlorida = true;
                IP = FloridaPolyIP;
            }
            else
            {
                UseFlorida = false;
            }
        }
        //Otherwise use local host, assume testing.

        FindObjectOfType<NetworkCore>().IP = IP;
        FindObjectOfType<NetworkCore>().UI_StartClient();
    }

    public IEnumerator Teleport(int scene)
    {
        NetworkCore MyCore = GameObject.FindObjectOfType<NetworkCore>();
        if (!IsTeleporting)
        {
            IsTeleporting = true;
            PreviousScene = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(MyCore.Disconnect(MyCore.LocalConnectionID, true));
            yield return new WaitForSeconds(MyCore.Connections[0].latency * 2);
            SceneManager.LoadScene(scene);
        }
    }
}
