using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RusiWishvel
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject MenuBg;
        public GameObject Setting;
        public GameObject Story;

        public GameObject SettingSound;
        public GameObject SettingBGSound;

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public void Start()
        {
            SetData();
        }
        public void BtnStart()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void BtnSetting()
        {
            MenuBg.GetComponent<Animator>().SetTrigger("Close");
            Invoke("OpenSetting", 1.5f);
        }

        public void BtnBack()
        {
            Setting.GetComponent<Animator>().SetTrigger("Close");
            Invoke("OpenMenuBg", 1.5f);
        }

        public void OpenMenuBg()
        {
            MenuBg.GetComponent<Animator>().SetTrigger("Open");
        }
        public void BtnExit()
        {
            Application.Quit();
        }

        void OpenSetting()
        {
            Setting.SetActive(true);
            Setting.GetComponent<Animator>().SetTrigger("Open");
        }

        public void BtnBGSound()
        {
            if (SettingBGSound.GetComponent<Text>().text == "¹è°æÀ½¾Ç")
            {
                SettingBGSound.GetComponent<Text>().text = "¹è°æÀ½¾Ç ²û";
                GameDataManager.instance.isMusisc = 0;
     
            }
            else
            {
                SettingBGSound.GetComponent<Text>().text = "¹è°æÀ½¾Ç";
                GameDataManager.instance.isMusisc = 1;
            }
            GameDataManager.instance.SaveData();
        }

      

        public void BtnSound()
        {
            if (SettingSound.GetComponent<Text>().text == "È¿°úÀ½")
            {
                SettingSound.GetComponent<Text>().text = "È¿°úÀ½ ²û";
                GameDataManager.instance.isSound = 0;

            }
            else
            {
                SettingSound.GetComponent<Text>().text = "È¿°úÀ½";
                GameDataManager.instance.isSound = 1;
            }
        }

        public void SetData()
        {
            if(GameDataManager.instance.isMusisc == 0)
            {
                SettingBGSound.GetComponent<Text>().text = "¹è°æÀ½¾Ç";

            }
            else if(GameDataManager.instance.isMusisc == 0)
            {
                SettingBGSound.GetComponent<Text>().text = "¹è°æÀ½¾Ç ²û";
            }
            if (GameDataManager.instance.isSound == 1)
            {
                SettingSound.GetComponent<Text>().text = "È¿°úÀ½";
            }
            else if(GameDataManager.instance.isSound == 0)
            {
                SettingSound.GetComponent<Text>().text = "È¿°úÀ½ ²û";
            }
            GameDataManager.instance.SaveData();

        }


        void Update()
        {
            
        }
    }
}
