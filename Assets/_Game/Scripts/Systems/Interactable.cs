using System.Collections.Generic;
using ExitClause.Player;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Base class for anything the player can use with the Interact key: doors, terminals,
    /// clues, cars, hiding spots, people. PlayerInteractor picks the nearest one and shows its prompt.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();

        public string prompt = "Interact";
        public float range = 2.2f;
        [Tooltip("Optional point used for range checks; defaults to this transform.")]
        public Transform focusPoint;

        public Vector3 FocusPosition => focusPoint != null ? focusPoint.position : transform.position;

        public virtual string Prompt => prompt;
        public virtual bool CanInteract(PlayerCharacter player) => isActiveAndEnabled;
        public abstract void Interact(PlayerCharacter player);

        protected virtual void OnEnable() => All.Add(this);
        protected virtual void OnDisable() => All.Remove(this);
    }
}
