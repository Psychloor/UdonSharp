namespace Psychloor.Keypad
{

    using System;

    using UdonSharp;

    using UdonToolkit;

    using UnityEngine;
    
    public class KeypadButton : UdonSharpBehaviour
    {
        public KeypadController keypad;

        [Popup("@ButtonTypes")]
        public int buttonType;

        public string customInput = "A";

        [NonSerialized, HideInInspector]
        public string[] ButtonTypes = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Confirm", "Reset", "Custom" };
        private const int ConfirmButton = 10;
        private const int ResetButton = 11;
        private const int CustomButton = 12;
        
        
        
        public override void Interact()
        {
            switch (buttonType)
            {
                case ConfirmButton:
                    keypad._ButtonConfirm();
                    break;
                
                case ResetButton:
                    keypad._ButtonReset();
                    break;
                
                case CustomButton:
                    keypad._ButtonCustom(customInput);
                    break;

                default:
                    if (buttonType < 0 || buttonType > 9)
                    {
                        Debug.LogError($"{this}: Invalid Button Type", this);
                        break;
                    }

                    keypad._ButtonCustom(buttonType.ToString());
                    break;
            }
        }

    }

}