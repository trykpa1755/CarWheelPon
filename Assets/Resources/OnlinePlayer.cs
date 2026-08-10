using UnityEngine;
using Photon.Pun;

public class OnlinePlayer : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;

    void Awake()
    {
        Debug.Log("=== OnlinePlayer Awake ===");
        Debug.Log("GameObject: " + gameObject.name);
        Debug.Log("PhotonView: " + photonView);
        Debug.Log("PhotonView.IsMine: " + photonView.IsMine);
        Debug.Log("PhotonView.Owner: " + photonView.Owner);
        Debug.Log("PhotonView.ViewID: " + photonView.ViewID);

        if (photonView.InstantiationData != null)
        {
            Debug.Log("InstantiationData istnieje. D³ugoœæ: " + photonView.InstantiationData.Length);
            Debug.Log("InstantiationData[0] playerName: " + photonView.InstantiationData[0]);
        }
        else
        {
            Debug.LogWarning("InstantiationData jest null.");
        }

        if (photonView.IsMine)
        {
            Debug.Log("To jest moje lokalne auto. Ustawiam LocalPlayerInstance.");
            LocalPlayerInstance = gameObject;
        }
        else
        {
            Debug.Log("To jest zdalne auto innego gracza.");

            string playerName = null;
            Color playerColor = Color.white;

            if (photonView.InstantiationData != null)
            {
                playerName = (string)photonView.InstantiationData[0];

                playerColor = ColorCar.IntToColor(
                    (int)photonView.InstantiationData[1],
                    (int)photonView.InstantiationData[2],
                    (int)photonView.InstantiationData[3]
                );
            }

            if (playerName != null)
            {
                Debug.Log("Ustawiam nazwê i kolor zdalnego auta: " + playerName);

                CarApperance carApperance = GetComponent<CarApperance>();

                if (carApperance == null)
                {
                    Debug.LogError("Brak CarApperance na zdalnym aucie!");
                }
                else
                {
                    carApperance.SetNameAndColor(playerName, playerColor);
                }
            }
        }

        Debug.Log("=== Koniec OnlinePlayer Awake ===");
    }

    private void OnDestroy()
    {
        Debug.Log("OnlinePlayer OnDestroy: " + gameObject.name);

        if (LocalPlayerInstance == gameObject)
        {
            Debug.Log("Czyszczê LocalPlayerInstance.");
            LocalPlayerInstance = null;
        }
    }
}