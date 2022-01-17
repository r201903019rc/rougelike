using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Constraction;

public class Input_Manager : MonoBehaviour
{
    public PlayerInput Input;
    public InputActionMap map;
   private InputAction[] inputs = new InputAction[(int)INputers.MAX];

    public bool Can_Controll;//操作可能かどうかを返す
    // Start is called before the first frame update
    void Start()
    {
        get_inputs(Input);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public bool Get_button(INputers puts) {
        if (Can_Controll) {
            return (inputs[(int)puts].ReadValue<float>() == 1);
        }
        else { return false; }
    }
    public bool Get_button_Down(INputers puts) {
        if (Can_Controll) {
            return inputs[(int)puts].triggered;
    }
        else { return false; }
    }

    public bool Get_button(InputAction input) {
        if (Can_Controll) {
            return (input.ReadValue<float>() == 1);
        }
        else { return false; }
    }
    public bool Get_button_Down(InputAction input) {
        if (Can_Controll) {
            return input.triggered;
        }
        else { return false; }
    }


    public Vector2 Get_Move(bool wasd) {
        if (Can_Controll) {
            if (wasd == true) {
                return inputs[(int)INputers.IN_Move].ReadValue<Vector2>();
            }
            else {
                return inputs[(int)INputers.IN_Move_nonWASD].ReadValue<Vector2>();
            }
        }
        else { return Vector2.zero; }
    }
    public Vector2 Get_Move_Down(bool wasd) {
        if (Can_Controll&& inputs[(int)INputers.IN_Move].triggered) {
            if (wasd == true) {
                return inputs[(int)INputers.IN_Move].ReadValue<Vector2>();
            }
            else {
                return inputs[(int)INputers.IN_Move_nonWASD].ReadValue<Vector2>();
            }
        }
        else { return Vector2.zero; }
    }

    void get_inputs(PlayerInput playerInput) {
        inputs[(int)INputers.IN_Move] = playerInput.actions["Move"];
        inputs[(int)INputers.IN_Move_nonWASD] = playerInput.actions["Move_NonWASD"];
        inputs[(int)INputers.IN_Attack] = playerInput.actions["Attack"];
        inputs[(int)INputers.IN_Around] = playerInput.actions["Turn_Around"];
        inputs[(int)INputers.IN_Dash] = playerInput.actions["Dash"];
        inputs[(int)INputers.IN_L] = playerInput.actions["Item_L"];
        inputs[(int)INputers.IN_R] = playerInput.actions["Item_R"];
        inputs[(int)INputers.IN_Item] = playerInput.actions["Item_Use"];
        inputs[(int)INputers.IN_Cancel] = playerInput.actions["Cancel"];
        inputs[(int)INputers.IN_Decision] = playerInput.actions["Decision"];
        inputs[(int)INputers.IN_Skip] = playerInput.actions["Turn_Skip"];
        inputs[(int)INputers.IN_GameReset] = playerInput.actions["GameReset"];
        inputs[(int)INputers.IN_Debug] = playerInput.actions["Debug"];
    }
}
