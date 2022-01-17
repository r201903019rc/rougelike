using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Constraction;

public class Message_Manager : MonoBehaviour
{
    //テキスト表示位置の親
    public GameObject Text_parent;
    //元となるテキスト
    public Text text;
    //文字送りの速度
    public float message_speed=1.0f;
    //メッセージのキュー
    public Queue<string> Message_queue=new Queue<string>();
    // CSVの中身を入れるリスト
    List<string[]> csvDatas = new List<string[]>();
    //文中の変数置き換え用の文字列を指定するstring
    public string Replace_Text = "string";

    //各メッセージ
    [EnumIndex(typeof(Message_Type))]
    public string[] message_ID=new string[(int)Message_Type.MAX];
    // Start is called before the first frame update
    void Start()
    {
        Message_Reset();
        File_Read();
    }

    // Update is called once per frame
    void Update()
    {
        //メッセージが残っている場合
        if (Message_queue.Count > 0) {
                Massage_Show_Start();
        }
    }
    //メッセージを表示
    public void Massage_Show_Start() {
        text.text = text.text + (text.text==""?"":"\n") + Message_Get();
        Text_parent.GetComponent<ScrollRect>().verticalNormalizedPosition = -1;
    }
    //メッセージをキューに追加
   public void Message_Add(string text) {
        Message_queue.Enqueue(text);
    }
    //メッセージをキューから取り出す
    string Message_Get() {
        if (Message_queue.Count > 0) {
            return Message_queue.Dequeue();
        }
        return null;
    }
    //メッセージの初期化
    public void Message_Reset() {
        text.text = "";
        Message_queue.Clear();
    }

    void File_Read() {
    TextAsset csvFile = (TextAsset)Resources.Load("Text/System_Mes");
        StringReader reader = new StringReader(csvFile.text);

        // , で分割しつつ一行ずつ読み込み
        // リストに追加していく
        while (reader.Peek() != -1) // reader.Peaekが-1になるまで
        {
            string line = reader.ReadLine(); // 一行ずつ読み込み
            csvDatas.Add(line.Split(',')); // , 区切りでリストに追加
        }
    }

    //メッセージのタイプを引数に、与えられたタイプの文字列をメッセージウインドウに表示する
    //strに与えられた文字列は、テキスト内の<string0>,<string1>...に置き換えられる
    public void File_Message(Message_Type type,params string[] str) {
        //文字列を取得
        List<string> message = Message_String(type, str);
        //文字列を表示キューに代入
        for (int i = 0; i < message.Count; i++) {
            //対戦シーンで、他にプレイヤーがいるなら他のプレイヤーにもメッセージを送信
            if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)&&GameMaster.Instance.online_Man.room_menber_count>1) {
                GameMaster.Instance.online_Man.RPC_view.Send_Message_Sync(message[i]);
            }
            //それ以外のシーンなら自分のみ
            else{
                Message_Add(message[i]);
            }
       }
        return;
    }

    //メッセージのタイプを引数に、System_Mes内のテキストを返す
    //strに与えられた文字列は、テキスト内の<string0>,<string1>...に置き換えられる
    public List<string> Message_String(Message_Type type, params string[] str) {
        //テキストのリスト
        List<string> message = new List<string>();
        //与えられたIDのメッセージを探す
        int message_num = Message_search(type);
        //もしメッセージが存在しなければ存在しない旨を返す
        if (message_num == -1) {
            message.Add("ERROR!! -NoMessage-");
        }
        else {
            //csvのデータから文字列を取得
            string tmp = csvDatas[message_num][(int)Message_System.Text];
            //もし置き換え文字列が存在すれば置き換える
            if (str.Length > 0) {
                for (int i = 0; i < str.Length; i++) {
                    tmp = tmp.Replace(Replace_Text.Remove(Replace_Text.Length - 1) + i.ToString() + ">", str[i]);
                }
            }
            //分割文字列が存在すれば分割する
            message.AddRange(tmp.Split('\\'));
        }
        return message;
    }

    //与えられたIDのメッセージ番号を返す、存在しないIDなら-1を返す
    int Message_search(Message_Type type) {
        for (int i = 0; i < csvDatas.Count; i++) {
            if (csvDatas[i][(int)Message_System.ID] == message_ID[(int)type]) { return i; }
        }
        return -1;
    }
}
