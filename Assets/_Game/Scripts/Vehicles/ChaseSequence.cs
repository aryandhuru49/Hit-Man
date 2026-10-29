using ExitClause.AI;
using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Vehicles
{
    /// <summary>
    /// Scripted car chase. Either the player escapes (pursuers wrecked or left far behind for a while,
    /// or the finish trigger is reached), or the player pursues a fleeing target until it is wrecked.
    /// </summary>
    public class ChaseSequence : MonoBehaviour
    {
        public enum ChaseKind { Escape, Pursue }

        public ChaseKind kind = ChaseKind.Escape;
        public CarController playerCar;
        public AIDriver[] pursuers;
        public AIDriver fleeingTarget;
        [Tooltip("Lose pursuers by staying this far ahead...")]
        public float escapeDistance = 120f;
        [Tooltip("...for this many seconds.")]
        public float escapeHoldTime = 6f;
        [Tooltip("Pursue chases fail if the target gets this far away.")]
        public float loseTargetDistance = 220f;
        public string objectiveId = "chase";
        public bool startOnObjective = true;
        public bool lockPlayerInCar = true;

        bool m_running;
        float m_escapeTimer;

        void Start()
        {
            SetActors(false);
            MissionManager.Instance.OnObjectiveActivated += o => { if (startOnObjective && o.id == objectiveId) Begin(); };
            if (startOnObjective && MissionManager.Instance.IsObjectiveActive(objectiveId)) Begin();
        }

        void SetActors(bool on)
        {
            foreach (var p in pursuers) if (p != null) p.enabled = on;
            if (fleeingTarget != null) fleeingTarget.enabled = on;
        }

        public void Begin()
        {
            if (m_running) return;
            m_running = true;
            SetActors(true);
            foreach (var p in pursuers) if (p != null) p.mode = AIDriver.DriveMode.ChasePlayer;
            if (fleeingTarget != null) fleeingTarget.mode = AIDriver.DriveMode.Flee;
            var player = PlayerCharacter.Instance;
            if (playerCar != null && player != null && player.CurrentVehicle == null) player.EnterVehicle(playerCar);
            if (playerCar != null) playerCar.lockedForPlayer = lockPlayerInCar;
        }

        void Update()
        {
            if (!m_running) return;
            var player = PlayerCharacter.Instance;
            if (player == null) return;
            Vector3 pos = player.transform.position;

            if (kind == ChaseKind.Escape)
            {
                bool allFar = true;
                foreach (var p in pursuers)
                {
                    if (p == null) continue;
                    var car = p.GetComponent<CarController>();
                    if (car.IsWrecked) continue;
                    if (Vector3.Distance(p.transform.position, pos) < escapeDistance) { allFar = false; break; }
                }
                m_escapeTimer = allFar ? m_escapeTimer + Time.deltaTime : 0f;
                if (m_escapeTimer >= escapeHoldTime) Finish(true);
            }
            else if (fleeingTarget != null)
            {
                var car = fleeingTarget.GetComponent<CarController>();
                if (car.IsWrecked) Finish(true);
                else if (Vector3.Distance(fleeingTarget.transform.position, pos) > loseTargetDistance)
                {
                    UIRoot.Instance.HUD.Notify("You lost them.");
                    Finish(false);
                }
            }
        }

        /// <summary>Also called by a finish-line trigger.</summary>
        public void Finish(bool success)
        {
            if (!m_running) return;
            m_running = false;
            if (playerCar != null) playerCar.lockedForPlayer = false;
            foreach (var p in pursuers) if (p != null) p.mode = AIDriver.DriveMode.Idle;
            if (success) MissionManager.Instance.Progress(objectiveId);
            else UIRoot.Instance.GameOver.Show("MISSION FAILED", "The target got away.");
        }
    }
}
