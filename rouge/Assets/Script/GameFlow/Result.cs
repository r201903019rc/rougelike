using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using UnityEngine.UI;
using TMPro;

public class Result : MonoBehaviour
{
    public Canvas Title_Canvas;
    [Header("接続関係")]
    private Online_Manager on_man;
    public GameObject Player_obj_origin;//プレイヤー情報を示すオブジェクト
    public List<GameObject> player_obj_list = new List<GameObject>();//プレイヤー情報を占めるオブジェクトのリスト

    public List<Color> Ranking_colors = new List<Color>();

    // Start is called before the first frame update
    void Start()
    {
        //描画カメラをUI用カメラに指定
        Title_Canvas.worldCamera = GameMaster.Instance.ui_camera;

        on_man = GameMaster.Instance.online_Man;
        //BGMを再生
        GameMaster.Instance.music_Man.PlayBGM(GameMaster.Instance.music_Man.BGM_Title, GameMaster.Instance.music_Man.fade_speed);
        Room_Start();
    }

    // Update is called once per frame
    void Update()
    {
            //画面が暗ければタイトルに戻ってきたフラグを立てる
                if (GameMaster.Instance.now_fade_type == Fade_Type.Black) {
                    GameMaster.Instance.Fade_Out();
                }
        Room_Update();
    }
    


    //オンラインのルーム接続開始
    void Room_Start() {
        //キャラ情報を更新
        Room_UI_Update();
        //部屋からの接続を切る
        on_man.Room_Disconnect();
            }
    //オンラインのルーム接続中
    void Room_Update() {
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
            Next_scene();
        }
        else if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Cancel)) {
        }
    }
    //部屋内のUIを更新
   void Room_UI_Update() {
        //UIの破棄
            for (int i = 0; i < player_obj_list.Count; i++) {
                Destroy(player_obj_list[i]);
            }
            player_obj_list.Clear();
        //全UIを作成
            for (int i = 0; i <Battle_Manager.Chara_Max; i++) {
                player_obj_list.Add(Instantiate(Player_obj_origin, Player_obj_origin.transform.parent));
                player_obj_list[i].SetActive(true);
            //色
                player_obj_list[i].GetComponent<Image>().color = GameMaster.Instance.Players_Theme_Color[i];
            //順位
            int rank = GameMaster.Instance.battle_Man.ranking[i];
            TextMeshProUGUI rank_text = player_obj_list[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            rank_text.text = rank.ToString();
            rank_text.color = Ranking_colors[rank - 1];
            //位置
            player_obj_list[i].transform.localPosition = new Vector3(player_obj_list[i].transform.localPosition.x * (i / 2 == 0 ? 1 : -1), player_obj_list[i].transform.localPosition.y * (i % 2 == 0 ? 1 : -1), player_obj_list[i].transform.localPosition.z);
            //何Pなのか
            player_obj_list[i].transform.GetChild(1).GetComponent<Text>().text = (i+1).ToString()+ "P";
            //名前
            if (i<on_man.players_list.Count) {//プレイヤーが存在するなら
                player_obj_list[i].transform.GetChild(0).GetComponent<Text>().text = on_man.players_list[i].player_name;
            }
            else { //存在しないなら
                player_obj_list[i].transform.GetChild(0).GetComponent<Text>().text = "CPU "+(i+1).ToString();
            }
        }
    }
    //次のシーンへの移行
    public void Next_scene() {
        GameMaster.Instance.Scene_Change(Game_Now_Phase.Title);
    }
}
