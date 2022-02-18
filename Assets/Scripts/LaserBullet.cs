using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Experimental.Rendering.Universal;

public class LaserBullet : MonoBehaviourPun
{
    public GameObject target;
    public GameObject audioSource;
    public GameObject particle;
    public GameObject light;
    private SpriteRenderer sr;
    private CapsuleCollider2D cc;
    Light2D lt;

    public float moveSpeed;
    public float destroyTime;
    public int direction = 1;


    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cc = GetComponent<CapsuleCollider2D>();
        audioSource.GetComponent<AudioManager>().Play("Shoot");
        StartCoroutine("DestroyByTime");
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(destroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.All);
        Debug.Log("destroyed by time");
    }

    public void SetDirection(int dir)
    {
        this.transform.localScale = new Vector3(this.transform.localScale.x * dir, this.transform.localScale.y, this.transform.localScale.z);
        this.direction = dir;
    }

    [PunRPC]
    public void DestroyObject()
    {
        sr.enabled = false;
        cc.enabled = false;

        lt = light.GetComponent<Light2D>();
        lt.pointLightOuterRadius = 15f;
        lt.intensity = 2.4f;

        particle.SetActive(true);
        particle.GetComponent<ParticleSystem>().Play();
        direction = 0;
        
        if (!photonView.IsMine) return;
        audioSource.GetComponent<AudioManager>().Play("Explode");
        StartCoroutine("Destroy");
    }

    IEnumerator Destroy()
    {
        for (int i = 0; i < 15; i++)
        {
            float maxIntensity = lt.intensity;
            yield return new WaitForSeconds(0.02f);
            lt.intensity -= maxIntensity/15;
        }
        
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void Update()
    {
        transform.Translate(Vector2.right* direction * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.layer != 3)
        {
            this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
            collision.gameObject.transform.GetComponent<PlayerStatus>()?.ChangeHealth(-40f);
        }
    }

}
