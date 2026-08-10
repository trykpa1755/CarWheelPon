using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class RaceController : MonoBehaviourPunCallbacks
{
    public CheckPointController[] carsController;

    public static bool racing = false;
    public static int totalLaps = 1;

    public int timer = 3;

    public Text startText;
    private AudioSource audioSource;
    public AudioClip count;
    public AudioClip start;

    public GameObject endPanel;

    public GameObject carPrefab;
    public Transform[] spawnPos;
    public int playerCount;

    public GameObject startRace;
    public GameObject waitingText;

    public RawImage mirror;

    void Start()
    {
        Debug.Log("=== RaceController Start ===");

        racing = false;
        timer = 3;

        if (endPanel != null)
            endPanel.SetActive(false);

        audioSource = GetComponent<AudioSource>();

        if (startText != null)
            startText.gameObject.SetActive(false);

        if (startRace != null)
            startRace.SetActive(false);

        if (waitingText != null)
            waitingText.SetActive(false);

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("PhotonNetwork.IsConnected = false. Scena wyœcigu zosta³a uruchomiona bez po³¹czenia z Photonem.");
            return;
        }

        Debug.Log("Po³¹czony z Photonem.");
        Debug.Log("Nick: " + PhotonNetwork.NickName);
        Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);
        Debug.Log("PhotonNetwork.InRoom: " + PhotonNetwork.InRoom);
        Debug.Log("PhotonNetwork.CurrentRoom: " + PhotonNetwork.CurrentRoom);
        Debug.Log("PhotonNetwork.IsMessageQueueRunning: " + PhotonNetwork.IsMessageQueueRunning);
        Debug.Log("PhotonNetwork.NetworkClientState: " + PhotonNetwork.NetworkClientState);

        StartCoroutine(SpawnPlayerCarWhenReady());
    }

    private IEnumerator SpawnPlayerCarWhenReady()
    {
        Debug.Log("=== SpawnPlayerCarWhenReady start ===");

        while (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || !PhotonNetwork.IsMessageQueueRunning)
        {
            Debug.Log("Czekam na gotowoœæ Photona... " +
                      "InRoom: " + PhotonNetwork.InRoom +
                      " | CurrentRoom: " + PhotonNetwork.CurrentRoom +
                      " | IsMessageQueueRunning: " + PhotonNetwork.IsMessageQueueRunning +
                      " | State: " + PhotonNetwork.NetworkClientState);

            yield return null;
        }

        // Ma³e opóŸnienie bezpieczeñstwa po za³adowaniu sceny przez PhotonNetwork.LoadLevel
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Photon gotowy do instantiate.");
        Debug.Log("Nick: " + PhotonNetwork.NickName);
        Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);
        Debug.Log("Nazwa pokoju: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Liczba graczy w pokoju: " + PhotonNetwork.CurrentRoom.PlayerCount);
        Debug.Log("ActorNumber lokalnego gracza: " + PhotonNetwork.LocalPlayer.ActorNumber);

        PrintPlayersInRoom();

        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        if (spawnPos == null || spawnPos.Length == 0)
        {
            Debug.LogError("Brak spawnPos! Nie mo¿na utworzyæ auta.");
            yield break;
        }

        if (spawnIndex < 0 || spawnIndex >= spawnPos.Length)
        {
            Debug.LogWarning("spawnIndex poza zakresem: " + spawnIndex + ". Liczba spawnów: " + spawnPos.Length + ". Ustawiam 0.");
            spawnIndex = 0;
        }

        Vector3 startPos = spawnPos[spawnIndex].position;
        Quaternion startRot = spawnPos[spawnIndex].rotation;

        Debug.Log("Wybrany spawnIndex: " + spawnIndex);
        Debug.Log("Start position: " + startPos);
        Debug.Log("Start rotation: " + startRot.eulerAngles);

        object[] instanceData = new object[4];
        instanceData[0] = PlayerPrefs.GetString("PlayerName");
        instanceData[1] = PlayerPrefs.GetInt("Red");
        instanceData[2] = PlayerPrefs.GetInt("Green");
        instanceData[3] = PlayerPrefs.GetInt("Blue");

        Debug.Log("PlayerPrefs PlayerName: " + instanceData[0]);
        Debug.Log("Kolor RGB: " + instanceData[1] + ", " + instanceData[2] + ", " + instanceData[3]);
        Debug.Log("OnlinePlayer.LocalPlayerInstance przed instantiate: " + OnlinePlayer.LocalPlayerInstance);

        GameObject playerCar = null;

        if (OnlinePlayer.LocalPlayerInstance == null)
        {
            if (carPrefab == null)
            {
                Debug.LogError("carPrefab nie jest przypisany w Inspectorze!");
                yield break;
            }

            Debug.Log("LocalPlayerInstance jest null, tworzê auto przez PhotonNetwork.Instantiate.");
            Debug.Log("carPrefab z Inspectora: " + carPrefab);
            Debug.Log("carPrefab.name: " + carPrefab.name);
            Debug.Log("UWAGA: Photon bêdzie szuka³ prefabu w Assets/Resources o nazwie: " + carPrefab.name);

            playerCar = PhotonNetwork.Instantiate(carPrefab.name, startPos, startRot, 0, instanceData);

            Debug.Log("PhotonNetwork.Instantiate zakoñczone. playerCar: " + playerCar.name);

            CarApperance carApperance = playerCar.GetComponent<CarApperance>();

            if (carApperance == null)
            {
                Debug.LogError("Na utworzonym aucie nie ma komponentu CarApperance!");
            }
            else
            {
                carApperance.SetLocalPlayer();
                Debug.Log("SetLocalPlayer wykonane dla lokalnego auta.");
            }
        }
        else
        {
            Debug.Log("LocalPlayerInstance ju¿ istnieje. U¿ywam istniej¹cego auta.");
            playerCar = OnlinePlayer.LocalPlayerInstance;
        }

        if (playerCar == null)
        {
            Debug.LogError("playerCar nadal jest null! Nie da siê w³¹czyæ DrivingScript i PlayerController.");
            yield break;
        }

        Debug.Log("Lokalne auto: " + playerCar.name);
        Debug.Log("Tag lokalnego auta: " + playerCar.tag);

        DrivingScript drivingScript = playerCar.GetComponent<DrivingScript>();
        PlayerController playerController = playerCar.GetComponent<PlayerController>();
        CheckPointController checkPointController = playerCar.GetComponent<CheckPointController>();
        PhotonView carPhotonView = playerCar.GetComponent<PhotonView>();

        Debug.Log("DrivingScript: " + drivingScript);
        Debug.Log("PlayerController: " + playerController);
        Debug.Log("CheckPointController: " + checkPointController);
        Debug.Log("PhotonView na aucie: " + carPhotonView);

        if (carPhotonView != null)
        {
            Debug.Log("PhotonView.IsMine auta: " + carPhotonView.IsMine);
            Debug.Log("PhotonView.Owner: " + carPhotonView.Owner);
            Debug.Log("PhotonView.ViewID auta: " + carPhotonView.ViewID);
        }

        if (drivingScript != null)
            drivingScript.enabled = true;
        else
            Debug.LogError("Brak DrivingScript na lokalnym aucie!");

        if (playerController != null)
            playerController.enabled = true;
        else
            Debug.LogError("Brak PlayerController na lokalnym aucie!");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Ten gracz jest MasterClient. Pokazujê przycisk startRace.");

            if (startRace != null)
                startRace.SetActive(true);

            if (waitingText != null)
                waitingText.SetActive(false);
        }
        else
        {
            Debug.Log("Ten gracz NIE jest MasterClient. Pokazujê waitingText.");

            if (startRace != null)
                startRace.SetActive(false);

            if (waitingText != null)
                waitingText.SetActive(true);
        }

        PrintCarsInScene();

        Debug.Log("=== Koniec SpawnPlayerCarWhenReady ===");
        Debug.Log("=== Koniec RaceController Start ===");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("=== OnPlayerEnteredRoom ===");
        Debug.Log("Do³¹czy³ gracz: " + newPlayer.NickName);
        Debug.Log("ActorNumber: " + newPlayer.ActorNumber);
        Debug.Log("Aktualna liczba graczy w pokoju: " + PhotonNetwork.CurrentRoom.PlayerCount);

        PrintPlayersInRoom();
        PrintCarsInScene();

        StartCoroutine(PrintCarsInSceneDelayed());

        Debug.Log("=== Koniec OnPlayerEnteredRoom ===");
    }

    private IEnumerator PrintCarsInSceneDelayed()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("=== OpóŸnione sprawdzenie aut po 2 sekundach ===");
        PrintPlayersInRoom();
        PrintCarsInScene();

        yield return new WaitForSeconds(3f);

        Debug.Log("=== OpóŸnione sprawdzenie aut po 5 sekundach ===");
        PrintPlayersInRoom();
        PrintCarsInScene();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("=== OnPlayerLeftRoom ===");
        Debug.Log("Wyszed³ gracz: " + otherPlayer.NickName);
        Debug.Log("ActorNumber: " + otherPlayer.ActorNumber);
        Debug.Log("Aktualna liczba graczy w pokoju: " + PhotonNetwork.CurrentRoom.PlayerCount);

        PrintPlayersInRoom();
        PrintCarsInScene();

        Debug.Log("=== Koniec OnPlayerLeftRoom ===");
    }

    void LateUpdate()
    {
        if (carsController == null)
            return;

        int finishedLap = 0;

        foreach (CheckPointController controller in carsController)
        {
            if (controller == null)
            {
                Debug.LogWarning("carsController zawiera null.");
                continue;
            }

            if (controller.lap == totalLaps + 1)
                finishedLap++;
        }

        if (finishedLap == carsController.Length && racing)
        {
            Debug.Log("Wszyscy ukoñczyli wyœcig. Pokazujê endPanel.");
            endPanel.SetActive(true);
            racing = false;
        }
    }

    void CountDown()
    {
        Debug.Log("CountDown. Timer = " + timer);

        if (startText != null)
            startText.gameObject.SetActive(true);

        if (timer != 0)
        {
            if (startText != null)
                startText.text = timer.ToString();

            if (audioSource != null && count != null)
                audioSource.PlayOneShot(count);

            timer--;
        }
        else
        {
            if (startText != null)
                startText.text = "START!!!";

            if (audioSource != null && start != null)
                audioSource.PlayOneShot(start);

            racing = true;
            Debug.Log("Wyœcig wystartowa³. racing = true");

            CancelInvoke("CountDown");
            Invoke("HideStartText", 1);
        }
    }

    void HideStartText()
    {
        if (startText != null)
            startText.gameObject.SetActive(false);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void BeginGame()
    {
        Debug.Log("=== BeginGame klikniête ===");
        Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);
        Debug.Log("CurrentRoom: " + PhotonNetwork.CurrentRoom);
        Debug.Log("PlayerCount: " + (PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "brak pokoju"));

        PrintPlayersInRoom();
        PrintCarsInScene();

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Wysy³am RPC StartGame do wszystkich.");
            photonView.RPC("StartGame", RpcTarget.All, null);
        }
        else
        {
            Debug.LogWarning("Ten gracz nie jest MasterClient, wiêc nie mo¿e uruchomiæ StartGame.");
        }
    }

    [PunRPC]
    public void StartGame()
    {
        Debug.Log("=== StartGame RPC odebrane ===");
        Debug.Log("Nick: " + PhotonNetwork.NickName);
        Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);

        CancelInvoke("CountDown");
        timer = 3;
        InvokeRepeating("CountDown", 3, 1);

        if (startRace != null)
            startRace.SetActive(false);

        if (waitingText != null)
            waitingText.SetActive(false);

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

        Debug.Log("FindGameObjectsWithTag(\"Car\") znalaz³o: " + cars.Length + " aut.");

        List<CheckPointController> controllers = new List<CheckPointController>();

        for (int i = 0; i < cars.Length; i++)
        {
            Debug.Log("Auto[" + i + "]: " + cars[i].name + " | tag: " + cars[i].tag);

            PhotonView pv = cars[i].GetComponent<PhotonView>();
            CheckPointController cp = cars[i].GetComponent<CheckPointController>();
            CarApperance ca = cars[i].GetComponent<CarApperance>();

            Debug.Log("Auto[" + i + "] PhotonView: " + pv);
            Debug.Log("Auto[" + i + "] CheckPointController: " + cp);
            Debug.Log("Auto[" + i + "] CarApperance: " + ca);

            if (pv != null)
            {
                Debug.Log("Auto[" + i + "] PhotonView.IsMine: " + pv.IsMine);
                Debug.Log("Auto[" + i + "] PhotonView.Owner: " + pv.Owner);
                Debug.Log("Auto[" + i + "] PhotonView.ViewID: " + pv.ViewID);
            }

            if (ca != null)
            {
                Debug.Log("Auto[" + i + "] playerName: " + ca.playerName);
            }

            if (cp != null)
            {
                controllers.Add(cp);
            }
            else
            {
                Debug.LogWarning("Auto[" + i + "] nie ma CheckPointController, wiêc nie dodajê go do carsController.");
            }
        }

        carsController = controllers.ToArray();

        Debug.Log("carsController ustawione. D³ugoœæ: " + carsController.Length);
        Debug.Log("=== Koniec StartGame RPC ===");
    }

    public void SetMirror(Camera backCamera)
    {
        Debug.Log("SetMirror wywo³ane. Kamera: " + backCamera);

        if (backCamera == null)
        {
            Debug.LogError("backCamera jest null!");
            return;
        }

        if (mirror == null)
        {
            Debug.LogError("mirror RawImage nie jest przypisany w Inspectorze!");
            return;
        }

        mirror.texture = backCamera.targetTexture;
    }

    void PrintPlayersInRoom()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("PrintPlayersInRoom: CurrentRoom jest null.");
            return;
        }

        Debug.Log("--- Lista graczy w pokoju ---");

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log("Gracz: " + player.NickName +
                      " | ActorNumber: " + player.ActorNumber +
                      " | IsLocal: " + player.IsLocal +
                      " | IsMasterClient: " + player.IsMasterClient);
        }

        Debug.Log("--- Koniec listy graczy ---");
    }

    void PrintCarsInScene()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

        Debug.Log("--- Auta w scenie, tag Car: " + cars.Length + " ---");

        for (int i = 0; i < cars.Length; i++)
        {
            PhotonView pv = cars[i].GetComponent<PhotonView>();
            CarApperance ca = cars[i].GetComponent<CarApperance>();
            CheckPointController cp = cars[i].GetComponent<CheckPointController>();

            Debug.Log("Car[" + i + "]: " + cars[i].name +
                      " | PhotonView: " + pv +
                      " | CarApperance: " + ca +
                      " | CheckPointController: " + cp);

            if (pv != null)
            {
                Debug.Log("Car[" + i + "] Owner: " + pv.Owner +
                          " | IsMine: " + pv.IsMine +
                          " | ViewID: " + pv.ViewID);
            }

            if (ca != null)
            {
                Debug.Log("Car[" + i + "] playerName: " + ca.playerName);
            }
        }

        Debug.Log("--- Koniec listy aut ---");
    }
}