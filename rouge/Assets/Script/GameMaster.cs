using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Constraction;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameMaster : SingletonMonoBehaviour<GameMaster>
{
    //各コンポーネントへのアクセス用変数たち
    [System.NonSerialized]
    public Message_Manager message_Man;
    [System.NonSerialized]
    public Charactor_Manager chara_Man;
    [System.NonSerialized]
    public Stage_Manager stage_Man;
    [System.NonSerialized]
    public Turn_Manager turn_Man;
    [System.NonSerialized]
    public Item_Manager item_Man;
    [System.NonSerialized]
    public Input_Manager input_Man;
    [System.NonSerialized]
    public Dangeon_Manager dan_Man;
    [System.NonSerialized]
    public Battle_Manager battle_Man;
    [System.NonSerialized]
    public Online_Manager online_Man;
    [System.NonSerialized]
    public Music_Manager music_Man;
    [System.NonSerialized]
    public Effect_Manager effect_Man;
    //二つのカメラへのアクセス用変数
    public Camera stage_camera;//ステージの描画カメラ
    public Camera chara_camera;//キャラの描画カメラ
    public Camera ui_camera;//UIの描画カメラ
    public Vector3 stage_cam_def;
    public Vector3 chara_cam_def;

    public bool debug_mode_now=false;//現在デバッグモードか否か

    //UI関係
    public Canvas UI_Canvas;//UI全体の親
    public UI_Frane Player_Color_Frame;//操作しているプレイヤーが何Pなのかわかりやすいようにフレームに色を付けるための処理をするスクリプトへの参照
    public GameObject Debug_Text;//デバッグ画面用のテキスト

    //フェードイン、フェードアウト関係
    public Image Fade_panel_image;//画面を覆うimage
    [System.NonSerialized]
    public Fade_Type now_fade_type;//現在のフェードタイプ
    public float fade_speed = 0.1f;//画面の色が変わる速度

    //現在プレイヤーのターンならtrue,他が動いているときはfalse
    [System.NonSerialized]
    public Turn now_player_turn;

    //プレイヤーの名前
    public string player_chara_name;
    //各プレイヤーの色
    public List<Color> Players_Theme_Color = new List<Color>(4);

    //全シーンの名前
    [EnumIndex(typeof(Game_Now_Phase))]
    public string[] Scene_name = new string[(int)Game_Now_Phase.Non_Scene];
    //シーンを移行する時に使われる、次のシーンが何になるかのフラグ
    private Game_Now_Phase scene_change_flag;
 
    //このスクリプトがアタッチされているGame_Masterオブジェクトをゲーム開始時に生成
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad() {
        //オブジェクトの生成
        GameObject masterobj = (GameObject)Instantiate(Resources.Load("GameMaster"));
        //シーンをまたいでも消えないように
        DontDestroyOnLoad(masterobj);
    }

    // Start is called before the first frame update
    void Start()
    {
        Get_Conponents();
        OnOff_Conponents(Now_mode());
        now_fade_type = Fade_Type.White;
        input_Man.Can_Controll = true;
        player_chara_name = "";
        scene_change_flag = Game_Now_Phase.Non_Scene;
        debug_mode_now = false;
    }

    // Update is called once per frame
    void Update() {
        fade();
        //シーン遷移フラグが立っていれば遷移
        if (scene_change_flag != Game_Now_Phase.Non_Scene&&now_fade_type== Fade_Type.Black) {
            online_Man.Scene_End();
            chara_Man.Scene_Change();
            StartCoroutine(Scene_Start(scene_change_flag));
            //シーン遷移フラグをオフに
            scene_change_flag = Game_Now_Phase.Non_Scene;
        }
        //もしゲームのリセットコマンドが押されたら再起動
        if(input_Man.Get_button_Down(INputers.IN_GameReset)) {
            Scene_Change( Game_Now_Phase.Title);
        }
        //オフラインモードで画面が明るく、戦闘シーンもしくは探索シーンでデバッグボタンが押されたらデバッグウインドウ起動
        if(online_Man.offline && (Now_Mode_Equal( Game_Now_Phase.Battle)|| Now_Mode_Equal(Game_Now_Phase.Dangeon)) &&now_fade_type==Fade_Type.White) {
            if (input_Man.Get_button_Down(INputers.IN_Debug)) {
                DebugMode();
            }
        }
    }

    IEnumerator Scene_Start(Game_Now_Phase next_scene) {
        yield return null;
        //遷移
        switch (Now_mode()) {
            case Game_Now_Phase.Dangeon:
                dan_Man.Stage_END();
                break;
            case Game_Now_Phase.Battle:
                battle_Man.Stage_End();
                break;
        }
        online_Man.LoadScene(next_scene);
        yield return new WaitUntil(() => Now_Mode_Equal(next_scene) == true);
        //シーンごとにコンポーネントを変化
        OnOff_Conponents(next_scene);
        //オンライン処理側の開始処理
        online_Man.Scene_Start();
        yield return null;
        //遷移したシーンによっての初期行動
        switch (next_scene) {
            case Game_Now_Phase.Title:
                Game_Reset();
                break;
            case Game_Now_Phase.Dangeon:
                dan_Man.Stage_Start();
                break;
            case Game_Now_Phase.Battle:
                battle_Man.Stage_Start();
                break;
            case Game_Now_Phase.Result:
                stage_Man.Map_Reset();
                stage_Man.Mini_Reset();
                break;
        }
        
        yield break;
    }
    //各種コンポーネントの取得
    void Get_Conponents() {
        chara_Man = GetComponent<Charactor_Manager>();
        stage_Man = GetComponent<Stage_Manager>();
        turn_Man = GetComponent<Turn_Manager>();
        item_Man = GetComponent<Item_Manager>();
        input_Man = GetComponent<Input_Manager>();
        message_Man = GetComponent<Message_Manager>();
        dan_Man = GetComponent<Dangeon_Manager>();
        battle_Man = GetComponent<Battle_Manager>();
        online_Man = GetComponent<Online_Manager>();
        music_Man = GetComponent<Music_Manager>();
        effect_Man = GetComponent<Effect_Manager>();
    }
    //シーンごとに使うコンポーネントのみをオンに
    void OnOff_Conponents(Game_Now_Phase mode) {
        switch (mode) {
            case Game_Now_Phase.Title:
                chara_Man.enabled =false;
                stage_Man.enabled = false;
                turn_Man.enabled = false;
                item_Man.enabled = false;
                input_Man.enabled = true;
                message_Man.enabled = true;
                dan_Man.enabled = false;
                UI_Canvas.enabled = false;
                battle_Man.enabled = false;
                online_Man.enabled = true;
                effect_Man.enabled = false;
                break;
            case Game_Now_Phase.Dangeon:
                chara_Man.enabled = true;
                stage_Man.enabled = true;
                turn_Man.enabled = true;
                item_Man.enabled = true;
                input_Man.enabled = true;
                message_Man.enabled = true;
                dan_Man.enabled = true;
                UI_Canvas.enabled = true;
                battle_Man.enabled = false;
                online_Man.enabled = true;
                effect_Man.enabled = true;
                break;
            case Game_Now_Phase.Battle:
                chara_Man.enabled = true;
                stage_Man.enabled = true;
                turn_Man.enabled = true;
                item_Man.enabled = true;
                input_Man.enabled = true;
                message_Man.enabled = true;
                dan_Man.enabled = false;
                UI_Canvas.enabled = true;
                battle_Man.enabled = true;
                online_Man.enabled = true;
                effect_Man.enabled = true;
                break;
            case Game_Now_Phase.Result:
                chara_Man.enabled = false;
                stage_Man.enabled = false;
                turn_Man.enabled = false;
                item_Man.enabled = false;
                input_Man.enabled = true;
                message_Man.enabled = true;
                dan_Man.enabled = false;
                UI_Canvas.enabled = false;
                battle_Man.enabled = false;
                online_Man.enabled = true;
                effect_Man.enabled = false;
                break;
        }
    }

    //現在のシーンenumを返す
   public Game_Now_Phase Now_mode() {
       string now_scene_name= SceneManager.GetActiveScene().name;
        for(int i = 0; i < (int)Game_Now_Phase.Non_Scene; i++) {
            if (now_scene_name == Scene_name[i]) {
                return (Game_Now_Phase)i;
            }
        }
        return Game_Now_Phase.Non_Scene;
    }
    //現在のシーンと引数に与えられたシーンが一致しているかを返す
   public bool Now_Mode_Equal(Game_Now_Phase scene) {
        return scene == Now_mode();
    }

    //呼ばれたらシーン遷移の準備を始める
    public void Scene_Change(Game_Now_Phase next_scene) {
        input_Man.Can_Controll = false;
        if (item_Man.enabled) { item_Man.Item_Use_Window_Start(false); }
        Fade_IN();
        scene_change_flag = next_scene;
    }
    //フェードアウト開始
    public void Fade_Out() {
        fade_change(1);
        now_fade_type = Fade_Type.DEC_Now;
    }
    //フェードイン開始
    public void Fade_IN() {
        fade_change(0);
        now_fade_type = Fade_Type.INC_Now;
        now_player_turn = Turn.Non;
        input_Man.Can_Controll = false;
    }

    //現在のFade_Typeに合わせてパネルの透明度を変化させる
    void fade() {
        if (now_fade_type == Fade_Type.DEC_Now) {
            fade_change(Fade_panel_image.color.a - fade_speed*Time.deltaTime);
            if (Fade_panel_image.color.a <= 0) { input_Man.Can_Controll = true; now_fade_type = Fade_Type.White; }
        }
        else if (now_fade_type == Fade_Type.INC_Now) {
            fade_change(Fade_panel_image.color.a + fade_speed * Time.deltaTime);
            if (Fade_panel_image.color.a >= 1) { now_fade_type = Fade_Type.Black; }
        }
    }

    //フェード用パネルの透明度を引数のものに変える
    void fade_change(float alpha) {
        Fade_panel_image.color = new Color(Fade_panel_image.color.r, Fade_panel_image.color.g, Fade_panel_image.color.b, alpha);
    }

    //ゲームの再起動
    public void Game_Reset() {
        //ルームから退出
        if (online_Man.room_now) {
            online_Man.Room_Disconnect();
        }
        //各変数の初期化
        {
            Player_Color_Frame.Change_Color(Color.white);
            {
                chara_Man.Player_chara = null;
                chara_Man.Player_obj = null;
                chara_Man.chara_list.Clear();
                chara_Man.Enemy_obj_list.Clear();
                chara_Man.chara_tmp = null;
            }
            {
                stage_Man.Map_Reset();
                stage_Man.Mini_Reset();
            }
            {
                item_Man.on_stage_items.Clear();
            }
            {
                dan_Man.minute = 0;
                dan_Man.second = 0;
                dan_Man.stage_criate_flag = STAGE_CRIATE.CRIATE_WAIT;
                dan_Man.Now_Floor_num = 0;
                dan_Man.now_turn_count = 0;
            }
            {
                battle_Man.players.Clear();
                battle_Man.now_turn = 0;
                battle_Man.now_turn_chara = 0;
                battle_Man.second = 0;
                battle_Man.stage_criate_flag = STAGE_CRIATE.CRIATE_WAIT;
                battle_Man.battle_start_flag = 0;
                battle_Man.action_end = 0;
                battle_Man.ranking = new int[Battle_Manager.Chara_Max];
            }
            {
                online_Man.players_list.Clear();
                online_Man.room_owner = false;
                online_Man.RPC_Destroy();
            }
        }

    }
    //デバッグモードのオンオフ
    void DebugMode() {
        debug_mode_now =!debug_mode_now;
        Debug_Text.SetActive(debug_mode_now);
    }
}

//定数定義系
namespace Constraction {
    //XY方向を示す
    public enum XY {
        Y, X, Not
    }

    //床のタイプ
    public enum Tile_Type {
        //壊れない壁
        Unbreakable_Wall,
        //壊れる(普通の)壁
        Wall,
        //廊下の床
        Hall_Floor,
        //部屋の床
        Room_Floor,
        //階段
        Next_Stage,
        //デバッグ用
        Debbug
    }

    //状態異常
    public enum Buf_Debuf {
        sleep = 1,
        conf = 2,
    }
    //ターンの初めに選択した行動
    public enum Turn_Action {
        Non, Move, Attack, Skip, Dash, Item_Drop, Item_Use, Item_Throw
    }
    //移動しようとしたときにそのタイルに何がいるかを返す
    public enum Can_Move_Tile {
        Null, Can_Move, Wall, Chara
    }

    //メッセージの内部定義
    public enum Message_System {
        ID, Text
    }

    public enum Message_Type {
        //キャラ死亡系
        Kill,
        Death,
        //アイテム系
        Get_Item,
        Cant_Get_Item,
        Use_Item,
        Throw_Item,
        //効果系
        Lv_Up,
        No_Effect,
        Damage_Effect,
        AID_Effect,
        MAX_Power_Up,
        Power_Down,
        HP_Up,
        Sleep_Start,
        Sleep_Now,
        Warp,
        //システム系
        Connect_Now,
        Connect_Success,
        Name_Entry,
        Room_Host,
        Room_Guest,
        Item_Switch,
        MAX
    }

    //アイテムの種類
    public enum Item_Type {
        Sword, Grass,Scroll, MAX
    }

    //左右
    public enum LR {
        Right, Left
    }

    //アイテムの効果一覧
    public enum Item_Use_Type {
        Attack_Damage,//攻撃力を加味したダメージ(引数:攻撃力)
        Const_Damage,//固定ダメージ(引数:ダメージ量)
        Ratio_Aid,//割合回復(引数:0割~10割)
        Const_Aid,//固定回復(引数:回復量)
        Warp,//ワープ
        Sleep,//睡眠(数ターン操作不能)
        Power_Down,//力変更
        Max_Power,//最大力上昇
        HP_Up,//最大HP変更
        Level,//レベル変更
        Full_Map,//ミニマップを全て公開
        Enemy_Map,//敵の位置をミニマップに描画
        Non//効果なし
    }

    //アイテムが効果を与える相手
    public enum Item_Effect_Target {
    Mine,Front_enemy,Straight_enemy,non
    }

    //入力判定フラグ
    public enum INputers { 
        IN_Move, IN_Move_nonWASD, IN_Attack,
        IN_Around, IN_Dash,
        IN_L, IN_R, IN_Item, 
        IN_Skip, IN_Decision,
        IN_Cancel,IN_GameReset,
        IN_Debug,
        MAX }

    //定数ダメージか、計算ダメージか
    public enum Damage_Type {
        Attack,Const
    }
    //フェードインアウトの状態
    public enum Fade_Type {
        Black,DEC_Now,INC_Now,White
    }

    public enum Game_Now_Phase {
        Title,Dangeon,Battle,Result,Non_Scene
    }
    public enum Turn {
        Player,Enemy,Non
    }
    public enum STAGE_CRIATE {
        CRIATE_WAIT,CRIATE_NOW,CRIATE_END
    }
    //アイテムを使うべき対象
    public enum ITEM_MERIT_TARGET {
        MINE,ANY
    }
    //アイテムが出現するシーン
    public enum ITEM_SPAWN_POINT {
    DANGEON,BATTLE,BOTH
    }
}
public class Abnormal {
    public Buf_Debuf type;//状態異常の種類
    public int re_turn;//残りターン数

    public Abnormal(Buf_Debuf ab,int turn){
        type = ab;
        re_turn = turn;
     }
}