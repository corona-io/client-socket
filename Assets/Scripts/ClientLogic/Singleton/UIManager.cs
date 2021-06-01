using System.Collections;
using System.Collections.Generic;
using MyExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClientLogic.Singleton
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Make Player")]
        public InputField nicknameInput;

        [Header("Dead Event")]
        public Image deadImage;

        public void OnStartingInput()
        {
            nicknameInput.onEndEdit.AddListener(OnEnterNickname);
        }

        private void OnEnterNickname(string name)
        {
            if (name.Contains(",")) return;
            
            var plr = Instantiate(Resources.Load<Player>("Prefabs/player"), Vector3.zero, Quaternion.identity);
            plr.name = name;
            plr.nickname = name;
            plr.isMine = true;
            plr.gameObject.AddComponent<CameraMover>();

            FindObjectOfType<SyncManager>().localPlayerName = name;
            
            nicknameInput.gameObject.SetActive(false);
        }

        public UnityAction OnChangeHealthPoint(Player plr, Image img)
        {
            return () =>
            {
                var hp = plr.HealthPoint;
                img.fillAmount = hp / 100f;
            };
        }

        public void ShowDeadEvent()
        {
            deadImage.gameObject.SetActive(true);
            StartCoroutine(ExecuteDeadEvent());
        }
        
        private IEnumerator ExecuteDeadEvent()
        {
            while (deadImage.color.a < 1f)
            {
                var color = deadImage.color;
                color.a += Time.deltaTime;
                deadImage.color = color;
                yield return null;
            }

            SceneManager.LoadScene(0);
        }
    }
}