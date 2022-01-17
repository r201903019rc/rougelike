using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using Photon.Pun;
using UnityEngine.UI;
[System.Serializable]
public class Character : MonoBehaviour
{
    public bool turn_act;//すでに行動したか否か
    protected Item_Manager item_man;
    protected Charactor_Manager chara_man;
    public int chara_num;//キャラ番号
   [System.NonSerialized]
    public int[] now_position=new int[2];//現在の座標
    [System.NonSerialized]
    public int now_direction;//現在向いている方向
    [System.NonSerialized]
    public Turn_Action now_action;//ターン中の行動
    

    //ステータス関係
    public string chara_name;//キャラ名
    public int level=1;//レベル
    [System.NonSerialized]
    public int now_HP=15;//現在体力
    public int max_HP=15;//最大体力
    [System.NonSerialized]
    public int now_Power=8;//現在の力
    public int max_Power=8;//最大の力
    public int EXP = 0;//経験値
    public List<Abnormal> ab_list=new List<Abnormal>();//状態異常
    protected bool abnormal_now;//状態異常にかかっているかどうかのフラグ
    private GameObject condition_obj;//状態異常にかかっている時表示するオブジェクト
    private SpriteRenderer condition_image;//状態異常にかかっている時表示する
    private int condition_now=0;//現在表示している状態異常の番号
    private float condition_time;//状態異常の更新累積時間
    

    //ターンの行動関係
    protected float anim_length;//再生中のアニメーションの長さ
    [System.NonSerialized]
    public Character tmp;//行動を及ぼした相手を保存しておく

    //アニメーション関係
    [System.NonSerialized]
    public Animator anim;
    protected AnimatorStateInfo now_anim_state;
    protected int anim_state_hash;

   //アイテム関係
    public List<Item> have_Items = new List<Item>(); //プレイヤーの持つアイテムのリスト
    public int have_item_Max = 3;//持てるアイテムの最大数
    public int now_selected_item = 0;//現在選択しているアイテム

    //同期関係
    protected PhotonView ph_view;
    protected PhotonTransformViewClassic ph_trans;

    //開始時の処理
    protected void Ct_Start() {
        anim = GetComponent<Animator>();
        chara_man = GameMaster.Instance.chara_Man;
        item_man = GameMaster.Instance.item_Man;
        if (tag != "Enemy") {
            ph_view = GetComponent<PhotonView>();
            ph_trans = GetComponent<PhotonTransformViewClassic>();
        }
        Chara_pos_Update(now_position);
    }
    //常に呼ばれる処理
    protected void Ct_Update() {
        //状態異常に罹っているならその旨を示す画像をキャラの上に表示
        if (abnormal_now) {
            Buf_Debuf_view_Update();
        }
    }

    //ターン開始時
    public virtual void Turn_Start() {
        turn_act = false;
    }
    //行動が終わったらtrue、実行中はfalse
    public virtual bool Turn_Update_Start() {
        return false;
    }

    //行動確定後の処理
    public virtual bool Turn_Update_End() {
        //現在のアニメーションを取得
        try {
            now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        }
        //Animatorが取得できていない場合はそれを取得し直す
        catch (System.NullReferenceException) {
            anim = GetComponent<Animator>();
            now_anim_state = anim.GetCurrentAnimatorStateInfo(0);

        }

        //現在の行動に合わせた処理
        switch (now_action) {
            case Turn_Action.Move:
                //プレイヤーの状態がDashのとき、他キャラは移動アニメーションをスキップしてターン終了
                if (chara_man.Player_chara.now_action == Turn_Action.Dash
                     || chara_man.Player_chara.now_action == Turn_Action.Skip) { return true; }
                //プレイヤーがその他の行動をとっているとき、移動アニメーション
                else {
                        return Move_Update();
                }
            case Turn_Action.Attack:
                return chara_man.Reaction_Update(this, tmp, chara_man.Damage_state);
            case Turn_Action.Skip:
                return true;
            case Turn_Action.Dash:
                return true;
            case Turn_Action.Item_Drop:
                return true;
            case Turn_Action.Item_Use:
                if (item_man.Item_Use_Update(chara_num)) {
                    item_man.Item_Use_End(chara_num);
                    return true;
                }
                break;
            case Turn_Action.Item_Throw:
                //アイテムが投げて当たるまで待つ
                if (item_man.Item_Throw_Update(chara_num)) {
                    item_man.Item_Throw_End();
                    return true;
                }
                break;
        }
        return false;
    }
    public virtual void Turn_End() {
        Buf_Debuf_Update();
        turn_act = false;
        tmp = null;
        anim_state_hash = chara_man.Idle_state;
        //自分の画面では動きが終わったことを他プレイヤーに送信
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)) {
            Act_End_Sync();
        }
        //ぶれた場合を考え、マス目の中心に移動
        Chara_pos_Update(now_position);
        return;
    }
    //行動が決定したときに呼ばれる
    public void Turn_Act_Decided(Turn_Action act) {
        now_action = act;
        turn_act = true;
        //戦闘シーンで、送受信をする権利のあるプレイヤーなら他のプレイヤーに行動データを送信する
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)) {
            if ((tag == "Player" && GameMaster.Instance.online_Man.player_mine(this)) || (tag == "Battle_Enemy" && GameMaster.Instance.online_Man.room_owner)) {
                ph_view.RPC(nameof(Act_Decision), RpcTarget.OthersBuffered, (int)now_action, now_direction, (have_Items[now_selected_item] !=null? have_Items[now_selected_item].item_id:-1));
            }
        }
    }

    //状態異常の更新
    public void Buf_Debuf_Update() {
        //解除すべき状態異常のリスト
        List<int> remove =new List<int>();
        //状態異常の残りターン数を計算し、0以下で解除すべきもののリストを作成
        for (int i = 0; i < ab_list.Count; i++) {
            ab_list[i].re_turn--;
           //残りターン数が0になったら解除
            if (ab_list[i].re_turn <= 0) { remove.Add(i); }
            //まだ続くなら現在の状態異常をメッセージに表示
            else {
                GameMaster.Instance.message_Man.File_Message(chara_man.BufDebuf_To_MessageType(ab_list[i].type, false), chara_name);
            }
        }
        //解除する状態異常のリストで状態を更新
        for(int i = remove.Count - 1; i >= 0; i--) {
            ab_list.RemoveAt(remove[i]);
        }
        //状態異常の数が0ならフラグを解除する
        if (abnormal_now == true&&ab_list.Count<=0) {
            abnormal_now = false;
            Buf_Debuf_view_End();
        }
    }
    //動けるかどうかを返す
    public bool Buf_Debuf_CanMove() {
        for (int i = 0; i < ab_list.Count; i++) {
            if (ab_list[i].type == Buf_Debuf.sleep) {return false ; }
        }
        return true;
    }
    //状態異常の追加
    public void Buf_Debuf_Add(Buf_Debuf ab,int add_turn) {
        //状態異常を追加
        for (int i = 0; i < ab_list.Count; i++) {
            if (ab_list[i].type == ab) { ab_list[i].re_turn+=add_turn; return; }
        }
        ab_list.Add(new Abnormal(ab,add_turn));
        //メッセージを表示
        GameMaster.Instance.message_Man.File_Message(chara_man.BufDebuf_To_MessageType(ab,true), chara_name);
        //状態異常が初めてついたなら状態異常フラグをオン
        if (abnormal_now == false) {
            abnormal_now = true;
            Buf_Debuf_view_Start();
        }
    }
    //状態異常の解除
    public void Buf_Debuf_Remove(Buf_Debuf ab) {
        for(int i = 0; i < ab_list.Count; i++) {
            if (ab_list[i].type == ab) { ab_list.RemoveAt(i);return; }
        }
    }
    //状態異常情報を表示するオブジェクトの生成
    void Buf_Debuf_view_Start() {
        //新たなオブジェクトを生成、親をこのオブジェクトに、コンポーネントを付ける
        condition_obj = new GameObject();
        condition_obj.transform.parent = transform;
        condition_image= condition_obj.AddComponent<SpriteRenderer>();
        //transformを調節
        condition_obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        condition_obj.transform.localPosition =new Vector3(0,3,0);
        condition_obj.transform.localScale = Vector3.one;
        //layerを変更、表示されるSpriteを正位置に
        condition_obj.layer = LayerMask.NameToLayer("Charactor");
        condition_image.flipX = true;
        //表示される状態異常情報を決めて表示を実行する
        condition_now = 0;
        Buf_Debuf_view_Change();
    }
    //状態異常情報の表示を更新
    void Buf_Debuf_view_Update() {
        condition_time += Time.deltaTime;
        if (condition_time >= chara_man.condition_update_time) {
            condition_time = 0;
            if (condition_now + 1 <= ab_list.Count) {
                condition_now++;
            }
            else {
                condition_now = 0;
            }
            Buf_Debuf_view_Change();
        }
    }
    //状態異常情報を表示するオブジェクトの削除
    void Buf_Debuf_view_End() {
        condition_image = null;
        Destroy(condition_obj);
    }
    void Buf_Debuf_view_Change() {
        if (ab_list.Count==0||condition_now == ab_list.Count) {
            condition_image.sprite = null;
        }
        else {
            condition_image.sprite = chara_man.Con_image[(int)ab_list[condition_now].type];
        }
    }

    //攻撃
    //引数の方向に攻撃する
    public bool Attack_Start(int power,Character at_dir,Damage_Type type,bool se) {
        if (se) { GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Punch); }
        //攻撃アニメーションの開始
        Anim_Start(chara_man.Attack_state);
        //敵がいれば攻撃ヒット処理
        if (at_dir != null) {
            if (type == Damage_Type.Attack) {
                chara_man.Attack_Damage(power, this, at_dir);
            }
            else if(type == Damage_Type.Const) {
                chara_man.Const_Damage(power,  at_dir);
            }
            tmp = at_dir;
            //敵のダメージアニメーション
            tmp.Anim_Start(tmp.now_HP<=0? chara_man.Death_state:chara_man.Damage_state);
            //もし相手がダッシュ状態ならそれを解除
            if (tmp.now_action== Turn_Action.Dash) { tmp.now_action = Turn_Action.Move; }
            return true;
        }
        //攻撃が当たらなかったときの処理
        else{
            return false; 
        }
    }

    public void Anim_Start(int hash) {
        anim.Play(hash);
        anim_state_hash =hash;
        anim.Update(0f);
    }


    //移動
    //方向を引数にし、そちらに移動する関数、移動出来たらtrueを返す
   public bool Move_Start(int dir) {
        int[] next = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, dir);
        //向いている方向のマスに移動可能なら
        if (Dir_Move(dir)) {
            //キャラの座標を更新
            now_position = next;
            Anim_Start(chara_man.Walk_state);
            return true;
        }
        return false;
    }
    //移動中に呼ばれる関数
    public bool Move_Update() {
        //歩行アニメーションの長さを計算
        now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        anim_length = now_anim_state.length;

        float anim_len = (1/anim_length) * Time.deltaTime;
        //MoveTowardで移動先マスと現在のマスを1フレーム辺り1/anim_lenの速度で移動
        transform.localPosition = Vector3.MoveTowards(transform.position ,GameMaster.Instance.stage_Man.Tile_To_World(now_position)+ chara_man.pos_def, anim_len);
        //同期元であれば座標の同期のための処理
        if (ph_view!=null&&ph_view.IsMine) {
            //移動速度を指定
            ph_trans.SetSynchronizedValues(transform.forward*anim_len, 0);
        }
       // return (Animaton_Now(chara_man.Walk_state) == true);
       //移動が終了していればtrueを返す
        return (transform.position== GameMaster.Instance.stage_Man.Tile_To_World(now_position) + chara_man.pos_def);

    }

    //アイテム関係

    //足元のアイテムを拾う処理
   protected void Get_Item() {
        for (int i = 0; i < item_man.on_stage_items.Count; i++) {
            if (GameMaster.Instance.stage_Man.Pos_Equal(now_position, item_man.on_stage_items[i].item_pos)) {
                GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Get);
                item_man.Item_Get_Stage(chara_num, i, true);
            }
        }
    }
    //アイテムをインベントリから落とす処理
    protected void Item_Slot_Drop(int item_id) {
        //足元にアイテムを落とせるかつ、選択中のスロットにアイテムが入っていればアイテムをその場に落とす
        if (GameMaster.Instance.item_Man.Item_Drop(item_id,now_position)) {
            GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Drop);
            //使用ウインドウを消す
            GameMaster.Instance.item_Man.Item_Use_Window_Start(false);
            //もし自分のキャラによる操作ならアイテムスロットを空に
            if (GameMaster.Instance.online_Man.player_mine(chara_man.Charanum_To_Chara(chara_num))) {
                have_Items[now_selected_item] = null;
            }
            GameMaster.Instance.item_Man.Item_Slot_Update();
            GameMaster.Instance.stage_Man.Item_view();
        }
    }
    //アイテムを使う処理
    protected void Use_Item(int item_id) {
        //目の前にいるキャラ
        Character next_chara = chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, now_direction));
        //アイテム仕様処理
        item_man.Item_Use_Start(chara_num, (next_chara != null ? next_chara.chara_num : -1),item_id);
    }
    //アイテムを投げる処理
 protected void Thorw_Item(int item_id) {
        item_man.Item_Throw_Start(chara_num, item_id);
    }
    //アイテムスロットにいくつ空きがあるかを返す
    public int Null_Itemslot_num() {
        int count = 0;
        for(int i = 0; i < have_item_Max; i++) {
            if (have_Items[i] == null) { count++; }
        }
        return count;
    }

    //アイテムスロットの数を与えられた数にする
    public void ItemSlot_Criate(int num) {
        //アイテムマネージャーの参照ができないなら取得し直す
        if (item_man == null) { item_man = GameMaster.Instance.item_Man; }
        //与えられた数が正常範囲内なら
        if (0 <= num && num <= item_man.item_slot_MAX) {
            have_item_Max = num;
            //アイテムスロットの数が与えられた数より大きい場合、同数になるまで末尾のスロットを消していく
            if (have_Items.Count > num) {
                while (have_Items.Count > num) {
                    //末尾に格納されたアイテムを床に落とす
                    item_man.Item_Drop(have_Items[have_Items.Count - 1].item_id, now_position);
                    //末尾を削除
                    have_Items.RemoveAt(have_Items.Count - 1);
                }
            }
            //アイテムスロットの数が与えられた数より小さい場合、同数になるまで末尾にスロットを追加する
            else if (have_Items.Count < num) {
                while (have_Items.Count < num) {
                    have_Items.Add(null);
                }
            }
            //プレイヤーキャラならスロットの描画を更新
            if (tag == "Player") {
                item_man.Item_slot_Criate();
            }
        }
    }
    //現在持っているアイテムの数を返す
   public int Have_Item_Num() {
        int count = 0;
        for(int i = 0; i < have_item_Max; i++) {
            if (have_Items[i] != null) { count++; }
        }
        return count;
    }

    //引数の方向に移動可能かどうかを返す
    public bool Dir_Move(int dir) {
        return (chara_man.Can_Move_pos(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, dir)) == Can_Move_Tile.Can_Move);
    }

    //キャラの方向を引数の向きに変える
   public void Direction_Change(int dir) {
        //キャラの向きを変更
        now_direction = dir;
        //向きを度数に変換
       int rota= Dir_to_Rota(dir);
        transform.localRotation = Quaternion.Euler(rota + 90f, -90f, -90f);
        //もし状態異常にかかっていればその表示の角度を更新
        if (abnormal_now) {
            condition_obj.transform.rotation =Quaternion.Euler(Vector3.zero);
        }
    }

    //与えられたアニメーションが再生中かどうかを返す
    public bool Animaton_Now(int state) {
        now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        //ダメージアニメーションは、死亡アニメと共通処理に
        if (state == chara_man.Damage_state) {
            return (now_anim_state.shortNameHash == chara_man.Damage_state || now_anim_state.shortNameHash == chara_man.Death_state);
        }
        else {
            return now_anim_state.shortNameHash == state;
       }
    } 

    //引数のキャラから経験値を獲得し、レベルを上げる
    public void exp_get(Character target) {
        if (target.tag == "Enemy") { EXP += target.EXP; }
        else if (target.tag == "Battle_Enemy") { EXP += target.EXP / 2; }

        int no = 8;
        int count = 1;
        while (no <= EXP) {
            count++;
            if (no < EXP && count > level) {
                Level_Up(level + 1,true,true);
            }
            no += (int)(no * 1.1f);
        }
    }
   
    //レベル上げ処理、(引数:変更後のレベル、レベルが上がったメッセージを表示するかどうか)
    public void Level_Up(int Change_level,bool message,bool action) {
        //ステータスを変化
        int greHP= Random.Range(2, 7) * Change_level;
        max_HP += greHP;
        now_HP += greHP;
        if (max_HP <= 0) {
            max_HP = 1;
            now_HP = 1;
        }
        //変化前のレベルを保存
        int bef_level = level;
        //変化後のレベルが1以上ならその値に、1より小さくなる場合は1にする
        if(Change_level + level > 1) {
            level += Change_level;
        }
        else {
            level = 1;
        }
        //レベル変動関係の演出をするかどうか
        if (action) {
            //上昇であれば喜びモーションを再生し、SEとエフェクトを流す
            if (Change_level > 0) {
                GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_LevelUp);
                Anim_Start(chara_man.LevelUp_state);
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Level_Up, transform.gameObject);
            }
            //減少であればSEとエフェクトのみ
            else {
                GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_LevelDown);
                GameMaster.Instance.effect_Man.Effect_Start(Effect_Manager.Effect_Type.Level_Down, transform.gameObject);
            }
        }
        //レベルに変動があり、ウインドウにメッセージを表示するならメッセージを送信
        if (bef_level!=level&&message) {
            GameMaster.Instance.message_Man.File_Message(Message_Type.Lv_Up, chara_name, Mathf.Abs(Change_level).ToString() + (Change_level > 0 ? "上" : "下"));
        }
    }

    int Dir_to_Rota(int dir) {
        try {
            return chara_man.Dir_To_Clock(8, dir) * 45;
        }
        catch (System.NullReferenceException) {
            return GameMaster.Instance.chara_Man.Dir_To_Clock(8, dir) * 45;
        }
    }
    
    //キャラの座標を移動させる関数
    public void Chara_pos_Update(int[] pos) {
        now_position = pos;
        transform.localPosition = GameMaster.Instance.stage_Man.Tile_To_World(pos)+ (chara_man!=null? chara_man.pos_def:Vector3.zero);
    }

    // キャラから見た与えられた座標の方角を返す関数
    public int Dirction_target(int[] target_pos) {
        if (target_pos == null) { return 5; }
        int dir = 5;
        //X軸が同じ
        if (now_position[(int)XY.X] == target_pos[(int)XY.X]) {
            if (now_position[(int)XY.Y] > target_pos[(int)XY.Y]) {
                dir = 2;
            }
            else if (now_position[(int)XY.Y] < target_pos[(int)XY.Y]) {
                dir = 8;
            }
        }
        //Y軸が同じ
        else if (now_position[(int)XY.Y] == target_pos[(int)XY.Y]) {
            if (now_position[(int)XY.X] < target_pos[(int)XY.X]) {
                dir = 6;
            }
            else if (now_position[(int)XY.X] > target_pos[(int)XY.X]) {
                dir = 4;
            }
        }
        //XY軸が被らないとき
        else {
            //斜め方向のどこに目的地があるのかを計算
            if (now_position[(int)XY.X] > target_pos[(int)XY.X]) { dir-=1; }
            else { dir+=1; }
            if (now_position[(int)XY.Y] > target_pos[(int)XY.Y]) { dir-=3; }
            else { dir+=3; }
            //完全に斜めの延長線上にあるならそのまま返し、斜めでも軸が合わなければ一番距離の近い方向を返す
            if (!(now_position[(int)XY.X] + (target_pos[(int)XY.Y] - now_position[(int)XY.Y]) == target_pos[(int)XY.X])) {
                dir = Dir_3_min_Distance(dir, target_pos)[0];
            }
        }

        return dir;
    }

    //与えられた方向centerから左右3方向をtargetに近い順番で返す
   public List<int> Dir_3_min_Distance(int center, int[] target_pos) {
        //3方向に移動した座標を計算
        int[] pos_center = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(center, 0));
        int[] pos_left = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(center, 1));
        int[] pos_right = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(center, 7));

        //それぞれの方向に進んだ時の目的地との距離を出す
        float dis_center = GameMaster.Instance.stage_Man.Tile_Distance(pos_center, target_pos);
        float dis_left = GameMaster.Instance.stage_Man.Tile_Distance(pos_left, target_pos);
        float dis_right = GameMaster.Instance.stage_Man.Tile_Distance(pos_right, target_pos);

        //3方向を距離が近い順にソートし、返す
        List<float> distance_list = new List<float> { dis_right, dis_left, dis_center };
        distance_list.Sort();
        List<int> dir = new List<int>();
        int d;
        for(int i = 0; i < distance_list.Count; i++) {
            if (distance_list[i] == dis_center) { d = chara_man.Center_To_Clock(center,0); }
            else if (distance_list[i] == dis_left) { d = chara_man.Center_To_Clock(center, 1); }
            else { d = chara_man.Center_To_Clock(center, 7); }
            dir.Add(d);
        }
        return dir;
    }
    //前方から左右3方向をtargetに近い順番に並び替えて返す
    public List<int> Dir_3_min_Distance(int[] target_pos) {

        //3方向に移動した座標を計算
        
        int[] pos_left = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(now_direction, 7));
        int[] pos_center = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(now_direction,0));
        int[] pos_right = GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(now_direction, 1));
     //   int[][] pos = { pos_left, pos_center, pos_right };

        //それぞれの方向に進んだ時の目的地との距離を出す
        float dis_left = GameMaster.Instance.stage_Man.Tile_Distance(pos_left, target_pos);
        float dis_center = GameMaster.Instance.stage_Man.Tile_Distance(pos_center, target_pos);
        float dis_right = GameMaster.Instance.stage_Man.Tile_Distance(pos_right, target_pos);
   //     float[] dis = { dis_left, dis_center, dis_right };

        //3方向を距離が近い順にソートし、返す
        List<float> distance_list = new List<float> { dis_right, dis_left, dis_center };
        distance_list.Sort();
        List<int> dir = new List<int>();
        int d;
        for (int i = 0; i < distance_list.Count; i++) {
            if (distance_list[i] == dis_center) { d = chara_man.Center_To_Clock(now_direction, 0); }//Debug.Log("方向:" + d + "距離:" + distance_list[i] + "center"); }
            else if (distance_list[i] == dis_left) { d = chara_man.Center_To_Clock(now_direction, 7); }// Debug.Log("方向:" + d + "距離:" + distance_list[i] + "left"); }
            else { d = chara_man.Center_To_Clock(now_direction, 1); }// Debug.Log("方向:" + d + "距離:" + distance_list[i] + "right"); }
           
            if (d== -1) { continue; }
            dir.Add(d);
        }
        return dir;

    }

    //与えられたキャラと同じ部屋にいるかどうかを返す
    public bool Charactor_same_Room(Character target_chara) {
        //引数のキャラがnullならfasle
        if (target_chara == null) { return false; }
        //自分のいる部屋を取得
        int now_room = GameMaster.Instance.stage_Man.int2_to_Room(now_position);
        //もしそもそも自分が部屋にいなければfalse
        if (now_room == -1) { return false; }
        //自分が部屋にいるとき
        else {
            return(now_room == GameMaster.Instance.stage_Man.int2_to_Room(target_chara.now_position));
        }
    }

    //受け取った地点までの移動路で、最短の道となる方角を返す
   public int Destination_Road(int[] target_point) {
        //目的地と現在地でXY、もしくは斜め方向に軸があっているとき
        if (target_point[(int)XY.X] == now_position[(int)XY.X]//X方向
            || target_point[(int)XY.Y] == now_position[(int)XY.Y]//Y方向
            || (now_position[(int)XY.X] + (target_point[(int)XY.Y] - now_position[(int)XY.Y]) == target_point[(int)XY.X])//斜め方向
            ) {
            //前方3方向(0,1,7)に進めるかを検証し、進めるか、キャラ方向であればその方角を返す
            for (int i = 0, count = 0; count < 3; count++, i++) {
                Can_Move_Tile a = chara_man.Can_Move_pos(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, chara_man.Center_To_Clock(now_direction, (i != 2 ? i : 7))));
                if (a == Can_Move_Tile.Can_Move || a == Can_Move_Tile.Chara) { return chara_man.Center_To_Clock(now_direction, (i != 2 ? i : 7)); }
            }
        }
        //目的地と現在地の軸があっていないとき
        else {
           // Debug.Log("非同軸");
            //向いている方向から3方向を目的地に近い順に並べ、それぞれ移動可能かどうかを判定
            List<int> dir_list = Dir_3_min_Distance(target_point);
            for (int i = 0; i < dir_list.Count; i++) {
              //  Debug.Log("候補:"+dir_list[i]) ;
                //進めるか、キャラ方向であればその方角を返す
                Can_Move_Tile a = chara_man.Can_Move_pos(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, dir_list[i]));
                if (a == Can_Move_Tile.Can_Move || a == Can_Move_Tile.Chara) { return dir_list[i]; }
            }
        }
        //もし目的地までの道が見つからない場合は左折の法則左折の法則
        return Dir_8_Law();
    }

    //左折の法則にしたがった方向を返す
   public int Dir_8_Law() {
        int[] count = new int[] { 0, 7, 1, 6, 2, 5, 3, 4 };
        //周囲8マスについて移動可能かどうかを判定
        for (int i = 0; i < count.Length; i++) {
            int next_dir = chara_man.Center_To_Clock(now_direction, count[i]);
            //移動可能ならそちらに移動させる
            if (chara_man.Can_Move_pos(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, next_dir)) == Can_Move_Tile.Can_Move) {
                return next_dir;
            }
        }
        //もしどこにも移動できなければその場にとどまる
        return 5;
    }
    //行動が送られてきたらその行動を実行する
    [PunRPC]
    protected void Act_Decision(int act, int dir,int now_item_id) {
        Direction_Change(dir);
            now_action = (Turn_Action)act;
            turn_act = true;
        //行動開始
        switch (now_action) {
            case Turn_Action.Move:
                Move_Start(now_direction);
                    break;
            case Turn_Action.Attack:
                Attack_Start(0, chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, now_direction)), Damage_Type.Attack,true);
                break;
            case Turn_Action.Item_Use:
                Use_Item(now_item_id);
                break;
            case Turn_Action.Item_Throw:
                Thorw_Item(now_item_id);
                break;
            case Turn_Action.Item_Drop:
                Item_Slot_Drop(now_item_id);
                break;
        }
    }
    //座標を全体で同期する
    public void Pos_Sync() {
        ph_view.RPC(nameof(Chara_pos_Update), RpcTarget.OthersBuffered, now_position[(int)XY.X], now_position[(int)XY.Y]);
    }
    //カメラ座標の更新
    public void Camera_pos_Update() {
        //戦闘シーンのときはカメラ注視中の場合のみカメラ位置を更新する
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle) == false ||
            (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle) && GameMaster.Instance.battle_Man.camera_chara == chara_num)) {
            Vector3 tmp_pos = new Vector3(transform.localPosition.x, transform.localPosition.y, GameMaster.Instance.chara_camera.transform.position.z);
            GameMaster.Instance.chara_camera.transform.position = tmp_pos + GameMaster.Instance.chara_cam_def;
            GameMaster.Instance.stage_camera.transform.position = tmp_pos + GameMaster.Instance.stage_cam_def;
        }
    }

    //このキャラの画面上の座標を返す
    public Vector3 Screen_Pos() {
        var pos = Vector2.zero;
        Vector3 screen_pos = RectTransformUtility.WorldToScreenPoint(GameMaster.Instance.chara_camera, transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GameMaster.Instance.UI_Canvas.GetComponent<RectTransform>(), screen_pos, GameMaster.Instance.ui_camera, out pos);
        return pos;
    }

    //行動が送られてきたらそれを実行するフラグを立てる
    [PunRPC]
    protected void Chara_pos_Update(int pos_x, int pos_y) {
            int[] pos=  Stage_Manager.Return_int2(pos_x, pos_y);
            Chara_pos_Update(pos);
    }
    //行動モーションが終わった旨を他プレイヤに伝える
    public void Act_End_Sync() {
        ph_view.RPC(nameof(Act_End), RpcTarget.All);
    }
    //アイテムスロットにランダムにアイテムを入手する
    public void Random_Item_Get(bool massage, Game_Now_Phase scene) {
        //引数のシーンで出現できるアイテムのリストを作成
        List<int> item_id_list = item_man.Scene_Spawn_Item_Id(scene);
        //出現できるアイテムが存在すればアイテムを入手
        if (item_id_list.Count > 0) {
            for (int i = 0; i < have_item_Max; i++) {
                item_man.Item_Get(chara_num, item_id_list[Random.Range(0, item_id_list.Count)], massage);
            }
            GameMaster.Instance.item_Man.Item_Slot_Update();
        }
    }
    //行動モーションが終わったフラグを増やす
    [PunRPC]
    protected  void Act_End() {
        GameMaster.Instance.battle_Man.action_end++;
    }
    //与えられた値に他プレイヤ上でのこのキャラのキャラ番号を変更
    public void Chara_num_Sync(int num) {
        GetComponent<PhotonView>().RPC(nameof(Change_Chara_Num), RpcTarget.All,num);
    }
    //与えられた値にキャラ番号を変更
    [PunRPC]
    protected void Change_Chara_Num(int num) {
        chara_num = num;
    }

}
