using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
public class Enemy : Character
{
    private int[] now_destination;//目的地

    //最後に通った出入口
    int[] now_door_way = new int[2];
    // Start is called before the first frame update
    void Start() {
        Ct_Start();
    }

    // Update is called once per frame
    void Update() {
        Ct_Update();
    }

    //ターン開始
    public override void Turn_Start() {
        //状態異常で動けなければスキップ
        if (Buf_Debuf_CanMove() == false) {
            now_action = Turn_Action.Skip;
            turn_act = true;
        }
        //攻撃できるなら攻撃フラグをオン
       else if (Attack_Bool()) {
            now_action = Turn_Action.Attack;
            turn_act = true;
        }
        //できなければ移動
        else {
            //移動する方向を指定
            int dir = Move_dir_AI();
            //方向を変える
            Direction_Change(dir);
            //移動
            Move_Start(dir);
           now_action= Turn_Action.Move;
            turn_act = true;
        }
        base.Turn_Start();
    }

    public override void Turn_End() {
        base.Turn_End();
    }

    //行動開始
    public override bool Turn_Update_Start() {
        now_anim_state = anim.GetCurrentAnimatorStateInfo(0);
        switch (now_action) {
            case Turn_Action.Attack:
                int at_dir = Attack_AI();
                Direction_Change(at_dir);
                Attack_Start(0, chara_man.Pos_To_Chara(GameMaster.Instance.stage_Man.Dir_Next_Pos(now_position, now_direction)), Damage_Type.Attack,true);
                break;
        }
        turn_act = true;
        return true;
    }

    //行動終了
    public override bool Turn_Update_End() {
        return base.Turn_Update_End();
    }
    //移動先を指定するアルゴリズム
    int Move_dir_AI() {
        int dir = 5;
        //もしプレイヤーと同じ部屋にいるならプレイヤーの座標を目的地にする
        if (Charactor_same_Room(chara_man.Player_chara)) {
            now_destination = chara_man.Player_chara.now_position;
            return Dirction_target(now_destination);
        }
        //目的地がない、もしくは到着したら新しい目的地探し
        else if (now_destination == null) {
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
        //目的地に着いたら目的地をnullにし、一歩進む
        else if (now_destination[0] == now_position[0] && now_destination[1] == now_position[1]) {
            now_destination = null;
            return Dir_8_Law();
        }
        //目的地があればその方向に向かう
        else if (now_destination != null) {
            //キャラを目的地のほうに向かせる
            Direction_Change(Dirction_target(now_destination));
            int d = Destination_Road(now_destination);
            return d;
        }
        //例外処理としてどこにも行けなければ左折の法則で動かす
        return Dir_8_Law();
    }
    //攻撃するか否かの判定
    bool Attack_Bool() {
        //プレイヤーの座標を取得
        int[] target_pos = chara_man.Player_chara.now_position;
        //プレイヤーと隣り合っていれば攻撃する
        return GameMaster.Instance.stage_Man.Near_Target_8(now_position,target_pos);
    }

    //攻撃先を指定するアルゴリズム
    int Attack_AI() {
        return Dirction_target(chara_man.Player_chara.now_position);
    }
}
