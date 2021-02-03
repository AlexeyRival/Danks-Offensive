using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class customNetworkHUD : NetworkManager
{
    public GUIStyle style;
    public bool isServer;
    private bool connected;
    public List<player_tank> Players = new List<player_tank>();
    public GameObject spawnpoint,cam,ui,shop,shopbutton;
    public Texture2D progressbar, tankicon, bar, bossicon, superbossicon, questionicon,logo;
    public int[] items = new int[6];
    public int progress;
    private int money;
    public bool isElite;
    #region singleton
    public static customNetworkHUD Instanse { get; private set; }
    #endregion
    private void Awake()
    {
        singleton = this;
        Instanse = this;
    }
    private void Start()
    {
        try
        {
            StreamReader file = new StreamReader(Application.dataPath + "/save.dxo");
            nickname = file.ReadLine();
            address = file.ReadLine();
            money = int.Parse(file.ReadLine());
            for (int i = 0; i < items.Length; ++i) {
                items[i] = int.Parse(file.ReadLine());
            }
            progress = int.Parse(file.ReadLine());
            file.Close();
        }
        catch
        {
            nickname = "Player" + Random.Range(0, 29032002);
            money = 0;
        }
    }
    public void Buy(string itm) {
        byte item = byte.Parse(itm);
        switch (item)
        {
            case 0:
                if (money >= 4) {
                    ++items[0];
                    money -= 4;
                }
                break;
            case 1:
                if (money >= 5)
                {
                    ++items[1];
                    money -= 5;
                }
                break;
            case 2:
                if (money >= 5)
                {
                    ++items[2];
                    money -= 5;
                }
                break;
            case 3:
                if (money >= 7)
                {
                    ++items[3];
                    money -= 7;
                }
                break;
            case 4:
                if (money >= 60)
                {
                    ++items[4];
                    money -= 60;
                }
                break;
            case 5:
                if (money >= 40)
                {
                    ++items[5];
                    money -= 40;
                }
                break;
            case 6:
                if (!isElite) {
                    if (money >= 100) {
                        money -= 100;
                        isElite = true;
                    }
                }
                break;
        }
    }
    public void OpenShop()
    {
        shop.SetActive(true);
        shopbutton.SetActive(false);
    }
    public void CloseShop()
    {
        shop.SetActive(false);
        shopbutton.SetActive(true);
    }
    private void Save() {
        StreamWriter file = new StreamWriter(Application.dataPath + "/save.dxo");
        file.WriteLine(nickname);
        file.WriteLine(address);
        file.WriteLine(money);
        for (int i = 0; i < items.Length; ++i) {
            file.WriteLine(items[i]);
        }
        file.WriteLine(progress);
        file.Close();
    }
    public void SpawnAny(int index) {
        NetworkServer.Spawn(Instantiate(spawnPrefabs[index]));
    }
    public void Disconnect() {
        singleton.StopClient();
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        Players.Add(player.GetComponent<player_tank>());
    }
    private string address;
    public string nickname="Playyyer";
    private void OnGUI()
    {
        if (!connected)
        {
            if (!shop.active)
            {
                if (GUI.Button(new Rect(Screen.width * 0.5f - 125, Screen.height * 0.5f + 40, 125, 20), "Создать", style))
                {
                    Save();
                    isServer = true;
                    cam.SetActive(false);
                    singleton.networkPort = 7777;
                    singleton.StartHost();
                    connected = true;
                    ui.SetActive(true);
                    shopbutton.SetActive(false);
                }
                GUI.Box(new Rect(Screen.width * 0.5f - 125, Screen.height * 0.5f, 50, 20), "IP:", style);
                address = GUI.TextField(new Rect(Screen.width * 0.5f - 75, Screen.height * 0.5f, 200, 20), address, style);
                GUI.Box(new Rect(Screen.width * 0.5f - 125, Screen.height * 0.5f + 20, 50, 20), "ИМЯ:", style);
                nickname = GUI.TextField(new Rect(Screen.width * 0.5f - 75, Screen.height * 0.5f + 20, 200, 20), nickname, style);
                if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.5f + 40, 125, 20), "Подключиться", style))
                {
                    Save();
                    cam.SetActive(false);
                    singleton.networkAddress = address;
                    singleton.networkPort = 7777;
                    singleton.StartClient();
                    connected = true;
                    ui.SetActive(true);
                    shopbutton.SetActive(false);
                }

                if (GUI.Button(new Rect(Screen.width * 0.5f + 125, Screen.height * 0.5f + 40, 120, 20), "Брифинг", style))
                {
                    Save();
                    Application.LoadLevel(2);
                }
                if (GUI.Button(new Rect(0, Screen.height -20, 120, 20), "Выйти", style))
                {
                    Save();
                    Application.Quit();
                }
                GUI.DrawTexture(new Rect(Screen.width*0.5f-207, Screen.height * 0.1f-60, 414, 120), logo);
                GUI.DrawTexture(new Rect(Screen.width - 869, Screen.height * 0.2f, 669, 28), progressbar);
                GUI.DrawTexture(new Rect(Screen.width - 660, Screen.height * 0.2f - 20, 4, 50), bar);
                GUI.DrawTexture(new Rect(Screen.width - 655, Screen.height * 0.2f - 20, 32, 32), bossicon);
                GUI.DrawTexture(new Rect(Screen.width - 460, Screen.height * 0.2f - 20, 4, 50), bar);
                GUI.DrawTexture(new Rect(Screen.width - 455, Screen.height * 0.2f - 20, 32, 32), superbossicon);
                GUI.DrawTexture(new Rect(Screen.width - 260, Screen.height * 0.2f - 20, 4, 50), bar);
                GUI.DrawTexture(new Rect(Screen.width - 255, Screen.height * 0.2f - 20, 32, 32), questionicon);
                GUI.DrawTexture(new Rect(Screen.width - 860 + progress * 2, Screen.height * 0.2f - 32, 64, 64), tankicon);
            }
            GUI.Box(new Rect(0, 0, 200, 20), "Баланс: " + money, style);
        }
        if (isServer)
        {
       //     GUI.Box(new Rect(Screen.width - 250, 0, 250, 25), "Это сервер");
    //        GUI.Box(new Rect(Screen.width * 0.5f - 150, Screen.height * 0.1f - 10, 300, 20), "Сервер работает. Зайдите с клиента на localhost");
            for (int p = 0; p < Players.Count; p++)
            {
              //  GUI.Box(new Rect(Screen.width - 250, 25 + 25 * p, 250, 25), Players[p].nick + "");
            }
        }
    }
}
