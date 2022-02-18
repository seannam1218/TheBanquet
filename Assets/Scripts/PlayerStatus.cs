using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerStatus : MonoBehaviourPun
{
    public Image healthBar;
    public Image hungerBar;
    
    public float maxHealth;
    private float curHealth;

    public float maxHunger;
    private float curHunger;
    public float decrementHunger;

    public float maxTimeToGrowl;
    private float timeToGrowl;
    public float growlChance;

    [SerializeField] GameObject audioSource;
    private float maxVolume = 0.4f;
    private float maxDistance = 14f;

    // Start is called before the first frame update
    void Start()
    {
        curHealth = maxHealth;
        healthBar.fillAmount = curHealth;
        
        curHunger = maxHunger;
        hungerBar.fillAmount = curHunger;

    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        photonView.RPC("ChangeHungerRpc", RpcTarget.All, -decrementHunger * Time.deltaTime);

        timeToGrowl -= Time.deltaTime;
        if (timeToGrowl <= 0)
        {
            if (Random.Range(0f, 1f) < growlChance && curHunger < 65f)
            {
                int choice = Random.Range(1, 5);  // creates a number between 1 and 4
                float power = (maxHunger - curHunger) / maxHunger;
                audioSource.GetComponent<AudioManager>().SetVolume("Stomach Growl " + choice.ToString(), maxVolume * power);
                audioSource.GetComponent<AudioManager>().SetMaxDistance("Stomach Growl " + choice.ToString(), maxDistance * power);
                audioSource.GetComponent<AudioManager>().Play("Stomach Growl " + choice.ToString());
            }
            timeToGrowl = Random.Range(maxTimeToGrowl/2, maxTimeToGrowl);
        }
    }

    public void ChangeHealth(float delta)
    {
        photonView.RPC("ChangeHealthRpc", RpcTarget.All, delta);
    }

    public void ChangeHunger(float delta)
    {
        photonView.RPC("ChangeHungerRpc", RpcTarget.All, delta);
    }

    [PunRPC]
    public void ChangeHealthRpc(float delta)
    {
        curHealth = Mathf.Min(curHealth + delta, maxHealth);
        if (curHealth <= 0) { curHealth = 0; }
        healthBar.fillAmount = curHealth / maxHealth;
    }

    [PunRPC]
    public void ChangeHungerRpc(float delta)
    {
        curHunger = Mathf.Min(curHunger + delta, maxHunger);
        if (curHunger <= 0) { curHunger = 0; }
        hungerBar.fillAmount = curHunger / maxHunger;
    }
}
