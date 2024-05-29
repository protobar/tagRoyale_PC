using Photon.Pun;
using UnityEngine;

public class PhysicsLagPhoton : MonoBehaviourPunCallbacks, IPunObservable
{


    public bool ball;

    Rigidbody r_body;
    [SerializeField]PhotonView photon_View;

    public float teleportDistance;

    private Vector3 _netPos;
    private Quaternion _netRot;
    private Vector3 _previosPos;

    [Header("Lerping Values")]
    public float smoothPos = 5.0f;
    public float smoothRot = 5.0f;

    private void Awake()
    {
        r_body = GetComponent<Rigidbody>();
        photon_View = GetComponent<PhotonView>();
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(r_body.position);
            stream.SendNext(r_body.rotation);
            stream.SendNext(r_body.velocity);

        }
        else if (stream.IsReading)
        {
            _netPos = (Vector3)stream.ReceiveNext();
            _netRot = (Quaternion)stream.ReceiveNext();
            r_body.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            r_body.position += (r_body.velocity * lag);


        }
    }



    private void FixedUpdate()
    {
        if (ball)
        {


            if (!photon_View.IsMine)
            {

                r_body.position = Vector3.Lerp(r_body.position, _netPos, smoothPos * Time.fixedDeltaTime);
                r_body.rotation = Quaternion.Lerp(r_body.rotation, _netRot, smoothRot * Time.fixedDeltaTime);

                if (Vector3.Distance(r_body.position, _netPos) > teleportDistance)
                {
                    r_body.position = _netPos;
                }

            }
        }
    }



}
