using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Pistol : MonoBehaviourPun
{
    PhotonView photonView;
    
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject bullet = PhotonNetwork.Instantiate("LaserBullet", transform.position, transform.parent.transform.rotation);
            //bullet.GetComponent<LaserBullet>().SetDirection((int) -transform.parent.transform.parent.transform.parent.transform.localScale.x);
            PhotonView bulletPv = bullet.GetComponent<PhotonView>();

/*            if (transform.parent.transform.parent.transform.parent.transform.localScale.x < 0)
            {
                photonView.RPC("SendGameObjectDirectionRpc", RpcTarget.All, bulletPv.ViewID, -1);
            }*/
            photonView.RPC("SendGameObjectDirectionRpc", RpcTarget.All, bulletPv.ViewID, (int) -transform.parent.transform.parent.transform.parent.transform.localScale.x);
        }
    }

    [PunRPC]
    private void SendGameObjectDirectionRpc(int Id, int direction)
    {
        PhotonView view = PhotonView.Find(Id);
        //Vector3 scale = view.gameObject.transform.localScale;
        //view.gameObject.transform.localScale = new Vector3(scale.x * direction, scale.y, scale.z);
        view.gameObject.GetComponent<LaserBullet>().SetDirection(direction);
    }
}
