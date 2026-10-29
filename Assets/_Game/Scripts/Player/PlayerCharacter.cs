using ExitClause.CameraSystem;
using ExitClause.Combat;
using ExitClause.Systems;
using ExitClause.UI;
using ExitClause.Vehicles;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// Hub for the player character: wires the motor, combat, stealth, interaction and rage
    /// components together, spawns at the right SpawnPoint, syncs health with GameState and
    /// handles entering and leaving vehicles.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour
    {
        public static PlayerCharacter Instance { get; private set; }

        public PlayerMotor Motor { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerStealth Stealth { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        public RageMode Rage { get; private set; }
        public Health Health { get; private set; }
        public CarController CurrentVehicle { get; private set; }

        /// <summary>Cutscenes and scripted moments switch this off.</summary>
        public bool InputEnabled = true;

        /// <summary>True when on-foot controls should be ignored.</summary>
        public bool ControlsLocked => !InputEnabled || GameInput.GameplayBlocked || CurrentVehicle != null || Health.IsDead;

        [Tooltip("Visual root hidden while driving.")]
        public GameObject bodyVisual;

        void Awake()
        {
            Instance = this;
            gameObject.tag = "Player";
            Motor = GetComponent<PlayerMotor>();
            Combat = GetComponent<PlayerCombat>();
            Stealth = GetComponent<PlayerStealth>();
            Interactor = GetComponent<PlayerInteractor>();
            Rage = GetComponent<RageMode>();
            Health = GetComponent<Health>();
        }

        void Start()
        {
            var p = GameSession.Instance.State.progress;
            var sp = SpawnPoint.Find(string.IsNullOrEmpty(p.spawnPointId) ? "default" : p.spawnPointId);
            if (sp != null) Teleport(sp.transform.position, sp.transform.rotation);

            var rig = GameCameraRig.Instance;
            if (rig == null) rig = new GameObject("[CameraRig]").AddComponent<GameCameraRig>();
            rig.SetPlayer(transform);

            var ps = GameSession.Instance.State.player;
            Health.maxHealth = ps.maxHealth;
            Health.ResetHealth(ps.health <= 0 ? ps.maxHealth : ps.health);
            Health.OnDamaged += _ => ps.health = Health.current;
            Health.OnDied += OnDied;
            GameInput.RefreshCursor();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Copies runtime component values into GameState before saving.</summary>
        public void WriteToState()
        {
            var ps = GameSession.Instance.State.player;
            ps.health = Health.current;
            ps.maxHealth = Health.maxHealth;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            var cc = GetComponent<CharacterController>();
            cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            cc.enabled = true;
            if (GameCameraRig.Instance != null) GameCameraRig.Instance.SetPlayer(transform);
        }

        public void EnterVehicle(CarController car)
        {
            if (CurrentVehicle != null || car == null) return;
            CurrentVehicle = car;
            GetComponent<CharacterController>().enabled = false;
            if (bodyVisual != null) bodyVisual.SetActive(false);
            transform.SetParent(car.transform, true);
            transform.localPosition = car.seatOffset;
            car.SetPlayerDriver(true);
            GameCameraRig.Instance?.SetVehicle(car.transform);
        }

        public void ExitVehicle()
        {
            if (CurrentVehicle == null) return;
            var car = CurrentVehicle;
            car.SetPlayerDriver(false);
            transform.SetParent(null, true);
            Vector3 exitPos = car.transform.position - car.transform.right * 1.8f + Vector3.up * 0.2f;
            if (Physics.CheckCapsule(exitPos + Vector3.up * 0.5f, exitPos + Vector3.up * 1.5f, 0.35f, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
                exitPos = car.transform.position + car.transform.right * 1.8f + Vector3.up * 0.2f;
            CurrentVehicle = null;
            if (bodyVisual != null) bodyVisual.SetActive(true);
            GameCameraRig.Instance?.SetVehicle(null);
            Teleport(exitPos, Quaternion.Euler(0, car.transform.eulerAngles.y, 0));
        }

        void OnDied(DamageInfo info)
        {
            if (CurrentVehicle != null) ExitVehicle();
            UIRoot.Instance.GameOver.Show("WASTED", "The agency always collects.");
        }
    }
}
