using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using Photon.Pun;

public class RPC_Sender : MonoBehaviour, IPunObservable {
    [System.NonSerialized]
    public PhotonView ph_view;
    //値が書き換えられた時に送受信される
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    }
    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(gameObject);
        ph_view = GetComponent<PhotonView>();
    }

    //メッセージの送信
    public void Send_Message_Sync(string text) {
        ph_view.RPC(nameof(Receive_Message_Sync), RpcTarget.OthersBuffered, text);
    }

    //メッセージの受信
    [PunRPC]
    protected void Receive_Message_Sync(string text) {
        GameMaster.Instance.message_Man.Message_Add(text);
    }

    //戦闘開始準備が終わったことを送信
    public void Send_Battle_Start_Sync() {
        ph_view.RPC(nameof(Receive_Battle_Start_Sync), RpcTarget.All);
    }

    //戦闘開始準備が終わったことを受信
    [PunRPC]
    protected void Receive_Battle_Start_Sync() {
        GameMaster.Instance.battle_Man.battle_start_flag++;
    }

    //シ－ン遷移命令を送信
    public void Send_Scene_Change_Sync(Game_Now_Phase next_scene) {
        ph_view.RPC(nameof(Receive_Scene_Change_Sync), RpcTarget.All, (int)next_scene);
    }

    //シ－ン遷移命令を受信し、遷移開始
    [PunRPC]
    protected void Receive_Scene_Change_Sync(int next_scene) {
        GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Decision);
        GameMaster.Instance.Scene_Change((Game_Now_Phase)next_scene);
    }

    //順位変更命令を送信
    public void Send_Ranking_Sync(int rank_chara, int rank) {
      //  Debug.Log("send RANK:"+rank);
        ph_view.RPC(nameof(Receive_Ranking_Sync), RpcTarget.All, rank_chara, rank);
    }

    //順位変更命令を受信
    [PunRPC]
    protected void Receive_Ranking_Sync(int rank_chara, int rank) {
        //Debug.Log("recieve RANK:" + rank);
        GameMaster.Instance.battle_Man.ranking[rank_chara] = rank;
    }

    //時間制限の変化を送信
    public void Send_Time_Limit_Sync() {
        ph_view.RPC(nameof(Receive_Time_Limit_Sync), RpcTarget.All, GameMaster.Instance.dan_Man.Time_Limit_second);
    }

    //時間制限の変化を受信
    [PunRPC]
    protected void Receive_Time_Limit_Sync(int time) {
        GameMaster.Instance.dan_Man.Time_Limit_second = time;
    }

    //アイテムスイッチの変化を送信
    public void Send_Item_Switch_Sync() {
        ph_view.RPC(nameof(Receive_Item_Switch_Sync), RpcTarget.All, GameMaster.Instance.dan_Man.item_switch);
    }

    //アイテムスイッチの変化を受信
    [PunRPC]
    protected void Receive_Item_Switch_Sync(bool item_switch) {
        GameMaster.Instance.dan_Man.item_switch = item_switch;
    }

    //自分の所持アイテムを送信
    public void Send_Have_Item_Sync(int chara_num,int[] items) {
        if (0 <= chara_num && chara_num < Battle_Manager.Chara_Max) {
            ph_view.RPC(nameof(Receive_Have_Item_Sync), RpcTarget.All, chara_num, items);
        }
    }

    //所持アイテムを受信
    [PunRPC]
    protected void Receive_Have_Item_Sync(int chara_num, int[] items) {
            for (int i = 0; i < GameMaster.Instance.battle_Man.players[chara_num].have_item_Max; i++) {
                GameMaster.Instance.battle_Man.players[chara_num].have_Items[i] = (items[i] == -1 ? null : new Item(GameMaster.Instance.item_Man.Item_list[items[i]]));
            }
    }

    //任意の座標に任意のアイテムを出現させる命令を送信
    public void Send_Item_Drop_Sync(int[] pos, int item_ID) {
        ph_view.RPC(nameof(Receive_Item_Drop_Sync), RpcTarget.All, pos, item_ID);
    }

    //任意の座標に任意のアイテムを出現させる命令を受信し、出現させる
    [PunRPC]
    protected void Receive_Item_Drop_Sync(int[] pos, int item_ID) {
        Item_Manager item_man= GameMaster.Instance.item_Man;
        item_man.on_stage_items.Add(new Item_puton(item_man.Item_list[item_ID], pos));
        //描画の更新
        GameMaster.Instance.stage_Man.Item_view();
    }

    //任意の座標のアイテムを削除する命令を送信
    public void Send_Item_Remove_Sync(int[] pos) {
        ph_view.RPC(nameof(Receive_Item_Remove_Sync), RpcTarget.All, pos);
    }

    //任意の座標のアイテムを削除する命令を受信
    [PunRPC]
    protected void Receive_Item_Remove_Sync(int[] pos) {
        Item_Manager item_man = GameMaster.Instance.item_Man;
       int i= item_man.ONStage_Item_Pos(pos);
        //その座標にアイテムが存在するならそれを削除する
        if (i != -1) {
            item_man.on_stage_items.RemoveAt(i);
            //描画の更新
            GameMaster.Instance.stage_Man.Item_view();
        }
    }
}

