using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Manager : MonoBehaviour
{
    [EnumIndex(typeof(Effect_Type))]
    public List<GameObject> Effects; 

    public enum Effect_Type {
        damage,
        aid,
        Good_Effect,
        Bad_Effect,
        Level_Up,
        Level_Down,
        Masic
    }
    //与えられた座標にエフェクトを生成
    public void Effect_Start(Effect_Type type,Vector3 pos) {
       GameObject new_obj= Instantiate(Type_To_Obj(type), pos,Quaternion.identity);
        new_obj.layer= LayerMask.NameToLayer("Charactor");
    }
    //与えられた座標にエフェクトを生成、オブジェクト追従版
    public void Effect_Start(Effect_Type type, GameObject obj) {
        GameObject new_obj = Instantiate(Type_To_Obj(type),obj.transform);
        new_obj.layer = LayerMask.NameToLayer("Charactor");
    }
    //エフェクトタイプを与えるとそのエフェクト本体を返す
    GameObject Type_To_Obj(Effect_Type type) {
        return Effects[(int)type];
    }
}
