using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Constraction;

public class Dangeon_Manager : MonoBehaviour {
    //参照用
    private Dangeon_Generator generator;//マップ生成スクリプト
    private Charactor_Manager chara_man;
    private Stage_Manager stage_man;

    public int now_turn_count = 0;//現在のターン

    [Header("ステージ全体関係")]
    public int Width_Max = 65;//横方向のマス数
    public int Height_Max = 42;//縦方向のマス数
    public Stage_Data stage_data;//マップの画像や名前などのデータ
    public int Now_Floor_num = 0;//現在の階数
    [System.NonSerialized]
    public STAGE_CRIATE stage_criate_flag;//ステージ作成フラグ
    [System.NonSerialized]
    public bool now_floor_next;//階段による遷移中かどうかフラグ
    public int start_enemy_num=5;//初期配置される敵の数

    [Header("キャラ関係")]
    public int Chara_Max=20;
    public int start_item_num = 8;//初期配置されるアイテムの数
    public int enemy_spown_turn = 10;//敵が湧くターンの周期
    public bool item_switch=false;//開始時にアイテムを持ったまま始めるかどうか

    [Header("時間関係関係")]
    public Text time_text;//残り時間表示用テキスト
    public Slider time_slider;//残り時間表示用スライダー
    public Image time_gauge;//残り時間のスライダーの画像
    public int Time_Limit_second=120; //制限時間
    public int Time_Limit_MAX=300; //制限時間の限界値
    [System.NonSerialized]
    public int minute = 0;//現在の分
    [System.NonSerialized]
    public float second = 0;//現在の秒
    private bool scene_end;//探索シーン終了フラグ

    // Start is called before the first frame update
    void Start()
    {
        stage_man = GameMaster.Instance.stage_Man;
        chara_man = GameMaster.Instance.chara_Man;
        scene_end = false;
    }

    // Update is called once per frame
    void Update()
    {
        //デバッグ用
        if (GameMaster.Instance.now_player_turn ==Turn.Player&& GameMaster.Instance.input_Man.Get_button_Down(GameMaster.Instance.input_Man.Input.actions["Debug_Stage_reset"])) {
            /*
            generator.Map_Organize();
                GameMaster.Instance.stage_Man.Map_view();
                GameMaster.Instance.stage_Man.Map_Update();
            */
        }
        //ステージ作成フラグが立っていればフェードアウト、フェードイン
        if (stage_criate_flag != STAGE_CRIATE.CRIATE_END) {
            if (stage_criate_flag == STAGE_CRIATE.CRIATE_WAIT) {
                //プレイヤーのターンでも敵のターンでもない用にフラグを書き換え
                GameMaster.Instance.now_player_turn = Turn.Non;
                //ステージ生成処理
                Stage_Criate_END();
                //フェードアウト開始
                GameMaster.Instance.Fade_Out();
            }
            else if (GameMaster.Instance.now_fade_type == Fade_Type.White && stage_criate_flag == STAGE_CRIATE.CRIATE_NOW) {
                //ステージ生成フラグをオフに
                stage_criate_flag = STAGE_CRIATE.CRIATE_END;
                //操作開始
                GameMaster.Instance.now_player_turn = Turn.Player;
                //階段による移行フラグをオフに
                GameMaster.Instance.dan_Man.now_floor_next = false;
            }
        }
        //時間の更新
        Time_UPdate();
        //ホストなら時間切れになったら次のシーンへ移行する命令を全プレイヤーに送信
        if (GameMaster.Instance.online_Man.room_owner) {
            if (Time_Limit_second < (minute * 60 + (int)second) && scene_end == false && GameMaster.Instance.turn_Man.turn_start == Turn_Manager.TURN_NOW.UPDATE && GameMaster.Instance.now_player_turn == Turn.Player) {
                //シーン遷移開始
                GameMaster.Instance.online_Man.Set_Scene(Game_Now_Phase.Battle);
                //終了フラグオン
                scene_end = true;
            }
        }
    }
    //シーン遷移後最初のフレームで呼ばれる処理
    public void Stage_Start() {
        stage_man.Stage_Reset(Height_Max, Width_Max);
        generator = new Dangeon_Generator(stage_man.stage, stage_man.Room_List);
        stage_criate_flag = STAGE_CRIATE.CRIATE_WAIT;
        scene_end = false;
        time_text.gameObject.SetActive(true);
        Time_View();
        time_gauge.color = GameMaster.Instance.Players_Theme_Color[GameMaster.Instance.online_Man.mine_number-1];
        time_slider.value = 1.0f;
        //BGMを再生
        GameMaster.Instance.music_Man.PlayBGM(GameMaster.Instance.music_Man.BGM_Dangeon, GameMaster.Instance.music_Man.fade_speed);
    }
    //シーン遷移前に最後に呼ばれる処理
    public void Stage_END() {
        time_text.gameObject.SetActive(false);
    }
    //ステージの再生産開始処理
    public void Stage_Criate_Start() {
        stage_criate_flag = STAGE_CRIATE.CRIATE_WAIT;
        GameMaster.Instance.Fade_IN();
    }
    //ステージの作成
    void Stage_Criate_END() {
          try {
            //ターン情報のリセット　
            GameMaster.Instance.turn_Man.Turn_Reset();
            //マップ生成
            generator.Map_Criate(Stage_Manager.Return_int2(Width_Max, Height_Max),
                Stage_Manager.Return_int2(stage_data.Block_X_num, stage_data.Block_Y_num),
                stage_data.Room_num);
            //キャラ生成
            chara_man.Parent_Criate();
            chara_man.Enemy_Reset();
            chara_man.Player_Reset();
            for (int i = 0; i < start_enemy_num; i++) { chara_man.Dan_Enemy_Add(); }
            //アイテム生成
            GameMaster.Instance.item_Man.Item_Stage_Reset();
            GameMaster.Instance.item_Man.Item_Stage_On(start_item_num);
            if (item_switch) {//もしアイテムを所持して開始するフラグが立っていればプレイヤーにランダムなアイテムを渡す
                chara_man.Player_chara.Random_Item_Get(true, Game_Now_Phase.Dangeon);
            }
            //マップの表示
            stage_man.enemy_view = false;
            stage_man.Map_view();
            stage_man.Map_Update();
            chara_man.Player_chara.Camera_pos_Update();
            //メッセージの初期化
            GameMaster.Instance.message_Man.Message_Reset();
            //階数の追加
            Now_Floor_num++;
            //プレイヤーの情報を表示
            chara_man.Status_UI_Update();
            //生成フラグを生成中に更新
            stage_criate_flag = STAGE_CRIATE.CRIATE_NOW;
            return;
        }
        catch(System.NullReferenceException) {
            return;
        }
    }

    //フロアの再生成処理
    public void Next_Floor() {
        //階段による移行フラグを立てる
        GameMaster.Instance.dan_Man.now_floor_next = true;
        //操作を一時停止
        GameMaster.Instance.now_player_turn = Turn.Non;
        //フェードアウト
        GameMaster.Instance.Fade_Out();
        //ステージの再生成
        GameMaster.Instance.dan_Man.Stage_Criate_Start();
    }

    //キャラが死んだときの処理
    public void Player_Death() {
        //新しいフロアへ
        GameMaster.Instance.dan_Man.Next_Floor();
        //レベルを1~2下げる
        chara_man.Player_chara.Level_Up(chara_man.Player_chara.level - Random.Range(1, 3),false,false);
        //HPを全快に
       chara_man.Player_chara.now_HP = chara_man.Player_chara.max_HP;
        //アニメーションを開始
        chara_man.Player_chara.Anim_Start(chara_man.Idle_state);
    }

    //時間の更新
    void Time_UPdate() {
        //デバッグモードがオフかつフェードインアウトをしていないとき、もしくは階段によるフェードインアウト中なら時間を更新する
        if (GameMaster.Instance.debug_mode_now==false&&(GameMaster.Instance.now_fade_type == Fade_Type.White|| now_floor_next==true)) {
            second += Time.deltaTime;
            //60秒で1分
            if (second >= 60f) {
                minute++;
                second = second - 60f;
            }
            //時間表示の更新
            Time_View();
        }
    }
    //時間表示の更新
    void Time_View() {
        //残り時間を計算
        int time_limit = ((int)(Time_Limit_second - ((float)minute * 60 + second)));
        //残り時間のテキスト更新
        if (((int)(Time_Limit_second - ((float)minute * 60 + second))) >= 0) {
            time_text.text = time_limit.ToString();
        }
        //残り時間のバーの更新
        time_slider.value = (1.0f - (((float)minute * 60 + second) / Time_Limit_second));
    }
}
