using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaserBullet : MonoBehaviourPun
{
    public GameObject target;
    public float moveSpeed;
    public float destroyTime;
    public int direction = 1;

    private void Awake()
    {
        StartCoroutine("DestroyByTime");
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(destroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.All);
    }

    public void SetDirection(int dir)
    {
        this.transform.localScale = new Vector3(this.transform.localScale.x * dir, this.transform.localScale.y, this.transform.localScale.z);
        this.direction = dir;
    }

    [PunRPC]
    public void DestroyObject()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void Update()
    {
        //if (!photonView.IsMine) return;
        transform.Translate(Vector2.right* direction * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //PhotonView hitTarget = collision.gameObject.GetComponent<PhotonView>();
        this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void DeclareTrajectory()
    {

    } 
}
