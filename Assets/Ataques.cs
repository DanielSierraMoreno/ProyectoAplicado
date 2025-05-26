using UnityEngine;

// Esto te permite crear instancias de este ScriptableObject desde el menú de Assets/Create
[CreateAssetMenu(fileName = "NuevoAtaque", menuName = "Ataques/AtaqueSO")]
public class AtaqueSO : ScriptableObject
{
	public AttackSystem.ComboAtaques combo;
	public AttackSystem.Ataques[] attacks;

}