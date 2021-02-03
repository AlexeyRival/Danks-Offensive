using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class leavescreen : MonoBehaviour
{
    public Image pic;
    public Sprite[] result;
    public Text personal,collector,all;
    private int timer = 0;
    void Start()
    {
        int all = 0,money=0;
        string nickname="", address="",prog="";
        string[] items = new string[6];
        try
        {
            StreamReader file = new StreamReader(Application.dataPath + "/logs.dxo");
            collector.text = "+"+file.ReadLine();
            all = int.Parse(file.ReadLine());
            personal.text = "+" + (all - int.Parse(collector.text)).ToString();
            this.all.text = "+" + all.ToString();
            pic.sprite = result[byte.Parse(file.ReadLine())];
            try
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = file.ReadLine();
                }
            }
            catch { }
            file.Close();
        }
        catch { }
        try
        {
            StreamReader file = new StreamReader(Application.dataPath + "/save.dxo");
            nickname = file.ReadLine();
            address = file.ReadLine();
            money = int.Parse(file.ReadLine())+all;
            for (int i = 0; i < items.Length; ++i) {
                file.ReadLine();
            }
            prog = file.ReadLine();
            file.Close();
        }
        catch { }
        {
            StreamWriter file = new StreamWriter(Application.dataPath + "/save.dxo");
            file.WriteLine(nickname);
            file.WriteLine(address);
            file.WriteLine(money);
            for (int i = 0; i < items.Length; ++i)
            {
                file.WriteLine(items[i]);
            }
            file.WriteLine(int.Parse(prog)> int.Parse(collector.text)?prog:collector.text);
            file.Close();
        }

    }

    void Update()
    {
        timer++;
        if (Input.GetKeyDown(KeyCode.Escape)&&timer>200) {
            Application.LoadLevel(0);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
