using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;
using UnityEngine.UI;

public class Battle_Manager : MonoBehaviour {
    //参照用
    private Stage_Manager stage_man;
    private Charactor_Manager chara_man;

    public int battle_start_flag;//戦闘開始準備が終わったプレイヤーがどれだけいるかをカウント
    //各プレイヤの順位リスト
    public int[] ranking;

    public int camera_chara;//現在カメラが追っているキャラ(プレイヤーが生きているうちは原則プレイヤー)
    
    public float paramater_time = 5f;//戦闘スタート時のステータス表示を何秒やるか
    private ParameterView parameter_view;//戦闘スタート時のステータス表示用

    [Header("ステージ全体関係")]
    private Map_Tile[,] stage;//ステージのマス目情報
    public int Width_Max = 10;//横方向のマス数
    public int Height_Max = 10;//縦方向のマス数
    private bool stage_reset_flag;//ステージ初期化フラグ
    [System.NonSerialized]
    public STAGE_CRIATE stage_criate_flag;//ステージ作成フラグ
    public int battle_start_item = 3;//初期床落ちアイテムの数
    [Range(0,100)]
    public int item_drop_prob = 50;//キャラが倒されたときのアイテムのドロップ確率
    public int item_frequency_turn = 3;//アイテムの出現頻度

    [Header("キャラ関係関係")]
    [System.NonSerialized]
    public List<Character> players = new List<Character>();//プレイヤーのリスト
    public const int Chara_Max = 4;//キャラ上限

    [Header("ターン管理関係")]
    public int now_turn;//全キャラが1度ずつ動いたら1ターンとしてターン数をカウント
    public int now_turn_chara;//現在捜査しているプレイヤーの番号
    public Text Turn_Text;//現在操作しているプレイヤーが何Pなのかを表示
    public int action_end;//各プレイヤの画面でキャラの動きが終わったなら1加算される

    [Header("時間関係")]
    public Slider Time_Slider;//残り時間表示用スライダー
    public Image Time_gauge;//残り時間のスライダーの画像
    public int Time_Limit_second; //制限時間
    [System.NonSerialized]
    public float second = 0;//現在の秒

    [System.NonSerialized]
    public int sorted;//キャラ番号の同期が終わった人数
    // Start is called before the first frame update
    void Start()
    {
        stage_man = GameMaster.Instance.stage_Man;
        chara_man = GameMaster.Instance.chara_Man;

        now_turn = 0;
        now_turn_chara = 0;
        stage_reset_flag = false;
        ranking= new int[Chara_Max];
    }
    
    //シーン遷移後最初のフレームで呼ばれる処理
    public void Stage_Start() {
        //  GameMaster.Instance.online_Man.Scene_Changed();
        stage_reset_flag = false;
        stage_man.Stage_Reset(Height_Max, Width_Max);
        stage_man.Mini_Reset();
        stage = stage_man.stage;
        Turn_Text.gameObject.SetActive(true);
        Turn_Text_Update();
        Time_Slider.value = 1.0f;
        //BGMを再生
        GameMaster.Instance.music_Man.PlayBGM(GameMaster.Instance.music_Man.BGM_Battle, GameMaster.Instance.music_Man.fade_speed);
        parameter_view= GameObject.FindWithTag("Parameter_View").GetComponent<ParameterView>();
    }
    public void Stage_End() {
        GameMaster.Instance.online_Man.Room_Disconnect();
        Turn_Text.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        //ステージ作成フラグが立っていればフェードアウト、フェードイン
        if (stage_criate_flag != STAGE_CRIATE.CRIATE_END) {
            //ステージ作成開始
            if (stage_criate_flag == STAGE_CRIATE.CRIATE_WAIT) {
                if (stage_reset_flag == false) {
                    stage_reset_flag = true;
                    Battle_Reset();
                }
            }
            //画面が明るくなり、ステージ作成が終わっていれば開始
            else if (GameMaster.Instance.now_fade_type == Fade_Type.White && stage_criate_flag == STAGE_CRIATE.CRIATE_NOW) {
                stage_criate_flag = STAGE_CRIATE.CRIATE_END;
                //操作開始
                GameMaster.Instance.now_player_turn = Turn.Player;
            }
        }
        //ステージ作成が終わり、ゲーム進行中
        else {
            //フェードインアウトしておらず、残りキャラが一人になったらタイトルへ
            if (GameMaster.Instance.now_fade_type == Fade_Type.White && Battle_end()) {
                //残ったキャラの順位を1に
                for(int i = 0; i < players.Count; i++) {
                    if (players[i] != null&&ranking[i]==0) {
                        GameMaster.Instance.online_Man.RPC_view.Send_Ranking_Sync(i, 1);
                    }
                }
                if (GameMaster.Instance.online_Man.room_owner) {
                    GameMaster.Instance.online_Man.Set_Scene(Game_Now_Phase.Result);
                }
            }
        }
        if (GameMaster.Instance.debug_mode_now == false && GameMaster.Instance.now_fade_type == Fade_Type.White) {
            //時間の更新
            second += Time.deltaTime;
            Time_Slider.value = 1.0f - (second / Time_Limit_second);
        }
       
    }

    //初期化
    void Battle_Reset() {
        try {
            //ステージの生成
            Stage_Data_Read();
        }
        catch (System.NullReferenceException) {
            return;
        }
        
        //床落ちアイテムのリセット
        GameMaster.Instance.item_Man.Item_Stage_Reset();
            //キャラの生成
            chara_man.Parent_Criate();
            chara_man.Enemy_Reset();
            //プレイヤーの作成
            StartCoroutine(Player_Criate());
        //ホストなら
        if (GameMaster.Instance.online_Man.room_owner) {
                // 余った枠分の敵キャラを作成
                StartCoroutine(Enemy_Criate());
            }
        //プレイヤーと敵キャラをリスト化して並び変える
        StartCoroutine(Players_Sort());
            stage_criate_flag = STAGE_CRIATE.CRIATE_NOW;
        return;
    }

    //ステージの作成
    void Stage_Data_Read() {
        //マップの初期化
        for (int i = 0; i < Height_Max; i++) {
            for (int j = 0; j < Width_Max; j++) {
                stage[i, j] = new Map_Tile();
                if ((i == 0 || i == (Height_Max - 1)) || (j == 0 || j == (Width_Max - 1))) {
                    stage[i, j].tile = Tile_Type.Unbreakable_Wall;
                }
                else {
                    stage[i, j].tile = Tile_Type.Room_Floor;
                }
            }
        }
        //部屋として登録
        stage_man.Room_List.Add(new Room_Data(0,0));
        stage_man.Room_List[0].Room_start_point = Stage_Manager.Return_int2(1,1);
        stage_man.Room_List[0].Room_end_point = Stage_Manager.Return_int2(Width_Max-1,Height_Max-1);
    }
    //敵の作成
    IEnumerator Enemy_Criate() {
        //もしルームにいる全員がプレイヤーの作成を終えたら
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length == GameMaster.Instance.online_Man.room_menber_count);
        //余った枠に敵キャラを埋める
        while ((GameObject.FindGameObjectsWithTag("Player").Length+ GameObject.FindGameObjectsWithTag("Battle_Enemy").Length) < Chara_Max) {
            chara_man.BT_Enemy_Add();
        }
        //終了
        yield break;
    }
    //プレイヤーの作成
    IEnumerator Player_Criate() {
        yield return null;
        //プレイヤーの作成
       chara_man.Player_Reset();
        //メッセージの初期化
        GameMaster.Instance.message_Man.Message_Reset();
        //プレイヤーの情報を表示
        chara_man.Status_UI_Update();
        //終了
        yield break;
    }

    //全キャラのリストインと並び替え
    IEnumerator Players_Sort() {
        //もしプレイヤーと敵の作成を終えたら
        yield return new WaitUntil(() => (GameObject.FindGameObjectsWithTag("Player").Length + GameObject.FindGameObjectsWithTag("Battle_Enemy").Length) == Chara_Max);
        //プレイヤーキャラと敵のGameObjectのリストをそれぞれ作成
        GameObject[] pl_list = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ene_list = GameObject.FindGameObjectsWithTag("Battle_Enemy");
        //プレイヤーと敵を合わせたリストを作成
        List<GameObject> obj_list = new List<GameObject>();
        obj_list.AddRange(pl_list);
        obj_list.AddRange(ene_list);
        //キャラマネージャーのキャラリストを消す
        Charactor_Manager charaman = GameMaster.Instance.chara_Man;
        charaman.chara_list.Clear();
        //GameObjectをCharaParentの子にし、全てのオブジェクトのキャラ情報をリストに
        for (int i = 0; i < obj_list.Count; i++) {
            //キャラをCharaParentの子に
            obj_list[i].transform.SetParent(GameMaster.Instance.chara_Man.Chara_Parent.transform);
           //プレイヤーのリストと、キャラマネージャーのキャラリストにそれぞれ追加
            Character obj_chara = obj_list[i].GetComponent<Character>();
            players.Add(obj_chara);
            charaman.chara_list.Add(obj_chara);
        }
        //自分の管理下でないプレイヤーのアイテムスロットを初期化しておく
        for(int i = 0; i < players.Count; i++) {
            if(GameMaster.Instance.online_Man.player_mine(players[i])==false) {
                players[i].ItemSlot_Criate(players[i].have_item_Max);
            }
        }
        //部屋主なら不備がないかをチェック
        if (GameMaster.Instance.online_Man.room_owner) {
            //キャラ番号(同フレームにキャラが生成されたりするとキャラ番号が被る可能性があるため)
            {
                List<bool> num_use = new List<bool>();
                //人数分のキャラ番号使用リストを作成
                for (int i = 0; i < Chara_Max; i++) {
                    num_use.Add(false);
                }
                for (int i = 0; i < Chara_Max; i++) {
                    Character tmp = obj_list[i].GetComponent<Character>();
                    //キャラの番号が既に使用済みの番号なら(=他のキャラと被っていたら、使用していない番号を探す
                    if (num_use[tmp.chara_num] == true) {
                        for (int j = 0; j < Chara_Max; j++) {
                            //使用していない番号があればそれに書き換え
                            if (num_use[j] == false) {
                                tmp.Chara_num_Sync(j);
                                break;
                            }
                        }
                    }
                    //キャラ番号の使用フラグをオンにする
                    num_use[tmp.chara_num] = true;
                }
            }
            //座標被りを無くす
            {
                for (int i = 0; i < Chara_Max; i++) {
                    Character players_chara = players[i].GetComponent<Character>();
                    int[] pos = players_chara.now_position;
                    while (GameMaster.Instance.chara_Man.Can_Move_pos(pos,i) != Can_Move_Tile.Can_Move) {
                        pos = GameMaster.Instance.stage_Man.Random_Room_Pos(false,true);
                    }
                    players_chara.now_position = pos;
                }
            }
        }
        //チェック終了フラグを立てる
        GameMaster.Instance.online_Man.Sort_End();
        //全プレイヤーによるキャラ番号チェックが終わったら
        yield return new WaitUntil(() => sorted==GameMaster.Instance.online_Man.room_menber_count);
        //キャラの座標を同期
        chara_man.Player_chara.Pos_Sync();
        //初期注視キャラを自分に
        camera_chara = chara_man.Player_chara.chara_num;
        //カメラの位置切り替え
        chara_man.Player_chara.Camera_pos_Update();
        //マップの表示
        stage_man.Map_view();
        //アイテムの描画
        stage_man.Item_view();
        //キャラの枠線の色を変える
        Line_Color_Change();
        //UIフレームの色を変える
        GameMaster.Instance.Player_Color_Frame.Change_Color(GameMaster.Instance.Players_Theme_Color[GameMaster.Instance.chara_Man.Player_chara.chara_num]);
        //リスト内のキャラをキャラ番号で並べ直す
        players.Sort((a,b)=>a.chara_num-b.chara_num);
        //敵キャラの座標を変更
        if (GameMaster.Instance.online_Man.room_owner) {
            for (int i = 0; i < Chara_Max; i++) {
                if (players[i].tag == "Battle_Enemy") {
                    players[i].Pos_Sync();
                }
            }
        }
        //プレイヤーの所持アイテムを同期
        for (int i = 0; i < Chara_Max; i++) {
            //自分の管理下にあるキャラのみ情報を送信
            if (GameMaster.Instance.online_Man.player_mine(players[i])) {
                List<int> tmp = new List<int>();
                for (int j = 0; j < players[i].have_item_Max; j++) {
                    tmp.Add(players[i].have_Items[j] == null ? -1 : players[i].have_Items[j].item_id);
                }
                GameMaster.Instance.online_Man.RPC_view.Send_Have_Item_Sync(i, tmp.ToArray());
            }
        }
        //開始する準備が終わったことを送信
        GameMaster.Instance.online_Man.RPC_view.Send_Battle_Start_Sync();
        //全プレイヤーの準備が終わったから進む
        yield return new WaitUntil(() => battle_start_flag == GameObject.FindGameObjectsWithTag("Player").Length);
        //paramater_time秒間全プレイヤーのパラメータを表示する
        parameter_view.Count_Start(paramater_time);
        //初期アイテムの配置
        for (int i = 0; i < battle_start_item; i++) {
            New_Item_Drop(stage_man.Random_Room_Pos_Item());
        }
        //表示が終わったら先へ
        yield return new WaitUntil(() => parameter_view.view_end == true);
        //アイテムスロットの表示を更新
        chara_man.Player_chara.Item_Slot_Select();
        //表示が終わったら消しつつフェードを開ける
        parameter_view.gameObject.SetActive(false);
        GameMaster.Instance.Fade_Out();
        //終了
        yield break;
    }
    public void Turn_Change(int i) {
        //ターンを更新
        now_turn_chara = i;
        //ターンが一周したら全体ターンをインクリメント
        if (now_turn_chara == 0) {
            now_turn++;
        }
        //画面上の現在の操作キャラ情報を更新
        Turn_Text_Update();
        //キャラの枠線の色を変える
        Line_Color_Change();
        //ターンの開始
        GameMaster.Instance.turn_Man.turn_start = Turn_Manager.TURN_NOW.START;
    }
    //左上の[1P]の表示を変更
    public void Turn_Text_Update() {
        Turn_Text.text = (now_turn_chara+1).ToString() + "P";
        Turn_Text.color = GameMaster.Instance.Players_Theme_Color[now_turn_chara];
        Time_gauge.color = GameMaster.Instance.Players_Theme_Color[now_turn_chara];
    }

    //キャラの枠線の色を変える
    public void Line_Color_Change() {
        for(int j = 0; j < Chara_Max; j++) {
            //既に死んでいなければ
            if (players[j] != null) {
                //色を取得し、現在操作できるキャラ以外は少し暗くする   
                Color tmp = GameMaster.Instance.Players_Theme_Color[j];
                    if (j != now_turn_chara) {
                        tmp = new Color(tmp.r / 2, tmp.g / 2, tmp.b / 2);
                    }
                    //色を適用
                    players[j].transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material.SetColor("_LineColor", tmp);
            }
        }
    }

    //全プレイヤーの座標を正しい地点に同期する
    public void Players_Pos_Update() {
        for (int i = 0; i < Chara_Max; i++) {
            //既に死んでいなければ
            if (players[i] != null) {
                players[i].Chara_pos_Update(players[i].now_position);
            }
        }
    }
    //現在のシーンに残るキャラが一人になっているかを返す
   public bool Battle_end() {
        int count = Chara_Max;
        for (int i = 0; i <Chara_Max; i++) {
            if (players[i] == null) {
                count--;
            }
        }
        return (count == 1);
    }
    //与えられた番号のキャラが生きているかどうか
    public bool Chara_Alive(int num) {
        return players[num] != null;
    }
    //新たなアイテムを出現させる
    public void New_Item_Drop(int[] pos) {
        List<int> item_list= GameMaster.Instance.item_Man.Scene_Spawn_Item_Id(Game_Now_Phase.Battle);
        //ホストのみ出現判定
        if (GameMaster.Instance.online_Man.room_owner) {
                GameMaster.Instance.item_Man.Item_Drop(item_list[Random.Range(0, item_list.Count)],pos);
        }
    }
    //新たなアイテムを出現させる、アイテムの種類指定版
    public void New_Item_Drop(int item_id, int[] pos) {
        //ホストのみ出現判定
        if (GameMaster.Instance.online_Man.room_owner) {
                GameMaster.Instance.item_Man.Item_Drop(item_id, pos);
        }
    }
}
