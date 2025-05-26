using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Player;

public class AttackSystem : MonoBehaviour
{
	public enum ComboAtaques { agua, fuego, special1, special2, Special3, Slide, Realista1 };

	public Player player;
	public GameObject effects;
	public GameObject cuts;
	public GameObject collisions;

	[System.Serializable]
	public struct Ataques
	{
		public float ataque;
		public float delay;
		public bool attacking;
		public string effects;
		public int[] cuts;
		public float effectDelay;
		public string name;
		public float animationSpeed;
		public float force;
		public float delayForce;
		public int col;
		public int Pesado;
		public float transition;
		public AnimationCurve curvaDeVelocidad;
		public float consumoEnergia;
		public Vector3 effectTransformPos;
		public Quaternion effectTransformRot;

	}
	[System.Serializable]
	public struct ListaAtaques
	{
		public ComboAtaques combo;
		public Ataques[] attacks;

	}
	[SerializeField]
	public AtaqueSO currentAttack;

	public AtaqueSO currentCombo;
	public AtaqueSO ComboFuerte;
	public AtaqueSO ComboDebil;

	int currentComboAttack;

	// Start is called before the first frame update
	void Start()
	{
		currentComboAttack = 0;

		currentAttack = currentCombo;

	}

	// Update is called once per frame
	void Update()
    {
		if (player.controller.rightTrigger.wasPressedThisFrame && !player.attacking)
		{
			currentCombo = ComboFuerte;
			currentAttack = currentCombo;

		}
		else if (player.controller.rightShoulder.wasPressedThisFrame && !player.attacking)
		{
			currentCombo = ComboDebil;
			currentAttack = currentCombo;

		}

		if (((currentAttack.attacks[currentComboAttack].Pesado == 1 && player.controller.rightTrigger.IsPressed()) || (currentAttack.attacks[currentComboAttack].Pesado == 0 && player.controller.rightShoulder.IsPressed())) && player.CheckIfCanAttack())
		{
			this.GetComponent<Rigidbody>().drag = 10;

			player.timeAttack = Time.time;

			player.attacking = true;
			currentComboAttack = 0;


			currentAttack = currentCombo;


			currentAttack.attacks[currentComboAttack].attacking = true;
			Invoke("addAttackForce", currentAttack.attacks[currentComboAttack].delayForce);

			GameObject[] planeCuts = new GameObject[currentAttack.attacks[currentComboAttack].cuts.Length];

			for (int i = 0; i < currentAttack.attacks[currentComboAttack].cuts.Length; i++)
			{
				planeCuts[i] = cuts.transform.GetChild(currentAttack.attacks[currentComboAttack].cuts[i]).gameObject;

			}


			StartCoroutine(EfectoAtaque(GetEffectByName(currentAttack.attacks[currentComboAttack].effects), currentAttack.attacks[currentComboAttack].effectDelay, planeCuts, collisions.transform.GetChild(currentAttack.attacks[currentComboAttack].col).GetComponent<CollisionesArma>()));

			player.anim.speed = currentAttack.attacks[currentComboAttack].animationSpeed;
			player.anim.CrossFadeInFixedTime(currentAttack.attacks[currentComboAttack].name, 0.2f);

			player.attackStartTime = Time.time;
		}

		if (player.attacking)
		{
			player.move = new Vector3();
			if (currentAttack.attacks[currentComboAttack].attacking)
			{
				if (currentAttack.attacks[currentComboAttack].curvaDeVelocidad.length != 0)
				{
					float a = Time.time - player.timeAttack;
					player.anim.speed = currentAttack.attacks[currentComboAttack].curvaDeVelocidad.Evaluate(a);
				}



				if ((Time.time - player.attackStartTime) > currentAttack.attacks[currentComboAttack].ataque)
				{
					if ((Time.time - player.attackStartTime) < currentAttack.attacks[currentComboAttack].delay + currentAttack.attacks[currentComboAttack].ataque && currentAttack.attacks.Length != currentComboAttack + 1)
					{
						if (((currentAttack.attacks[currentComboAttack + 1].Pesado == 1 && player.controller.rightTrigger.IsPressed()) || (currentAttack.attacks[currentComboAttack + 1].Pesado == 0 && player.controller.rightShoulder.IsPressed())) && player.energia > 0)
						{

							currentComboAttack++;
							Invoke("addAttackForce", currentAttack.attacks[currentComboAttack].delayForce);

							//effects[0].transform.localRotation = new Quaternion();
							//effects[0].transform.Rotate(new Vector3(0, 0, rotation2));

							GameObject[] planeCuts = new GameObject[currentAttack.attacks[currentComboAttack].cuts.Length];

							for (int i = 0; i < currentAttack.attacks[currentComboAttack].cuts.Length; i++)
							{
								planeCuts[i] = cuts.transform.GetChild(currentAttack.attacks[currentComboAttack].cuts[i]).gameObject;

							}

							StartCoroutine(EfectoAtaque(GetEffectByName(currentAttack.attacks[currentComboAttack].effects), currentAttack.attacks[currentComboAttack].effectDelay, planeCuts, collisions.transform.GetChild(currentAttack.attacks[currentComboAttack].col).GetComponent<CollisionesArma>()));
							player.anim.speed = currentAttack.attacks[currentComboAttack].animationSpeed;
							player.anim.CrossFadeInFixedTime(currentAttack.attacks[currentComboAttack].name, currentAttack.attacks[currentComboAttack - 1].transition);


							player.attackStartTime = Time.time;
							currentAttack.attacks[currentComboAttack - 1].attacking = false;
							currentAttack.attacks[currentComboAttack].attacking = true;

						}
					}
					else
					{
						player.delayLastAttack = Time.time;

						currentAttack.attacks[currentComboAttack].attacking = false;
						player.attacking = false;
						player.anim.speed = 1;
						player.returnNormal();
						this.GetComponent<Rigidbody>().drag = 20;

						currentComboAttack = 0;

					}
				}
			}
		}
	}
	void addAttackForce()
	{
		this.GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * currentAttack.attacks[currentComboAttack].force, ForceMode.Impulse);

	}
	GameObject GetEffectByName(string name)
	{
		for (int i = 0; i < effects.transform.childCount; i++)
		{
			if (effects.transform.GetChild(i).name == name)
			{
				return effects.transform.GetChild(i).gameObject;

			}

		}
		return null;
	}

	void QuitarColision()
	{
		for (int i = 0; i < collisions.transform.childCount; i++)
		{
			collisions.transform.GetChild(i).gameObject.SetActive(false);

		}

	}

	IEnumerator EfectoAtaque(GameObject effect, float delay, GameObject[] plane, CollisionesArma col)
	{
		if (effect != null)
		{
			//Invoke("DevolverEfecto", currentAttack.attacks[currentComboAttack].delay);



			yield return new WaitForSeconds(delay);

			col.gameObject.SetActive(true);
			Invoke("QuitarColision", 0.1f);

			//effect.transform.SetParent(GuardarEffectos.transform);
			effect.transform.localPosition = currentAttack.attacks[currentComboAttack].effectTransformPos;
			effect.transform.localRotation = currentAttack.attacks[currentComboAttack].effectTransformRot;

			effect.GetComponent<ParticleSystem>().Play();
			player.energia -= currentAttack.attacks[currentComboAttack].consumoEnergia;
		}

		yield return new WaitForSeconds(0.15f);
		//effect.transform.SetParent(GuardarEffectos.transform);

		for (int i = 0; i < plane.Length; i++)
		{

			plane[i].GetComponent<DynamicMeshCutter.PlaneBehaviour>().Cut(col.GetObjects());
			yield return new WaitForSeconds(0.1f);

		}
	}
}
