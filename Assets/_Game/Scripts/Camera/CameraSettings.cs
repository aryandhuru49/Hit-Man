using UnityEngine;

namespace ExitClause.CameraSystem
{
    /// <summary>
    /// Every number that shapes the Vice City style camera. Edit the asset at
    /// Assets/_Game/Data/Camera/CameraSettings.asset while in Play mode to tune the feel live.
    /// </summary>
    [CreateAssetMenu(menuName = "Exit Clause/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("On foot chase camera")]
        [Tooltip("Height of the orbit pivot above the player's feet.")]
        public float pivotHeight = 1.4f;
        [Tooltip("How far behind the pivot the camera sits.")]
        public float distance = 5f;
        [Tooltip("Extra height between pivot and camera arm.")]
        public float verticalArm = 0.35f;
        public Vector3 shoulderOffset = Vector3.zero;
        [Tooltip("Downward tilt in degrees when the camera is resting behind the player.")]
        public float defaultPitch = 14f;
        public float minPitch = -25f;
        public float maxPitch = 65f;
        [Tooltip("Positional lag (x side, y up, z forward). Higher = looser.")]
        public Vector3 followDamping = new Vector3(0.25f, 0.4f, 0.3f);
        public float fieldOfView = 60f;

        [Header("Mouse / stick")]
        public float sensitivityX = 3f;
        public float sensitivityY = 2.2f;

        [Header("Swing back behind the player")]
        [Tooltip("Seconds without mouse input before the camera eases back while the player stands still.")]
        public float recenterDelay = 1.5f;
        [Tooltip("Seconds without mouse input before the camera eases back while the player moves.")]
        public float movingRecenterDelay = 0.4f;
        [Tooltip("Smoothing time of the swing (seconds). Higher = lazier swing.")]
        public float recenterSmoothTime = 0.45f;
        [Tooltip("Degrees per second the pitch returns to default.")]
        public float pitchReturnSpeed = 25f;

        [Header("Aim (over the shoulder)")]
        public float aimPivotHeight = 1.55f;
        public float aimDistance = 1.8f;
        public float aimVerticalArm = 0.1f;
        public Vector3 aimShoulderOffset = new Vector3(0.6f, 0f, 0f);
        public Vector3 aimDamping = new Vector3(0.05f, 0.05f, 0.05f);
        public float aimFieldOfView = 45f;
        public float aimSensitivityScale = 0.6f;
        [Header("Lock-on (classic auto aim)")]
        public float lockOnRange = 35f;
        [Tooltip("Max angle from the camera forward for a target to be picked.")]
        public float lockOnMaxAngle = 45f;
        public float lockOnTurnSpeed = 10f;

        [Header("Vehicle chase camera")]
        public float vehiclePivotHeight = 1.2f;
        public float vehicleDistance = 8f;
        public float vehicleVerticalArm = 1.9f;
        public float vehicleDefaultPitch = 9f;
        public Vector3 vehicleDamping = new Vector3(0.4f, 0.5f, 0.7f);
        public float vehicleFieldOfView = 60f;
        public float vehicleMaxFieldOfView = 74f;
        [Tooltip("Speed (m/s) where the speed zoom starts and where it is at full strength.")]
        public float speedZoomStart = 15f;
        public float speedZoomFull = 45f;
        public float speedZoomExtraDistance = 4f;
        [Tooltip("Smoothing time for the camera swinging behind the car.")]
        public float vehicleYawSmoothTime = 0.35f;

        [Header("Fixed high-angle survey camera (T)")]
        public float topDownHeight = 32f;
        public float topDownBack = 9f;
        public float topDownFieldOfView = 50f;
        public float topDownFollowSmoothing = 4f;

        [Header("Collision")]
        public float collisionRadius = 0.3f;
        [Tooltip("How fast the camera pulls in when blocked (lower = faster).")]
        public float dampingIntoCollision = 0.05f;
        [Tooltip("How fast it eases back out once clear (higher = smoother).")]
        public float dampingFromCollision = 0.6f;

        [Header("Blends")]
        public float blendTime = 0.35f;
    }
}
