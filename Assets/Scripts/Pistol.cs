using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Pistol : MonoBehaviourPun
{
    public float maxCooldown;
    private float cooldown;

    private void Start()
    {
        if (photonView.IsMine) cooldown = 0;
    }

    private void Update()
    {
        if (!photonView.IsMine) { return; }
            
        if (cooldown <= 0 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject bullet = PhotonNetwork.Instantiate("LaserBullet", transform.position, transform.parent.transform.rotation);
            PhotonView bulletPv = bullet.GetComponent<PhotonView>();
            photonView.RPC("SendGameObjectDirectionRpc", RpcTarget.All, bulletPv.ViewID, (int) -transform.parent.transform.parent.transform.parent.transform.localScale.x);

            cooldown = maxCooldown;
        }

        cooldown -= Time.deltaTime;
    }

    [PunRPC]
    private void SendGameObjectDirectionRpc(int Id, int direction)
    {
        PhotonView view = PhotonView.Find(Id);
        view.gameObject.GetComponent<LaserBullet>().SetDirection(direction);
    }
}
