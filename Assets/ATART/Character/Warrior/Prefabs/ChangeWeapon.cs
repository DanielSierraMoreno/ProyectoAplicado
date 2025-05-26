using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWeapon : MonoBehaviour
{
    public AttackSystem attackSystem;

    public List<GameObject> weapons;
    int currentWeaponIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndChangeWeapon()
    {
		attackSystem.player.changingWeapon = false;
        attackSystem.player.returnNormal();
	}

	public void SetNewWeapon()
	{
        AtaqueSO basico = attackSystem.ComboDebil;
        AtaqueSO fuerte = attackSystem.ComboFuerte;

		attackSystem.ComboDebil = attackSystem.ComboDebilNoEquipado;
		attackSystem.ComboFuerte = attackSystem.ComboFuerteNoEquipado;

		attackSystem.ComboDebilNoEquipado = basico;
		attackSystem.ComboFuerteNoEquipado = fuerte;

        weapons[currentWeaponIndex].SetActive(false);
        currentWeaponIndex++;
		currentWeaponIndex = currentWeaponIndex% weapons.Count;
		weapons[currentWeaponIndex].SetActive(true);

	}
}
