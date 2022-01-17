using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using UnityEngine.UI;
public class Charactor_Manager : MonoBehaviour {
    [System.NonSerialized]
    public List<Character> chara_list=new List<Character>();
    [System.NonSerialized]
    public List<GameObject> Enemy_obj_list=new List<GameObject>();
    [System.NonSerialized]
    public GameObject Chara_Parent;
    [System.NonSerialized]
    public GameObject Player_obj;
    [System.NonSerialized]
    public Player Player_chara;

    [Header("状態異常関係")]
    [EnumIndex(typeof(Buf_Debuf))]
    public Sprite[] Con_image;//睡眠状態の時に表示するアイコン
    public float condition_update_time = 1f;//状態異常の表示の更新頻度(秒)

    [Header("アニメーション関係")]
    //アニメーションの内容のハッシュ
    public int Walk_state = Animator.StringToHash("Walk");
    public int Idle_state = Animator.StringToHash("Idle");
    public int Attack_state = Animator.StringToHash("Attack");
    public int Damage_state = Animator.StringToHash("Damage");
    public int Death_state = Animator.StringToHash("Death");
    public int LevelUp_state = Animator.StringToHash("LevelUp");
    
    public Player chara_tmp;//キャラ情報を保存しておく

    public GameObject enemy;//生成する敵オブジェクト

    public Vector3 pos_def = new Vector3(0, -0.25f, 0);//キャラがマス目の中央から表示位置がどれだけずれるか

    //8方向表記と時計回り表記の変換表、[中心にする点-1,方向-1]で呼べる
    public static int[,] Dir_to_clock_hash =
    { { 0, 1, 2, 7, 8, 3, 6, 5, 4 }
    , { 7, 0, 1, 6, 8, 2, 5, 4, 3 }
    , { 6, 7, 0, 5, 8, 1, 4, 3, 2 }
    , { 1, 2, 3, 0, 8, 4, 7, 6, 5 }
    , { 8, 8, 8, 8, 8, 8, 8, 8, 8}
    , { 5, 6, 7, 4, 8, 0, 3, 2, 1 }
    , { 2, 3, 4, 1, 8, 5, 0, 7, 6 }
    , { 3, 4, 5, 2, 8, 6, 1, 0, 7 }
    , { 4, 5, 6, 3, 8, 7, 2, 1, 0 } };
    enum chara_type{
    PLAYER,MONSTER
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    //シーン遷移時にオブジェクト参照をすべてリセットする
    public void Scene_Change() {
        player_copy(Player_chara);
        chara_list = new List<Character>();
        Enemy_obj_list = new List<GameObject>();
        Chara_Parent = null;
        Player_obj = null;
    }

    //キャラたちの親オブジェクトを作成
  public  void Parent_Criate() {
        if (Chara_Parent == null) {
        Chara_Parent = new GameObject("Chara_Parent");
        }
    }

    //プレイヤーの作成
    public Character Player_Criate() {
        //親オブジェクトが存在しなければ親を作成
        Parent_Criate();
        //もし対戦画面ならオンライン用の生成
        if (GameMaster.Instance.Now_Mode_Equal( Game_Now_Phase.Battle)) {
            Player_obj = GameMaster.Instance.online_Man.player_criate();
        }
        //その他の画面ならそのまま生成
        else {
            Player_obj = (GameObject)Instantiate(Resources.Load("Player"), Chara_Parent.transform);
        }
        Player_chara = Player_obj.GetComponent<Player>();

        //一時保存データが存在するならそれをプレイヤーキャラに上書き
        if (chara_tmp != null) {
            chara_list.Add(Player_chara);
            Player_chara.chara_num = chara_list.Count - 1;
            player_paste(Player_chara);
            Player_chara.Direction_Change(8);
            int[] pos;
                pos = GameMaster.Instance.stage_Man.Random_Room_Pos(false,true);
            Player_chara.Chara_pos_Update(pos);
        }
        //データがなければ新たに作成
        else {
            Chara_Add(Player_chara);
            player_copy(Player_chara);
        }
        Player_chara.chara_name = GameMaster.Instance.player_chara_name;
        GameMaster.Instance.item_Man.Item_slot_Criate();
        Status_UI_Start();

        return Player_chara;
    }

    //プレイヤーのリセット処理
    public Character Player_Reset() {
        //もしプレイヤーが存在しなければ新しく作る
        if (Player_obj == null) { Player_Criate(); }
        //存在するなら向きと座標を初期化
        else {
            Player_chara.Direction_Change(8);
            Player_chara.Chara_pos_Update(GameMaster.Instance.stage_Man.Random_Room_Pos(false,true));
            Status_UI_Start();
        }
        return Player_chara;
    }
    //渡されたプレイヤーデータを保存
    void player_copy(Player origin) {
        if (origin == null) {  return; }
        if (chara_tmp == null) { chara_tmp = GetComponent<Player>(); }
        chara_tmp.max_HP = origin.max_HP;
        chara_tmp.max_Power = origin.max_Power;
        chara_tmp.level = origin.level;
        chara_tmp.EXP = origin.EXP;
        chara_tmp.have_item_Max = origin.have_item_Max;

        chara_tmp.have_Items.Clear();
        for (int i = 0; i < origin.have_item_Max; i++) {
            chara_tmp.have_Items.Add(origin.have_Items[i]);
        }
    }

    //一時保存していたプレイヤーデータを渡されたプレイヤーに上書き
    Character player_paste(Player copy) {
        if (chara_tmp == null) { return null; }
        if (copy == null) {copy = GetComponent<Player>(); }
        copy.max_HP = chara_tmp.max_HP;
        copy.max_Power = chara_tmp.max_Power;
        copy.now_HP = copy.max_HP;
        copy.now_Power = copy.max_Power;
        copy.level = chara_tmp.level;
        copy.EXP = chara_tmp.EXP;
        copy.have_item_Max = chara_tmp.have_item_Max;

        copy.have_Items.Clear();
        for (int i = 0; i < chara_tmp.have_item_Max; i++) {
            copy.have_Items.Add(chara_tmp.have_Items[i]);
        }
        return copy;
    }
    //キャラ追加処理
    public void Chara_Add(Character chara) {
        chara_list.Add(chara);
        chara.chara_num = chara_list.Count-1;
        chara.Direction_Change(8);
        int[] pos;
          pos = GameMaster.Instance.stage_Man.Random_Room_Pos(false,true);
        chara.Chara_pos_Update(pos);
        chara.now_HP = chara_list[chara_list.Count - 1].max_HP;
        chara.now_Power = chara_list[chara_list.Count - 1].max_Power;
        //アイテム所持欄の作成
        chara.ItemSlot_Criate(chara_list[chara_list.Count - 1].have_item_Max);
    }
    //ダンジョンでの敵追加処理
    public Character Dan_Enemy_Add() {
        //親オブジェクトが存在しなければ作成
        Parent_Criate();
        //キャラ上限に引っかからなければキャラを生成
        if (chara_list.Count<GameMaster.Instance.dan_Man.Chara_Max) {
            Enemy_obj_list.Add((GameObject)Instantiate(enemy, Chara_Parent.transform));
            Character ene = Enemy_obj_list[Enemy_obj_list.Count - 1].GetComponent<Character>();
            Chara_Add(ene);
            return ene;
        }
        return null;
    }
    //戦闘シーンでの敵追加処理
    public Character BT_Enemy_Add() {
        //親オブジェクトが存在しなければ作成
        Parent_Criate();
        Character ene=GameMaster.Instance.online_Man.BTchara_criate().GetComponent<Character>();
        //他のキャラの平均レベルからレベルを決める
        GameObject[] players= GameObject.FindGameObjectsWithTag("Player");
        int level_sum=0;
        for(int i=0;i< players.Length; i++) {
            level_sum+= players[i].GetComponent<Character>().level;
        }
        //レベルを設定
        ene.Level_Up((level_sum / players.Length)+Random.Range(-2,3),false,false);
        //キャラの初期化
        Chara_Add(ene);
        //アイテムをランダムに設定
        ((Battle_Chara)ene).Random_Item_Get(false,Game_Now_Phase.Battle);
        return ene;
    }
    //敵初期化処理
    public void Enemy_Reset() {
        if (Enemy_obj_list != null) {
            //負荷軽減のため末尾から削除
            for (int i = Enemy_obj_list.Count - 1; i >= 0; i--) {
                chara_list.Remove(Enemy_obj_list[i].GetComponent<Character>());
                Destroy(Enemy_obj_list[i]);
                Enemy_obj_list.RemoveAt(i);
            }
        }
    }
    //キャラ削除処理
    public void Chara_Remove(Character chara) {
        //戦闘シーンなら削除が同期されるようにして削除
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)) {
            //カメラで注視中のキャラであった場合は他のキャラを注視させる
            if (GameMaster.Instance.battle_Man.camera_chara == chara.chara_num) {
                for (int i = 0; i < Battle_Manager.Chara_Max; i++) {
                    //削除されるキャラは飛ばす
                    if (i == chara.chara_num) { continue; }
                    //生きているキャラを見つけたらそのキャラに注視を変更
                    if (GameMaster.Instance.battle_Man.Chara_Alive(i)) {
                        GameMaster.Instance.battle_Man.camera_chara = i;
                        break;
                    }
                }
            }
            //順位のリストを更新
            GameMaster.Instance.online_Man.RPC_view.Send_Ranking_Sync(chara.chara_num, chara_list.Count);
            //死んだキャラの足元に、持っていたアイテムをランダムにドロップさせる
            for(int i = 0; i < chara.have_item_Max; i++) {
                if (chara.have_Items[i] != null&&GameMaster.Instance.battle_Man.item_drop_prob< Random.Range(0,100)) {
                    GameMaster.Instance.battle_Man.New_Item_Drop(chara.have_Items[i].item_id, chara.now_position);
                    chara.have_Items[i] = null;
                }
            }
            if (chara == Player_chara) { GameMaster.Instance.item_Man.Item_Slot_Update(); }
            //オブジェクトの削除
            GameMaster.Instance.online_Man.player_destroy(chara);
            //リストから除外
            chara_list.Remove(chara);
        }
        //探索シーンかつ、自キャラが死んだとき
        else  if (chara.tag=="Player"&&GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Dangeon)) {
            GameMaster.Instance.dan_Man.Player_Death();
        }
        else {
            //その他のシーンなら
            for (int i = Enemy_obj_list.Count - 1; i >= 0; i--) {
                if (Enemy_obj_list[i].GetComponent<Character>() == chara) {
                    //リストから除外
                    chara_list.Remove(Enemy_obj_list[i].GetComponent<Character>());
                    //オブジェクトの削除
                    Destroy(Enemy_obj_list[i]);
                    Enemy_obj_list.RemoveAt(i);
                }
            }
        }
    }


    //引数で指定された位置に移動可能かどうかを返す
    public Can_Move_Tile Can_Move_pos(int[] pos) {
        //もしnullならfalse
        if (pos == null) { return Can_Move_Tile.Null; }
        //床でなければfalse
        if(!
            (GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile== Tile_Type.Hall_Floor
            || GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Room_Floor
            || GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Next_Stage)
            ) {
            return Can_Move_Tile.Wall;
        }
            //キャラが存在すればfalse
        for (int i = 0; i < chara_list.Count; i++) {
            if (GameMaster.Instance.stage_Man.Pos_Equal(pos, chara_list[i].now_position)) {
                return Can_Move_Tile.Chara;
             }
        }
        return Can_Move_Tile.Can_Move;
    }
    //引数で指定された位置に移動可能かどうかを返す,chara_numに渡されたキャラは無視する
    public Can_Move_Tile Can_Move_pos(int[] pos,int chara_num) {
        //もしnullならfalse
        if (pos == null) { return Can_Move_Tile.Null; }
        //床でなければfalse
        if (!
            (GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Hall_Floor
            || GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Room_Floor
            || GameMaster.Instance.stage_Man.stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Next_Stage)
            ) {
            return Can_Move_Tile.Wall;
        }
        //キャラが存在すればfalse
        for (int i = 0; i < chara_list.Count; i++) {
            if (i!=chara_num&&GameMaster.Instance.stage_Man.Pos_Equal(pos, chara_list[i].now_position)) {
                return Can_Move_Tile.Chara;
            }
        }
        return Can_Move_Tile.Can_Move;
    }

    //何かしらが敵に当たった時に反応が終わったかどうかを返す
    public bool Reaction_Update(Character mine,Character target,int target_state) {
        
        //自分が待機モーションに戻り、相手も反応モーションが終わっていたらターン終了
        if (mine.Animaton_Now(Idle_state)) {
            //もし敵がいなければ即終了
            if (target == null) { return true; }
            //敵がいれば終わるまで待つ
            else if (target.Animaton_Now(target_state)==false) {
                //もし相手のHPが0以下になっていたら消滅させる
                if (target.now_HP < 1) {
                    GameMaster.Instance.chara_Man.Death_Message(target);
                    GameMaster.Instance.chara_Man.Chara_Remove(target);
                    mine.exp_get(target);
                }
                //ターン終了
                return true;
            }
        }
        return false;
    }

    //倒した/倒されたときにメッセージを表示
    public void Death_Message(Character chara) {
        if (chara.tag == "Player"||chara.tag=="Battle_Enemy") {
            GameMaster.Instance.message_Man.File_Message(Message_Type.Death, chara.chara_name);
        }
        else {
            GameMaster.Instance.message_Man.File_Message(Message_Type.Kill, chara.chara_name);
        }
    }

    //計算式によるダメージ判定,武器の威力、攻撃する人、される人
    public void Attack_Damage(int attack_power,Character mine,Character target) {
        Damage(target,(mine.level * (mine.now_Power + attack_power + 8)) / 4);
    }
    //固定ダメージ判定、固定ダメ―ジ、攻撃される人
    public void Const_Damage(int attack_power,Character target) {
        Damage(target, attack_power);
    }

    //ダメージ処理、攻撃される人、ダメージ
    public void Damage(Character target, int damage) {
        GameMaster.Instance.message_Man.File_Message(Message_Type.Damage_Effect, target.chara_name, damage.ToString());
        target.now_HP -= damage;
        GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.damage, target.transform.gameObject);
        if (target.now_HP < 1) {
            target.now_HP = 0;
        }
    }

    //回復処理
    public void AID(Character target, int aid) {
        GameMaster.Instance.message_Man.File_Message(Message_Type.AID_Effect, target.chara_name, aid.ToString());
        target.now_HP += aid;
        GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.aid, target.transform.gameObject);
        if (target.now_HP > target.max_HP) {
            target.now_HP = target.max_HP;
        }
    }

    //引数で指定された位置にキャラがいればそのキャラを返し、いなければnullを返す
    public Character Pos_To_Chara(int[] pos) {
        for (int i = 0; i < chara_list.Count; i++) {
            if (GameMaster.Instance.stage_Man.Pos_Equal(pos,chara_list[i].now_position)) { return chara_list[i]; }
        }
        return null;
    }

    //向いている方向centerを0として、時計回りに変換したときのdir方向を返す
    public int Dir_To_Clock (int center, int dir) {
        try {
            return Dir_to_clock_hash[center - 1, dir-1];
        }
        catch (System.IndexOutOfRangeException) {
            Debug.LogError((center - 1) + ":" +( dir ));
            return 5;
        }
     }

    //centerを0として、時計回りにnumだけ回転したときのdirを返す
    public int Center_To_Clock(int center,int num) {
        //与えられた値が範囲外の時例外処理
        if ((center < 1 || 9 < center)||(num < 0 || 8 < num)) { Debug.LogError(center + ":" + num + ":例外"); return 5; }
        for(int i = 1; i < 10; i++) {
            if (Dir_to_clock_hash[center-1, i-1] == num) { return i; }
        }
        return 5;
    }

    //向いている方向と座標を引数に、その直線状にキャラがいればそのキャラ番号を返す
    public int Dir_To_Arrow_Chara(int[] pos,int dir) {
        int[] next_pos=pos;
        for(int i = 0; i < 10; i++) {
            next_pos=GameMaster.Instance.stage_Man.Dir_Next_Pos(next_pos, dir);
            Character tmp = Pos_To_Chara(next_pos);
            if (tmp != null) { return tmp.chara_num; }
        }
        return -1;
    }
    //引数の2キャラが直線にいるかどうかを返す
    public bool Pos_To_Arrow_Chara(int base_chara,int target_chara) {
        //キャラの値がおかしかったら即返す
        if(Charanum_To_Chara(base_chara) == null || Charanum_To_Chara(target_chara) == null) {
            return false;
        }
        //2キャラの座標を取得
        int[] pos=Charanum_To_Chara(base_chara).now_position;
        int[] tmp= Charanum_To_Chara(target_chara).now_position;
        //縦横斜めで計算
        return (
            pos[(int)XY.X] == tmp[(int)XY.X] ||
            pos[(int)XY.Y] == tmp[(int)XY.Y] ||
            ((pos[(int)XY.X] - tmp[(int)XY.X]) == (pos[(int)XY.Y] - tmp[(int)XY.Y]))
           ) ;
    }
    //キャラ番号を引数にキャラを返す
    public Character Charanum_To_Chara(int charanum) {
        for(int i=0;i<chara_list.Count; i++) {
            if (chara_list[i].chara_num == charanum) { return chara_list[i]; }
        }
        return null;
    }

    //状態異常を与えると、メッセージのタイプに変更して返す
    public Message_Type BufDebuf_To_MessageType(Buf_Debuf ab,bool start) {
        switch (ab) {
            case Buf_Debuf.sleep:
                if (start) {return Message_Type.Sleep_Start;}
                  else{ return Message_Type.Sleep_Now; }
        }
        return Message_Type.No_Effect;
    }
    //ステータスUI関係
    public Text name_text;
    public Text level_text;
    public Text HP_text;
    public Slider HP_slider;
    public Text power_text;
    public Slider power_slider;
    public Text floor_text;

    //ステータスUIの初期化
    public void Status_UI_Start() {
        name_text.text = Player_chara.chara_name;
        Status_UI_Update();
    }
    //ステータスUIの更新
    public void Status_UI_Update() {
        level_text.text = ("Lv."+Player_chara.level);
       HP_text.text = (Player_chara.now_HP + "/" + Player_chara.max_HP);
      //  HP_slider.fillRect.anchorMax = new Vector2(Player_chara.max_HP * 0.005f+1, HP_slider.fillRect.anchorMax.y);
        HP_slider.maxValue = Player_chara.max_HP;
        HP_slider.value = Player_chara.now_HP;

        power_text.text = (Player_chara.now_Power + "/" + Player_chara.max_Power);
       // power_slider.fillRect.anchorMax = new Vector2(Player_chara.max_Power * 0.01f + 1, power_slider.fillRect.anchorMax.y);
        power_slider.maxValue = Player_chara.max_Power;
        power_slider.value = Player_chara.now_Power;

        floor_text.text = GameMaster.Instance.dan_Man.Now_Floor_num+"階";
    }
}
