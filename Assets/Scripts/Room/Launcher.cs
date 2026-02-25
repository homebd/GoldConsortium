using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MyCompany.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string gameSceneName = "Main";

        string gameVersion = "1";

        [SerializeField] private GameObject controlPanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private GameObject progressLabel;
        [SerializeField] private TextMeshProUGUI memberText;
        [SerializeField] private Button connectBtn;

        [SerializeField] private ConfigSO config;

        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            connectBtn.onClick.AddListener(Connect);

            Screen.SetResolution(960, 540, false);
        }


        void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        private void UpdateStateText(int cur, int max)
        {
            memberText.text = $"({cur}/{max})";
        }

        public void Connect()
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            PhotonNetwork.NickName = inputField.text;

            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = config.MaxPlayer });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            TryStartGame();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom: {newPlayer.NickName} Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");

            TryStartGame();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            TryStartGame();
        }

        private void TryStartGame()
        {
            // 아직 룸 정보가 없으면 종료
            if (PhotonNetwork.CurrentRoom == null) return;

            int current = PhotonNetwork.CurrentRoom.PlayerCount;
            int target = PhotonNetwork.CurrentRoom.MaxPlayers; // 보통 6

            UpdateStateText(current, target);

            // 마스터만 씬 전환을 트리거해야 함
            if (!PhotonNetwork.IsMasterClient) return;

            // 6명(=MaxPlayers) 꽉 차면 시작
            if (current >= target)
            {
                Debug.Log($"Room full ({current}/{target}). Loading {gameSceneName}");
                PhotonNetwork.LoadLevel(gameSceneName);
            }
            else
            {
                Debug.Log($"Waiting... ({current}/{target})");
            }
        }
    }
}