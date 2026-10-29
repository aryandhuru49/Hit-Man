using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Trigger volume that advances an objective when the player enters it (optionally only in a car).
    /// Can also act as a checkpoint that autosaves.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectiveTrigger : MonoBehaviour
    {
        public string objectiveId;
        public bool requireVehicle;
        public bool checkpoint;
        public string checkpointSpawnId;
        public Vehicles.ChaseSequence finishesChase;
        public bool oneShot = true;

        bool m_fired;

        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = Layers.Trigger;
        }

        void OnTriggerEnter(Collider other)
        {
            var p = other.GetComponentInParent<PlayerCharacter>();
            if (p == null)
            {
                var car = other.GetComponentInParent<Vehicles.CarController>();
                if (car != null && car.IsPlayerDriven) p = PlayerCharacter.Instance;
            }
            if (p == null) return;
            if (requireVehicle && p.CurrentVehicle == null) return;
            if (!string.IsNullOrEmpty(objectiveId) && !MissionManager.Instance.IsObjectiveActive(objectiveId)) return;
            if (oneShot && m_fired) return;
            m_fired = true;

            if (finishesChase != null) finishesChase.Finish(true);
            else MissionManager.Instance.Progress(objectiveId);

            if (checkpoint)
            {
                GameSession.Instance.State.progress.spawnPointId = checkpointSpawnId;
                SaveSystem.Save(SaveSystem.AutosaveSlot);
            }
        }
    }
}
