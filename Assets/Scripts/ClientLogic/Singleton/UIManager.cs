using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClientLogic.Singleton
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Make Player")] public InputField nicknameInput;

        [Header("In Playing")] public Text packetText;

        public void OnStartingInput()
        {
            nicknameInput.onEndEdit.AddListener(OnEnterNickname);
        }

        private void OnEnterNickname(string name)
        {
            var plr = Instantiate(Resources.Load<Player>("Prefabs/player"), Vector3.zero, Quaternion.identity);
            plr.name = name;
            plr.nickname = name;
            plr.isMine = true;
            plr.gameObject.AddComponent<CameraMover>();

            FindObjectOfType<SyncManager>().localPlayerName = name;
            
            nicknameInput.gameObject.SetActive(false);
        }

        public void SetPacketMessage(string msg)
        {
            packetText.text = msg;
        }
    }
}