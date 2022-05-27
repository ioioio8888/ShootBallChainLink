using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
    public class TestEnvironment : MonoBehaviour
    {
        public GameObject[] NetworkObjects;
        public SkillPanel skillPanel;
        public GameObject Player;
        public GameEvent StartGame;
        public Transform spawnPoint;
        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinRandomOrCreateRoom();
            //Cursor.lockState = CursorLockMode.Locked;
            int x = 0;
            foreach (var item in NetworkObjects)
            {
                PhotonNetwork.Instantiate(item.name, Vector3.zero + new Vector3(x * 2, 0, 0), Quaternion.identity);
                x++;
            }
            GameObject p = PhotonNetwork.Instantiate(Player.name, spawnPoint.position, spawnPoint.rotation);
            skillPanel.SetUpPanel(p);
            StartGame.Raise();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
