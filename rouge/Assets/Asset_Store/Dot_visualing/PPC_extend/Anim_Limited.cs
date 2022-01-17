using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_Limited : MonoBehaviour {
	private Animator anim;

	//アニメーションを再生する頻度
	[Range(1, 60)]
	public int frame_speed = 15;

	//リスト内ののアニメーションに遷移したとき、瞬時にアニメーションを再生する(Idle→walkでもたついて滑ってるように見えないようwalkをリストに入れておく、とか)
	public List<string> anim_to_after;

	//時間のカウント
	private int count = 0;
	//現在再生されているアニメーション
	private AnimatorStateInfo animstate_now;
	//前フレーム再生されていたアニメーション
	private AnimatorStateInfo animstate_bef;

	//キャラの描画角度を更新する角度
	[Range(1, 360)]
	public int Limits_Angels=30;
	//実際の回転を保存しておく
	private Quaternion cash_rota;

	//角度制限をする方向
	public bool Limits_RotaX=true;
	public bool Limits_RotaY = true; 
	public bool Limits_RotaZ = true;

	void Start() {
		anim=GetComponent<Animator>();
	}
	void Update() {
		//frame_speedが1のときはなにもせず通常通り動かす
		if (frame_speed != 1) {
			//アニメーション遷移の調整
			listanim_st();
			//アニメーションをカクつかせる
			animation_off();
		}
	}
	void LateUpdate() {
		//本フレームの処理終了後に回転を階調化
		if (Limits_Angels > 1) rotation_off();
	}
	//オブジェクトの描画後に呼ばれる関数でオブジェクトの回転を実際のものに戻す
	void OnRenderObject() {
#if UNITY_EDITOR
		//カメラがオブジェクトを描画していれば(これがないとすべてのカメラで括弧内の処理が行われてバグる)
		if (((Camera.current.cullingMask & (1 << transform.gameObject.layer)) == 1) &&
			(UnityEditor.SceneView.lastActiveSceneView != null ?
				!(Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera) :
				true)
		) {
			//回転を戻す
			if (Limits_Angels > 1) transform.rotation = cash_rota;
		}
		 #else
		 //カメラがオブジェクトを描画していれば(これがないとすべてのカメラで括弧内の処理が行われてバグる)
		if ((Camera.current.cullingMask & (1 << transform.gameObject.layer)) == 1) {
			//回転を戻す
			if (Limits_Angels > 1) transform.rotation = cash_rota;
		}
#endif
	}
	//指定のアニメーションが開始されるときの処理
	void listanim_st() {
		if (anim_to_after.Count > 0) {
			//現在のアニメーションを取得
			animstate_now = anim.GetCurrentAnimatorStateInfo(0);
			for (int i = 0; i < anim_to_after.Count; i++) {
				if (animstate_now.shortNameHash.Equals(Animator.StringToHash(anim_to_after[i]))
					&& !animstate_bef.shortNameHash.Equals(Animator.StringToHash(anim_to_after[i]))) {
					count = frame_speed;
				}
			}
			animstate_bef = animstate_now;
		}
	}
	//アニメーションをカクつかせる処理
	void animation_off() {
		anim.enabled = (count % frame_speed == 0);
		count++;
		anim.speed = frame_speed;
	}

	//回転を制限する処理
	void rotation_off() {
		//実際の回転を保存
		cash_rota = transform.rotation;
		//弧度法から度数法に変換
		Vector3 rota_vec = transform.rotation.eulerAngles;
		//角度を階調化して弧度法に戻す
		Quaternion tmp = Quaternion.Euler(
			(Limits_RotaX?floor_rot(rota_vec.x): rota_vec.x),
			(Limits_RotaY ? floor_rot(rota_vec.y) : rota_vec.y),
			(Limits_RotaZ ? floor_rot(rota_vec.z) : rota_vec.z));
		//オブジェクトの回転に代入
		transform.rotation = tmp;
	}
	//引数を近似する処理
	int floor_rot(float v) {
		int tmp = (int)(v / Limits_Angels) * Limits_Angels;

		int t;
		if (v >= tmp + ((float)Limits_Angels / 2)){
			t= tmp + Limits_Angels;
		}
		else if (v >= tmp - ((float)Limits_Angels / 2)) {
			t= tmp;
		}
		else {
			t=tmp-Limits_Angels;
		}

		return t;
	}


}
