using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>Tuning data for a firearm, shared by the player and enemy agents.</summary>
    [CreateAssetMenu(menuName = "Exit Clause/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float damage = 25f;
        [Tooltip("Shots per second.")]
        public float fireRate = 4f;
        public bool automatic;
        public int magazineSize = 12;
        public int startingReserve = 36;
        public float reloadTime = 1.4f;
        public float range = 60f;
        [Tooltip("Random cone in degrees while aiming.")]
        public float spread = 1.5f;
        [Tooltip("Extra spread when firing from the hip.")]
        public float hipSpread = 5f;
        [Header("Recoil (degrees added to camera per shot)")]
        public float recoilPitch = 1.5f;
        public float recoilYaw = 0.5f;
        [Header("Stealth")]
        public bool suppressed;
        public float noiseRadius = 35f;
        [Header("AI use")]
        [Range(0, 1)] public float aiAccuracy = 0.35f;
        public AudioClip fireSound;
    }
}
