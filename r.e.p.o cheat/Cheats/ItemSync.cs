using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemSync : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data != null && data.Length == 3)
        {
            Vector3 initialPosition = new Vector3((float)data[0], (float)data[1], (float)data[2]);
            transform.position = initialPosition;
            Debug.Log("Posição inicial definida para: " + initialPosition);
        }
        else
        {
            Debug.LogWarning("Dados de instanciação ausentes ou inválidos!");
        }

        if (!info.photonView.IsMine)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                Debug.Log("Rigidbody definido como kinematic no cliente");
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            if (rb != null)
            {
                stream.SendNext(rb.velocity);
                stream.SendNext(rb.angularVelocity);
            }
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            if (rb != null)
            {
                rb.velocity = (Vector3)stream.ReceiveNext();
                rb.angularVelocity = (Vector3)stream.ReceiveNext();
            }
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Posição atual no cliente: " + transform.position);
        }
    }
}