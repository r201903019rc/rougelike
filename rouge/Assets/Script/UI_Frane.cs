using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Frane : MonoBehaviour {
    private Image[] frame_images;//フレームのオブジェクトについているImage
    public GameObject frames;
    // Start is called before the first frame update
    void Start()
    {
        frame_images = frames.transform.GetComponentsInChildren<Image>();
    }

    //与えられた色にフレームの色を変える
  public void Change_Color(Color change_color) {
        for(int i = 0; i < frame_images.Length; i++) {
            frame_images[i].color = change_color;
        }
    }
}
