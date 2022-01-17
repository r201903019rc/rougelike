using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using Photon.Pun;
using Photon.Realtime;


public class Online_Manager : MonoBehaviourPunCallbacks {
    public Room now_room;//ルーム
  public  List<player_obj> players_list = new List<player_obj>();//プレイヤー表示
    [System.NonSerialized]
    public bool room_now;//ルーム入室中フラグ
    [System.NonSerialized]
    public bool list_change; //キャラリストに変更があったフラグ
    public int room_menber_count;//ルーム内の全プレイヤーの数
    public int mine_number=1;//自分のルーム内の識別番号
    private int bef_players_count;//前フレームの全プレイヤーの数
    public bool room_owner;//自分がオーナーかどうか
    public int scene_changed;//シーン遷移済みの人たち
    public int player_obj_criate;//シーン内にいるプレイヤーOBJの数
    [System.NonSerialized]
    public bool offline;//オフラインモードか否か

    //カスタムプロパティ関係
    ExitGames.Client.Photon.Hashtable properties;
    public string On_Hash_Battle_Turn = "now_battle_turn";
    public string On_Hash_Sorted_Chata = "sorted_chata";


    public RPC_Sender RPC_view;//全体として他プレイヤへの同期を行うためのRPCを管理

    public class player_obj {
        public int player_number;
        public string player_name;
        public player_obj(int num,string name) {
            player_number = num;
            player_name = name;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
   void Update() {
        room_now = PhotonNetwork.InRoom;
        try {
            //ルーム入室中
            if (room_now) {
                //ルーム内の人数に変化があるとき
                if (bef_players_count != now_room.PlayerCount) {
                    //キャラのリストを更新
                    chara_list_Update();
                    //変化フラグを立てる
                    list_change = true;
                }
            }
        }
        catch (System.NullReferenceException) { }
    }
    void LateUpdate()
    {
        try {
            //ルーム入室中
            if (room_now) {
                bef_players_count = now_room.PlayerCount;
                room_now = true;
            }
        }
        catch (System.NullReferenceException) { }
    }


    //ルームへの入室
    public void Room_conect(bool on) {
        //入室用コルーチン
        StartCoroutine(Conect_Coroutine(on));
    }
    //ルームへの入室処理
    IEnumerator Conect_Coroutine(bool online) {
        //二重に入室しないよう、一応部屋から退出する処理を挟む
        Room_Disconnect();
        //部屋に入っていなければ進行
        yield return new WaitUntil(()=> PhotonNetwork.IsConnected==false);
        //オンライン接続するならば接続を試行
        if (online == true) {
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.ConnectUsingSettings();
        }
        //しないならオフラインモードで起動
        else {
            PhotonNetwork.OfflineMode = true;
            room_now = true;
            room_owner = true;
        }
        offline = PhotonNetwork.OfflineMode;
    }
    //ルームからの退室
    public void Room_Disconnect() {
        //接続中であれば
        if (PhotonNetwork.IsConnected) {
            //部屋主なら部屋を閉じる処理も
            if (room_owner) {
                RPC_Destroy();
                now_room.IsVisible = false;
                Room_Close();
            }
            //番号を初期化
            mine_number = 1;
            //切断
            PhotonNetwork.Disconnect();
        }
    }

    //ルームへの入室を止める
    public void Room_Close() {
        now_room.IsOpen = false ;
    }

    //サーバーへの接続が完了したら呼ばれる
    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinRandomRoom();
    }
    //ルームへの入室が失敗したら呼ばれ、自動でルームを作成する
    public override void OnJoinRandomFailed(short returnCode, string message) {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // 4人まで入室可能
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        PhotonNetwork.CreateRoom((PhotonNetwork.CountOfRooms+1).ToString(), roomOptions); //第一引数はルーム名
    }
    //入室が成功したら呼ばれる
    public override void OnJoinedRoom() {
        //部屋を取得
        now_room = PhotonNetwork.CurrentRoom;
        //自分の番号を取得
        mine_number = now_room.PlayerCount;
        //キャラのリストの更新
        chara_list_Update();
        list_change = true;
        //部屋主ならRPC処理用のオブジェクトを各プレイヤーに作成
        if (GameMaster.Instance.online_Man.room_owner) {
           RPC_Criate();
        }
        //部屋主でなければそれを取得する
        else {
           RPC_Get();
        }
    }
    //キャラのリストの更新
    void chara_list_Update() {
        players_list.Clear();
        for (int i = 0; i < now_room.PlayerCount; i++) {
            players_list.Add(new player_obj(i+1, PhotonNetwork.PlayerList[i].NickName));
        }
        room_menber_count = now_room.PlayerCount;
        room_owner = PhotonNetwork.IsMasterClient;
    }

    //プレイヤーの作成
   public GameObject player_criate() {
        Transform tmp = GameMaster.Instance.chara_Man.Chara_Parent.transform;
        GameObject obj = PhotonNetwork.Instantiate("Player", tmp.position, tmp.rotation);
        obj.name += ":" + PhotonNetwork.NickName;
         obj.transform.parent = tmp;
        obj.GetComponent<Character>().chara_num = (GameObject.FindGameObjectsWithTag("Player").Length + GameObject.FindGameObjectsWithTag("Battle_Enemy").Length)-1;
        return obj;
    }
    //敵キャラの作成
    public GameObject BTchara_criate() {
        Transform tmp = GameMaster.Instance.chara_Man.Chara_Parent.transform;
        GameObject obj = PhotonNetwork.Instantiate("Battle_Enemy", tmp.position, tmp.rotation);
        obj.transform.parent = tmp;
        //現在シーン内にいるプレイヤーと敵の合計数から番号を割り振り
        Battle_Chara bt = obj.GetComponent<Battle_Chara>();
       bt.chara_num = (GameObject.FindGameObjectsWithTag("Player").Length + GameObject.FindGameObjectsWithTag("Battle_Enemy").Length)-1;
        bt.chara_name += (":"+bt.chara_num.ToString());
        return obj;
    }
    //キャラの破棄
    public void player_destroy(Character a) {
        //キャラが存在するとき
        if (a != null ){
            //そのキャラが自分に所有権があるならそれを削除
            if (player_mine(a)) {
                //キャラを全プレイヤーの画面で削除
                PhotonNetwork.Destroy(a.gameObject);
            }
        }
        return;
    }

    //RPC処理仲介オフジェクトの生成
  public  void RPC_Criate() {
        GameObject obj = PhotonNetwork.Instantiate("RPC_ProPerty", Vector3.zero, Quaternion.identity);
        RPC_view= obj.GetComponent<RPC_Sender>();
    }
    //RPC処理仲介オフジェクトの破棄
   public void RPC_Destroy() {
        if (RPC_view != null) {
            PhotonNetwork.Destroy(RPC_view.gameObject);
            RPC_view = null;
        }
    }
    //RPC処理仲介オフジェクトの取得
    public void RPC_Get() {
        StartCoroutine(RPC_Get_Co());
    }
    //RPC処理仲介オフジェクトの取得用コルーチン
    IEnumerator RPC_Get_Co() {
        //もしプレイヤーと敵の作成を終えたら
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("RPC_Property")!=null);
        RPC_view= GameObject.FindGameObjectWithTag("RPC_Property").GetComponent<RPC_Sender>();
    }
        //与えられたキャラが自分の操作しているものかを返す
        public bool player_mine(Character chara) {
        if (chara == null) { return false; }
      return (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Dangeon) | chara.GetComponent<PhotonView>().IsMine);
    }

    public void Name_Change() {
        //ルーム中での名前を最初に入力した名前に変更
        PhotonNetwork.NickName = GameMaster.Instance.player_chara_name;
    }
    //シーンのセット
    public void Set_Scene(Game_Now_Phase scene_set) {
        RPC_view.Send_Scene_Change_Sync(scene_set);
    }

    //キャラのソートが終わったかどうか
    public void Sort_End() {
       properties = now_room.CustomProperties;
        properties[On_Hash_Sorted_Chata] = ((properties[On_Hash_Sorted_Chata] is int value) ? value : 0) + 1; ;
        now_room.SetCustomProperties(properties);
    }
    //ターンが終了したかどうか
    public void Turn_Add() {
        properties = now_room.CustomProperties;
        int chan = ((properties[On_Hash_Battle_Turn] is int value) ? value : 0) + 1;
        chan = chan % Battle_Manager.Chara_Max;
        properties[On_Hash_Battle_Turn] = chan;
        now_room.SetCustomProperties(properties);
    }

    public void Scene_Start() {
        PhotonNetwork.IsMessageQueueRunning = true;
    }
    public void Scene_End() {
        PhotonNetwork.IsMessageQueueRunning = false;
    }

    // ルームプロパティが更新された時に呼ばれる
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        //戦闘シーンでの現在のターンを変化
        int turn_end_num = (now_room.CustomProperties[On_Hash_Battle_Turn] is int turn) ? turn : 0;
        //ターンが変化していたら
        if (GameMaster.Instance.battle_Man.now_turn_chara != turn_end_num) {
            GameMaster.Instance.battle_Man.Turn_Change(turn_end_num);
        }
        //戦闘シーンの開始時に、キャラを全プレイヤーが生成済みかどうか
        GameMaster.Instance.battle_Man.sorted = (now_room.CustomProperties[On_Hash_Sorted_Chata] is int sort) ? sort : 0;
    }

    //シーンのロード時に呼ばれる
    public void LoadScene(Game_Now_Phase scene_change_flag) {
        PhotonNetwork.LoadLevel(GameMaster.Instance.Scene_name[(int)scene_change_flag]);
            }
}
