// GENERATED AUTOMATICALLY FROM 'Assets/Script/Input/like.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Like : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Like()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""like"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""81f718f7-12e6-4d8e-a41c-d0c506bea464"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""d4bb446d-f783-4004-92a6-afcc67af43c4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move_NonWASD"",
                    ""type"": ""Value"",
                    ""id"": ""9457a3c2-6fd6-4cdd-a3b3-a1f9fac45ea4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""bb67b631-79ce-4835-ae5d-9f6b7355048e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Turn_Around"",
                    ""type"": ""Button"",
                    ""id"": ""f50c34d6-3463-4f80-b09a-eaed2c1f43d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dash"",
                    ""type"": ""Button"",
                    ""id"": ""504ad7f0-dcb6-4b3f-b93a-f410bb392f14"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold""
                },
                {
                    ""name"": ""Item_L"",
                    ""type"": ""Button"",
                    ""id"": ""bccfdbe4-b49f-47b3-813a-4959f1431cfa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Item_R"",
                    ""type"": ""Button"",
                    ""id"": ""09ca71eb-afdd-44bb-9cde-2514b19775e7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Item_Use"",
                    ""type"": ""Button"",
                    ""id"": ""615e2a12-a10c-4fdd-9c70-c287947878bf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Turn_Skip"",
                    ""type"": ""Button"",
                    ""id"": ""7491ebb2-fcb7-4dcb-a4cd-a72644fa735a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Debug_Stage_reset"",
                    ""type"": ""Button"",
                    ""id"": ""4a352b30-8fff-4fa8-9fa5-19bcec2f39c1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Decision"",
                    ""type"": ""Button"",
                    ""id"": ""bf5fec1e-dd79-47e9-b2b7-d03002c01969"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""0ffaf4f2-ef77-47c9-a8bb-690a2548f3fa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""GameReset"",
                    ""type"": ""Button"",
                    ""id"": ""ea5600b3-7dcb-4898-9fdc-a9844f081807"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Debug"",
                    ""type"": ""Button"",
                    ""id"": ""19d5cd6c-d056-4423-a575-5e8eade9ed88"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""978bfe49-cc26-4a3d-ab7b-7d7a29327403"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""00ca640b-d935-4593-8157-c05846ea39b3"",
                    ""path"": ""Dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e2062cb9-1b15-46a2-838c-2f8d72a0bdd9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8180e8bd-4097-4f4e-ab88-4523101a6ce9"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""320bffee-a40b-4347-ac70-c210eb8bc73a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1c5327b5-f71c-4f60-99c7-4e737386f1d1"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d2581a9b-1d11-4566-b27d-b92aff5fabbc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""2e46982e-44cc-431b-9f0b-c11910bf467a"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fcfe95b8-67b9-4526-84b5-5d0bc98d6400"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""77bff152-3580-4b21-b6de-dcd0c7e41164"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d12f1a87-620f-4b41-9aa8-622b2cdd5286"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn_Around"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fc077b92-06ab-4a00-a604-bf382ba2cce1"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Turn_Around"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0de588e2-571b-45fe-87a5-97fa27611162"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f463b15a-cd6a-4fca-adce-469b89f921d5"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f34cd96-b1db-4ca8-9e49-556f9def579d"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Item_L"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1b4b75a2-bc43-4c0f-a3b5-c91364b6e027"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Item_L"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c8cc784f-e4f4-4cc2-99cf-377fd8fe2511"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Item_R"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ea06325-8702-45ac-8932-b8497d429a5e"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Item_R"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d8bbdf04-cfb7-4ffb-9b09-907932f54e3d"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Item_Use"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4d86d731-265c-4050-a558-19842901ad17"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Item_Use"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bc99a97d-dbfc-4047-aab6-36b69f01ffb8"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn_Skip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""82012f14-531d-4c34-83e3-29c19ce9796f"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d7f97508-21c3-496c-9475-bbae83b9f864"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66ab56de-050f-4f70-9108-d2d3c15f1dde"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Debug_Stage_reset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""26cdb636-dc55-4c5c-9521-89c5c0a535e9"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Decision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c27c5c1-5f76-4f4f-aee0-d24c590aec27"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Decision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5d3fbedf-50e5-4007-9723-1ba9b32e6476"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""afce98d8-937c-4ea3-92c6-1d1286cfc760"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8b0d8821-0d13-48fc-9899-dc1e54931a44"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""40a8caba-5f4f-4491-96cb-37dfcf46feed"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""1217629b-c60a-40ff-89f5-ef4c9f1b201e"",
                    ""path"": ""Dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""79c393eb-20c7-4a6c-80c5-131d3cb2784a"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ca61e453-5224-45f2-8bd2-6ed65eb89ffe"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8f720c33-5fc6-4f7e-b9e4-c5b6d89519cb"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fb4bbacd-addf-430b-b850-66e0e1b4dded"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move_NonWASD"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""391e63ea-6c14-48a7-b963-cc7dcb11c2a3"",
                    ""path"": ""<Keyboard>/f12"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""GameReset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c5f74e61-6738-49da-b420-3ee1a4e28126"",
                    ""path"": ""<Keyboard>/f5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Debug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""59a78831-93e9-428d-9b15-bfd057f1c9b2"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""f07fe02d-aa58-4054-acdb-269ad4d53e16"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Decision"",
                    ""type"": ""Button"",
                    ""id"": ""67d232d8-61c3-45f2-9fe2-3c63dcd15efe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""48c00413-d086-445c-abc9-957d7e04a25e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""918d6698-5a7a-472b-a326-963baf3073b5"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""5f013552-8b69-4e0a-bc82-35b81c83142f"",
                    ""path"": ""Dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""7d679b1b-dd15-4faa-bdfc-dbc57d291bbd"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""f81fb7ba-3204-4ab6-95f2-4f0c95e106d0"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b2b0312d-31f5-4de1-b8e1-fe9b95dcd630"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""da33177a-50a8-4327-9788-0ac38fd87f07"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""7ce94cc5-413b-4014-ad56-d02f83b911d9"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""07294bb9-50c6-4d77-9213-3f02fc3df0e5"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8fa65cca-7222-460a-8ac5-58dfe20a5535"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""a030d7c6-78b3-4464-b348-49360c819607"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""fb5457fc-fb44-448a-8f33-ca4a538cee8b"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Decision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65bc4699-4dfc-4dea-ab7e-2ec45d494f2a"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Decision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""200c1d94-40af-4949-981a-03f58b8879c7"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""50853a16-7d78-4de0-9f39-cac22e0a1346"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Move_NonWASD = m_Player.FindAction("Move_NonWASD", throwIfNotFound: true);
        m_Player_Attack = m_Player.FindAction("Attack", throwIfNotFound: true);
        m_Player_Turn_Around = m_Player.FindAction("Turn_Around", throwIfNotFound: true);
        m_Player_Dash = m_Player.FindAction("Dash", throwIfNotFound: true);
        m_Player_Item_L = m_Player.FindAction("Item_L", throwIfNotFound: true);
        m_Player_Item_R = m_Player.FindAction("Item_R", throwIfNotFound: true);
        m_Player_Item_Use = m_Player.FindAction("Item_Use", throwIfNotFound: true);
        m_Player_Turn_Skip = m_Player.FindAction("Turn_Skip", throwIfNotFound: true);
        m_Player_Debug_Stage_reset = m_Player.FindAction("Debug_Stage_reset", throwIfNotFound: true);
        m_Player_Decision = m_Player.FindAction("Decision", throwIfNotFound: true);
        m_Player_Cancel = m_Player.FindAction("Cancel", throwIfNotFound: true);
        m_Player_GameReset = m_Player.FindAction("GameReset", throwIfNotFound: true);
        m_Player_Debug = m_Player.FindAction("Debug", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Move = m_UI.FindAction("Move", throwIfNotFound: true);
        m_UI_Decision = m_UI.FindAction("Decision", throwIfNotFound: true);
        m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Move_NonWASD;
    private readonly InputAction m_Player_Attack;
    private readonly InputAction m_Player_Turn_Around;
    private readonly InputAction m_Player_Dash;
    private readonly InputAction m_Player_Item_L;
    private readonly InputAction m_Player_Item_R;
    private readonly InputAction m_Player_Item_Use;
    private readonly InputAction m_Player_Turn_Skip;
    private readonly InputAction m_Player_Debug_Stage_reset;
    private readonly InputAction m_Player_Decision;
    private readonly InputAction m_Player_Cancel;
    private readonly InputAction m_Player_GameReset;
    private readonly InputAction m_Player_Debug;
    public struct PlayerActions
    {
        private @Like m_Wrapper;
        public PlayerActions(@Like wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Move_NonWASD => m_Wrapper.m_Player_Move_NonWASD;
        public InputAction @Attack => m_Wrapper.m_Player_Attack;
        public InputAction @Turn_Around => m_Wrapper.m_Player_Turn_Around;
        public InputAction @Dash => m_Wrapper.m_Player_Dash;
        public InputAction @Item_L => m_Wrapper.m_Player_Item_L;
        public InputAction @Item_R => m_Wrapper.m_Player_Item_R;
        public InputAction @Item_Use => m_Wrapper.m_Player_Item_Use;
        public InputAction @Turn_Skip => m_Wrapper.m_Player_Turn_Skip;
        public InputAction @Debug_Stage_reset => m_Wrapper.m_Player_Debug_Stage_reset;
        public InputAction @Decision => m_Wrapper.m_Player_Decision;
        public InputAction @Cancel => m_Wrapper.m_Player_Cancel;
        public InputAction @GameReset => m_Wrapper.m_Player_GameReset;
        public InputAction @Debug => m_Wrapper.m_Player_Debug;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move_NonWASD.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove_NonWASD;
                @Move_NonWASD.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove_NonWASD;
                @Move_NonWASD.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove_NonWASD;
                @Attack.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Turn_Around.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Around;
                @Turn_Around.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Around;
                @Turn_Around.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Around;
                @Dash.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @Dash.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @Dash.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @Item_L.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_L;
                @Item_L.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_L;
                @Item_L.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_L;
                @Item_R.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_R;
                @Item_R.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_R;
                @Item_R.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_R;
                @Item_Use.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_Use;
                @Item_Use.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_Use;
                @Item_Use.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnItem_Use;
                @Turn_Skip.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Skip;
                @Turn_Skip.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Skip;
                @Turn_Skip.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTurn_Skip;
                @Debug_Stage_reset.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug_Stage_reset;
                @Debug_Stage_reset.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug_Stage_reset;
                @Debug_Stage_reset.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug_Stage_reset;
                @Decision.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDecision;
                @Decision.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDecision;
                @Decision.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDecision;
                @Cancel.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @GameReset.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnGameReset;
                @GameReset.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnGameReset;
                @GameReset.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnGameReset;
                @Debug.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug;
                @Debug.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug;
                @Debug.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDebug;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Move_NonWASD.started += instance.OnMove_NonWASD;
                @Move_NonWASD.performed += instance.OnMove_NonWASD;
                @Move_NonWASD.canceled += instance.OnMove_NonWASD;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Turn_Around.started += instance.OnTurn_Around;
                @Turn_Around.performed += instance.OnTurn_Around;
                @Turn_Around.canceled += instance.OnTurn_Around;
                @Dash.started += instance.OnDash;
                @Dash.performed += instance.OnDash;
                @Dash.canceled += instance.OnDash;
                @Item_L.started += instance.OnItem_L;
                @Item_L.performed += instance.OnItem_L;
                @Item_L.canceled += instance.OnItem_L;
                @Item_R.started += instance.OnItem_R;
                @Item_R.performed += instance.OnItem_R;
                @Item_R.canceled += instance.OnItem_R;
                @Item_Use.started += instance.OnItem_Use;
                @Item_Use.performed += instance.OnItem_Use;
                @Item_Use.canceled += instance.OnItem_Use;
                @Turn_Skip.started += instance.OnTurn_Skip;
                @Turn_Skip.performed += instance.OnTurn_Skip;
                @Turn_Skip.canceled += instance.OnTurn_Skip;
                @Debug_Stage_reset.started += instance.OnDebug_Stage_reset;
                @Debug_Stage_reset.performed += instance.OnDebug_Stage_reset;
                @Debug_Stage_reset.canceled += instance.OnDebug_Stage_reset;
                @Decision.started += instance.OnDecision;
                @Decision.performed += instance.OnDecision;
                @Decision.canceled += instance.OnDecision;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @GameReset.started += instance.OnGameReset;
                @GameReset.performed += instance.OnGameReset;
                @GameReset.canceled += instance.OnGameReset;
                @Debug.started += instance.OnDebug;
                @Debug.performed += instance.OnDebug;
                @Debug.canceled += instance.OnDebug;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Move;
    private readonly InputAction m_UI_Decision;
    private readonly InputAction m_UI_Cancel;
    public struct UIActions
    {
        private @Like m_Wrapper;
        public UIActions(@Like wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_UI_Move;
        public InputAction @Decision => m_Wrapper.m_UI_Decision;
        public InputAction @Cancel => m_Wrapper.m_UI_Cancel;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMove;
                @Decision.started -= m_Wrapper.m_UIActionsCallbackInterface.OnDecision;
                @Decision.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnDecision;
                @Decision.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnDecision;
                @Cancel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Decision.started += instance.OnDecision;
                @Decision.performed += instance.OnDecision;
                @Decision.canceled += instance.OnDecision;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
            }
        }
    }
    public UIActions @UI => new UIActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_TouchSchemeIndex = -1;
    public InputControlScheme TouchScheme
    {
        get
        {
            if (m_TouchSchemeIndex == -1) m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
            return asset.controlSchemes[m_TouchSchemeIndex];
        }
    }
    private int m_JoystickSchemeIndex = -1;
    public InputControlScheme JoystickScheme
    {
        get
        {
            if (m_JoystickSchemeIndex == -1) m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
            return asset.controlSchemes[m_JoystickSchemeIndex];
        }
    }
    private int m_XRSchemeIndex = -1;
    public InputControlScheme XRScheme
    {
        get
        {
            if (m_XRSchemeIndex == -1) m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
            return asset.controlSchemes[m_XRSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnMove_NonWASD(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnTurn_Around(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnItem_L(InputAction.CallbackContext context);
        void OnItem_R(InputAction.CallbackContext context);
        void OnItem_Use(InputAction.CallbackContext context);
        void OnTurn_Skip(InputAction.CallbackContext context);
        void OnDebug_Stage_reset(InputAction.CallbackContext context);
        void OnDecision(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnGameReset(InputAction.CallbackContext context);
        void OnDebug(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnDecision(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
    }
}
