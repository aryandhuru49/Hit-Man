using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Named area (aisle, parking lot). Tells ObservableClues inside it which zone they are in.</summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneVolume : MonoBehaviour
    {
        public string zoneId = "zone";

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = Layers.Trigger;
        }

        void OnTriggerStay(Collider other)
        {
            var clue = other.GetComponentInParent<ObservableClue>();
            if (clue != null) clue.CurrentZone = zoneId;
        }
    }
}
