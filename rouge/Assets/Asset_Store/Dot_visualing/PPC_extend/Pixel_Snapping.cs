using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Pixel_Snapping: MonoBehaviour {
    //更新前、更新後の座標
    private Vector3 cashPosition;
    private Vector3 nextPosition;
    //画面内のドットの数の検出に利用
    private PixelPerfectCamera pixel_camera;

    void Start() {
        //もしこれがアタッチされているオブジェクトがカメラそのものであるならそのカメラにアタッチされたPPCを
        if (GetComponent<Camera>()) {
            pixel_camera = GetComponent<PixelPerfectCamera>();
        }
        //カメラでなければキャラカメラのPPCを取得
        else {
            pixel_camera = GameMaster.Instance.chara_camera.GetComponent<PixelPerfectCamera>();
        }
    }
    void OnRenderObject() {
#if UNITY_EDITOR
        //描画用の位置から本来の位置に戻す
        if (((Camera.current.cullingMask & (1 << transform.gameObject.layer)) == 1) &&
            (UnityEditor.SceneView.lastActiveSceneView != null ?
                !(Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera) :
                true)
        ) {
            transform.position = cashPosition;
        }
#else
//描画用の位置から本来の位置に戻す
        if ((Camera.current.cullingMask & (1 << transform.gameObject.layer)) == 1) {
            transform.position = cashPosition;
		}
#endif
    }
    void LateUpdate() {
        //描画用に位置を移動
            transform.position = change_vec3(transform.position);
    }

    Vector3 change_vec3(Vector3 vec) {
        //現在の物理的位置を保存
        cashPosition = vec;
        //描画用位置の計算
        nextPosition = pixel_camera.RoundToPixel(cashPosition);
        //描画用位置の代入処理
       return nextPosition;
    }
}