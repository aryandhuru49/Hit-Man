using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Vehicles
{
    /// <summary>Lets the player get into a car with the Interact key.</summary>
    public class VehicleEntry : Interactable
    {
        public CarController car;
        public bool playerCanUse = true;
        [Tooltip("Objective progressed when the player gets in (optional).")]
        public string objectiveOnEnter;

        void Awake()
        {
            if (car == null) car = GetComponentInParent<CarController>();
            prompt = "Get in";
            range = 3f;
        }

        public override bool CanInteract(PlayerCharacter player) =>
            base.CanInteract(player) && playerCanUse && car != null && !car.IsWrecked && !car.IsPlayerDriven;

        public override void Interact(PlayerCharacter player)
        {
            var ai = car.GetComponent<AI.AIDriver>();
            if (ai != null) ai.enabled = false;
            player.EnterVehicle(car);
            if (!string.IsNullOrEmpty(objectiveOnEnter)) MissionManager.Instance.Progress(objectiveOnEnter);
        }
    }
}
