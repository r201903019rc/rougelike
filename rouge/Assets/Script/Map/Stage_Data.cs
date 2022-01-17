using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Stage_Data : MonoBehaviour
{
    public string stage_name;//ステージ名
    [Header("画像関係")]
    public TileBase floor_tile;//床の画像
    public TileBase wall_tile;//壁の画像
    public TileBase stair_tile;//階段の画像
    public List<GameObject> wall_object;//壁のオブジェクト
    [Range(1,100)]
    public int wall_prob=50;//壁オブジェクトが生成される確率
    [Header("部屋の形関係")]
    public int Block_X_num = 5;//横方向のブロック数
    public int Block_Y_num = 3;//縦方向のブロック数
    public int Room_num = 8;//部屋数
}
