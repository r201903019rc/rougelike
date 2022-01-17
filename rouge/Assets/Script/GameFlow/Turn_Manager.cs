using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;

public class Turn_Manager : MonoBehaviour
{
  private  Charactor_Manager chara_man;
    private Battle_Manager bt_man;
    List<bool> chara_act_end = new List<bool>();//このターンのキャラの動きが終了していればtrue,していなければfalse
    List<bool> chara_act_start = new List<bool>();//このターンのキャラの動きが開始していればtrue,していなければfalse

   public TURN_NOW turn_start;//ターン開始時の処理がすでに走ったかどうかのフラグ

   public enum TURN_NOW {
        START,UPDATE,CHARA_END,TURN_END,TURN_NEXT
    }
    // Start is called before the first frame update
    void Start()
    {
        chara_man = GameMaster.Instance.chara_Man;
        bt_man = GameMaster.Instance.battle_Man;
        GameMaster.Instance.now_player_turn = Turn.Non;
    }

    // Update is called once per frame
    void Update() {
        if (GameMaster.Instance.Now_Mode_Equal( Game_Now_Phase.Dangeon)) {
            Dangeons_Turn_Update();
        }
        else if(GameMaster.Instance.Now_Mode_Equal( Game_Now_Phase.Battle)) {
            Battles_Turn_Update();
        }
    }
    void Dangeons_Turn_Update() {
        if (GameMaster.Instance.now_player_turn == Turn.Player) {
            //ターン開始処理
            if (turn_start == TURN_NOW.START) {
                chara_man.Player_chara.Turn_Start();
                //ターンフラグを更新中に
                turn_start = TURN_NOW.UPDATE;
                //キャラの行動リストを更新
                Action_list_Update();
            }
            //ターン中
            else if (turn_start == TURN_NOW.UPDATE) {
                //ターン内の処理
                if (chara_man.Player_chara.turn_act == false) {//行動決定前
                    chara_man.Player_chara.Turn_Update_Start();
                }
                else {//行動決定後
                    if (chara_man.Player_chara.now_action == Turn_Action.Move) {
                        //ターンフラグを終了に
                        turn_start = TURN_NOW.CHARA_END;
                    }
                    else {
                        if (chara_man.Player_chara.Turn_Update_End()) {
                            chara_act_end[chara_man.Player_chara.chara_num] = true;
                            chara_man.Player_chara.Turn_End();
                            //ターンフラグを終了に
                            turn_start = TURN_NOW.CHARA_END;
                        }
                    }
                }
            }
            //ターン終了処理
            else if (turn_start == TURN_NOW.CHARA_END) {
                //敵にターンを渡す
                GameMaster.Instance.now_player_turn = Turn.Enemy;
                //ターンフラグを初期化
                turn_start = TURN_NOW.START;
            }
        }
        else if (GameMaster.Instance.now_player_turn == Turn.Enemy) {
            //turn_startがfalseならターン開始処理
            if (turn_start == TURN_NOW.START) {
                Enemys_Start();
                //ターンフラグを更新中に
                turn_start = TURN_NOW.UPDATE;
            }
            //ターン中
            else if (turn_start == TURN_NOW.UPDATE) {
                //ターン内の処理
                Enemys_Update();
                //ターン内の処理が終わったら終了処理
                if (Enemys_Turn_End() == true) {
                    //ターンフラグを終了に
                    turn_start = TURN_NOW.CHARA_END;
                }
            }
            //ターン終了処理
            else if (turn_start == TURN_NOW.CHARA_END) {
                //ターン終了処理
                Turn_End();
                //ターンフラグを初期化
                turn_start = TURN_NOW.START;
            }
        }
        //敵たちの処理開始
        void Enemys_Start() {
            //敵の行動開始
            for (int i = 0; i < chara_man.chara_list.Count; i++) {
                //プレイヤーはすでに行動済みなので飛ばす
                if (chara_man.chara_list[i].tag == "Player") { chara_act_start[i] = true; continue; }
                chara_man.chara_list[i].Turn_Start();
            }
        }
        //敵たちの処理中
        void Enemys_Update() {
            for (int i = 0; i < chara_man.chara_list.Count; i++) {
                //もしすでに行動終了済みなら飛ばす
                if (chara_act_end[i] == true) { continue; }
                //プレイヤーのとき
                if (chara_man.chara_list[i].tag == "Player") {
                    //移動中なら処理を続行
                    if (chara_man.Player_chara.now_action == Turn_Action.Move) {
                        chara_act_end[i] = chara_man.Player_chara.Turn_Update_End();
                        if (chara_act_end[i] == true) {
                            chara_man.Player_chara.Turn_End();
                        }
                    }
                }
                //敵の場合は行動
                else {
                    //移動処理ならば問答無用で行動させる
                    if (chara_man.chara_list[i].now_action == Turn_Action.Move) {
                        chara_act_start[i] = true;
                        chara_act_end[i] = chara_man.chara_list[i].Turn_Update_End();
                    }
                    //まだ行動が開始していなければ行動開始判定
                    else if (chara_act_start[i] == false) {
                        int count = 0;
                        //既にキャラ番号が自分より若い全員の行動が終了しているなら自分も行動開始
                        {
                            for (int j = 0; j < i; j++) {
                                if (chara_act_end[j]) { count++; }
                            }
                            if (count == i) { chara_act_start[i] = chara_man.chara_list[i].Turn_Update_Start(); }
                        }
                    }
                    //行動開始済みなら終了までの処理を行う
                    else {
                        chara_act_end[i] = chara_man.chara_list[i].Turn_Update_End();
                    }
                    //行動が終了したら終了処理を走らせる
                    if (chara_act_end[i] == true) {
                        chara_man.chara_list[i].Turn_End();
                    }
                }
            }
        }
        //ターン終了
        void Turn_End() {
            //階段が足元にあれば次の階へ
            if (GameMaster.Instance.stage_Man.stage[chara_man.Player_chara.now_position[(int)XY.Y], chara_man.Player_chara.now_position[(int)XY.X]].tile == Tile_Type.Next_Stage) {
                GameMaster.Instance.music_Man.PlaySE(GameMaster.Instance.music_Man.SE_Stair);
                GameMaster.Instance.dan_Man.Next_Floor();
            }
            //なければターンをプレイヤーに回す
            else {
                GameMaster.Instance.now_player_turn = Turn.Player;
                //ミニマップとアイテムの描画
                GameMaster.Instance.stage_Man.Map_Update();
                //プレイヤーのステータスの更新
                GameMaster.Instance.chara_Man.Status_UI_Update();
            }
            //ターン数を増加し、一定のターン数が溜まったら敵を追加
            GameMaster.Instance.dan_Man.now_turn_count++;
            if (GameMaster.Instance.dan_Man.now_turn_count % GameMaster.Instance.dan_Man.enemy_spown_turn == 0) {
               Character add_enemy= chara_man.Dan_Enemy_Add();
                add_enemy.now_position = GameMaster.Instance.stage_Man.Random_Room_Pos(false, false);
            }
        }
    
        //全員のUpdateが終了しているか否か
        bool Enemys_Turn_End() {
            int count = 0;
            for (int i = 0; i < chara_man.chara_list.Count; i++) {
                //プレイヤーが移動以外の時、もしくは行動が終わったらcountを追加
                if (chara_act_end[i] == true) { count++; }
            }
            return (count == chara_man.chara_list.Count);
        }
    }
    void Battles_Turn_Update() {
        //フェードアウトが明けていればターン開始
        if (GameMaster.Instance.now_fade_type == Fade_Type.White) {
            //カメラ位置を更新
            if (bt_man.players[bt_man.now_turn_chara] == null) {
                bt_man.players[bt_man.camera_chara].Camera_pos_Update();
            }
            //ターン開始処理
            if (turn_start == TURN_NOW.START) {
                //既に該当のキャラが倒されていたら飛ばす
                if (bt_man.players[bt_man.now_turn_chara]==null) {
                    chara_num_inc();
                }
                //デバッグモードがオンならプレイヤー以外の行動を飛ばす
                else if(GameMaster.Instance.debug_mode_now&& bt_man.now_turn_chara != chara_man.Player_chara.chara_num) {
                    chara_num_inc();
                }
                else {
                    bt_man.second = 0;
                    bt_man.players[bt_man.now_turn_chara].Turn_Start();
                    //ターンフラグを更新中に
                    turn_start = TURN_NOW.UPDATE;
                }
            }
            //ターン中
            else if (turn_start == TURN_NOW.UPDATE) {
                //ターン内の処理
                if (bt_man.players[bt_man.now_turn_chara].turn_act == false) {//行動決定前
                    //時間切れになったら次の人へ強制移行
                    if (bt_man.Time_Limit_second < (bt_man.second)) {
                        turn_start = TURN_NOW.CHARA_END;
                    }
                    bt_man.players[bt_man.now_turn_chara].Turn_Update_Start();
                }
                else {//行動決定後
                    if (bt_man.players[bt_man.now_turn_chara].Turn_Update_End()) {
                        chara_act_end[chara_man.Player_chara.chara_num] = true;
                        //ターンフラグを終了に
                        turn_start = TURN_NOW.CHARA_END;
                    }
                }
            }
            //ターン終了処理
            else if (turn_start == TURN_NOW.CHARA_END) {
                bt_man.players[bt_man.now_turn_chara].Turn_End();
                //アイテム描画、ステータスUIの更新
                GameMaster.Instance.stage_Man.Item_view();
                GameMaster.Instance.chara_Man.Status_UI_Update();
                //ターン終了後の処理へ
                turn_start = TURN_NOW.TURN_END;
                
            }
            else if (turn_start == TURN_NOW.TURN_END) {
                //全プレイヤーの画面上で動きが終わったら
                if (GameMaster.Instance.battle_Man.action_end == GameMaster.Instance.online_Man.room_menber_count) {
                    //オンライン上で座標を同期
                    if (GameMaster.Instance.Now_Mode_Equal(Game_Now_Phase.Battle) &&
                        (bt_man.players[bt_man.now_turn_chara] .tag=="Player"&& GameMaster.Instance.online_Man.player_mine(bt_man.players[bt_man.now_turn_chara]))
                        || (bt_man.players[bt_man.now_turn_chara].tag == " Battle_Enemy" && GameMaster.Instance.online_Man.room_owner)) {
                        bt_man.players[bt_man.now_turn_chara].Pos_Sync();
                    }
                    GameMaster.Instance.battle_Man.action_end = 0;
                    //次のキャラにターンを遷移
                    chara_num_inc();
                    //アイテム出現ターンの開始時、アイテムを出現させる
                    if ((bt_man.now_turn % bt_man.item_frequency_turn) == 0 && bt_man.now_turn_chara == 0) {
                        bt_man.New_Item_Drop(GameMaster.Instance.stage_Man.Random_Room_Pos_Item());
                    }
                }
            }
        }
        

        
        //次のキャラにターンを譲る処理
        void chara_num_inc() {
            turn_start = TURN_NOW.TURN_NEXT;
            //もし決着がついていなければ進む
            if (bt_man.Battle_end() == false){
                //現在のターン数を更新
                GameMaster.Instance.online_Man.Turn_Add();
                //座標を同期し直す
                bt_man.Players_Pos_Update();
            }
        }
    }

    void Action_list_Update() {
        //敵が増えたり減ったりする可能性もあるので毎ターン要素数をリセット
        chara_act_end.Clear();
        chara_act_start.Clear();
        for (int i = 0; i < chara_man.chara_list.Count; i++) {
            chara_act_end.Add(false);
            chara_act_start.Add(false);
        }
    }

    //ターンの初めに戻す
  public void Turn_Reset() {
        turn_start = TURN_NOW.START;
        GameMaster.Instance.now_player_turn = Turn.Non;
    }
}
