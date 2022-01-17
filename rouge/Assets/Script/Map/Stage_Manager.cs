using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Constraction;

public class Stage_Manager : MonoBehaviour
{
    public Map_Tile[,] stage=new Map_Tile[1,1];
    public List<Room_Data> Room_List = new List<Room_Data>();
    private Charactor_Manager chara_man;
    public Stage_Data stage_data;
    [System.NonSerialized]
    public int Now_Width_Max;//横方向のマス数
    [System.NonSerialized]
    public int Now_Height_Max ;//縦方向のマス数

    [Header("マップ表示関係")]
    public int mapping_distance=3;//周囲何マス分マップに表示するのか
    public Tilemap floor_maps;//床を描画するレイヤー
    public Tilemap wall_maps;//壁を描画するレイヤー
    public Tilemap item_maps;//アイテムを描画するレイヤー
    public GameObject wall_obj_parent;//壁オブジェクトの親

    [EnumIndex(typeof(Item_Type))]
    public TileBase[] Item_chips=new TileBase[(int)Item_Type.MAX];//アイテムの各種類の画像
    public TileBase Debug_chips;//デバッグ用タイルの画像
    public float mass_size;//マス目のワールド座標での大きさ
   


        // Start is called before the first frame update
        void Start()
    {
        chara_man = GameMaster.Instance.chara_Man;
        mass_size = floor_maps.size.x;
    }
    //ステージの初期化
    public void Stage_Reset(int height,int width) {
        Now_Width_Max = width;
        Now_Height_Max = height;
        stage = new Map_Tile[Now_Height_Max, Now_Width_Max];
        Room_List = new List<Room_Data>();
        enemy_view = false;
        Map_Reset();
    }
    public void Map_Update() {
        Item_view();
        
      // ミニマップを更新
        Minimap_view();
    }

    //ステージの描画の更新
   public void Map_view() {
        Map_Reset();
        for (int y = 0; y < Now_Height_Max; y++) {
            for (int x = 0; x < Now_Width_Max; x++) {
                if (stage[y, x].tile == Tile_Type.Hall_Floor || stage[y, x].tile == Tile_Type.Room_Floor) {
                    floor_maps.SetTile(new Vector3Int(x, y, 0), stage_data.floor_tile);
                }
                else if (stage[y, x].tile == Tile_Type.Unbreakable_Wall || stage[y, x].tile == Tile_Type.Wall) {
                    wall_maps.SetTile(new Vector3Int(x, y, 0), stage_data.wall_tile);
                    if ((100-stage_data.wall_prob) <Random.Range(1,100)) {
                        Vector3 vec = new Vector3(x + 0.5f, y + 0.5f, 0);
                        GameObject obj = Instantiate(stage_data.wall_object[Random.Range(0,stage_data.wall_object.Count)], wall_obj_parent.transform);
                        obj.transform.localPosition = vec;
                        obj.transform.rotation = Quaternion.Euler(90, 0, 0);
                    }
                }
                else if (stage[y, x].tile == Tile_Type.Next_Stage) {
                    floor_maps.SetTile(new Vector3Int(x, y, 0), stage_data.stair_tile);
                }
                else if (stage[y, x].tile == Tile_Type.Debbug) {
                    floor_maps.SetTile(new Vector3Int(x, y, 0), Debug_chips);
                }
            }
        }
    }
    //ステージの描画の初期化
    public void Map_Reset() {
        floor_maps.ClearAllTiles();
        wall_maps.ClearAllTiles();
        item_maps.ClearAllTiles();
        foreach (Transform child in wall_obj_parent.transform) {
            Destroy(child.gameObject);
        }
    }
    //アイテムの描画
  public  void Item_view() {
        item_maps.ClearAllTiles();
        List<Item_puton> item = GameMaster.Instance.item_Man.on_stage_items;
        for (int i = 0; i < item.Count; i++) {
            Vector3Int tmp = new Vector3Int(item[i].item_pos[(int)XY.X], item[i].item_pos[(int)XY.Y], 0);
            item_maps.SetTile(tmp, Item_chips[(int)item[i].item_one.Item_type]);
            }
    }

    //全ての部屋の中からランダムに一点を返す、(引数:他キャラと被っていいかどうか、プレイヤーと同じ部屋になっていいか)
    public int[] Random_Room_Pos(bool chara_same,bool player_same) {
        //もし部屋がなければnullを返す
        if (Room_List.Count == 0) { return null; }
        //部屋情報を取得
        List<Room_Data> room = Room_List;
        //プレイヤーのいる部屋と同室でないなら、プレイヤーのいる部屋をリストから除外
        if (player_same == false) {
            int player_room = int2_to_Room(chara_man.Player_chara.now_position);
            for (int i = 0; i < room.Count; i++) {
                if (room[i].room_num == player_room) { room.RemoveAt(i); }
            }
        }
        //除外後に部屋がなければnullを返す
        if (room.Count == 0) { return null; }
        //部屋をランダムに選ぶ
        int j = Random.Range(0, room.Count);
        //部屋内の座標を保存する変数
        int[] pos = new int[2];
        //他キャラと被っていいならランダムに返すだけ
        if (chara_same == true) {
                pos[(int)XY.X] = Random.Range(room[j].Room_start_point[(int)XY.X], room[j].Room_end_point[(int)XY.X]);
                pos[(int)XY.Y] = Random.Range(room[j].Room_start_point[(int)XY.Y], room[j].Room_end_point[(int)XY.Y]);
        }
        //他キャラとかぶってはいけないならその座標を確認しつつ返す
        else {
            do {
                pos[(int)XY.X] = Random.Range(room[j].Room_start_point[(int)XY.X], room[j].Room_end_point[(int)XY.X]);
                pos[(int)XY.Y] = Random.Range(room[j].Room_start_point[(int)XY.Y], room[j].Room_end_point[(int)XY.Y]);
            }
            while (chara_man.Can_Move_pos(pos, -1) != Can_Move_Tile.Can_Move);
        }

        return Return_int2(pos);
    }

    public int[] Random_Room_Pos_Item() {
        //部屋情報を取得
        List<Room_Data> room = Room_List;
        //部屋をランダムに選ぶ
        int j = Random.Range(0, room.Count);
        //部屋内の座標を保存する変数
        int[] pos = new int[2];
        //他のアイテムと被っていない座標をposに格納
            do {
                pos[(int)XY.X] = Random.Range(room[j].Room_start_point[(int)XY.X], room[j].Room_end_point[(int)XY.X]);
                pos[(int)XY.Y] = Random.Range(room[j].Room_start_point[(int)XY.Y], room[j].Room_end_point[(int)XY.Y]);
            }
            while(item_pos(pos)==false);
        return Return_int2(pos);
    }

    //すでに床落ちしているアイテムと座標が被ればtrue、被らなければfalse
    bool item_pos(int[] pos) {
        for (int i = 0; i < GameMaster.Instance.item_Man.on_stage_items.Count; i++) {
            if (pos == GameMaster.Instance.item_Man.on_stage_items[i].item_pos) { return true; }
        }
        return true;
    }
    //与えられた座標がどの部屋に値するかを返す、部屋でない場合は-1を返す
    public int int2_to_Room(int[] pos) {
        try {
            if (stage[pos[(int)XY.Y], pos[(int)XY.X]].tile != Tile_Type.Room_Floor) { return -1; }
        }
        catch (System.IndexOutOfRangeException) {
            Debug.LogError(point_log(pos));
            return -1;
        }
        for(int i = 0; i < Room_List.Count; i++) {
            if ((Room_List[i].Room_start_point[(int)XY.X]<= pos[(int)XY.X] && pos[(int)XY.X] <= Room_List[i].Room_end_point[(int)XY.X])
                && (Room_List[i].Room_start_point[(int)XY.Y] <= pos[(int)XY.Y] && pos[(int)XY.Y] <= Room_List[i].Room_end_point[(int)XY.Y])) {
                return i;
            }
        }
        return -1;
    }

    //タイル座標をワールド座標に直す
   public Vector3 Tile_To_World(int[] pos) {
       return floor_maps.GetCellCenterWorld(new Vector3Int(pos[(int)XY.X], pos[(int)XY.Y], 0));
    }
    //ワールド座標をタイル座標に直す
   public int[] World_To_Tile(Vector3 vec) {
       Vector3 pos= floor_maps.WorldToCell(vec);
      return Return_int2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
    }

    //2点間の距離を返す
    public float Tile_Distance(int[]pos_1,int[] pos_2) {
        return Mathf.Pow(pos_1[(int)XY.X]-pos_2[(int)XY.X],2)+ Mathf.Pow(pos_1[(int)XY.Y]- pos_2[(int)XY.Y],2);
    }
    //2点間のマス目単位での距離を返す(pos_1の周囲8マスでは1,さらにその周囲16マスでは2…)
    public int Tile_Distance_8dir(int[] pos_1, int[] pos_2) {
        return Mathf.Max(Mathf.Abs(pos_1[(int)XY.X] - pos_2[(int)XY.X]), Mathf.Abs(pos_1[(int)XY.Y] - pos_2[(int)XY.Y]));
    }

    //与えられた2点が等しいかを返す
    public bool Pos_Equal(int[] pos1,int[] pos2) {
        return (pos1[0] == pos2[0] && pos1[1] == pos2[1]);
    }
    //与えられた2点が隣り合っているかを返す
    public bool Pos_Equal_four(int[] pos1, int[] pos2) {
        return ((pos1[0] == pos2[0] && pos1[1] == pos2[1]) ||
            (pos1[0] == pos2[0]-1 && pos1[1] == pos2[1]) ||
            (pos1[0] == pos2[0]+1 && pos1[1] == pos2[1]) ||
            (pos1[0] == pos2[0] && pos1[1] == pos2[1]-1)||
            (pos1[0] == pos2[0] && pos1[1] == pos2[1]+1));
    }
    //座標posと方向dirを渡すと、その座標からdir方向に1マス分移動したマスをかえす
    public int[] Dir_Next_Pos(int[] pos, int dir) {
        //もしdirが1~9でなければnullを返す
        if ((dir > 0 || dir < 10) == false) { return pos; }
        //posをdir方向に動かした座標としてnext_posを宣言
        int[] next_pos = new int[2];
        next_pos[(int)XY.X] = pos[(int)XY.X] + (((dir - 1) % 3) - 1);
        next_pos[(int)XY.Y] = pos[(int)XY.Y] + (((dir - 1) / 3) - 1);
        return next_pos;
    }

    //centerがtargetの周囲8マスにあるかどうかを返す
    public bool Near_Target_8(int[] center,int[] target) {
       return (Mathf.Abs(center[0] - target[0]) <= 1 && Mathf.Abs(center[1] - target[1]) <= 1);
    }
    //centerがtargetの縦横マスにあるかどうかを返す
    public bool Near_Target_4(int[] center, int[] target) {
        int y_def = Mathf.Abs(center[0] - target[0]);
        int x_def = Mathf.Abs(center[1] - target[1]);
        return ((y_def <= 1 && x_def <= 1)&& (x_def+y_def)<=1);
    }
    //参照しないよう値のみを返す関数
    static public int[] Return_int2(int[] pos) {
        int[] tmp = new int[2];
        tmp[(int)XY.X] = pos[(int)XY.X];
        tmp[(int)XY.Y] = pos[(int)XY.Y];
        return tmp;
    }
    static public int[] Return_int2(int x, int y) {
        int[] tmp = new int[2];
        tmp[(int)XY.Y] = y;
        tmp[(int)XY.X] = x;
        return tmp;
    }
    //与えられた座標を文字列にして返す
    static public string point_log(int[] p) {
        if (p == null) { return "(null)"; }
        return ("(" + p[(int)XY.X] + "," + p[(int)XY.Y] + ")");
    }
   
    //ミニマップ関係
    [Header("ミニマップ関係")]
    //描画するレイヤー
    public Tilemap mini_ground;
    public Tilemap mini_chara;
    public Tilemap mini_item;
    //描画するタイル情報
    public TileBase Hall_floor_tile;
    public TileBase Room_floor_tile;
    public TileBase Next_Door_tile;
    public TileBase player_tile;
    public TileBase enemy_tile;
    public TileBase item_tile;
    //敵の位置を問答無用で描画するフラグ
    public bool enemy_view;
    //ミニマップの表示
     void Minimap_view() {
        Mini_Reset();
        //プレイヤーのいるマスを開ける
        GameMaster.Instance.chara_Man.Player_chara.Mapping_Update();
        //床タイル描画
        for (int y = 0; y < Now_Height_Max; y++) {
            for (int x = 0; x < Now_Width_Max; x++) {
                //そのマスが表示化されていれば表示
                if (stage[y, x].mapping) {
                    switch (stage[y, x].tile) {
                        case Tile_Type.Hall_Floor:
                            mini_ground.SetTile(new Vector3Int(x, y, 0), Hall_floor_tile);
                            break;
                        case Tile_Type.Room_Floor:
                            mini_ground.SetTile(new Vector3Int(x, y, 0), Room_floor_tile);
                            break;
                        case Tile_Type.Next_Stage:
                            mini_ground.SetTile(new Vector3Int(x, y, 0), Next_Door_tile);
                            break;
                        default:

                            break;
                    }
                }
            }
        }
        //アイテム描画
        GameMaster.Instance.item_Man.Item_Mapping();//アイテムを表示するか否かの情報の更新
        List<Item_puton> item=GameMaster.Instance.item_Man.on_stage_items;
        for(int i=0;i< item.Count; i++) {
            if (item[i].mapping == true) {
                mini_item.SetTile(new Vector3Int(item[i].item_pos[(int)XY.X], item[i].item_pos[(int)XY.Y], 0), item_tile);
            }
        }
        int player_room = int2_to_Room(chara_man.Player_chara.now_position);
        //キャラの表示
        for (int i = 0; i < chara_man.chara_list.Count; i++) {
            //キャラが存在していれば
            if (chara_man.chara_list[i].gameObject != null) {
                //キャラがプレイヤーと同じ部屋にいる、もしくはmapping_distance以内の距離にいる、もしくは敵キャラ描画フラグが立っているならキャラを描画
                int now_room = int2_to_Room(chara_man.chara_list[i].now_position);
                if (chara_man.chara_list[i].tag == "Player" ||
                    enemy_view == true ||
                    (now_room != -1 && now_room == player_room) ||
                    (Tile_Distance_8dir(chara_man.chara_list[i].now_position, chara_man.Player_chara.now_position) <= mapping_distance)) {
                    mini_chara.SetTile(new Vector3Int(chara_man.chara_list[i].now_position[(int)XY.X], chara_man.chara_list[i].now_position[(int)XY.Y], 0),
                        chara_man.chara_list[i].tag == "Player" ? player_tile : enemy_tile);
                }
            }
        }
    }
   public void Mini_Reset() {
        mini_ground.ClearAllTiles();
        mini_chara.ClearAllTiles();
        mini_item.ClearAllTiles();
    }
    //ミニマップに情報を公開
    public void Minimap_Full_view(bool map,bool item,bool enemy) {
        //マップ形状を公開する
        if (map) {
            for (int y = 0; y < Now_Height_Max; y++) {
                for (int x = 0; x < Now_Width_Max; x++) {
                    stage[y, x].mapping = true;
                }
            }
        }
        //アイテム位置を公開する
        if (item) {
            List<Item_puton> item_list = GameMaster.Instance.item_Man.on_stage_items;
          for (int i=0;i< item_list.Count; i++) {
                item_list[i].mapping = true;
            }
        }
        //敵の位置を公開する
        if (enemy) {
            enemy_view = true;
        }
        Minimap_view();
    }
    
}
public class Room_Data {
    //部屋番号
    public int room_num;
    //部屋の存在するblock番号
    public int room_block;
    //部屋の出入り口
    public List<int[]> room_doorway = new List<int[]>();

    public int[] Room_start_point;
    public int[] Room_end_point;
    //ミニマップに表示済みか否か
    public bool mapping;
    public Room_Data(int num, int block) {
        room_num = num;
        room_block = block;
        mapping = false;
    }
}

public class Map_Tile {
    public Tile_Type tile;
    public bool mapping;
    public Map_Tile() {
        tile = Tile_Type.Unbreakable_Wall;
        mapping = false;
    }

}
