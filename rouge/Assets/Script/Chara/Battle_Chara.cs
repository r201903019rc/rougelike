using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using Photon.Pun;

public class Battle_Chara : Character, IPunObservable {
    private int[] now_destination;//目的地
    private Character target_chara;//攻撃対象のキャラ
    private List<int> use_item_list=new List<int>();//使う候補となるアイテム 
    [Range(0,100)]
    public int item_use_probability=50;//アイテムが使える状況でのアイテム使用率、高い方がアイテムを使いやすい
    [Range(0, 100)]
    public int item_or_target = 50;//アイテムを拾いに行くか敵に近づくかの率、低ければアイテムへ高ければ敵へ近づきやすい
    //最後に通った出入口
    int[] now_door_way = new int[2];

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(chara_name);
            stream.SendNext(level);
            stream.SendNext(max_HP);
            stream.SendNext(now_HP);
            stream.SendNext(max_Power);
            stream.SendNext(now_Power);
            stream.SendNext(EXP);
        }
        else {
            chara_name = (string)stream.ReceiveNext();
            level = (int)stream.ReceiveNext();
            max_HP = (int)stream.ReceiveNext();
            now_HP = (int)stream.ReceiveNext();
            max_Power = (int)stream.ReceiveNext();
            now_Power = (int)stream.ReceiveNext();
            EXP = (int)stream.ReceiveNext();
        }
    }
    void Start() {
        Ct_Start();
        
    }

    // Update is called once per frame
    void Update() {
        Ct_Update();
    }

    //ターン開始
    public override void Turn_Start() {
        //自分の操作キャラでないなら操作せずにそのまま返す
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle) && GameMaster.Instance.online_Man.room_owner == false) {
            return;
        }
        //現在のターゲットを選択
        Target_Change();
        //使用アイテムのリストを初期化
        use_item_list.Clear();
        //状態異常で動けなければスキップ
        if (Buf_Debuf_CanMove() == false) {
            Turn_Act_Decided(Turn_Action.Skip);
        }
        //アイテムを使うか
        else if (Item_Bool()) {
            //相手に向き直す
            Direction_Change(Dirction_target(target_chara.now_position));
            Item_Use();
        }
        //攻撃できるなら攻撃フラグをオン
        else if (Attack_Bool()) {
            //攻撃する方向に向き直す
            Direction_Change(Dirction_target(target_chara.now_position));
            Turn_Act_Decided(Turn_Action.Attack);
        }
        //できなければ移動
        else {
            //移動する方向を指定
            int dir = Move_dir_AI();
            //方向を変える
            Direction_Change(dir);
            //移動
            Move_Start(dir);
            Turn_Act_Decided(Turn_Action.Move);
        }
        base.Turn_Start();
    }

    //行動開始
    public override bool Turn_Update_Start() {
        //自分の操作キャラでないなら操作せずにそのまま返す
        if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle) && GameMaster.Instance.online_Man.room_owner == false) {
            return false;
        }
        now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        switch (now_action) {
            case Turn_Action.Attack:
                Attack_Start(0, chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, now_direction)), Damage_Type.Attack,true);
                break;
        }
        turn_act = true;
        return true;
    }

    //行動中
    public override bool Turn_Update_End() {
        return base.Turn_Update_End();
    }

    //行動終了
    public override void Turn_End() {
        //このターンの行動が移動であるとき
        if (now_action == Turn_Action.Move) {
            //足元にアイテムがあれば拾う
            Get_Item();
        }
        base.Turn_End();
    }
    //狙うキャラを決めるアルゴリズム
    void Target_Change() {
        //全キャラのリスト
        List<Character> chara_list = GameMaster.Instance.battle_Man.players;
        //狙うための関心度
        float chara_interest;
        int tmp_charanum= 0;
        float tmp_interest=0;
        for (int i = 0; i < chara_list.Count; i++) {
            //もしそのキャラが自分自身、もしくは既に死んでいたら関心度0
            if (chara_list[i] == this || chara_list[i] == null) {
                chara_interest = 0;
            }
            else {
                //自分とそのキャラの距離を出す
                float distance =1/ GameMaster.Instance.stage_Man.Tile_Distance(now_position, chara_list[i].now_position);
                //そのキャラの残りHP
                float Rem_HP = chara_list[i].now_HP;
                //そのキャラを動かしているのが人なのかどうか
                bool isPlayer = (chara_list[i].tag == "Player");
                //直線上にいるかどうか
                bool arrow= chara_man.Pos_To_Arrow_Chara(chara_num, tmp_charanum);
                //キャラの関心度を出す
                chara_interest = (distance*10 + (Rem_HP * 0.5f)) * (isPlayer ? 1.1f : 1)*(arrow?2:1);
            }
            if (tmp_interest < chara_interest) { tmp_charanum = i; tmp_interest = chara_interest; }
        }
        //関心度が最も高かったキャラを現在のターゲットに変更
        target_chara = chara_list[tmp_charanum];
    }

    //移動先を指定するアルゴリズム
    int Move_dir_AI() {
        int dir = 5;
        //近くのアイテムの位置
        int[] near_item_pos = Near_Stage_Item_Pos();
        //ターゲットのキャラの位置
        int[] near_target_pos = target_chara.now_position;
        //アイテムを拾いに行く最低条件、床落ちアイテムが存在し手持ちに空きがある
        bool item_ok = (Have_Item_Num() < have_item_Max && item_man.on_stage_items.Count > 0);
        //敵の方に移動する最低条件、ターゲットとなるキャラと同じ部屋にいる
        bool target_ok = Charactor_same_Room(target_chara);

        now_destination = null;
        //アイテムが拾えず、敵がいるなら必ず敵方向へ
        if (item_ok==false&&target_ok==true) {
                now_destination = near_target_pos;
        }
        //アイテムが拾え、敵がいないなら必ずアイテム方向へ
        else if (item_ok == true && target_ok == false) {
            now_destination = near_item_pos;
        }
        //どちらにも行けるなら閾値で計算
        else if (item_ok == true && target_ok == true) {
            float target_num=  GameMaster.Instance.stage_Man.Tile_Distance(now_position, near_target_pos)*(item_or_target);
            float item_num = GameMaster.Instance.stage_Man.Tile_Distance(now_position, near_item_pos)*(have_item_Max+0.5f) * Mathf.Max(100-item_or_target,0);
            now_destination = (target_num <= item_num ? near_target_pos:near_item_pos) ;
        }
        //どれにも当てはまらなければ目的地なしnull
        else {
            now_destination = null;
        }

        //目的地があればその方向に向かう
        if (now_destination != null) {
            //キャラを目的地のほうに向かせる
            Direction_Change(Dirction_target(now_destination));
            int d = Destination_Road(now_destination);
            return d;
        }
        //目的地がなければ新しい目的地を探す
        else {
            int now_room = GameMaster.Instance.stage_Man.int2_to_Room(now_position);
            //廊下にいるとき
            if (now_room == -1) {
                //左折の法則に従って動く
                return Dir_8_Law();
            }
            //部屋にいるとき
            else {
                //部屋内の出入り口のリストを作成
                List<int[]> door_way_list = new List<int[]>();
                door_way_list.AddRange(GameMaster.Instance.stage_Man.Room_List[now_room].room_doorway);

                //今いる地点が出入口なら、その出入口をリストから除外する
                for (int i = 0; i < door_way_list.Count; i++) {
                    if (GameMaster.Instance.stage_Man.Near_Target_8(door_way_list[i], now_position)) {
                        now_door_way = door_way_list[i];
                        door_way_list.RemoveAt(i);
                        break;
                    }
                }

                //出入口があれば、その中から一つを選び、そちらへ移動
                if (door_way_list.Count > 0) {
                    now_destination = door_way_list[Random.Range(0, door_way_list.Count)];
                    dir = Dirction_target(now_destination);
                }
                //出入口が無ければ現在いる出入口に戻る、出入口がないなら左折の法則で移動
                else {
                    if (now_door_way != null) {
                        now_destination = now_door_way;
                        dir = Dirction_target(now_destination);
                    }
                    else {
                        dir = Dir_8_Law();
                    }
                }
            }
            return dir;
        }
    }
    //床落ちアイテムのうち、一番近いものの座標を返す
    int[] Near_Stage_Item_Pos() {
        int min_count=-1;
        float min_distance =GameMaster.Instance.stage_Man.Now_Width_Max* GameMaster.Instance.stage_Man.Now_Height_Max;
        float tmp;
         for (int i = 0; i < item_man.on_stage_items.Count; i++) {
            tmp = GameMaster.Instance.stage_Man.Tile_Distance(item_man.on_stage_items[i].item_pos, now_position);
            if (tmp < min_distance) {
                min_count = i;
                min_distance = tmp;
            }
        }
        if (min_count == -1) { return null; }
        else {
            return item_man.on_stage_items[min_count].item_pos;
        }
    }

    //攻撃するか否かの判定
    bool Attack_Bool() {
        //狙うキャラの座標を取得
        int[] target_pos = target_chara.now_position;
        //狙うキャラと隣り合っていれば攻撃する
        bool a = GameMaster.Instance.stage_Man.Near_Target_8(now_position, target_pos);
        return a;
    }
 
    //アイテムを使うか否かの判定
    bool Item_Bool() {
        bool a = false;
        //自分に使うか
        //もし所持アイテムに自分にメリットを及ぼすものがあればそれを使用候補に
       a |=Use_Item_Flag(ITEM_MERIT_TARGET.MINE);
        //敵に使うか
        //もし狙うキャラが直線状にいるなら
        if (chara_man.Pos_To_Arrow_Chara(chara_num, target_chara.chara_num)) {
            //もし所持アイテムに敵へデメリットを及ぼすものがあればそれを使用候補に
            a |= Use_Item_Flag(ITEM_MERIT_TARGET.ANY);
        }
        //アイテムを使えるなら、使うかどうかの確率に通す
        if (a) {
            a=(item_use_probability<=Random.Range(0, 100));
        }
        return a;
    }
    //使用アイテムの候補をリストに入れる、自分にメリットがあるものを探知したければtrue、敵にデメリットがあるものならfalse、1つでも候補が存在したらtrueを返す
    bool Use_Item_Flag(ITEM_MERIT_TARGET merit) {
        bool tmp= false;
        for (int i = 0; i < have_item_Max; i++) {
            if (have_Items[i] != null) {
                if (have_Items[i].target_type == merit) {
                    use_item_list.Add(i);
                    tmp = true;
                }
            }
        }
        return tmp;
    }
    //アイテムの使用処理
    void Item_Use() {
        //使用するアイテムをランダムに決定
        now_selected_item = use_item_list[Random.Range(0, use_item_list.Count)];
        //アイテムを使うならtrue、投げるならfalse
        bool use_flag=true;
        
        //アイテムを使うか、投げるかを判定
        switch (have_Items[now_selected_item].Item_type) {
            case Item_Type.Sword:
                //隣り合っているなら普通に使い、隣り合っていなければ投げる
                use_flag = GameMaster.Instance.stage_Man.Near_Target_8(now_position, target_chara.now_position);
                break;
            default:
                //自分に効果があるなら使い、敵に効果があるなら投げる
                use_flag = have_Items[now_selected_item].target_type == ITEM_MERIT_TARGET.MINE;
                break;
        }
        //アイテムの使用処理
        //使うとき
        if (use_flag) {
            Use_Item(have_Items[now_selected_item].item_id);
            Turn_Act_Decided(Turn_Action.Item_Use);
        }
        //投げるとき
        else {
            Thorw_Item(have_Items[now_selected_item].item_id);
            Turn_Act_Decided(Turn_Action.Item_Throw);
        }
    }
}
