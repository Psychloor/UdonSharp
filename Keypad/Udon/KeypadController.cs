using JetBrains.Annotations;

using UdonSharp;

using UnityEngine;

using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Psychloor.Keypad
{

    using System;

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KeypadController : UdonSharpBehaviour
    {
        // @formatter:off
        public string password = "123456";
        public bool isSynced;
        [Tooltip("Auto check if the password is right when it's the same length as the password")]
        public bool autoCheck = true;
        
        [Header("Unlocked")]
        public GameObject[] targetsToEnable;
        public GameObject[] targetsToDisable;

        [Header("Listener Events")]
        public UdonBehaviour listener;
        public string unlockedEventName = "_OnKeypadUnlocked";
        public string lockedEventName = "_OnKeypadLocked";

        [Header("Audio")]
        public AudioSource speaker;
        public AudioClip[] buttonPressedClips, errorClips, successClips;
    
        [UdonSynced, FieldChangeCallback(nameof(IsLocked))]
        private bool syncLocked = true;
        private bool localLocked = true;

        [HideInInspector]
        public string input = string.Empty;
        // @formatter:on
    
        private bool IsLocked
        {
            get => isSynced ? syncLocked : localLocked;
            set
            {
                if (isSynced)
                {
                    syncLocked = value;
                    if(Networking.IsOwner(gameObject))
                        RequestSerialization();
                }
                else
                {
                    localLocked = value;
                }
            
                UpdateState();
            }
        }
    
        [PublicAPI]
        public void _Button0()
        {
            PlayButtonClip();
            input += "0";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button1()
        {
            PlayButtonClip();
            input += "1";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button2()
        {
            PlayButtonClip();
            input += "2";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button3()
        {
            PlayButtonClip();
            input += "3";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button4()
        {
            PlayButtonClip();
            input += "4";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button5()
        {
            PlayButtonClip();
            input += "5";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button6()
        {
            PlayButtonClip();
            input += "6";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button7()
        {
            PlayButtonClip();
            input += "7";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button8()
        {
            PlayButtonClip();
            input += "8";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _Button9()
        {
            PlayButtonClip();
            input += "9";
            
            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void _ButtonConfirm()
        {
            PlayButtonClip();
            if (string.IsNullOrEmpty(input)) return;
            if (input.Equals(password, StringComparison.Ordinal))
            {
                if (isSynced)
                {
                    if (!Networking.IsOwner(gameObject))
                        Networking.SetOwner(Networking.LocalPlayer, gameObject);
                }

                IsLocked = false;

                _ButtonReset();
                if (isSynced)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlaySuccessClip));
                else
                    PlaySuccessClip();

            }
            else
            {
                _ButtonReset();
                PlayErrorClip();
            }
        }
    
        [PublicAPI]
        public void _ButtonReset()
        {
            PlayButtonClip();
            input = string.Empty;
        }

        public void _ButtonCustom(string text)
        {
            PlayButtonClip();
            input += text;

            if(autoCheck && input.Length == password.Length)
                _ButtonConfirm();
        }
    
        [PublicAPI]
        public void PlayButtonClip()
        {
            PlayClip(buttonPressedClips);
        }

        [PublicAPI]
        public void PlayErrorClip()
        {
            PlayClip(errorClips);
        }

        [PublicAPI]
        public void PlaySuccessClip()
        {
            PlayClip(successClips);
        }
    
        private void UpdateState()
        {
            foreach (GameObject obj in targetsToDisable)
            {
                obj.SetActive(IsLocked);
            }

            foreach (GameObject obj in targetsToEnable)
            {
                obj.SetActive(!IsLocked);
            }
        
            if (listener)
                listener.SendCustomEvent(IsLocked ? lockedEventName : unlockedEventName);
        }

        private void PlayClip(AudioClip[] clips)
        {
            if (!speaker || clips.Length == 0) return;
            // ReSharper disable once RedundantNameQualifier
            speaker.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }
    }

}
