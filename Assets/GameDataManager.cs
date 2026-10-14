using UnityEngine;

namespace RusiWishvel
{
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager instance;
        public int isMusisc = 0;
        public int isSound = 0;
        public float gameTime = 0;
        public int gameScore = 0;
        public string curId;

        

        public float maxHp = 10f;
        public int upgrade = 0;
        public int bomb = 0;
        public int maxbomb = 3;
        

        void Awake()
        {
            
            instance = this;
            DontDestroyOnLoad(instance);

        }

        void Start()
        {
           
            LoadData();

        }

        public void SaveData()
        {
            if(PlayerPrefs.HasKey("id"))
            {
                string id = PlayerPrefs.GetString("id");
                Debug.Log(id);
                PlayerPrefs.DeleteKey(id);
            }
            else
            {
                PlayerPrefs.SetString("id", "Gonutyun");
            }
            PlayerPrefs.SetInt("Music", isMusisc);
            PlayerPrefs.SetInt("Sound", isSound);

            string saveData = curId + "," + gameScore;
            PlayerPrefs.SetString("saveData", saveData);

            

        }

        public void LoadData()
        {
            if(!PlayerPrefs.HasKey("Music"))
            {
                PlayerPrefs.SetInt("Music", 1);
            }

            if (!PlayerPrefs.HasKey("Sound"))
            {
                PlayerPrefs.SetInt("Sound", 1);

            }
            isMusisc = PlayerPrefs.GetInt("Music");
            isSound = PlayerPrefs.GetInt("Sound");

        }
        
        public void LoadData2()
        {
            if (!PlayerPrefs.HasKey("saveData"))
            {
                string saveData = curId + "," + gameScore;
                PlayerPrefs.SetString("saveData", saveData);
            }

            string tempData = PlayerPrefs.GetString("saveData");
            string[] data = tempData.Split(',');

            curId = data[0];
            gameScore = int.Parse(data[1]);
        }
        

       
    }
}
