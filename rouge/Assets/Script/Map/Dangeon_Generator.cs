using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constraction;

public class Dangeon_Generator  {
    private Map_Tile[,] stage;
    public int end_start_def = 1;

    public int add_way_limit=4;//部屋から生える廊下の最大本数

    private int Width_Max;//横方向のマス数
    private int Height_Max;//縦方向のマス数

    private int Block_X_num;//横方向のブロック数
    private int Block_Y_num;//縦方向のブロック数

    private int Block_Width;//1ブロックの横マス数
    private int Block_Height;//1ブロックの縦マス数

    private int Block_All_num;//ブロック数

    //通路生成
    private List<bool> block_aisle;//すでにブロック内に通路が存在するかどうか
    private List<int[]> branch_point = new List<int[]>();//枝分かれ点のリスト
    private List<int[]> branch_neighbor = new List<int[]>();
    private List<int[]> deadend_point = new List<int[]>();//行き止まり地点のリスト
    //部屋生成
    private int Room_num;
    private List<Room_Data> Room;
    private List<bool> block_room;//すでにブロック内に通路が存在するかどうか

    public Dangeon_Generator(Map_Tile[,] stage_map, List<Room_Data> Room_list) {
        stage = stage_map;
        Room = Room_list;
    }


    //マップの作成
  public void Map_Criate(int[] Map_num, int[] Block_Num,int room_num) {
        Num_Reset(Map_num,Block_Num, room_num);
        Map_Reset();//マップの初期化
        //通路作り
        Aisle_Criate();
        //部屋作り
        Room_Criate();
        //フロアの地形整頓
        Map_Organize();
        //階段作り
        Next_door_Criate();
    }
    void Num_Reset(int[] Map_num, int[] Block_Num,int room_num) {
        //マス目数
        Width_Max = Map_num[(int)XY.X];
        Height_Max = Map_num[(int)XY.Y];
        //block数
        Block_X_num = Block_Num[(int)XY.X];
        Block_Y_num = Block_Num[(int)XY.Y];
        Block_All_num = Block_X_num * Block_Y_num;
        //部屋数
        Room_num = room_num;

        Block_Width = Width_Max / Block_X_num;
        Block_Height = Height_Max / Block_Y_num;
        block_aisle = new List<bool>();
        block_room = new List<bool>();
        for (int i = 0; i < Block_All_num; i++) {
            block_aisle.Add(false);
            block_room.Add(false);
        }
    }
    //通路作り
    void Aisle_Criate() {
            Aisle_One_Stroke();//通路を一筆書き
            Nothing_Aisle_Stroke();//通路のないブロックを無くす
            for (int i = 0, j = 0; i < 3 && j < 10; j++) { if (Random_Aisle()) { i++; } }//通路を3本ランダムに増やす、それぞれ最大10回トライし生成できなかったら飛ばす


        //通路を一筆書きで伸ばしていく
        void Aisle_One_Stroke() {
            //始まりとなるブロックを選択
            int now_block = Random.Range(0, Block_All_num);
            //そのブロックの通路情報をtrueに
            block_aisle[now_block] = true;
            //始点を決定
            int[] now_point = Block_To_Random_Point(now_block, 2);
             deadend_point.Add(Stage_Manager.Return_int2(now_point));
            //次点を宣言
            int[] next_point;
            //道の方向
            XY vector = XY.Not;
            {
                //一筆書きのルーチン
                for (int i = 0; i < Block_All_num; i++) {
                    //周囲4方向でまだ通路ができていないブロックを受け取る
                    int[] empty_blocks = Enable_Aisle_Block(now_block, false);
                    //もし4方向全て通路ができていれば終了
                    if (empty_blocks.Length == 0) {
                        //branch_point.RemoveAt(i-1);
                        break;
                    }
                    //通路ができていないブロックがあればその中からランダムで1つブロックを選ぶ
                    else {
                        //新たなブロックを選出
                        int next_block = empty_blocks[Random.Range(0, empty_blocks.Length)];
                        if (i == 0) { vector = Block_XY(now_block, next_block); }
                        next_point = Block_To_Random_Point(next_block, 2);
                        //通路を床で埋める
                        Fill_Foor(Fill_List(now_point, next_point, Block_XY(now_block, next_block), false), false);
                        //方向ベクトルが変化したら枝分かれした点を保存
                        if (vector != Block_XY(now_block, next_block) && !(i == (Block_All_num - 1) || i == 0)) {
                            branch_point.Add(Stage_Manager.Return_int2(now_point[(int)XY.X], now_point[(int)XY.Y]));
                        }
                        //now_pointを更新
                        now_point[(int)Block_XY(now_block, next_block)] = next_point[(int)Block_XY(now_block, next_block)];
                        //方向を保存
                        vector = Block_XY(now_block, next_block);
                        //ブロックを更新
                        now_block = next_block;
                        //現在のブロックが通路を引いたことを記録
                        block_aisle[now_block] = true;
                    }
                }
                deadend_point.Add(Stage_Manager.Return_int2(now_point));
            }
            //枝分かれ点の更新
            Branch_Add();
            //branch_point.RemoveAt(branch_point.Count);

        }
        //通路のないブロックに通路を伸ばす
        void Nothing_Aisle_Stroke() {
            //まだ通路のないブロックを保存
            List<int> nothing_aisle_block = new List<int>();
            for (int i = 0; i < Block_All_num; i++) {
                if (block_aisle[i] == false) { nothing_aisle_block.Add(i); }
            }
            int count = 0;
            //通路のないブロックが存在する限りループ
            while (nothing_aisle_block.Count != 0 && count < 16) {
                count++;
                //通路のない各ブロックについてループ
                for (int i = 0; i < nothing_aisle_block.Count; i++) {
                    //現在のブロックから上下左右4方向の通路のあるブロックの番号を配列に保存
                    int[] aisle_block = Enable_Aisle_Block(nothing_aisle_block[i], true);

                    //もし通路のあるブロックが上下左右4方向になければそのブロックは飛ばす
                    if (aisle_block.Length == 0) { continue; }
                    else {
                        //通路のあるブロックの中から1つブロックを選ぶ
                        int next_block = aisle_block[Random.Range(0, aisle_block.Length)];
                        XY dir = Block_XY(nothing_aisle_block[i], next_block);
                        int[] now_point = Block_To_Random_Point(nothing_aisle_block[i], 2);
                        int[] next_point = Block_To_Floor_Random_Point(next_block, 2, dir == XY.X ? XY.Y : XY.X);
                        if (next_point == null) { continue; }
                        //選ばれたブロックにある通路と、現在通路のないブロックのランダムな点を繋ぐ
                        List<int[]> a = Fill_List(next_point, now_point, dir, false);
                        Fill_Foor(a, false);
                        branch_point.Add(next_point);
                        //枝分かれした点を保存
                        Branch_Add();
                        deadend_point.Add(Stage_Manager.Return_int2(a[a.Count-1]));
                        //繋げたら現在のブロックを通路のないブロックから除外
                        block_aisle[Point_To_Block(Stage_Manager.Return_int2(a[a.Count - 1]))] = true;
                        nothing_aisle_block.Remove(Point_To_Block(Stage_Manager.Return_int2(a[a.Count - 1])));
                    }
                }
            }
        }
        //ランダムにブロックを繋げる、成功したらtrueを返す
        bool Random_Aisle() {
            //開始地点
            int start_block = 0;
            int[] start_point = new int[2];
            //終端地点
            int end_block = 0;
            int[] end_point = new int[2];
            //終端地点になれる床のリスト
            List<int[]> end_point_list_after = new List<int[]>();
            //二点の中間地点
            int[] tmp = new int[2];
            //開始地点から見た終端地点の方向
            XY dir;
            //開始地点から中間地点までの通路の座標リスト
            List<int[]> start_to_tmp = new List<int[]>();
            //開始点から4方向のblock番号のリスト
            List<int> end_block_list = new List<int>();
            //まだトライしていない開始blockのリスト
            List<int> start_block_list = new List<int>();
            //開始blockの初期化
            for (int i = 0; i < Block_All_num; i++) {
                start_block_list.Add(i);
            }
            //終端地点のリスト作りは8回トライする
            for (int i = 0; i < 8; i++) {
                start_block = start_block_list[Random.Range(0, start_block_list.Count)];
                end_block_list = Four_dir_Block(start_block, false);
                //開始するblockから点をランダムに設定
                start_point = Block_To_Random_Point(start_block, 2);
                //4方向のブロック分forを回す
                for (int block = 0; block < end_block_list.Count; block++) {
                    //blockを1つ代入し、その方向を計算
                    end_block = end_block_list[block];
                    dir = Block_XY(start_block, end_block);
                    //end_block内のdir方向となる床を取得
                    List<int[]> end_point_list_before = Floor_list(end_block, 2, dir);
                    //取得した床部分に通路を貼れるかの確認をする
                    for (int j = 0; j < end_point_list_before.Count; j++) {
                        tmp = Tmp_point(start_point, end_point_list_before[j]);
                        //各点が条件を満たしていればリストに追加
                        if (Random_point_check(start_point, tmp, end_point_list_before[j]) == true) { end_point_list_after.Add(end_point_list_before[j]); }
                    }
                }
                if (end_point_list_after.Count == 0) { start_block_list.Remove(start_block); }
                else { break; }
            }

            //終端地点にできる床座標のリストが空でなければ床を貼る
            if (end_point_list_after.Count != 0) {
                //リストからランダムに終端地点を決める
                end_point = end_point_list_after[Random.Range(0, end_point_list_after.Count)];
                end_block = Point_To_Block(end_point);
                dir = Block_XY(start_block, end_block);
                tmp = Tmp_point(start_point, end_point);
                //開始点～中間点を埋める
                start_to_tmp = Fill_List(start_point, tmp, dir, false);
                Fill_Foor(start_to_tmp, true);
                //途中で線が床に被らないかつ、中間点と終端地点が異なったたときは中間点～終端地点の間も埋める
                if ((tmp[0] == start_to_tmp[start_to_tmp.Count - 1][0] && tmp[1] == start_to_tmp[start_to_tmp.Count - 1][1]) && !(tmp[0] == end_point[0] && tmp[1] == end_point[1])) {
                    List<int[]> tmp_to_end = Fill_List(tmp, end_point, (dir == XY.X ? XY.Y : XY.X), false);
                    //途中で道にぶつかった時、ぶつかった点を分岐点にする
                    if (tmp_to_end[tmp_to_end.Count - 1] != end_point) {
                        branch_point.Add(tmp_to_end[tmp_to_end.Count - 1]);
                    }
                    //中間点～終端地点を埋める
                    Fill_Foor(tmp_to_end, true);
                }
                //枝分かれした点を保存
                Branch_Add();
                deadend_point.Add(Stage_Manager.Return_int2(start_point));
                return true;
            }
            return false;

            //開始点と終端点がきちんと条件を満たしているかを返す
            bool Random_point_check(int[] start, int[] tmps, int[] end) {
                //もしend_pointがnullならfalse
                if (end == null) { return false; }

                //開始点から中間地点までの床リストを作成
                start_to_tmp = Fill_List(start, tmps, dir, false);
                //ぶつかった点が開始点からend_start_defより小さければfalse
                if (Mathf.Abs(start[(int)dir] - start_to_tmp[start_to_tmp.Count - 1][(int)dir]) < end_start_def) {
                    return false;
                }

                //開始地点と終端地点の距離がend_start_defより小さければfalse
                if (Mathf.Abs(end[0] - start[0]) < end_start_def || Mathf.Abs(end[1] - start[1]) < end_start_def) {
                    return false;
                }
                //もしstart_pointがend_blockの周囲四方向にあるneighoberの点とX軸もしくはY軸で被っていたらfalse
                List<int[]> nei = branch_block_neighbor(Point_To_Block(start), true);
                for (int j = 0; j < nei.Count; j++) {
                    if ((start[0] == nei[j][0] || start[1] == nei[j][1])) {
                        return false;
                    }
                }
                return true;
            }

            int[] Tmp_point(int[] start, int[] end) {
                int[] tmp_point = new int[2];
                //2つのblockの方向と、開始地点と終端地点の中間地点を作成
                if (dir == XY.Y) {
                    tmp_point = Stage_Manager.Return_int2(start[(int)XY.X], end[(int)XY.Y]);
                }
                else if (dir == XY.X) {
                    tmp_point = Stage_Manager.Return_int2(end[(int)XY.X], start[(int)XY.Y]);
                }
                return tmp_point;
            }
        }

        //ブロック番号が渡されると、そのブロックに属する地点をランダムに返す関数
        int[] Block_To_Random_Point(int block, int exclusion) {
            int[] a = new int[2];
            int now_block_y = block / Block_X_num;
            int now_block_x = block % Block_X_num;
            //ランダムに範囲内の点を返す
            a[(int)XY.X] = Random.Range(now_block_x * Block_Width + exclusion, (now_block_x + 1) * Block_Width - exclusion);
            a[(int)XY.Y] = Random.Range(now_block_y * Block_Height + exclusion, (now_block_y + 1) * Block_Height - exclusion);
            //exclusion分のblockの端は返さない

            //画面端は1マス分返さない
            if (a[(int)XY.X] == 0) { a[(int)XY.X]++; }
            else if (a[(int)XY.X] == Width_Max - 1) { a[(int)XY.X]--; }
            if (a[(int)XY.Y] == 0) { a[(int)XY.Y]++; }
            else if (a[(int)XY.Y] == Height_Max - 1) { a[(int)XY.Y]--; }
            return a;
        }
        //ブロック番号が渡されると、その4方向で通路の生成がasile_boolなブロックを返す
        int[] Enable_Aisle_Block(int block, bool asile_bool) {
            List<int> a = new List<int>();
            int now_block_y = block / Block_X_num;
            int now_block_x = block % Block_X_num;
            //右端でない
            if (now_block_x != Block_X_num - 1) { if (block_aisle[block + 1] == asile_bool) { a.Add(block + 1); } }
            //下端でない
            if (now_block_y != Block_Y_num - 1) { if (block_aisle[block + Block_X_num] == asile_bool) { a.Add(block + Block_X_num); } }
            //左端でない
            if (now_block_x != 0) { if (block_aisle[block - 1] == asile_bool) { a.Add(block - 1); } }
            //上端でない
            if (now_block_y != 0) { if (block_aisle[block - Block_X_num] == asile_bool) { a.Add(block - Block_X_num); } }
            //  Debug.Log(a.Count);
            return a.ToArray();
        }
        //blockの4方向のblockに存在するbranch_neighborの点をすべて返す、centerがtrueであれば同じブロック内のものもカウント
        List<int[]> branch_block_neighbor(int block, bool center) {
            List<int[]> a = new List<int[]>();
            for (int i = 0; i < branch_neighbor.Count; i++) {
                int branch_block = Point_To_Block(branch_neighbor[i]);
                int now_block_y = branch_block / Block_X_num;
                int now_block_x = branch_block % Block_X_num;
                if ((branch_block == block) && center) {
                    a.Add(branch_neighbor[i]);
                    continue;
                }
                if (((now_block_x != Block_X_num - 1) && (branch_block + 1 == block))
                    || ((now_block_x != 0) && (branch_block - 1 == block))
                    || ((now_block_y != Block_Y_num - 1) && (branch_block + Block_X_num == block))
                    || ((now_block_y != 0) && (branch_block - Block_X_num == block))) {
                    a.Add(branch_neighbor[i]);
                }
            }
            return a;
        }

        //分岐点周辺をリストとして保持
        void Branch_Add() {
            //branch_neighbor.Clear();
            //リストの更新
            for (int i = 0; i < branch_point.Count; i++) {
                try {
                    if (neighbor_chack(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y]))) { branch_neighbor.Add(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y])); }
                    if (neighbor_chack(Stage_Manager.Return_int2(branch_point[i][(int)XY.X] - 1, branch_point[i][(int)XY.Y]))) { branch_neighbor.Add(Stage_Manager.Return_int2(branch_point[i][(int)XY.X] - 1, branch_point[i][(int)XY.Y])); }
                    if (neighbor_chack(Stage_Manager.Return_int2(branch_point[i][(int)XY.X] + 1, branch_point[i][(int)XY.Y]))) { branch_neighbor.Add(Stage_Manager.Return_int2(branch_point[i][(int)XY.X] + 1, branch_point[i][(int)XY.Y])); }
                    if (neighbor_chack(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y] - 1))) { branch_neighbor.Add(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y] - 1)); }
                    if (neighbor_chack(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y] + 1))) { branch_neighbor.Add(Stage_Manager.Return_int2(branch_point[i][(int)XY.X], branch_point[i][(int)XY.Y] + 1)); }
                }
                catch (System.NullReferenceException) {
                }
            }
            //すでにリストに存在していればfalseを返す
            bool neighbor_chack(int[] point) {
                for (int i = 0; i < branch_neighbor.Count; i++) {
                    if (point[0] == branch_neighbor[i][0] && point[1] == branch_neighbor[i][1]) { return false; }
                }
                return true; ;
            }
        }
    }
    //部屋作り
    void Room_Criate() {
        Room_num_Sort();
        Room_Size();
        //どのblockに部屋を作るかを決める
        void Room_num_Sort() {
            //まだ部屋のないblockのリスト
            List<int> blank_room_block = new List<int>();
            //room情報の初期化
            Room.Clear();
            for (int i = 0; i < block_room.Count; i++) {
                block_room[i] = false;
                blank_room_block.Add(i);
            }
            //部屋が入るblockを選択
            for (int i = 0; i < Room_num; i++) {
                //まだ部屋のないblock番号を一つ選ぶ
                int seed = Random.Range(0, blank_room_block.Count);
                //もしその部屋に通路がなければ飛ばす
                if (block_aisle[blank_room_block[seed]] == false) { continue; }
                //新しく部屋を作成
                Room.Add(new Room_Data(i, blank_room_block[seed]));
                //部屋の存在するblockを記録
                block_room[blank_room_block[seed]] = true;
                //部屋のないblockのリストから除去
                blank_room_block.Remove(blank_room_block[seed]);
            }
        }

        void Room_Size() {
            for (int i = Room.Count-1; i >=0; i--) {
                //今作ろうとしている部屋のブロック情報を計算
               int now_block_y = Room[i].room_block / Block_X_num;
                int now_block_x = Room[i].room_block % Block_X_num;
                int[] center = new int[2];
                //block内の分岐点、終端点を取得
                List<int[]> block_branch_point = Branch_Point_Block(Room[i].room_block);
                List<int[]> block_deadend_point =Deadend_Point_Block(Room[i].room_block);
                //部屋サイズを初期化
                int x_size = Random.Range(4, Block_Width - 2);
                int y_size = Random.Range(4, Block_Height - 2);
                //部屋の中心点を初期化
                center[(int)XY.X] = Random.Range(now_block_x * Block_Width+ x_size, (now_block_x + 1) * Block_Width- x_size);
                center[(int)XY.Y] = Random.Range(now_block_y * Block_Height+ y_size, (now_block_y + 1) * Block_Height- y_size);
                //終端点を元に中心点を移動
                if (block_deadend_point.Count == 1){//そのブロックにある終端点の数が1つならその点を中心にする
                    center[(int)XY.X] = block_deadend_point[0][(int)XY.X];
                    center[(int)XY.Y]=block_deadend_point[0][(int)XY.Y];
                    Remove_Deadend(block_deadend_point);
                }
                else if (block_deadend_point.Count > 1) {//終端点の数が2個以上なら、その各点の平均座標を中心にし、部屋サイズを変化させて全ての終端点が部屋内に含まれるようにする
                    int[] def = List_Max_def(block_deadend_point);
                    x_size = Random.Range(def[(int)XY.X] > 4 ? def[(int)XY.X] : 4, Block_Width - 2);
                    y_size = Random.Range(def[(int)XY.Y] > 4 ? def[(int)XY.Y] : 4, Block_Height - 2);
                    center = List_Average(block_deadend_point);
                    Remove_Deadend(block_deadend_point);
                }
                else {
                    //もし終端点がなければ分岐点を元に移動
                    if (block_branch_point.Count == 1) {//そのブロックにある分岐点の数が1つならその点を中心にする
                        center[(int)XY.X] = block_branch_point[0][(int)XY.X];
                        center[(int)XY.Y] = block_branch_point[0][(int)XY.Y];
                        Remove_Branch(block_branch_point);
                    }
                    else if (block_branch_point.Count > 1) {//分岐点の数が2個以上なら、その各点の平均座標を中心にし、部屋サイズを変化させて全ての分岐点が部屋内に含まれるようにする
                        int[] def=  List_Max_def(block_branch_point);
                        x_size = Random.Range(def[(int)XY.X]>4? def[(int)XY.X]:4, Block_Width - 2);
                        y_size = Random.Range(def[(int)XY.Y]>4?def[(int)XY.Y]:4, Block_Height - 2);
                        center = List_Average(block_branch_point);
                        Remove_Branch(block_branch_point);
                    }
                    else {
                        //終端点も分岐点もなければ通路の床を取得
                        int[] floor = Block_To_Floor_Random_Point(Room[i].room_block, 3, XY.Not);
                        //もし通路が存在すれば取得したその点をcenterに
                        if (floor != null) {
                            center = Stage_Manager.Return_int2(floor);
                        }
                        //通路が存在しなければ部屋を作れないので、部屋情報を削除し次のループに飛ばす
                        else {
                            Room.RemoveAt(i);
                            continue;
                        }
                    }
                }
                //部屋を床マスで埋める
                Room_Fill(i,x_size, y_size, center);
            }

        }

        //与えられた部屋情報を元にその全てを床で埋める
        void Room_Fill(int room_num, int x_size,int y_size,int[] center) {
            int now_block = Point_To_Block(center);

            int now_block_y = now_block / Block_X_num;
            int now_block_x = now_block % Block_X_num;
            int y_start = center[(int)XY.Y] - y_size;
            int y_end = center[(int)XY.Y] + y_size;
            int x_start = center[(int)XY.X] - x_size;
            int x_end = center[(int)XY.X] + x_size;

            //もしxyが現在のblockより小さければ現在のブロックの範囲内に直す
            if (x_start <= now_block_x * Block_Width) {x_start = now_block_x * Block_Width + 1;}
            if (x_end >= (now_block_x + 1) * Block_Width) {x_end = (now_block_x + 1) * Block_Width - 1;}
            if (y_start <= now_block_y * Block_Height) {y_start = now_block_y * Block_Height + 1;}
            if (y_end >= (now_block_y + 1) * Block_Height) {y_end = (now_block_y + 1) * Block_Height - 1;}
            //部屋内のマスを全て床で塗る
            for (int y = y_start; y < y_end; y++) {
                for (int x = x_start; x < x_end; x++) {
                    stage[y, x].tile = Tile_Type.Room_Floor;
                }
            }
            //部屋情報を反映
            Room[room_num].Room_start_point = Stage_Manager.Return_int2(x_start, y_start);
            Room[room_num].Room_end_point = Stage_Manager.Return_int2(x_end - 1, y_end - 1);

            //出入口の処理を行う
            {
                //上端の行
                for (int k = x_start; k < x_end; k++) {
                    Four_dir_Doorway(Stage_Manager.Return_int2(k, y_start-1));
                }
                //下端の行
                for (int k = x_start; k < x_end; k++) {
                    Four_dir_Doorway(Stage_Manager.Return_int2(k, y_end));
                }
                //右端の行
                for (int k = y_start; k < y_end; k++) {
                    Four_dir_Doorway(Stage_Manager.Return_int2(x_end, k));
                }
                //左端の行
                for (int k = y_start; k < y_end; k++) {
                    Four_dir_Doorway(Stage_Manager.Return_int2(x_start-1, k));
                }
            }
            //指定されたが廊下であればその座標を出入口として登録
            void Four_dir_Doorway(int[] pos) {
                if (stage[pos[(int)XY.Y], pos[(int)XY.X]].tile == Tile_Type.Hall_Floor) { Room[room_num].room_doorway.Add(Stage_Manager.Return_int2(pos[(int)XY.X], pos[(int)XY.Y])); }
            }
        }
        //与えられた値のリストのうち最大値と最低値の差を返す
        int[] List_Max_def(List<int[]> list) {
            int x_def = 0;
            int y_def = 0;
            for (int i = 0; i < list.Count; i++) {
                for (int j = 0; j < list.Count - 1; j++) {
                    if (Mathf.Abs(list[i][(int)XY.X] - list[j][(int)XY.X]) > x_def) { x_def = Mathf.Abs(list[i][(int)XY.X] - list[j][(int)XY.X]); }
                    if (Mathf.Abs(list[i][(int)XY.Y] - list[j][(int)XY.Y]) > y_def) { y_def = Mathf.Abs(list[i][(int)XY.Y] - list[j][(int)XY.Y]); }
                }
            }
            return Stage_Manager.Return_int2(x_def, y_def);
        }
        //与えられた座標のリストの平均となる座標を返す
        int[] List_Average(List<int[]> list) {
            int[] a = new int[2];
            for (int i = 0; i < list.Count; i++) {
                a[(int)XY.X] += list[i][(int)XY.X];
                a[(int)XY.Y] += list[i][(int)XY.Y];
            }
            return Stage_Manager.Return_int2((a[(int)XY.X] / list.Count), (a[(int)XY.Y] / list.Count));
        }
        //与えられた座標を分岐点のリストから除外する
        void Remove_Branch(List<int[]> list) {
            for (int i = 0; i < list.Count; i++) {
                for (int j = 0; j < branch_point.Count; j++) {
                    if (GameMaster.Instance.stage_Man.Pos_Equal(list[i], branch_point[j])) {
                        branch_point.RemoveAt(j);
                    }
                }
            }
        }
        //与えられた座標を終端点のリストから除外する
        void Remove_Deadend(List<int[]> list) {
            for (int i = 0; i < list.Count; i++) {
                for(int j = 0; j < deadend_point.Count; j++) {
                    if (GameMaster.Instance.stage_Man.Pos_Equal(list[i], deadend_point[j])) {
                        deadend_point.RemoveAt(j);
                    }
                }
            }
        }
        //指定したblock内の分岐点をすべて返す
        List<int[]> Branch_Point_Block(int block) {
            List<int[]> a = new List<int[]>();
            for (int i = 0; i < branch_point.Count; i++) {
                if (Point_To_Block(branch_point[i]) == block) {
                    a.Add(branch_point[i]);
                }
            }
            return a;
        }
        //指定したblock内の終端点をすべて返す
        List<int[]> Deadend_Point_Block(int block) {
            List<int[]> a = new List<int[]>();
            for (int i = 0; i < deadend_point.Count; i++) {
                if (Point_To_Block(deadend_point[i]) == block) {
                    a.Add(deadend_point[i]);
                }
            }
            return a;
        }
    }
    //フロアの整理整頓
     public void Map_Organize() {
        //行き止まりとなる通路を削除
        {
            for (int i = deadend_point.Count - 1; i >= 0; i--) {
                //各行き止まり点について削除をする再帰を開始、削除できたならその点はもう行き止まりではないはずなのでリストから除外
                if (Point_Recursion(deadend_point[i])) {
                    deadend_point.RemoveAt(i);
                }
            }

            //与えられた座標の周囲4マスを、分岐がなければ壁にしていく再帰、startがtrueであれば再帰の開始である
            bool Point_Recursion(int[] center) {
                //周囲4マスのリスト
                List<int[]> nei_point = Nei_4dir_point(center);
                //もし周囲4マスのうち2マス以上が床なら(=分岐点なら)消さず、再帰できなかったことを示すfalse
                if (nei_point.Count >= 2) { return false; }
                else {
                    //もしただの通路であればその点を壁にして、次の点に進む
                    stage[center[(int)XY.Y], center[(int)XY.X]].tile = Tile_Type.Wall;
                    //2個以上要素が来るはずはないけど一応ループに
                    for (int i = 0; i < nei_point.Count; i++) {
                        Point_Recursion(nei_point[i]);
                    }
                }
                //再帰ができたらtrue
                return true;
            }

            //与えられた座標の周囲四方向の廊下の床を返す
            List<int[]> Nei_4dir_point(int[] center_pos) {
                //与えられた座標の四方向をリスト化
                List<int[]> pos_list = new List<int[]>();
                if (center_pos[0] > 0) { pos_list.Add(new int[] { center_pos[0] - 1, center_pos[1] }); }
                if (center_pos[0] < Height_Max - 1) { pos_list.Add(new int[] { center_pos[0] + 1, center_pos[1] }); }
                if (center_pos[1] > 0) { pos_list.Add(new int[] { center_pos[0], center_pos[1] - 1 }); }
                if (center_pos[1] < Width_Max - 1) { pos_list.Add(new int[] { center_pos[0], center_pos[1] + 1 }); }
                //各座標が指し示すタイルが廊下かどうかを調べ、廊下でなければ除外
                for (int i = pos_list.Count - 1; i >= 0; i--) {
                    if (stage[pos_list[i][0], pos_list[i][1]].tile != Tile_Type.Hall_Floor) {
                        pos_list.RemoveAt(i);
                    }
                }
                return pos_list;
            }
        }
        //出入口が少ない部屋について、周囲に他に部屋があれば一定の確率で通路を伸ばす
        {
            for (int i = Room.Count - 1; i >= 0; i--) {
                //もし出入口が1つも無ければ(そんなことになってたらマズい)周囲の部屋から通路を伸ばす
                if (Room[i].room_doorway.Count == 0) {
                   Room_To_Nei(i);
                }
                //もし出入口がadd_way_limitより少なければ周囲の部屋から通路を一定の確率で伸ばす
                else if (Room[i].room_doorway.Count < add_way_limit) {
                    //1-(Room[i].room_doorway.Count/add_way_limit)の確率で作成
                    if (Random.Range(0, add_way_limit) >= Room[i].room_doorway.Count) {
                        Room_To_Nei(i);
                    }
                }
            }

            //指定された部屋の周囲の部屋から通路を伸ばし、できたかどうかをかえす
            bool Room_To_Nei(int room_num) {
                //現在部屋があるところから周囲のブロックの番号を取得
              List<int>four_block=  Four_dir_Block(Room[room_num].room_block,false);
                //周囲の4ブロックに存在する、部屋を保存するリスト
                List<Room_Data> room_block = new List<Room_Data>();
                //周囲のブロックに部屋があるかどうかを調査
                for (int j = 0; j < four_block.Count; j++) {
                    for (int i = 0; i < Room.Count; i++) {
                        //部屋が存在するならroom_blockのリストにイン
                        if (Room[i].room_block == four_block[j]) {
                            //その部屋との間にすでに通路があるなら除外する
                            if (Room[i].room_doorway.Count >= 1 && Room[room_num].room_doorway.Count >= 1) {
                                for (int k = 0; k < Room[i].room_doorway.Count; k++) {
                                    for (int l = 0; l < Room[room_num].room_doorway.Count; l++) {
                                        //出入口のX座標もしくはY座標が一致していたら同じ通路であるとみなせるので、どちらも一致していない場合のみリストへ
                                        if ((Room[i].room_doorway[k][0] == Room[room_num].room_doorway[l][0] || Room[i].room_doorway[k][1] == Room[room_num].room_doorway[l][1])==false) {
                                            room_block.Add(Room[i]);
                                            break;
                                        }
                                    }
                                }
                            }
                            //そもそも部屋に通路がなければ即リストへ
                            else {
                                room_block.Add(Room[i]);
                                break;
                            }
                        }
                    }
                }
                
                //周囲のブロックに部屋が存在すればその部屋と接続する
                if (room_block.Count>=1) {
                    //部屋を1つ選ぶ
                    Room_Data branch_room = room_block[Random.Range(0, room_block.Count)];
                    //部屋を繋ぐ
                   return Room_To_Room_way(Room[room_num], branch_room);
                }
                return false;
            }
            //与えられた2部屋の間にランダムに通路を引く
            bool Room_To_Room_way(Room_Data start_room,Room_Data end_room) {
                //start_pos~start_tmp_pos~end_tmp_pos~end_posという流れで通路を引く
                int[] start_pos = new int[2];
                int[] end_pos = new int[2];
                //きちんと通路が引けたかどうか
                bool key = false;
                //繋ぐ道の座標候補
                List<int> new_way_list = new List<int>();
                XY dir = Block_XY(start_room.room_block, end_room.room_block);
                List<int[]> way_list = new List<int[]>();
                //2つの部屋が横並びか縦並びかでつなぐ点の軸を設定
                switch (dir) {
                    case XY.X://横並び
                        {
                            //左側に部屋があるとき
                            if (start_room.room_block - end_room.room_block > 0) {
                                start_pos[(int)XY.X] = start_room.Room_start_point[(int)XY.X];
                                end_pos[(int)XY.X] = end_room.Room_end_point[(int)XY.X];
                            }
                            //右側に部屋があるとき
                            else {
                                start_pos[(int)XY.X] = start_room.Room_end_point[(int)XY.X];
                                end_pos[(int)XY.X] = end_room.Room_start_point[(int)XY.X];
                            }
                            break;
                        }
                    case XY.Y://縦並び
                         {
                            //上側に部屋があるとき
                            if (start_room.room_block - end_room.room_block < 0) {
                                start_pos[(int)XY.Y] = start_room.Room_end_point[(int)XY.Y];
                                end_pos[(int)XY.Y] = end_room.Room_start_point[(int)XY.Y];
                            }
                            //下側に部屋があるとき
                            else {
                                start_pos[(int)XY.Y] = start_room.Room_start_point[(int)XY.Y];
                                end_pos[(int)XY.Y] = end_room.Room_end_point[(int)XY.Y];
                            }
                            break;
                        }
                }
                //各部屋のサイズから開始点と終端点を確定させて通路を繋ぐ
                {
                    //2つの部屋がX軸で被っている部分を計算
                    int def_start = Mathf.Max(start_room.Room_start_point[(int)reverse_dir(dir)], end_room.Room_start_point[(int)reverse_dir(dir)]);
                    int def_end = Mathf.Min(start_room.Room_end_point[(int)reverse_dir(dir)], end_room.Room_end_point[(int)reverse_dir(dir)]);
                    
                    //被っていれば通路を作成する
                    if ((def_end < def_start)==false){
                        //被っている範囲を候補としてリスト化する
                        for (int i = def_start; i <= def_end; i++) {
                            new_way_list.Add(i);
                        }
                        //すでに出入口があるならその周辺1マスは候補から除外する
                        for (int k = new_way_list.Count-1; k >=0;k--) {
                            int[] now_pos_start = new int[2];
                            int[] now_pos_end = new int[2];
                            //候補から座標を作成
                            if (dir == XY.X) {
                                now_pos_start = Stage_Manager.Return_int2(start_pos[(int)XY.X], new_way_list[k]);
                                now_pos_end = Stage_Manager.Return_int2(end_pos[(int)XY.X], new_way_list[k]);
                            }
                            else if (dir == XY.Y) {
                                now_pos_start = Stage_Manager.Return_int2(new_way_list[k], start_pos[(int)XY.Y]);
                                now_pos_end = Stage_Manager.Return_int2(new_way_list[k], end_pos[(int)XY.Y]);
                            }
                            //その座標が各部屋の出入口と被っていれば削除する
                            for (int i = 0; i < start_room.room_doorway.Count; i++) {
                                if (GameMaster.Instance.stage_Man.Pos_Equal_four(now_pos_start, start_room.room_doorway[i])) {
                                    if (Remove() == false) {return false; }
                                    break;
                                }
                            }
                            for (int i = 0; i < end_room.room_doorway.Count; i++) {
                                if (GameMaster.Instance.stage_Man.Pos_Equal_four(now_pos_end, end_room.room_doorway[i])) {
                                    if (Remove() == false) { return false; }
                                    break;
                                }
                            }
                            //候補のリストのうち、引数のものの前後の値の分を削除
                            bool Remove() {
                                    try {
                                        new_way_list.Remove(new_way_list[k] + 1);
                                        new_way_list.Remove(new_way_list[k] - 1);
                                        new_way_list.Remove(new_way_list[k]);
                                    return true;
                                    }
                                    //もし多重削除しようとしてエラーが出たら、変な挙動を防ぐためにその時点で通路作りを止める
                                    catch (System.ArgumentOutOfRangeException) {
                                        new_way_list.Clear();
                                        return false;
                                    }
                            }
                        }
                        //結果候補が残っていれば
                        if (new_way_list.Count > 0) {
                            //候補の中からランダムに1つを選び、それを軸にする
                            int x = new_way_list[Random.Range(0, new_way_list.Count)];
                            start_pos[(int)reverse_dir(dir)] = x;
                            end_pos[(int)reverse_dir(dir)] = x;
                            //実際に引く道の座標でリストを作成
                            way_list = Fill_List(start_pos, end_pos, dir, false);
                            //先頭と末端は部屋内の座標を示しているので除外
                            way_list.RemoveAt(way_list.Count - 1);
                            way_list.RemoveAt(0);
                            //道を作成
                            Fill_Foor(way_list, true);
                            //引いたフラグを立てておく
                            key = true;
                        }
                    }
                }
                //通路が引けていれば
                if (key) {
                    //それぞれ出入口情報を保存しtrue
                    start_room.room_doorway.Add(way_list[0]);
                    end_room.room_doorway.Add(way_list[way_list.Count-1]);
                    return true;
                }
                //引けなかったならfalse
                else {
                    return false;
                }
            }
        }
       
    }
    //階段作り
    void Next_door_Criate() {
        int[] pos=GameMaster.Instance.stage_Man.Random_Room_Pos(true,true);
        stage[pos[0], pos[1]].tile = Tile_Type.Next_Stage;
    }
    //startからendまでのリストを作成する、throughがfalseであれば途中で床があればそこで止まる
    List<int[]> Fill_List(int[] start, int[] end, XY dir, bool through) {
        List<int[]> list = new List<int[]>();
        if (start == null || end == null) { return list; }
        int start_pos = 0;//dirがXなら開始位置のX座標、Yなら開始位置のY座標
        int end_pos = 0;//上の終端位置ver
        int UP = 1;//値が増加していくか減少していくかのフラグ
        switch (dir) {
            case XY.X: //startからendまでX軸にずらして通路に
                start_pos = start[(int)XY.X];
                end_pos = end[(int)XY.X];
                UP = (start[(int)XY.X] > end[(int)XY.X] ? -1 : 1);
                break;
            case XY.Y://startからendまでY軸にずらして通路に
                start_pos = start[(int)XY.Y];
                end_pos = end[(int)XY.Y];
                UP = (start[(int)XY.Y] > end[(int)XY.Y] ? -1 : 1);
                break;
        }
        //start_posからend_posまで埋める
        for (int i = start_pos, j = 0; j <= Mathf.Abs(start_pos - end_pos); i += UP, j++) {
            list.Add(Stage_Manager.Return_int2((dir == XY.X ? i : start[(int)XY.X]), (dir == XY.X ? start[(int)XY.Y] : i)));
            //次塗る地点がすでに床である座標と被ったらそこで終了する
            if (through == false) {
                if (stage[(dir == XY.X ? start[(int)XY.Y] : i + UP), (dir == XY.X ? i + UP : start[(int)XY.X])].tile == Tile_Type.Hall_Floor) {
                    list.Add(Stage_Manager.Return_int2((dir == XY.X ? i + UP : start[(int)XY.X]), (dir == XY.X ? start[(int)XY.Y] : i + UP)));
                    return list;
                }
            }
        }
        return list;
    }
    //渡された開始地点から終了地点まで通路を敷き詰める、addがtrueなら、敷き詰めた最後の点を分岐点として登録する
    void Fill_Foor(List<int[]> fill_point_list, bool branch_add) {
        for (int i = 0; i < fill_point_list.Count; i++) {
            stage[fill_point_list[i][(int)XY.Y], fill_point_list[i][(int)XY.X]].tile = Tile_Type.Hall_Floor;
        }
        if (branch_add) {
            branch_point.Add(fill_point_list[fill_point_list.Count - 1]);
        }
        return;
    }

    //ブロック番号が渡されると、そのブロックに属する床となる地点をランダムに返す関数、外周exclusionマス分はカウントしない、dirにXYを入れるとそれに対応した方向の道のみが返る
    int[] Block_To_Floor_Random_Point(int block, int exclusion, XY dir) {
        List<int[]> Floor_point = Floor_list(block, exclusion, dir);
        if (Floor_point.Count == 0) { return null; }
        //Floorとなる座標の中からランダムに1つを返す
        return Floor_point[Random.Range(0, Floor_point.Count)];
    }
    //ブロック番号が渡されると、そのブロックに属する床となる地点をリストにして返す関数、外周exclusionマス分はカウントしない、dirにXYを入れるとそれに対応した方向の道のみが返る
    List<int[]> Floor_list(int block, int exclusion, XY dir) {
        List<int[]> Floor_point = new List<int[]>();
        int now_block_y = block / Block_X_num;
        int now_block_x = block % Block_X_num;
        List<int[]> tmp_point = new List<int[]>();
        switch (dir) {
            case XY.X:
                //ブロック内の床を全て探索してFloorとなる座標をリスト化
                for (int y = (now_block_y * Block_Height) + exclusion; y < ((now_block_y + 1) * Block_Height) - exclusion; y++) {
                    tmp_point.Clear();
                    for (int x = (now_block_x * Block_Width) + exclusion; x < ((now_block_x + 1) * Block_Width) - exclusion; x++) {
                        int[] floor_stage_tile = { y, x };
                        if (stage[y, x].tile == Tile_Type.Hall_Floor) {
                            tmp_point.Add(floor_stage_tile);
                        }
                    }
                    //通路が縦方向に伸びているとき、その座標を追加
                    if (tmp_point.Count > 1) {
                        Floor_point.AddRange(tmp_point);
                    }
                }
                break;
            case XY.Y:
                //ブロック内の床を全て探索してFloorとなる座標をリスト化
                for (int x = (now_block_x * Block_Width) + exclusion; x < ((now_block_x + 1) * Block_Width) - exclusion; x++) {
                    tmp_point.Clear();
                    for (int y = (now_block_y * Block_Height) + exclusion; y < ((now_block_y + 1) * Block_Height) - exclusion; y++) {

                        int[] floor_stage_tile = { y, x };
                        if (stage[y, x].tile == Tile_Type.Hall_Floor) {
                            tmp_point.Add(floor_stage_tile);
                        }
                    }
                    //通路が横方向に伸びているとき、その座標を追加
                    if (tmp_point.Count > 1) {
                        Floor_point.AddRange(tmp_point);
                    }
                }
                break;
            default:
                //ブロック内の床を全て探索してFloorとなる座標をリスト化
                for (int i = (now_block_y * Block_Height) + exclusion; i < ((now_block_y + 1) * Block_Height) - exclusion; i++) {
                    for (int j = (now_block_x * Block_Width) + exclusion; j < ((now_block_x + 1) * Block_Width) - exclusion; j++) {
                        try {
                            int[] floor_stage_tile = { i, j };
                            if (stage[i, j].tile ==Tile_Type.Hall_Floor && Branch_Check(floor_stage_tile)) {
                                Floor_point.Add(floor_stage_tile);
                            }
                        }
                        catch (System.IndexOutOfRangeException) {
                        }
                    }
                }
                break;
        }
        return Floor_point;
        //枝分かれ点の上下左右の点であればfalse、その他の点ならtrueを返す
        bool Branch_Check(int[] point) {
            for (int i = 0; i < branch_neighbor.Count; i++) {
                if (point == branch_neighbor[i]) { return false; }
            }
            return true;
        }
    }

   public int Point_To_Block(int[] point) {
        int block_x = point[(int)XY.X] / (Width_Max / Block_X_num);
        int block_y = point[(int)XY.Y] / (Height_Max / Block_Y_num);
        return block_x + block_y * Block_X_num;
    }
  

    //与えられたブロックの上下左右のブロックのリストを返す
    List<int> Four_dir_Block(int block, bool center) {
        List<int> block_list = new List<int>();
        if (center == true) {
            block_list.Add(block);
        }
        int now_block_y = block / Block_X_num;
        int now_block_x = block % Block_X_num;
        //各方向への制限をかける
        //右端でない
        if ((now_block_x != Block_X_num - 1)) { block_list.Add(block + 1); }
        //左端でない
        if ((now_block_x != 0)) { block_list.Add(block - 1); }
        //下端でない
        if ((now_block_y != Block_Y_num - 1)) { block_list.Add(block + Block_X_num); }
        //上端でない
        if ((now_block_y != 0)) { block_list.Add(block - Block_X_num); }
        return block_list;
    }
    //渡された二つのブロックが上下左右どちらに面しているかを返す
    XY Block_XY(int block_A, int block_B) {
        return (block_A - 1 == block_B || block_A + 1 == block_B) ? XY.X : XY.Y;
    }

    //XYを反転させて返す
   XY reverse_dir(XY dir) {
        switch (dir) {
            case XY.X:
                return XY.Y;
            case XY.Y:
                return XY.X;
        }
        return XY.Not;
    }

    void Map_Reset() {
        //通路情報の初期化
        for (int i = 0; i < Block_All_num; i++) {
            block_aisle[i] = false;
        }
        //枝分かれ点の初期化
        branch_point.Clear();
        deadend_point.Clear();
        //マップの初期化
        for(int i = 0; i < Height_Max; i++) {
            for(int j = 0; j < Width_Max; j++) {
                stage[i, j] = new Map_Tile();
                if ((i == 0 || i == (Height_Max - 1))||(j==0||j==(Width_Max-1))) {
                    stage[i, j].tile = Tile_Type.Unbreakable_Wall;
                }
                else {
                    stage[i, j].tile = Tile_Type.Wall;
                }
            }
        }
        stage[1, 1].tile = Tile_Type.Wall;
    }
}
