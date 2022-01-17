using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParameterView : MonoBehaviour
{
    private Online_Manager on_man;
    public Text time_text;

   private float now_count;
    private bool view_start;
    [System.NonSerialized]
    public bool view_end;


    public GameObject players_obj_origin;//プレイヤー情報を示すオブジェクト
    private List<GameObject> player_obj_list = new List<GameObject>();//プレイヤー情報を占めるオブジェクトのリスト
    // Start is called before the first frame update
    void Start()
    {
        view_start = false;
        view_end = false;
        on_man = GameMaster.Instance.online_Man;
        players_obj_origin.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (view_start == true && view_end == false) {
            now_count -= Time.deltaTime;
            time_text.text = Mathf.FloorToInt(now_count+1).ToString();
            if (now_count <= 0f) {
                view_end = true;
            }
        }
    }

    public void Count_Start(float max_time) {
        now_count = max_time;
        view_start = true;
        for (int i = 0; i < Battle_Manager.Chara_Max; i++) {
            GameObject a = Instantiate(players_obj_origin, players_obj_origin.transform.parent);
            a.SetActive(true);
            player_obj_list.Add(a);
            player_obj_list[i].GetComponent<Image>().color = GameMaster.Instance.Players_Theme_Color[i];
            a.transform.localPosition = new Vector3(a.transform.localPosition.x * (i / 2 == 0 ? 1 : -1), a.transform.localPosition.y * (i % 2 == 0 ? 1 : -1), a.transform.localPosition.z);
            //処理簡略化のためにキャラクタ情報を取得
            Character chara = GameMaster.Instance.chara_Man.Charanum_To_Chara(i);
            //名前
            if (i < on_man.players_list.Count) {//プレイヤーが存在するなら
                player_obj_list[i].transform.GetChild(0).GetComponent<Text>().text = chara.chara_name;
            }
            else { //存在しないなら
                player_obj_list[i].transform.GetChild(0).GetComponent<Text>().text = "CPU " + (i + 1).ToString();
            }
            //プレイヤー番号
            player_obj_list[i].transform.GetChild(1).GetComponent<Text>().text = (i + 1).ToString() + "P";
            //レベル、HP、力
            player_obj_list[i].transform.GetChild(2).GetComponent<Text>().text = "Lv." + chara.level;
            player_obj_list[i].transform.GetChild(3).GetComponent<Text>().text = "HP:" + chara.max_HP + " POW:" + chara.max_Power;
            //所持アイテム
            GameObject itemslot = player_obj_list[i].transform.GetChild(4).gameObject;

                List<Item> items = GameMaster.Instance.battle_Man.players[i].have_Items;
            for (int j = 0; j < items.Count; j++) {
                //アイテム表示スロットを生成
                if (j > 0) {
                    itemslot = Instantiate(player_obj_list[i].transform.GetChild(4).gameObject, player_obj_list[i].transform).gameObject;
                    itemslot.transform.localPosition = new Vector3(itemslot.transform.localPosition.x + (j * 60), itemslot.transform.localPosition.y, itemslot.transform.localPosition.z);
                }
                //各アイテムを表示、持っていなければnullとして表示しない
                itemslot.transform.GetChild(0).GetComponent<Image>().sprite = (items[j] == null ? null : items[j].Item_sprite);
            }
        }
    } 

    
}
