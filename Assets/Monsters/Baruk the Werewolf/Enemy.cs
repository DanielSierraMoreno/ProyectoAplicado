using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public Slider vidaSlider;
	Player player;
	public Animator anim;
	float timer;
    public float moveSpeed;
	public float ditanceBetweenEnemiesForce;

	public enum States { WALK, IDLE, ATTACK, DEATH, STUN, HIT}

    public States states = States.IDLE;
	int maxHealth = 200;

	int health = 200;

	public GameObject fixPoint;
	List<Enemy> otherEnemies;

	public GameObject hitEffect;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        anim.CrossFadeInFixedTime("Idle",0.2f);
        timer = Time.time;

		otherEnemies = new List<Enemy>();
		otherEnemies.AddRange(GameObject.FindObjectsOfType<Enemy>());
		otherEnemies.Remove(this);
	}

    // Update is called once per frame
    void Update()
    {
		vidaSlider.value = health / (maxHealth / 100);

		switch (states)
        {
            case States.IDLE:
                if ((Time.time - timer) > 2)
                {
                    if (Vector3.Distance(this.transform.position, player.transform.position) > 2)
                    {
                        states = States.WALK;
						anim.CrossFadeInFixedTime("Walk", 0.2f);

					}
                    else
                    {
						states = States.ATTACK;
						anim.CrossFadeInFixedTime("Attack", 0.2f);
                        timer = Time.time;
					}
				}
                break;
			case States.WALK:
				if (Vector3.Distance(this.transform.position, player.transform.position) <= 2)
				{
					states = States.ATTACK;
					anim.CrossFadeInFixedTime("Attack", 0.2f);
					timer = Time.time;
				}
                Vector3 target = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);

				Vector3 enemy = Vector3.zero;
				float distance = 100000;
				foreach(var enem in otherEnemies)
				{
					if (Vector3.Distance(enem.transform.position, this.transform.position) < 2)
					{
						distance = Vector3.Distance(enem.transform.position, this.transform.position);
						enemy = enem.transform.position;
					}
				}
				
				if(enemy != Vector3.zero)
				{
					enemy = (enemy - this.transform.position).normalized;
				}

				this.transform.position -= (this.transform.position - target).normalized * moveSpeed * Time.timeScale;
				this.transform.position -= enemy.normalized * ditanceBetweenEnemiesForce * Time.timeScale;

				this.transform.LookAt(target);
				break;
			case States.ATTACK:
                if ((Time.time - timer) > 2)
                {
					states = States.IDLE;
					anim.CrossFadeInFixedTime("Idle", 0.2f);
				}

				break;
			case States.STUN:
				if ((Time.time - timer) > 3)
				{
					states = States.IDLE;
					anim.CrossFadeInFixedTime("Idle", 0.2f);
				}
				break;
			case States.HIT:
				if ((Time.time - timer) > 1)
				{
					states = States.IDLE;
					anim.CrossFadeInFixedTime("Idle", 0.2f);
				}
				break;
			case States.DEATH:

				break;
		}

    }

    public void SetStun()
    {
        states = States.STUN;
		anim.CrossFadeInFixedTime("Stun", 0.1f);
        timer = Time.time;
	}

	 
	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<CollisionesArma>() != null && states != States.DEATH)
		{
			health -= 20;

			for(int i = 0; i <  hitEffect.transform.childCount; i++)
			{
				if (!hitEffect.transform.GetChild(i).gameObject.activeSelf) 
				{
					hitEffect.transform.GetChild(i).gameObject.SetActive(true);
					break;
				}
			}

			hitEffect.SetActive(true);
			if (health <= 0)
			{
				Destroy(fixPoint);
				
				states = States.DEATH;
				anim.CrossFadeInFixedTime("Death", 0.1f);
				timer = Time.time;
			}
			else
			{
				states = States.HIT;
				anim.CrossFadeInFixedTime("Hit", 0.1f);
				timer = Time.time;
			}




		}
	}
}
