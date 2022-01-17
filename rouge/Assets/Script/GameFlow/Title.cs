using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using UnityEngine.UI;
using TMPro;

public class Title : MonoBehaviour
{
    private title_mode now_mode;

    public Canvas title_canvas;
    [EnumIndex(typeof(title_mode))]
    public GameObject[] mode_obj_parents=new GameObject[(int)title_mode.MAX];//各画面で表示するUIの親
    //入力
    private float input_x;
    private float input_y;

    [Header("メニュー選択")]
    public Button name_entry_button;//ネームエントリー画面へのボタン
    public Button option_button;//オプション画面へのボタン
    private MenuSelect menu_select;//メニューの選択状況
    enum MenuSelect {NameEntry,Option, Non };

    [Header("オプション画面")]
    private OptionSelect op_select;
    public Button BGM_button;
    public Button SE_button;
    public Text BGM_text;
    public Text SE_text;
    public float def = 0.05f;
   public enum OptionSelect { BGM_vo, SE_vo,Non};

    [Header("ネームエントリー")]
    public InputField name_field;
    public Button ON_connect_button;
    public Button OFF_connect_button;
    public Text name_entry_text;
    private NameEntry name_select;//現在選択中のボタン
    enum NameEntry {Name,Online,Offline,Non };

    [Header("待機部屋")]
    private Online_Manager on_man;
    //入室前
    private bool on_line_off;//オンラインで実行するか否か
    public Text room_conect_text;//接続状況を示すテキスト
    //入室後
    private bool online_bef;//前フレームでオンラインであったかどうか
    public GameObject players_obj_origin;//プレイヤー情報を示すオブジェクト
    private List<GameObject> player_obj_list = new List<GameObject>();//プレイヤー情報を占めるオブジェクトのリスト
    private bool scene_chang_flag;//シーン変化フラグ
    public Text room_owner_text;//部屋主かどうかを示すテキスト

    //選択項目関係
    private Room_Entry room_select;//現在選択中の項目
    public Image start_Image;//ゲーム開始ボタンの親画像
    public Button start_button;//ゲーム開始ボタン
    public Image time_limit_image;//時間制限変更ボタンの親画像
    public TextMeshProUGUI time_limit_text;//時間制限の時間
    public int time_limit_def=30;//1押しで変化する秒数
    public Image item_image;//アイテム入手ボタンの親画像
    public Toggle item_toggle;//アイテム入手ボタン
    public Button time_plus_button;//時間追加ボタン
    public Button time_minus_button;//時間減少ボタン
    public Text item_text;//アイテム入手説明テキスト


    enum Room_Entry {Player, GameStart,Dangeon_Time,Item_Switch, Max }
    // Start is called before the first frame update
    void Start()
    {
        //描画カメラをUI用カメラに指定
        title_canvas.worldCamera = GameMaster.Instance.ui_camera;
        scene_chang_flag = false;
       
        now_mode = title_mode.title;
        Parents_setActive();
        on_man = GameMaster.Instance.online_Man;
        player_obj_list = new List<GameObject>();
        players_obj_origin.SetActive(false);
        //BGMを再生
        GameMaster.Instance.music_Man.PlayBGM(GameMaster.Instance.music_Man.BGM_Title, GameMaster.Instance.music_Man.fade_speed);
        //テキストの初期化
        name_entry_text.text= GameMaster.Instance.message_Man.Message_String(Message_Type.Name_Entry)[0];
        time_limit_text.text = GameMaster.Instance.dan_Man.Time_Limit_second.ToString();
        item_text.text= GameMaster.Instance.message_Man.Message_String(Message_Type.Item_Switch)[0];
    }

    // Update is called once per frame
    void Update() {
         input_x = GameMaster.Instance.input_Man.Get_Move_Down(true).x;
         input_y = GameMaster.Instance.input_Man.Get_Move_Down(true).y;
        //シーン遷移フラグが立っていなれば通常の処理
        if (scene_chang_flag == false) {
            switch (now_mode) {
                case title_mode.title:
                    title_Update();
                    break;
                case title_mode.select_mode:
                    select_Update();
                    break;
                case title_mode.Option:
                    Option_Update();
                    break;
                case title_mode.name_entry:
                    name_Update();
                    break;
                case title_mode.Room_Wait:
                    Room_Update();
                    break;
            }
            //画面が暗ければタイトルに戻ってきたフラグを立てる
            if (GameMaster.Instance.now_fade_type == Fade_Type.Black) {
                now_mode = title_mode.title;
                GameMaster.Instance.Fade_Out();
            }
        }
    }

    void Parents_setActive() {
        for(int i=0;i<(int)title_mode.MAX;i++) {
            if (i == (int)now_mode) { mode_obj_parents[i].SetActive(true); }
            else { mode_obj_parents[i].SetActive(false); }
        }
    }
    void Title_Start() {
        Mode_Select(title_mode.title);
    }
    void title_Update() {
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
            Select_Start();
        }
    }

    //メニューセレクトの開始
   public void Select_Start() {
        Mode_Select(title_mode.select_mode);
        entry_select(MenuSelect.NameEntry,false);
    }
    //メニューセレクト時のUpdate
    void select_Update() {
        //決定キーの動作
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
            //今選択中の内容についてそれぞれ動く
            switch (menu_select) {
                case MenuSelect.NameEntry:
                    name_start();
                    break;
                case MenuSelect.Option:
                    Option_Start();
                    break;
            }
        }
        else  if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Cancel)) {
            Title_Start();
        }
        //各項目の選択
        switch (menu_select) {
            case MenuSelect.NameEntry:
                if (GameMaster.Instance.input_Man.Get_Move_Down(false).y != 0) {
                    entry_select(MenuSelect.Option,true);
                }
                break;
            case MenuSelect.Option:
                if (GameMaster.Instance.input_Man.Get_Move_Down(false).y != 0) {
                    entry_select(MenuSelect.NameEntry,true);
                }
                break;
        }
    }
    //与えられたメニューを選択
    void entry_select(MenuSelect s,bool se) {
        if (se) {
            GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Cursor);
        }
        switch (s) {
            case MenuSelect.NameEntry:
                menu_select = MenuSelect.NameEntry;
                name_entry_button.Select();
                break;
            case MenuSelect.Option:
                menu_select = MenuSelect.Option;
                option_button.Select();
                break;
        }
    }

    //オプション画面の開始
    public void Option_Start() {
        Mode_Select(title_mode.Option);
        op_change(OptionSelect.Non,false);
        Volume_Text();
    }
    //オプション画面中
    void Option_Update() {
        //メニュー画面に戻る
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Cancel)) {
            Select_Start();
        }
       //音量の変更
        if (GameMaster.Instance.input_Man.Get_Move_Down(true).x != 0) {
            Volume_Change(GameMaster.Instance.input_Man.Get_Move_Down(true).x > 0);
        } 
        //各項目の処理
        switch (op_select) {
            case OptionSelect.BGM_vo:
                //上下ボタンで項目を切り替え
                if (GameMaster.Instance.input_Man.Get_Move_Down(true).y != 0) {
                    op_change(OptionSelect.SE_vo, true);
                }
                break;
            case OptionSelect.SE_vo:
                //上下ボタンで項目を切り替え
                if (GameMaster.Instance.input_Man.Get_Move_Down(true).y != 0) {
                    op_change(OptionSelect.BGM_vo, true);
                }
                break;
            case OptionSelect.Non:
                //項目が選択されていなければ自動的にBGMへ
                    op_change(OptionSelect.BGM_vo, true);
                break;
        }

    }
    //オプション画面での項目変更
    void op_change(OptionSelect sel, bool se) {
        if (se) { GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Cursor); }
        //選択を変更
        op_select = sel;
        //各項目選択時には色を濃く
        switch (op_select) {
            case OptionSelect.BGM_vo:
                BGM_button.Select();
                break;
            case OptionSelect.SE_vo:
                SE_button.Select();
                break;
        }
    }
    //オプション画面での項目変更
  public  void op_change(int sel) {
        op_select = (OptionSelect)sel;
        op_change(op_select, false);
    }

    //上げ下げどちらなのかを引数にして音量を変更する
    public void Volume_Change(bool up) {
        //正負を計算
        float d = def * (up ? 1 : -1);
        //音量の変更
        switch (op_select) {
            case OptionSelect.BGM_vo:
                GameMaster.Instance.music_Man.ChangeVolume(GameMaster.Instance.music_Man.AttachBGMSource.volume + d, GameMaster.Instance.music_Man.AttachSESource.volume);
                break;
            case OptionSelect.SE_vo:
                GameMaster.Instance.music_Man.ChangeVolume(GameMaster.Instance.music_Man.AttachBGMSource.volume, GameMaster.Instance.music_Man.AttachSESource.volume + d);
                break;
        }
        //現在の音量を表示
        Volume_Text();
        //効果音を鳴らす
        GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Cursor);
    }
    //現在の音量を画面に反映
    void Volume_Text() {
        BGM_text.text = ((int)(GameMaster.Instance.music_Man.AttachBGMSource.volume * 100)).ToString();
        SE_text.text = ((int)(GameMaster.Instance.music_Man.AttachSESource.volume * 100)).ToString();
    }

    //名前入力開始
    public void name_start() {
        Mode_Select(title_mode.name_entry);
        if (GameMaster.Instance.player_chara_name != "") {
             name_field.text= GameMaster.Instance.player_chara_name;
            name_select = NameEntry.Offline;
        }
        else {
            name_select = NameEntry.Non;
        }
    }
    //名前入力中
    void name_Update() {
        //決定キー、キャンセルキーの動作
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
            //今選択中の内容についてそれぞれ動く
            switch (name_select) {
                case NameEntry.Name:
                    name_field.Select();
                    break;
                case NameEntry.Offline:
                    name_end(false);
                    break;
                case NameEntry.Online:
                    name_end(true);
                    break;
            }
        }
        else if(GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Cancel)) {
            //名前入力中は戻らないようにする(Xキーで戻ってしまうため)
            if (name_select != NameEntry.Name) {
                Select_Start();
            }
        }

        //方向キーで各ボタンやフィールドに移動
        switch (name_select) {
            case NameEntry.Name:
                //下方向に入力があればオンラインの選択へ(名前入力欄が選択されているときはWASDでの入力を無視する)
                if ((name_field.isFocused == false && (input_y < 0||input_x<0))
                    || (name_field.isFocused == true && (GameMaster.Instance.input_Man.Get_Move_Down(false).y < 0|| GameMaster.Instance.input_Man.Get_Move_Down(false).x < 0))) {
                    entry_select(NameEntry.Online, true);
                }
                break;
            case NameEntry.Offline:
                //名前入力欄が選択されていなければ
                if (name_field.isFocused == false) {
                    //各項目へ
                    if (input_y > 0) {
                        entry_select(NameEntry.Name, true);
                    }
                    else if (input_x != 0) {
                        entry_select(NameEntry.Online, true);
                    }
                }
                break;
            case NameEntry.Online:
                //名前入力欄が選択されていなければ
                if (name_field.isFocused == false) {
                    //各項目へ
                    if (input_y > 0) {
                        entry_select(NameEntry.Name, true);
                    }
                    else if (input_x != 0) {
                        entry_select(NameEntry.Offline, true);
                    }
                }
                break;
            case NameEntry.Non:
                entry_select(NameEntry.Name,false);
                break;
        }
        //各項目の選択
        void entry_select(NameEntry s,bool se) {
            if (se) { GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Cursor);}
            switch (s) {
                case NameEntry.Name:
                    name_select = NameEntry.Name;
                    name_field.Select();
                    break;
                case NameEntry.Online:
                    name_select = NameEntry.Online;
                    ON_connect_button.Select();
                    break;
                case NameEntry.Offline:
                    name_select = NameEntry.Offline;
                    OFF_connect_button.Select();
                    break;
            }
        }
    }
    //名前入力終了
    public void name_end(bool ONLine) {
        //名前が入力されていれば
        if (name_field.text != "") {
            on_man.Room_Disconnect();
            Mode_Select(title_mode.Room_Wait);
            on_line_off = ONLine;
            Name_Entry_End();
            Room_Start();
        }
    }
    //名前入力が終わったらそれを反映
    public void Name_Entry_End() {
        if (name_field.text != "") {
            GameMaster.Instance.player_chara_name = name_field.text;
        }
        else {
            GameMaster.Instance.player_chara_name = "";
        }
        //名前をオンライン上に適用
        on_man.Name_Change();
        name_select = NameEntry.Online;
        ON_connect_button.Select();
    }

    //オンラインのルーム接続開始
    void Room_Start() {
        room_conect_text.text = GameMaster.Instance.message_Man.Message_String(Message_Type.Connect_Now)[0] ;
        room_owner_text.text = "";
        online_bef = false;
        on_man.Room_conect(on_line_off);
        //各要素を非表示に
        start_Image.gameObject.SetActive(false);
        time_limit_image.gameObject.SetActive(false);
        item_image.gameObject.SetActive(false);
        //アイテムのToggleを同期
        item_toggle.isOn = GameMaster.Instance.dan_Man.item_switch;
    }
    //オンラインのルーム接続中
    void Room_Update() {
       //キャンセルボタンで退出
        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Cancel)) {
            Mode_Select(title_mode.name_entry);
            Room_End();
        }

        //接続中
        if (on_man.room_now == true) {
            //接続した瞬間に呼ばれる
            if (online_bef != on_man.room_now&&on_man.room_now==true) {
                room_conect_text.text =  GameMaster.Instance.message_Man.Message_String(Message_Type.Connect_Success, (on_line_off?"No."+on_man.now_room.Name+ " room " : on_man.now_room.Name))[0];
                //各要素を表示
                start_Image.gameObject.SetActive(true);
                time_limit_image.gameObject.SetActive(true);
                item_image.gameObject.SetActive(true);
                //部屋主なら
                if (on_man.room_owner) {
                    //アイテムスイッチをオフに
                    GameMaster.Instance.dan_Man.item_switch = false;
                    Item_Toggle_Mark();
                }
            }
            //部屋主であれば項目選択
            if (on_man.room_owner) {
                //項目の変更
                if (input_x < 0) { Select_UI_Change(room_select - 1); }
                else if (input_x > 0) { Select_UI_Change(room_select + 1); }

                //選択中の項目ごとに変化
                switch (room_select) {
                    case Room_Entry.Player:
                        break;
                    case Room_Entry.GameStart://ゲームスタート
                        //決定ボタンでゲームスタート
                        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
                                Next_scene();
                                Parents_setActive();
                        }
                        break;
                    case Room_Entry.Dangeon_Time://探検パートの制限時間調節
                        if (input_y < 0) { Time_Limit_Change(-time_limit_def); }
                        else if (input_y > 0) { Time_Limit_Change(time_limit_def); }
                        break;
                    case Room_Entry.Item_Switch:
                        //決定ボタンでアイテムの出現フラグを変更
                        if (GameMaster.Instance.input_Man.Get_button_Down(INputers.IN_Decision)) {
                            Item_Toggle();
                            Item_Toggle_Mark();
                        }
                        break;
                }
            }
            else {
                //時間に変更があればテキストを変更
               if(time_limit_text.text!= GameMaster.Instance.dan_Man.Time_Limit_second.ToString()) {
                    time_limit_text.text = GameMaster.Instance.dan_Man.Time_Limit_second.ToString();
                }
               //アイテムスイッチに変更があればチェックボックスの画像を変更
                if (item_toggle.isOn != GameMaster.Instance.dan_Man.item_switch) {
                    Item_Toggle_Mark();
                }
            }
        }

        //プレイヤーが新たに入退室したとき
        if (on_man.list_change) {
            Room_UI_Update();
        }
        //オンライン状況を更新
        online_bef = on_man.room_now;
    }
    //部屋内のUIを更新
   void Room_UI_Update() {
        //UIの破棄
            for (int i = 0; i < player_obj_list.Count; i++) {
                Destroy(player_obj_list[i].transform.parent.gameObject);
            }
            player_obj_list.Clear();
        //全UIを作成
            for (int i = 0; i < on_man.players_list.Count; i++) {
                GameObject a = Instantiate(players_obj_origin, players_obj_origin.transform.parent);
                a.SetActive(true);
                player_obj_list.Add(a.transform.GetChild(0).gameObject);
                player_obj_list[i].GetComponent<Image>().color = GameMaster.Instance.Players_Theme_Color[i];
                a.transform.localPosition = new Vector3(a.transform.localPosition.x * (i / 2 == 0 ? 1 : -1), a.transform.localPosition.y * (i % 2 == 0 ? 1 : -1),a.transform.localPosition.z);
                player_obj_list[i].transform.GetChild(0).GetComponent<Text>().text = on_man.players_list[i].player_name;
                player_obj_list[i].transform.GetChild(1).GetComponent<Text>().text = on_man.players_list[i].player_number.ToString() + "P";
            }
            //部屋主か否かで構成を変更
            if (on_man.room_owner) {
            //ボタンを押せるように
            item_toggle.interactable = true;
            time_plus_button.interactable = true;
            time_minus_button.interactable = true;
            //ルーム主用の表示
            room_owner_text.text = GameMaster.Instance.message_Man.Message_String(Message_Type.Room_Host)[0];
            }
            else {
            item_toggle.interactable = false;
            time_plus_button.interactable = false;
            time_minus_button.interactable = false;
            room_owner_text.text = GameMaster.Instance.message_Man.Message_String(Message_Type.Room_Guest)[0];
        }
        start_button.gameObject.SetActive(on_man.room_owner);
        //オンラインマネージャ側のフラグを変更
        on_man.list_change = false;
        //選択中のUI情報の更新
        Select_UI_Change(room_select);
        //ホストならアイテムスイッチと時間表示を同期
        if (on_man.room_owner&&on_man.room_now) {
            GameMaster.Instance.online_Man.RPC_view.Send_Item_Switch_Sync();
            GameMaster.Instance.online_Man.RPC_view.Send_Time_Limit_Sync();
        }
        //ゲストはアイテムスイッチが誤動作を起こさないように設定
        else {
            Item_Toggle_Mark();
        }
    }
    //選択中の項目を変更
    void Select_UI_Change(Room_Entry sel) {
        int mine_num = GameMaster.Instance.online_Man.mine_number-1;
        //上限下限の設定
        if (sel < 0) { sel = Room_Entry.Max - 1; }
        else if (sel >= Room_Entry.Max) { sel = 0; }
        //代入
        room_select = sel;
        //選択中の項目ごとに変化
        switch (room_select) {
            case Room_Entry.Player:
                if (player_obj_list.Count > 0) {
                    player_obj_list[mine_num].transform.parent.GetComponent<Image>().enabled = true;
                }
                time_limit_image.enabled = false;
                start_Image.enabled = false;
                item_image.enabled = false;
                break;
            case Room_Entry.GameStart:
                if (player_obj_list.Count > 0) {
                    player_obj_list[mine_num].transform.parent.GetComponent<Image>().enabled = false;
                }
                time_limit_image.enabled = false;
                start_Image.enabled = true;
                item_image.enabled = false;
                break;
            case Room_Entry.Dangeon_Time:
                if (player_obj_list.Count > 0) {
                    player_obj_list[mine_num].transform.parent.GetComponent<Image>().enabled = false;
                }
                time_limit_image.enabled = true;
                start_Image.enabled = false;
                item_image.enabled = false;
                break;
            case Room_Entry.Item_Switch:
                if (player_obj_list.Count > 0) {
                    player_obj_list[mine_num].transform.parent.GetComponent<Image>().enabled = false;
                }
                time_limit_image.enabled = false;
                start_Image.enabled = false;
                item_image.enabled = true;
                break;
        }
    }
    //制限時間を変更
  public void Time_Limit_Change(int def_time) {
        //時間を変更
        if (GameMaster.Instance.dan_Man.Time_Limit_second == 1) {
            GameMaster.Instance.dan_Man.Time_Limit_second = 0;
        }
        GameMaster.Instance.dan_Man.Time_Limit_second += (def_time);
        //範囲制限
        if (GameMaster.Instance.dan_Man.Time_Limit_second <= 1) { GameMaster.Instance.dan_Man.Time_Limit_second = 1; }
        else if (GameMaster.Instance.dan_Man.Time_Limit_second > GameMaster.Instance.dan_Man.Time_Limit_MAX) {
            GameMaster.Instance.dan_Man.Time_Limit_second = GameMaster.Instance.dan_Man.Time_Limit_MAX;
        }
        time_limit_text.text = GameMaster.Instance.dan_Man.Time_Limit_second.ToString();
        //他プレイヤーにも変更を送信
        GameMaster.Instance.online_Man.RPC_view.Send_Time_Limit_Sync();
    }
    //アイテム出現フラグを変更
    public void Item_Toggle() {
        GameMaster.Instance.dan_Man.item_switch = !GameMaster.Instance.dan_Man.item_switch;
        //オンラインに接続されていれば他プレイヤーの画面でも出現フラグを同期させる
        if (GameMaster.Instance.online_Man.RPC_view != null) {
            GameMaster.Instance.online_Man.RPC_view.Send_Item_Switch_Sync();
        }
    }
    //isOnのコールバックを起こさないようにチェックボックスを変更(https://baba-s.hatenablog.com/entry/2019/03/18/151500)
    void Item_Toggle_Mark() {
        Toggle.ToggleEvent onValueChanged = item_toggle.onValueChanged;
        item_toggle.onValueChanged = new Toggle.ToggleEvent();
        item_toggle.isOn = GameMaster.Instance.dan_Man.item_switch; ;
        item_toggle.onValueChanged = onValueChanged;
    }
    //オンラインのルームからの離脱
    void Room_End() {
        if (on_man.room_now) {
            on_man.Room_Disconnect();
        }
        online_bef = false;
        for (int i = 0; i < player_obj_list.Count; i++) {
            Destroy(player_obj_list[i].transform.parent.gameObject);
        }
        player_obj_list.Clear();
        room_owner_text.text = "";
    }

    void Mode_Select(title_mode mode) {
        GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Cancel);
        now_mode =mode;
        Parents_setActive();
    }

    //次のシーンへの移行
    public void Next_scene() {
        //決定ボタンが押されすでに名前が入力されており、自分が部屋主なら次のシーンへ
        if (GameMaster.Instance.player_chara_name != ""&&on_man.room_owner) {
            //新たに人が入ってこないように設定
            on_man.Room_Close();
            on_man.Set_Scene(Game_Now_Phase.Dangeon);
            scene_chang_flag = true;
        }
    }
    enum title_mode {
        title,select_mode,name_entry,Room_Wait,Option,MAX
    }
}
