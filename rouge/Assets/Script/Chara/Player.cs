using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using Photon.Pun;

[System.Serializable]
public class Player : Character, IPunObservable {
    //参照用
    private Input_Manager in_man;


    public bool bef_item_window;
    public bool enemy_dir;//敵の方を一度向いたかどうか
                          
    //値が書き換えられた時に送受信される
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
            chara_name =(string)stream.ReceiveNext();
            level = (int)stream.ReceiveNext();
            max_HP = (int)stream.ReceiveNext();
            now_HP = (int)stream.ReceiveNext();
            max_Power = (int)stream.ReceiveNext();
            now_Power = (int)stream.ReceiveNext();
            EXP = (int)stream.ReceiveNext();
        }
    }
    void Start()
    {
        Ct_Start();
        in_man = GameMaster.Instance.input_Man;
        Item_Slot_Select();
    }

    // Update is called once per frame
    void Update() {
        //アイテム使用中でなければ
        if (GameMaster.Instance.item_Man.now_use_item_id == -1) {
            //スロットの選択
            if (in_man.Get_button_Down(INputers.IN_L)) { now_selected_item--; Item_Slot_Select(); }
            else if (in_man.Get_button_Down(INputers.IN_R)) { now_selected_item++; Item_Slot_Select(); }
        }
        Ct_Update();
    }

    public override void Turn_Start() {
        base.Turn_Start();
    }

    public override void Turn_End() {
        //このターンの行動が移動であるとき
        if (now_action == Turn_Action.Move) {
            //足元にアイテムがあれば拾う
            Get_Item();
        }
        base.Turn_End();
    }

    public override bool Turn_Update_Start() {
        //自分の操作キャラでないなら操作せずにそのまま返す
        if(GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle)&&GameMaster.Instance.online_Man.player_mine(this)==false) {
            return false;
        }
        Camera_pos_Update();
        //状態異常で動けなければスキップ
        if (Buf_Debuf_CanMove() == false) {
            Turn_Act_Decided(Turn_Action.Skip);
            return true;
        }
        now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        //もしダッシュ中なら、行動を再開するか否かの判定
        if (now_action == Turn_Action.Dash) {
            //前に進めるならそのまま進む
            if (Dash_Start(now_direction)) {
                Turn_Act_Decided(Turn_Action.Dash);
                return true;
            }
            //進めなければそこで停止し、ターン開始
            else {
                now_action = Turn_Action.Move;
                return false;
            }
        }

        //アイテム仕様画面のオンオフ
        {
            bool tmp_item_window = in_man.Get_button(INputers.IN_Item);
            //押された瞬間もしくは話された瞬間
            if (bef_item_window != tmp_item_window) {
                //押された瞬間かつアイテムがスロットにあれば使用画面をオンに
                if ((tmp_item_window == true)&& (have_Items[now_selected_item] != null)) {
                    item_man.Item_Use_Window_Start(true);
                    //メッセージウインドウを消して、代わりにアイテム説明ウインドウを開く
                    item_man.Item_Explain_Window(true, have_Items[now_selected_item]);
                }
                //離された瞬間オフに
                else if(tmp_item_window == false) {
                    //アイテム説明ウインドウを消し、代わりにメッセージウインドウを開く
                    item_man.Item_Explain_Window(false, null);
                    item_man.Item_Use_Window_Start(false);
                }
            }
            //アイテム仕様画面のオンオフフラグを記録
            bef_item_window = tmp_item_window;
        }
           

        //アイテム画面が開かれていなければその他の行動
        if (bef_item_window == false) {
            //攻撃
            {
                if (in_man.Get_button(INputers.IN_Attack)) {
                    Attack_Start(0,chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position,now_direction)), Damage_Type.Attack,true);
                    Turn_Act_Decided(Turn_Action.Attack);
                    return true;
                }
            }
            //方向転換キーが押されたらその周囲のキャラの方に向く
            if (in_man.Get_button(INputers.IN_Around) == true) {
                //敵の方を一度も向いていなければ(=方向転換キーが押された瞬間だけ)敵のほうを向く
                if (enemy_dir == false) {
                    //時計回りに敵がいるかを探知
                    for(int i = 0; i < 9; i++) {
                        if(chara_man.Can_Move_pos(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position,chara_man.Center_To_Clock(now_direction,i))) == Can_Move_Tile.Chara) {
                            Direction_Change(chara_man.Center_To_Clock(now_direction, i));
                            break;
                        }
                    }
                    enemy_dir = true;
                }
            }
            else {
                enemy_dir = false;
            }
            //移動
            {
                int dir = 5;
                //方向キーによって移動方向を指定
                if (in_man.Get_Move(true).y > 0) { dir -= 3; }
                else if (in_man.Get_Move(true).y < 0) { dir += 3; }
                if (in_man.Get_Move(true).x < 0) { dir -= 1; }
                else if (in_man.Get_Move(true).x > 0) { dir += 1; }
                //方向が決まったら移動処理
                if (dir != 5) {
                    //方向を変える
                    Direction_Change(dir);
                    //方向転換キーが押されていなければ移動、押されていれば方向転換のみで終了
                    if (in_man.Get_button(INputers.IN_Around) == false) {
                        //その先に移動可能なら進む
                        if (Dir_Move(dir)) {
                            //もし戦闘シーンでないかつダッシュキーが押されていたらダッシュし、押されていなければ通常移動
                            if (GameMaster.Instance.Now_Mode_Equal( Game_Now_Phase.Battle)==false&&in_man.Get_button(INputers.IN_Dash)) {
                                Move_Start(dir);
                                Turn_Act_Decided(Turn_Action.Dash);
                            }
                            else {
                                Move_Start(dir);
                                Turn_Act_Decided(Turn_Action.Move);
                            }
                            return true;
                        }
                    }
                    
                }

            }
            //ターン飛ばし
            {
                if (in_man.Get_button(INputers.IN_Skip)) {
                    Turn_Act_Decided(Turn_Action.Skip);
                    return true;
                }
            }
        }
        //アイテム画面が開かれているならアイテム用の行動
        else {
            //上ボタンでアイテムを落とす
            if (in_man.Get_Move(true).y > 0) {
                item_man.Item_Explain_Window(false, null);
                Turn_Act_Decided(Turn_Action.Item_Drop);
                if (have_Items[now_selected_item] != null) {
                    Item_Slot_Drop(have_Items[now_selected_item].item_id);
                }
                Item_Slot_Select();
                return true;
            }
            //左ボタンでアイテム使用
            if (in_man.Get_Move(true).x < 0) {
                //もし現在選択中のスロットが空でなければ
                if (have_Items[now_selected_item] != null) {
                    item_man.Item_Explain_Window(false, null);
                    Turn_Act_Decided(Turn_Action.Item_Use);
                    Use_Item(have_Items[now_selected_item].item_id);
                    Item_Slot_Select();
                    return true;
                }
            }
            //右ボタンでアイテム投げ
            else if (in_man.Get_Move(true).x > 0) {
                //もし現在選択中のスロットが空でなければ
                if (have_Items[now_selected_item] != null) {
                    item_man.Item_Explain_Window(false, null);
                    Turn_Act_Decided(Turn_Action.Item_Throw);
                    Thorw_Item(have_Items[now_selected_item].item_id);
                    Item_Slot_Select();
                    return true;
                }
            }

            //もしデバッグモードなら攻撃ボタンでアイテムを変更
            if (GameMaster.Instance.debug_mode_now && in_man.Get_button_Down(INputers.IN_Attack)) {
                if (have_Items[now_selected_item] != null) {
                    int id = have_Items[now_selected_item].item_id+1;
                    if (id >= item_man.Item_list.Count) { id = 0; }
                    have_Items[now_selected_item] = new Item(item_man.Item_list[id]);
                    item_man.Item_Slot_Update();
                }
            }
        }
        return false;
    }

    //行動確定後の処理
    public override bool Turn_Update_End() {
        Camera_pos_Update();
        //現在の行動に合わせた処理
        switch (now_action) {
            case Turn_Action.Dash:
                Dash_Update();
                return true;
        }
        return base.Turn_Update_End();
    }
    //ダッシュ中に呼ばれる関数
    public bool Dash_Update() {
        transform.localPosition = GameMaster.Instance.stage_Man.Tile_To_World(now_position) + chara_man.pos_def;
            Camera_pos_Update();
        return true;
    }
    //アイテムを選択する処理
   public void Item_Slot_Select() {
        //自分のキャラでなければ即選択処理をやめる
        if(GameMaster.Instance.online_Man.player_mine(this) == false) { return; }
        //選択番号を範囲内に収める
        List<GameObject> slot_pal = GameMaster.Instance.item_Man.item_slot_parent;
        if (now_selected_item < 0) { now_selected_item = slot_pal.Count - 1; }
        else if (slot_pal.Count - 1 < now_selected_item) { now_selected_item = 0; }
        //選択中のスロットはちょい大きめに、他は小さめに
        for (int i = 0; i < slot_pal.Count; i++) {
            if (i == now_selected_item) { slot_pal[i].transform.localScale = Vector3.one * 1.2f; }
            else { slot_pal[i].transform.localScale = Vector3.one; }
        }
    }
    //与えられた座標をdir方向を向いて左右の状況を返す
    Tile_Type[] Dash_LR_Update(int dir, int[] pos) {
        Tile_Type[] a = new Tile_Type[2];
        int[] LR_pos;
        try {
            LR_pos = GameMaster.Instance.stage_Man.Dir_Next_Pos(pos, chara_man.Center_To_Clock(dir, 2));
            a[(int)LR.Right] = GameMaster.Instance.stage_Man.stage[LR_pos[0], LR_pos[1]].tile;
        }
        catch (System.IndexOutOfRangeException) {
            a[(int)LR.Right] = Tile_Type.Unbreakable_Wall;
        }
        try {
            LR_pos = GameMaster.Instance.stage_Man.Dir_Next_Pos(pos, chara_man.Center_To_Clock(dir, 6));
            a[(int)LR.Left] = GameMaster.Instance.stage_Man.stage[LR_pos[0], LR_pos[1]].tile;
        }
        catch (System.IndexOutOfRangeException) {
            a[(int)LR.Right] = Tile_Type.Unbreakable_Wall;
        }
        return a;
    }

    //引数の座標にダッシュ可能かどうかを返し、移動できるなら移動させる
    bool Dash_Start(int dir) {
        //現在の左右状況と、1歩先のマスの左右状況を取得
        Tile_Type[] Dash_tile_LR = Dash_LR_Update(dir, now_position);
        Tile_Type[] now_tile = Dash_LR_Update(dir, GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, dir));
        //状況に変化があるならfalse、ただし例外アリ
        //例外:(部屋内にいて、次のマスが出入口でない)場合はfalseしない
        if (!(Dash_tile_LR[0]== now_tile[0]&& Dash_tile_LR[1] == now_tile[1])) {
            if ((GameMaster.Instance.stage_Man.int2_to_Room(now_position) != -1
                &&
                GameMaster.Instance.stage_Man.Near_Target_4(now_position, GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, dir)) == false) == false) {
                return false;
            }
        }
        //敵に隣接していればfalse
        for (int i = 0; i < chara_man.chara_list.Count; i++) {
            if (chara_man.chara_list[i].tag == "Player") { continue; }
           if(GameMaster.Instance.stage_Man.Near_Target_8(now_position, chara_man.chara_list[i].now_position)){ 
                return false;
            }
        }
        //アイテムに隣接していればfalse
        for (int i = 0; i < GameMaster.Instance.item_Man.on_stage_items.Count; i++) {
            if (GameMaster.Instance.stage_Man.Near_Target_8(now_position, GameMaster.Instance.item_Man.on_stage_items[i].item_pos)) {
                return false;
            }
        }
        //階段に隣接していればfalse
        if (Dir_8_Stair()) {
            return false;
        }
        //最後にマスが移動可能かどうかを調べ、移動できるなら座標を更新
        return Move_Start(dir);

        //周囲8方向に階段があるかどうかを返す
        bool Dir_8_Stair() {
            return ((GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y], now_position[(int)XY.X]].tile == Tile_Type.Next_Stage)
            || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y] + 1, now_position[(int)XY.X]].tile == Tile_Type.Next_Stage)
             || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y] - 1, now_position[(int)XY.X]].tile == Tile_Type.Next_Stage)
              || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y], now_position[(int)XY.X] + 1].tile == Tile_Type.Next_Stage)
               || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y], now_position[(int)XY.X] - 1].tile == Tile_Type.Next_Stage)
               || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y]-1, now_position[(int)XY.X] - 1].tile == Tile_Type.Next_Stage)
               || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y]+1, now_position[(int)XY.X] - 1].tile == Tile_Type.Next_Stage)
               || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y]-1, now_position[(int)XY.X] + 1].tile == Tile_Type.Next_Stage)
               || (GameMaster.Instance.stage_Man.stage[now_position[(int)XY.Y]+1, now_position[(int)XY.X] + 1].tile == Tile_Type.Next_Stage)
            );
        }
    }

    //ミニマップに現在プレイヤーがいるマスを表示化する処理(マップ表示処理とは別物)
    public void Mapping_Update() {
        Stage_Manager stage_man = GameMaster.Instance.stage_Man;
        //今いるマスからmapping_distanceの距離内のマスを表示化
        int start_y = now_position[(int)XY.Y] - stage_man.mapping_distance;
        start_y = 0 > start_y ? 0 : start_y;
        int start_x = now_position[(int)XY.X] - stage_man.mapping_distance;
        start_x = 0 > start_x ? 0 : start_x;
        for (int y = start_y; y <= now_position[(int)XY.Y] + stage_man.mapping_distance&&y< stage_man.Now_Height_Max; y++) {
            for (int x = start_x; x <= now_position[(int)XY.X] + stage_man.mapping_distance && x < stage_man.Now_Width_Max; x++) {
                stage_man.stage[y, x].mapping = true;
            }
        }
        //現在いる部屋番号を取得
        int room_num = stage_man.int2_to_Room(now_position);
        //現在部屋内にいてかつ、部屋がまだマップに表示されていなければ全て表示化する
        if (room_num != -1 && stage_man.Room_List[room_num].mapping == false) {
            //部屋内
            for (int y = stage_man.Room_List[room_num].Room_start_point[(int)XY.Y]; y <= stage_man.Room_List[room_num].Room_end_point[(int)XY.Y]; y++) {
                for (int x = stage_man.Room_List[room_num].Room_start_point[(int)XY.X]; x <= stage_man.Room_List[room_num].Room_end_point[(int)XY.X]; x++) {
                    stage_man.stage[y, x].mapping = true;
                }
            }
            //出入口
            for (int i = 0; i < stage_man.Room_List[room_num].room_doorway.Count; i++) {
                stage_man.stage[stage_man.Room_List[room_num].room_doorway[i][(int)XY.Y], stage_man.Room_List[room_num].room_doorway[i][(int)XY.X]].mapping = true;
            }
            //マップ化済みフラグを立てる
            stage_man.Room_List[room_num].mapping = true;
        }
    }
}
