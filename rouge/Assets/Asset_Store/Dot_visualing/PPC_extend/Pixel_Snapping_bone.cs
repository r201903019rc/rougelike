using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Pixel_Snapping_bone: MonoBehaviour {
    //更新前、更新後の座標
    private Vector3 cashPosition;
    private Vector3 nextPosition;
    //画面内のドットの数の検出に利用
    private PixelPerfectCamera pixel_camera;

    //ボーン関係
    private bool use_bone_snap; 
    private Animator anim;
    public bone bones;
    public List<HumanBodyBones> bones_List=new List<HumanBodyBones>();
    public Vector3[] cash_list;
    public Vector3[] next_list;

    public enum bone {
        Non=0,
        Hips = 1<<0,
        LeftUpperLeg = 1 << 1,
        RightUpperLeg = 1 << 2,
        LeftLowerLeg = 1 << 3,
        RightLowerLeg = 1 << 4,
        LeftFoot = 1 << 5,
        RightFoot = 1 << 6,
        Spine = 1 << 7,
        Chest = 1 << 8,
        Neck = 1 << 9,
        Head = 1 << 10,
        LeftShoulder = 1 << 11,
        RightShoulder = 1 << 12,
        LeftUpperArm = 1 << 13,
        RightUpperArm = 1 << 14,
        LeftHand = 1 << 17,
        RightHand = 1 << 18,
        LeftToes = 1 << 19,
        RightToes = 1 << 20,
        MAX=24
    }
    void Start() {
        pixel_camera = GameMaster.Instance.chara_camera.GetComponent<PixelPerfectCamera>();
        anim = GetComponent<Animator>();
        bones_List = IntToBones((int)bones);
        use_bone_snap = !(anim == null || bones == bone.Non);
        if (use_bone_snap) {
            cash_list =new Vector3[bones_List.Count];
            next_list = new Vector3[bones_List.Count];
        }
    }
    void OnRenderObject() {
#if UNITY_EDITOR
        //描画用の位置から本来の位置に戻す
        if ((Camera.current == GameMaster.Instance.chara_camera) &&
            (UnityEditor.SceneView.lastActiveSceneView != null ?
                !(Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera) :
                true)
        ) {
            //ボーンも同様に処理
            if (use_bone_snap) {
                for (int i = 0; i < bones_List.Count; i++) {
                    anim.GetBoneTransform(bones_List[i]).position = cash_list[i];
                }
            }
            else { transform.position = cashPosition; }
        }
#else
        //描画用の位置から本来の位置に戻す
        if (Camera.current==GameMaster.Instance.chara_camera) {
            //ボーンも同様に処理
            if (use_bone_snap) {
                for (int i = 0; i < bones_List.Count; i++) {
                    anim.GetBoneTransform(bones_List[i]).position = cash_list[i];
                }
            }
            else { transform.position = cashPosition; }
        }
#endif
    }
    void LateUpdate() {
        
        //ボーンも同様に処理
        if (use_bone_snap) {
            for (int i = 0; i < bones_List.Count; i++) {
               change_vec3_bone(anim.GetBoneTransform(bones_List[i]).position, i);
            }
            for (int i = 0; i < bones_List.Count; i++) {
                anim.GetBoneTransform(bones_List[i]).position = next_list[i];
                    }
        }
        else {//描画用に位置を移動
            transform.position = change_vec3(transform.position);
        }
    }

    Vector3 change_vec3(Vector3 vec) {
        //現在の物理的位置を保存
        cashPosition = vec;
        //描画用位置の計算
        nextPosition = pixel_camera.RoundToPixel(cashPosition);
        //描画用位置の代入処理
       return nextPosition;
    }
    Vector3 change_vec3_bone(Vector3 vec,int list_num) {
        //現在の物理的位置を保存
        cash_list[list_num] = vec;
        //描画用位置の計算
        next_list[list_num] = pixel_camera.RoundToPixel(cash_list[list_num]);
        //描画用位置の代入処理
        return next_list[list_num];
    }

   public List<HumanBodyBones> IntToBones(int t) {
        List<HumanBodyBones> list=new List<HumanBodyBones>();
        int i = 0;
            while ((t != 0) || (i > (int)bone.MAX)) {
                string a = System.Convert.ToString(t, 2);
                if ((a[a.Length - 1]) == '1') {
                    list.Add((HumanBodyBones)i);
                }
                t = t >> 1;
                i++;
            }
        
        return list;
    }
}