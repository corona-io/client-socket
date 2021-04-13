using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClientLogic.Singleton
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Make Player")]
        public InputField nicknameInput;

        public void OnStartingInput()
        {
            nicknameInput.onEndEdit.AddListener(OnEnterNickname);
        }

        private void OnEnterNickname(string name)
        {
            Instantiate(Resources.Load("Prefabs/player"), Vector3.zero, Quaternion.identity).name = name;
            nicknameInput.gameObject.SetActive(false);
        }
    }
}
