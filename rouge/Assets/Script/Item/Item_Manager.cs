using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using System.IO;
using UnityEngine.UI;

public class Item_Manager : MonoBehaviour {
    //参照用
    private Stage_Manager stage_man;
    private Charactor_Manager chara_man;
    //アイテムの一覧
    public List<Item> Item_list=new List<Item>();

    //床落ちアイテムのリスト
    [System.NonSerialized]
    public List<Item_puton> on_stage_items = new List<Item_puton>();

    //現在動いているキャラが使用中のアイテム番号、なにも使用中でないなら-1
    public int now_use_item_id;

    //投げ関係
    //投げ中のゲームオブジェクト
    private GameObject thorw_item_obj;
    //投げ中のオブジェクトの座標
    private int[] thorw_pos;
    //敵に当たったかどうか、当たっていればそのキャラ番号を、当たっていなければ-1
    private int hit_chara_aft;
    //何マス投げたかのカウント
    private int thorw_count;

    [Header("アイテムスロット描画関係")]
    public List<Image> itemslot_list=new List<Image>();//アイテムスロットのアイテム画像のリスト
    public List<GameObject> item_slot_parent=new List<GameObject>();//アイテムスロットのGameObjectリスト
    public Canvas item_Canvas; //アイテムスロットを描画するCanvas
    public int item_slot_MAX=9;//アイテムスロットの最大数

    [Header("アイテム使用ウインドウ関係")]
    //アイテム使用時のウインドウ関係
    public GameObject use_window_parent;//出現するウインドウの親
    private List<GameObject> use_windows = new List<GameObject>();//出現するウインドウのリスト
    private List<Vector3> use_windows_pos = new List<Vector3>();//出現するウインドウの位置
    private bool use_window_bool;//ウインドウが出現しているか否か
    public float use_window_y_def=30.0f;//出現するウインドウがプレイヤーの座標からどれだけy座標ずれているか
    public float speed;//ウインドウが展開する速度
    //説明文用のテキストの親
    public GameObject Explain_parent;

    //アイテムの構造
    public enum Item_Constraction {
        Type, Name, Sprite,Use_Type,Use_Num,Use_Count, Thorw_Type, Thorw_Num,Target_Type,Spawn_Type
    }
    // Start is called before the first frame update
    void Start()
    {
        chara_man = GameMaster.Instance.chara_Man;
        stage_man = GameMaster.Instance.stage_Man;
        //アイテムスロット関係のオブジェクトを取得
        item_slot_parent.Add(item_Canvas.transform.GetChild(0).gameObject);
        itemslot_list.Add(item_slot_parent[0].transform.GetChild(0).GetComponent<Image>());
        
        //全アイテムのリストを作成
        Item_list_Read();
        //使用中のアイテムIDの初期化
        now_use_item_id = -1;
        //当たった敵情報の初期化
        hit_chara_aft = -1;
        //アイテム使用ウインドウ
        for (int i = 0; i < use_window_parent.transform.childCount; i++) {
            use_windows.Add(use_window_parent.transform.GetChild(i).gameObject);
            use_windows_pos.Add(use_windows[i].transform.localPosition);
        }
        Item_Use_Window_Start(false);
    }

    // Update is called once per frame
    void Update()
    {
        //もしアイテ使用ウインドウを出すフラグが立っているなら
        if (use_window_bool) {
            int count = 0;
            //それぞれのアイテム使用ウインドウをひらく
            for (int i = 0; i < use_window_parent.transform.childCount; i++) {
                if(use_windows[i].transform.localPosition == use_windows_pos[i]) { count++; }
                //時間に合わせてLerpで移動
                use_windows[i].transform.localPosition = Vector3.Lerp(use_windows[i].transform.localPosition, use_windows_pos[i], Time.deltaTime * speed);
            }
            //開き終わったらフラグをオフに
            if(count== use_window_parent.transform.childCount) { use_window_bool = false; }
        }
    }

    //アイテムをファイルから読み込む
    void Item_list_Read() {
        TextAsset csvFile = (TextAsset)Resources.Load("Item/Item_list");
        StringReader reader = new StringReader(csvFile.text);
        int count =0;
        //先頭の行は説明なので読み飛ばす
        reader.ReadLine();
        //アイテムをリストに追加
        while (reader.Peek() != -1) {
            string[] item_one = reader.ReadLine().Split(',');
            Item_list.Add(new Item(
                count,
                item_one[(int)Item_Constraction.Type],
                item_one[(int)Item_Constraction.Name],
                item_one[(int)Item_Constraction.Sprite],
                item_one[(int)Item_Constraction.Use_Type],
                item_one[(int)Item_Constraction.Use_Num],
                item_one[(int)Item_Constraction.Use_Count],
                item_one[(int)Item_Constraction.Thorw_Type],
                item_one[(int)Item_Constraction.Thorw_Num],
                item_one[(int)Item_Constraction.Target_Type],
                item_one[(int)Item_Constraction.Spawn_Type]
                ));
            count++;
        }

    }

    //ステージ上のアイテムのリストの初期化
   public void Item_Stage_Reset() {
        on_stage_items.Clear();
    }
    //アイテムをステージ上に配置する
    public void Item_Stage_On(int num) {
        //出現するアイテムのリストを作成
        List<int> item_id_list = Scene_Spawn_Item_Id(GameMaster.Instance.Now_mode());
        //アイテムのリストが0より大きければ配置、なければ配置しない
        if (item_id_list.Count > 0) {
            for (int i = 0; i < num; i++) {
                //アイテムの落ちる座標をランダムに選び、そこに落ちることができなければその周辺に落下させる点を作成
                 int[] drop_pos=  Item_Drop_floor(GameMaster.Instance.stage_Man.Random_Room_Pos(true,true));
                //どこにも落下できなければスキップ
                if (drop_pos == null) { continue; }
                //出現させる
                Item_Drop(item_id_list[Random.Range(0, item_id_list.Count)], drop_pos);
            }
        }
    }

    //引数のシーンで出現可能なアイテムのIDのリストを返す
   public List<int> Scene_Spawn_Item_Id(Game_Now_Phase scene) {
        List<int> item_id_list = new List<int>();
        for (int i = 0; i < Item_list.Count; i++) {
            //出現可能ならリストイン
            if (Item_Spawn_Scene(Item_list[i].spawn_type, scene)) { item_id_list.Add(i); }
        }
        return item_id_list;
    }
    //引数のアイテム出現タイプが引数のシーンにおいて出現可能かどうかを返す
    bool Item_Spawn_Scene(ITEM_SPAWN_POINT sp_type, Game_Now_Phase now_scene) {
        //探索シーンの時
        if (now_scene == Game_Now_Phase.Dangeon) {
            if(sp_type== ITEM_SPAWN_POINT.DANGEON || sp_type == ITEM_SPAWN_POINT.BOTH) { 
                return true; 
            }
        }
        //戦闘シーンの時
        else if (now_scene == Game_Now_Phase.Battle) {
            if (sp_type == ITEM_SPAWN_POINT.BATTLE || sp_type == ITEM_SPAWN_POINT.BOTH) {
                return true;
            }
        }
        //該当しなければ出現しない
        return false;
    }

    //アイテムを落とせる引数の座標の周辺を返す
    int[] Item_Drop_floor(int[] pos) {
        int[] count = { 5, 1, 2, 3, 4, 6, 7, 8, 9 };
        for(int i = 0; i < 9;i++) {
            int[] next_pos = GameMaster.Instance.stage_Man.Dir_Next_Pos(pos, count[i]);
            if (Drop_ok(next_pos)) { return next_pos; }
        }
        return null;
    } 
    //アイテムが引数の地点に出現できるか否か
    bool Drop_ok(int[] pos) {
        //既にアイテムが存在する、もしくはそのマスが床でなければfalse
        if ((GameMaster.Instance.stage_Man.stage[pos[0], pos[1]].tile == Tile_Type.Room_Floor|| GameMaster.Instance.stage_Man.stage[pos[0], pos[1]].tile == Tile_Type.Hall_Floor)==false) { return false; }
        //その座標にアイテムが存在すればfalse,存在しなければtrueを返す
        return (ONStage_Item_Pos(pos)==-1?true: false);
    }
    //引数の地点にあるアイテムの床落ちアイテムリストの番号を返す、存在しなければ-1
   public int ONStage_Item_Pos(int[] pos) {
        for (int i = 0; i < on_stage_items.Count; i++) {
            if (GameMaster.Instance.stage_Man.Pos_Equal(pos, on_stage_items[i].item_pos)) { return i; }
        }
        return -1;
    }
    //引数で与えられたキャラが引数の床落ちアイテムを入手する
    public void Item_Get_Stage(int chara_num, int stage_item_num,bool message) {
        //指定されたアイテムが正常な値かつ、アイテムスロットに空きがあればアイテムを入手
        if (0<=stage_item_num&&stage_item_num < on_stage_items.Count) {
            //アイテムスロットに空きがあればアイテムを入手
            if (GameMaster.Instance.chara_Man.Charanum_To_Chara(chara_num).Null_Itemslot_num()>0) {
              //  Debug.LogError(stage_item_num + ":" + on_stage_items.Count);
                Item_Get(chara_num,on_stage_items[stage_item_num].item_one.item_id, message);
                Item_Slot_Update();
                if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)) {
                    GameMaster.Instance.online_Man.RPC_view.Send_Item_Remove_Sync(on_stage_items[stage_item_num].item_pos);
                }
                else {
                    on_stage_items.RemoveAt(stage_item_num);
                }
                //描画の更新
               stage_man.Item_view();
            }
            //空きがなければその旨を表示
            else {
                if (message&&GameMaster.Instance.chara_Man.Player_chara.chara_num==chara_num) {
                    GameMaster.Instance.message_Man.File_Message(Message_Type.Cant_Get_Item);
                }
            }
        }
    }

    //与えられたIDのアイテムをchara_numのキャラが入手する、messageがtrueなら入手メッセージを表示する
    public void Item_Get(int chara_num,int item_num,bool message) {
        if (0 <= item_num && item_num < Item_list.Count) {
            Character chara = chara_man.Charanum_To_Chara(chara_num);
            //キャラの入手アイテムに加える
            for (int i = 0; i < chara.have_Items.Count; i++) {
                if (chara.have_Items[i] == null) {
                    chara.have_Items[i] =new Item(Item_list[item_num]);
                    break;
                }
            }
            if (message) {
                GameMaster.Instance.message_Man.File_Message(Message_Type.Get_Item, chara.chara_name, Item_list[item_num].Item_name);
            }
        }
    }
    //アイテムをマップに表示する処理
   public void Item_Mapping() {
        for(int i = 0; i < on_stage_items.Count; i++) {
            //まだマップに表示されていなければ表示化する
            if (on_stage_items[i].mapping == false) {
                //現在いるマスの部屋番号を取得
                int room_num = stage_man.int2_to_Room(on_stage_items[i].item_pos);
                //もしそこが部屋かつプレイヤーと同じ部屋であるとき表示化
                if (room_num != -1) {
                    if(room_num== stage_man.int2_to_Room(chara_man.Player_chara.now_position)) {
                        on_stage_items[i].mapping = true;
                    }
                }
                //部屋でなければプレイヤーと座標がmapping_distance以下の時表示化
                else {
                    if(stage_man.Tile_Distance_8dir(chara_man.Player_chara.now_position,on_stage_items[i].item_pos)<=stage_man.mapping_distance) {
                        on_stage_items[i].mapping = true;
                    }
                }
            }
        }
    }
    //アイテムスロットを引数の数だけ作成する
    public void Item_slot_Criate() {
        Item_slot_Reset();
        for (int i = 1; i < chara_man.Player_chara.have_Items.Count; i++) {
            item_slot_parent.Add(Instantiate(item_slot_parent[0], item_Canvas.transform));
            item_slot_parent[i].name = item_slot_parent[0] + ":" + i;
            item_slot_parent[i].transform.position = item_slot_parent[0].transform.position + new Vector3(-1 * i, 0, 0);
            itemslot_list.Add(item_slot_parent[i].transform.GetChild(0).GetComponent<Image>());
        }
        Item_Slot_Update();
    }
    //アイテムスロットを初期化する、あくまでスロットの描画のみで手持ちのアイテム情報は残る
    void Item_slot_Reset() {
        for (int i = 1; i < item_slot_parent.Count; i++) {
            Destroy(item_slot_parent[i]);
        }
        item_slot_parent.RemoveRange(1, item_slot_parent.Count - 1);
        itemslot_list.RemoveRange(1, itemslot_list.Count - 1);
    }
    //アイテムスロットの描画を更新する処理
    public  void Item_Slot_Update() {
        for (int i = 0; i < itemslot_list.Count; i++) {
            if (chara_man.Player_chara.have_Items[i] == null) {
                itemslot_list[i].sprite = null;
                itemslot_list[i].transform.GetChild(0).gameObject.SetActive(false);
                itemslot_list[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            else {
                itemslot_list[i].transform.GetChild(0).gameObject.SetActive(true);
                itemslot_list[i].transform.GetChild(1).gameObject.SetActive(true);
                itemslot_list[i].sprite = chara_man.Player_chara.have_Items[i].Item_sprite;
                itemslot_list[i].transform.GetChild(1).GetComponent<Text>().text = chara_man.Player_chara.have_Items[i].use_count.ToString();
            }
        }
    }

    //アイテムを床に出現させる処理
   public bool Item_Drop(int Item_ID,int[] pos) {
        //指定された床の周辺に出現可能ならそこに出現させる
        int[] drop_point = Item_Drop_floor(pos);
        if (drop_point!=null) {
            //戦闘シーンでかつホストなら全プレイヤーに対して床落ちの命令を送信する
            if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)) {
                if (GameMaster.Instance.online_Man.room_owner) {
                    GameMaster.Instance.online_Man.RPC_view.Send_Item_Drop_Sync(drop_point, Item_ID);
                }
            }
            //その他のシーンならそのまま床落ちアイテムに追加
            else {
                on_stage_items.Add(new Item_puton(Item_list[Item_ID], drop_point));
                //描画更新
                GameMaster.Instance.stage_Man.Item_view();
            }
            return true;
        }
        return false;
    }

    //アイテムの使用開始
    public void Item_Use_Start(int Use_chara_ID,int Target_chara_ID,int Item_ID) {
        //アイテム使用メッセージ
        GameMaster.Instance.message_Man.File_Message(Message_Type.Use_Item, chara_man.Charanum_To_Chara(Use_chara_ID).chara_name, Item_list[Item_ID].Item_name);
        //使用ウインドウを消す
        Item_Use_Window_Start(false);
        //アイテムIDが正しいなら実行される
        if (0 <= Item_ID && Item_ID < Item_list.Count) {
            now_use_item_id = Item_ID;
            //効果を発動
            Item_list[now_use_item_id].Item_Effect(Turn_Action.Item_Use, Use_chara_ID,Target_chara_ID);
        }
    }
    //アイテム使用中に呼ばれる
    public bool Item_Use_Update(int Chara_ID) {
        //アイテムIDが正しければ実行される
        if (0 <= now_use_item_id && now_use_item_id < Item_list.Count) {
            Character use_chara = chara_man.Charanum_To_Chara(Chara_ID);
            switch (Item_list[now_use_item_id].use_type) {
                case Item_Use_Type.Attack_Damage://攻撃力を加味したダメージ
                    //使用モーションと敵の反応モーションが終わるまで待機
                    return (chara_man.Reaction_Update(use_chara,
                       chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(use_chara.now_position, use_chara.now_direction)),
                      chara_man.Damage_state));
                case Item_Use_Type.Const_Damage://固定ダメージ
                    //使用モーションと敵の反応モーションが終わるまで待機
                    return (chara_man.Reaction_Update(use_chara,
                       chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(use_chara.now_position, use_chara.now_direction)),
                        chara_man.Damage_state));
                case Item_Use_Type.Ratio_Aid://割合回復
                    return true;
                case Item_Use_Type.Const_Aid://固定量の回復 
                    return true;
                default:
                    return true;
            }
        }
        //正しくないときはそこで終了
        else {
            return true;
        }
      
    }
    //アイテムの使用が終了したときに呼ばれる
   public void Item_Use_End(int Chara_ID) {
        Character use_chara = chara_man.Charanum_To_Chara(Chara_ID);

        if (GameMaster.Instance.online_Man.player_mine(use_chara)) {
            Item item = use_chara.have_Items[use_chara.now_selected_item];
            //使用できる回数を1減らす
            item.use_count--;
            //使用できる回数が0になったら消す
            if (item.use_count <= 0) {
                //スロットから削除
                use_chara.have_Items[use_chara.now_selected_item] = null;
            }
            Item_Slot_Update();
        }
        now_use_item_id = -1;
    }
    //アイテムの投げ開始
    public void Item_Throw_Start(int Throw_chara_ID, int Item_ID) {
        //アイテムIDが正しければ実行される
        if (0 <= Item_ID && Item_ID < Item_list.Count) {
            now_use_item_id = Item_ID;
            Character pl = chara_man.Charanum_To_Chara(Throw_chara_ID);
            GameMaster.Instance.message_Man.File_Message(Message_Type.Throw_Item, pl.chara_name, Item_list[now_use_item_id].Item_name);
            //使用ウインドウを消す
            Item_Use_Window_Start(false);
            //投げ中のオブジェクトを生成
            thorw_item_obj = Instantiate((GameObject)Resources.Load("Thorw_Item"));
            //画像を変更
            thorw_item_obj.GetComponent<SpriteRenderer>().sprite = Item_list[Item_ID].Item_sprite;
            thorw_item_obj.transform.SetParent(pl.transform);
            //位置を変更
            thorw_pos = pl.now_position;
            thorw_item_obj.transform.position = GameMaster.Instance.stage_Man.Tile_To_World(thorw_pos);
            //投げたマス数のカウントを初期化
            thorw_count = 1;
            if (GameMaster.Instance.online_Man.player_mine(chara_man.Charanum_To_Chara(Throw_chara_ID))) {
                //スロットから削除
                pl.have_Items[pl.now_selected_item] = null;
                Item_Slot_Update();
            }
        }
    }
    //アイテムの投げ中に呼ばれる
    public bool Item_Throw_Update(int Throw_chara_ID) {
        //移動中のアイテムオブジェクトが存在しなければ終了させる
        if (thorw_item_obj == null) { return true; }
        Character throw_chara = chara_man.Charanum_To_Chara(Throw_chara_ID);
        //既に当たっているとき
        if (hit_chara_aft != -1) {
            //相手のアニメーションが終わるのを待機
                return (chara_man.Reaction_Update(
                   throw_chara,
                   chara_man.Charanum_To_Chara(hit_chara_aft),
                   chara_man.Damage_state));
        }
        //移動中のアイテムが次のマスに到達していたらマス目移動処理
        if (thorw_item_obj.transform.position == GameMaster.Instance.stage_Man.Tile_To_World(thorw_pos)) {
            //移動したマス数を増やす
            thorw_count++;
            //もし10マス以上移動していたらその場に落とす
            if (thorw_count >= 10) {
                Item_Throw_Drop(thorw_pos);
                return true;
            }
            int[] next_pos = GameMaster.Instance.stage_Man.Dir_Next_Pos(thorw_pos, throw_chara.now_direction);
            //まだ進めるならfalse、進めなければ処理してtrueを返す
            switch (chara_man.Can_Move_pos(next_pos)) {
                case Can_Move_Tile.Chara:
                    //当たったキャラのキャラ番号を取得
                    hit_chara_aft = chara_man.Dir_To_Arrow_Chara(throw_chara.now_position, throw_chara.now_direction);
                    //投げたアイテムのオブジェクトを消す
                        thorw_item_obj.GetComponent<SpriteRenderer>().enabled = false;
                    //当たった時の処理を動かす
                        Item_Throw_Hit(Throw_chara_ID, hit_chara_aft);
                    break;
                case Can_Move_Tile.Wall:
                    Item_Throw_Drop(thorw_pos);
                    return true;
                case Can_Move_Tile.Can_Move:
                    thorw_pos = next_pos;
                    break;
            }
        }
        //到達していなければ動かす処理
        else {
            thorw_item_obj.transform.position = Vector3.MoveTowards(
                 thorw_item_obj.transform.position,
                 GameMaster.Instance.stage_Man.Tile_To_World(thorw_pos)
                 , 1);
        }

        return false;
    }
    //投げたアイテムが敵に命中したとき
    void Item_Throw_Hit(int Use_chara_ID, int Target_chara_ID) {
        //効果を発動
        Item_list[now_use_item_id].Item_Effect(Turn_Action.Item_Throw, Use_chara_ID,Target_chara_ID);
    }
    //投げたアイテムが落下したとき
    void Item_Throw_Drop(int[] pos) {
        Item_Drop(now_use_item_id, pos);
    }
    //アイテム投げの処理終了
   public void Item_Throw_End() {
        Destroy(thorw_item_obj);
        thorw_pos = null;
        hit_chara_aft = -1;
        now_use_item_id = -1;
    }

    //アイテム使用ウインドウがオンオフ
    public void Item_Use_Window_Start(bool set) {
        //消すとき
        if (!set) {
            for (int i = 0; i < use_window_parent.transform.childCount; i++) {
                use_windows[i].transform.localPosition = Vector3.zero;
            }
            use_window_bool = false;
        }
        //付けるとき
        else {
            use_window_parent.GetComponent<RectTransform>().localPosition = chara_man.Player_chara.Screen_Pos()+Vector3.up* use_window_y_def;
            use_window_bool = true;
        }
        use_window_parent.SetActive(set);
    }

    //渡されたアイテムが使用時に自分に使うものであるかどうかを返す
   public Item_Effect_Target Item_use_self(Item item) {
        switch (item.Item_type) {
            case Item_Type.Sword:
                return Item_Effect_Target.Front_enemy;
            case Item_Type.Grass:
                return Item_Effect_Target.Mine;
            case Item_Type.Scroll:
                return Item_Effect_Target.Mine;
            default:
                return Item_Effect_Target.non;
        }
    }

    //アイテムの説明文を生成する
    string Item_effect_type(Item item) {
        //使用時の効果の相手から文面を変更
        string Self_or_Enemy = "";
        switch (Item_use_self(item)){
            case Item_Effect_Target.Mine:
                Self_or_Enemy = "自分";
                break;
            case Item_Effect_Target.Front_enemy:
                Self_or_Enemy = "目の前の敵";
                break;
            default:
                Self_or_Enemy = "";
                break;
        }
        string use_text = (":使用時\n  " + Self_or_Enemy +effect_type_text(item.use_type));
        string thorw_text= (":投げ時\n  当たった敵" + effect_type_text(item.thorw_type));
        return item.Item_name+"\n"+use_text + "\n" + thorw_text;

        //各アイテムのテキストを返す
        string effect_type_text(Item_Use_Type type) {
            switch (type) {
                case Item_Use_Type.Attack_Damage:
                    return "に"+item.use_effect_num + "の攻撃ダメージ";
                case Item_Use_Type.Const_Damage:
                    return "に" + item.use_effect_num + "の定数ダメージ";
                case Item_Use_Type.Ratio_Aid:
                    return "の最大HPの"+item.use_effect_num + "割を回復する";
                case Item_Use_Type.Const_Aid:
                    return "のHPを"+ item.use_effect_num + "回復する";
                case Item_Use_Type.Warp:
                    return "のをフロアのどこかにワープさせる";
                case Item_Use_Type.Sleep:
                    return "を" + item.use_effect_num + "ターンの間眠らせる";
                case Item_Use_Type.Power_Down:
                    return "の力を" + item.use_effect_num + "減らす";
                case Item_Use_Type.Max_Power:
                    return "の力の上限を" + item.use_effect_num + "上昇させる";
                case Item_Use_Type.HP_Up:
                    return "の体力を" + item.use_effect_num + "上昇させる";
                case Item_Use_Type.Level:
                    return "のレベルを" + item.use_effect_num + "上昇させる";
                case Item_Use_Type.Full_Map:
                    return "フロアの構造を明らかにする";
                case Item_Use_Type.Enemy_Map:
                    return "敵の位置を明らかにする";
                default:
                    return "になんの効果も与えない";
            }
        }
    }
    //アイテムの解説文を表示する
    public void Item_Explain_Window(bool set,Item item) {
        GameMaster.Instance.message_Man.Text_parent.SetActive(!set);
        if (set) {
            Explain_parent.SetActive(true);
            Explain_parent.transform.GetChild(0).GetComponent<Text>().text = Item_effect_type(item);
        }
        else {
            Explain_parent.SetActive(false);
        }
    }
}

//アイテム情報
public class Item {
    public int item_id;//ID
    public string Item_name;//名前
    public Item_Type Item_type;//アイテムの種類
    public Sprite Item_sprite;//画像
    public Item_Use_Type use_type;//使用効果
    public int use_effect_num;//使用効果用の数値
    public int use_count;//アイテムを使える回数
    public Item_Use_Type thorw_type;//使用効果
    public int thorw_effect_num;//使用効果用の数値
    public ITEM_MERIT_TARGET target_type;//NPC用の仕様対象識別子
    public ITEM_SPAWN_POINT spawn_type;//どのシーンで出現するアイテムか

    public Item(int id, string it_type,string it_name,string it_sprite,string it_use_type,string it_use_num,string it_use_count,string it_thorw_type, string it_thorw_num,string it_target_type,string it_spawn_type) {
        item_id=id;
        Item_name = it_name;
        Item_type = (Item_Type)System.Enum.Parse(typeof(Item_Type), it_type);
        Item_sprite = Resources.Load<Sprite>("Item/Sprite/"+ it_sprite);
        use_type = (Item_Use_Type)System.Enum.Parse(typeof(Item_Use_Type), it_use_type);
        use_effect_num = System.Convert.ToInt32(it_use_num);
        use_count = System.Convert.ToInt32(it_use_count);
        thorw_type= (Item_Use_Type)System.Enum.Parse(typeof(Item_Use_Type), it_thorw_type);
        thorw_effect_num = System.Convert.ToInt32(it_thorw_num);
        target_type = (ITEM_MERIT_TARGET)System.Enum.Parse(typeof(ITEM_MERIT_TARGET), it_target_type);
        spawn_type = (ITEM_SPAWN_POINT)System.Enum.Parse(typeof(ITEM_SPAWN_POINT), it_spawn_type);
    }
    public Item(int id, Item_Type it_type, string it_name, Sprite it_sprite, Item_Use_Type it_use_type, int it_use_num, int it_use_count, Item_Use_Type it_thorw_type, int it_thorw_num, ITEM_MERIT_TARGET it_target_type, ITEM_SPAWN_POINT it_spawn_type) {
        item_id = id;
        Item_name = it_name;
        Item_type =  it_type;
        Item_sprite =  it_sprite;
        use_type = it_use_type;
        use_effect_num = it_use_num;
        use_count = it_use_count;
        thorw_type = it_thorw_type;
        thorw_effect_num = it_thorw_num;
        target_type = it_target_type;
        spawn_type = it_spawn_type;
    }
    public Item(Item tmp) {
        item_id = tmp.item_id;
        Item_name = tmp.Item_name;
        Item_type = tmp.Item_type;
        Item_sprite = tmp.Item_sprite;
        use_type = tmp.use_type;
        use_effect_num = tmp.use_effect_num;
        use_count = tmp.use_count;
        thorw_type = tmp.thorw_type;
        thorw_effect_num = tmp.thorw_effect_num;
        target_type = tmp.target_type;
        spawn_type = tmp.spawn_type;
    }

    //使用なのか投げなのかの情報と、効果を及ぼす相手を渡されるとその効果を発動する
    public void Item_Effect(Turn_Action act, int Use_Chara_ID, int Target_Chara_ID) {
        //引数のTurn_Actionが投げか使用以外ならreturn
        if (!(act == Turn_Action.Item_Use || act == Turn_Action.Item_Throw)) { return; }
        //投げなのか、使用なのかを判別
        int effect_num = use_effect_num;
        Item_Use_Type effect_type = use_type;
        if (act == Turn_Action.Item_Throw) {
            effect_num = thorw_effect_num;
            effect_type = thorw_type;
        }
        Charactor_Manager chara_man = GameMaster.Instance.chara_Man;

        //通常は相手に対して使用されるが
        //草など使うときには自分に効果対象がある場合は対象を変更
        int chara_ID = Target_Chara_ID;
        if ((act == Turn_Action.Item_Use) &&
            GameMaster.Instance.item_Man.Item_use_self(this) == Item_Effect_Target.Mine
            ) { 
            chara_ID = Use_Chara_ID;
        }
        Character target_chara= chara_man.Charanum_To_Chara(chara_ID);
        //もしIDがおかしければ(=効果対象がいるはずなのに効果対象がいなければ)その場でかえす
        if (GameMaster.Instance.item_Man.Item_use_self(this) != Item_Effect_Target.non && chara_ID == -1) { return; }
        //効果音を再生
        GameMaster.Instance.music_Man.PlaySE(Item_SE());
        //各効果を発動
        switch (effect_type) {
            case Item_Use_Type.Attack_Damage://攻撃力を加味したダメージ
                chara_man.Charanum_To_Chara(Use_Chara_ID).Attack_Start(effect_num, target_chara, Damage_Type.Attack,false);
                break;
            case Item_Use_Type.Const_Damage://固定ダメージ
                chara_man.Charanum_To_Chara(Use_Chara_ID).Attack_Start(effect_num, target_chara, Damage_Type.Const,false);
                break;
            case Item_Use_Type.Ratio_Aid://割合回復
                chara_man.AID(target_chara, (int)(chara_man.chara_list[chara_ID].max_HP * effect_num * 0.1f));
                break;
            case Item_Use_Type.Const_Aid://固定量の回復
                chara_man.AID(target_chara, effect_num);
                break;
            case Item_Use_Type.Warp://フロア内ワープ
                int[] next_pos = GameMaster.Instance.stage_Man.Random_Room_Pos(false, true);
                if (next_pos != null) {
                    target_chara.Chara_pos_Update(next_pos);
                    GameMaster.Instance.message_Man.File_Message(Message_Type.Warp, target_chara.chara_name);
                    GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Masic, target_chara.transform.gameObject);
                }
                break;
            case Item_Use_Type.Sleep://ターン飛ばし
                target_chara.Buf_Debuf_Add(Buf_Debuf.sleep, effect_num);
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Bad_Effect, target_chara.transform.gameObject);
                break;
            case Item_Use_Type.Max_Power://力の上限アップ
                target_chara.max_Power += effect_num;
                target_chara.now_Power += effect_num;
                GameMaster.Instance.message_Man.File_Message(Message_Type.MAX_Power_Up, target_chara.chara_name, Mathf.Abs(effect_num).ToString() + (effect_num > 0 ? "上" : "下"));
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Good_Effect, target_chara.transform.gameObject);
                break;
            case Item_Use_Type.Power_Down://力を減少
                target_chara.now_Power += effect_num;
                GameMaster.Instance.message_Man.File_Message(Message_Type.Power_Down, target_chara.chara_name, Mathf.Abs(effect_num).ToString()+ (effect_num>0?"上":"下"));
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Bad_Effect, target_chara.transform.gameObject);
                break;
            case Item_Use_Type.HP_Up://HPの上限アップ
                target_chara.max_HP += effect_num;
                target_chara.now_HP += effect_num;
                GameMaster.Instance.message_Man.File_Message(Message_Type.HP_Up, target_chara.chara_name, Mathf.Abs(effect_num).ToString()+(effect_num > 0 ? "上" : "下"));
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Good_Effect, target_chara.transform.gameObject);
                break;
            case Item_Use_Type.Level://レベルアップ
                target_chara.Level_Up(effect_num,true,true);
                break;
            case Item_Use_Type.Full_Map://マップを埋めてアイテムと階段位置を描画
                if (Use_Chara_ID == chara_man.Player_chara.chara_num) {
                    GameMaster.Instance.stage_Man.Minimap_Full_view(true, true, false);
                    GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Masic, target_chara.transform.gameObject);
                }
                break;
            case Item_Use_Type.Enemy_Map://ミニマップに敵の位置を描画
                if (Use_Chara_ID == chara_man.Player_chara.chara_num) {
                    GameMaster.Instance.stage_Man.Minimap_Full_view(false, false, true);
                    GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Masic, target_chara.transform.gameObject);
                }
                break;
            default://効果がないとき
                GameMaster.Instance.message_Man.File_Message(Message_Type.No_Effect, target_chara.chara_name, Item_name);
                break;
        }
    }

    //効果音を返す
    public string Item_SE() {
        switch (Item_type) {
            case Item_Type.Sword:
                return GameMaster.Instance.music_Man.SE_Sword;
            case Item_Type.Grass:
                return GameMaster.Instance.music_Man.SE_Grass;
            case Item_Type.Scroll:
                return GameMaster.Instance.music_Man.SE_Scroll;
        }
        return null;
    }

}

//落ちているアイテムの情報
public class Item_puton {
    public Item item_one;//アイテム自体
    public int[] item_pos = new int[2];//座標
    public bool mapping = false;//マップ化情報

    public Item_puton(Item it,int[] pos) {
        item_one = it;
        item_pos = Stage_Manager.Return_int2(pos);
    }
}