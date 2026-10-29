using ExitClause.Combat;
using ExitClause.Systems;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExitClause.CameraSystem
{
    /// <summary>
    /// Vice City style gameplay camera built on Cinemachine. It owns four CinemachineCameras
    /// (on foot, aim, vehicle, high-angle survey) and a pivot whose yaw/pitch it drives:
    /// free mouse orbit, then a damped swing back behind the player after a short pause.
    /// Cinemachine handles damping, collision pull-in and blends; Timeline cutscenes override it.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class GameCameraRig : MonoBehaviour
    {
        public enum Mode { OnFoot, Aim, Vehicle, TopDown }

        public static GameCameraRig Instance { get; private set; }

        public CameraSettings settings;
        public Mode CurrentMode { get; private set; }
        public Camera MainCamera { get; private set; }
        public LockOnTarget LockTarget { get; private set; }
        public float Yaw => m_yaw;
        public bool AimRequested { get; set; }
        public bool TopDownEnabled { get; private set; }

        Transform m_player;
        Transform m_vehicle;
        Rigidbody m_vehicleBody;
        Transform m_pivot;
        CinemachineCamera m_camOnFoot, m_camAim, m_camVehicle, m_camTopDown;
        CinemachineThirdPersonFollow m_tpfOnFoot, m_tpfAim, m_tpfVehicle;
        float m_yaw, m_pitch, m_yawVel, m_lastLookTime = -10f;
        Vector3 m_lastPlayerPos;
        bool m_playerMoving;

        void Awake()
        {
            Instance = this;
            if (settings == null && GameDatabase.Instance != null) settings = GameDatabase.Instance.cameraSettings;
            if (settings == null) settings = ScriptableObject.CreateInstance<CameraSettings>();

            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                MainCamera = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            MainCamera.nearClipPlane = 0.1f;
            var brain = MainCamera.GetComponent<CinemachineBrain>();
            if (brain == null) brain = MainCamera.gameObject.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, settings.blendTime);

            m_pivot = new GameObject("CameraPivot").transform;
            m_pivot.SetParent(transform, false);

            m_camOnFoot = CreateCam("CM_OnFoot", out m_tpfOnFoot);
            m_camAim = CreateCam("CM_Aim", out m_tpfAim);
            m_camVehicle = CreateCam("CM_Vehicle", out m_tpfVehicle);
            m_camTopDown = CreateCam("CM_TopDown", out _);
            ApplySettings();
            m_pitch = settings.defaultPitch;
        }

        CinemachineCamera CreateCam(string name, out CinemachineThirdPersonFollow follow)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var cam = go.AddComponent<CinemachineCamera>();
            cam.Priority = 10;
            follow = null;
            if (name != "CM_TopDown")
            {
                follow = go.AddComponent<CinemachineThirdPersonFollow>();
                cam.Target.TrackingTarget = m_pivot;
            }
            return cam;
        }

        /// <summary>Pushes the ScriptableObject values into the Cinemachine components (call after editing the asset).</summary>
        public void ApplySettings()
        {
            var s = settings;
            Configure(m_tpfOnFoot, s.distance, s.verticalArm, s.shoulderOffset, s.followDamping, Layers.EnvironmentMask);
            Configure(m_tpfAim, s.aimDistance, s.aimVerticalArm, s.aimShoulderOffset, s.aimDamping, Layers.EnvironmentMask);
            Configure(m_tpfVehicle, s.vehicleDistance, s.vehicleVerticalArm, Vector3.zero, s.vehicleDamping,
                (1 << Layers.Default) | (1 << Layers.Props));
            m_camAim.Target.TrackingTarget = m_pivot;
            m_camVehicle.Target.TrackingTarget = m_pivot;
        }

        void Configure(CinemachineThirdPersonFollow f, float dist, float arm, Vector3 shoulder, Vector3 damping, int mask)
        {
            f.CameraDistance = dist;
            f.VerticalArmLength = arm;
            f.ShoulderOffset = shoulder;
            f.CameraSide = 1f;
            f.Damping = damping;
            var o = f.AvoidObstacles;
            o.Enabled = true;
            o.CollisionFilter = mask;
            o.IgnoreTag = "Player";
            o.CameraRadius = settings.collisionRadius;
            o.DampingIntoCollision = settings.dampingIntoCollision;
            o.DampingFromCollision = settings.dampingFromCollision;
            f.AvoidObstacles = o;
        }

        public void SetPlayer(Transform player)
        {
            m_player = player;
            m_yaw = player.eulerAngles.y;
            m_pitch = settings.defaultPitch;
            m_lastPlayerPos = player.position;
            UpdatePivot(1f);
            foreach (var c in new[] { m_camOnFoot, m_camAim, m_camVehicle, m_camTopDown })
                c.OnTargetObjectWarped(m_pivot, Vector3.zero);
        }

        public void SetVehicle(Transform vehicle)
        {
            m_vehicle = vehicle;
            m_vehicleBody = vehicle != null ? vehicle.GetComponent<Rigidbody>() : null;
            if (vehicle != null) { m_yaw = vehicle.eulerAngles.y; m_pitch = settings.vehicleDefaultPitch; }
        }

        public void AddRecoil(float pitch, float yaw)
        {
            m_pitch -= pitch;
            m_yaw += Random.Range(-yaw, yaw);
        }

        /// <summary>Ray from the screen centre, used for free aim and interaction.</summary>
        public Ray CenterRay() => MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        public Vector3 FlatForward
        {
            get { var f = Quaternion.Euler(0, m_yaw, 0) * Vector3.forward; return f; }
        }

        void LateUpdate()
        {
            if (m_player == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (GameInput.Pressed(GameInput.TopDown)) TopDownEnabled = !TopDownEnabled;

            Vector3 delta = m_player.position - m_lastPlayerPos;
            delta.y = 0;
            m_playerMoving = delta.magnitude / dt > 0.5f;
            m_lastPlayerPos = m_player.position;

            var mode = m_vehicle != null ? Mode.Vehicle : (AimRequested ? Mode.Aim : Mode.OnFoot);
            if (TopDownEnabled && mode != Mode.Aim) mode = Mode.TopDown;
            if (mode != Mode.Aim) LockTarget = null;
            CurrentMode = mode;

            Vector2 look = GameInput.Vector(GameInput.Look);
            var dev = GameInput.Look?.activeControl?.device;
            if (dev is Gamepad) look *= dt * 60f;
            var prefs = SettingsManager.Instance != null ? SettingsManager.Instance.Data : new SettingsData();
            float sens = prefs.mouseSensitivity;
            float lookX = look.x * settings.sensitivityX * sens;
            float lookY = look.y * settings.sensitivityY * sens * (prefs.invertY ? -1f : 1f);
            bool hasLook = Mathf.Abs(lookX) + Mathf.Abs(lookY) > 0.01f;

            switch (mode)
            {
                case Mode.OnFoot:
                case Mode.TopDown:
                    if (hasLook) { m_yaw += lookX; m_pitch -= lookY; m_lastLookTime = Time.time; }
                    else
                    {
                        float delay = m_playerMoving ? settings.movingRecenterDelay : settings.recenterDelay;
                        if (Time.time - m_lastLookTime > delay)
                        {
                            m_yaw = Mathf.SmoothDampAngle(m_yaw, m_player.eulerAngles.y, ref m_yawVel, settings.recenterSmoothTime);
                            m_pitch = Mathf.MoveTowards(m_pitch, settings.defaultPitch, settings.pitchReturnSpeed * dt);
                        }
                    }
                    break;
                case Mode.Aim:
                    UpdateAim(lookX, lookY, prefs.freeAim, dt);
                    break;
                case Mode.Vehicle:
                    if (hasLook) { m_yaw += lookX; m_pitch -= lookY; m_lastLookTime = Time.time; }
                    else if (Time.time - m_lastLookTime > settings.movingRecenterDelay)
                    {
                        m_yaw = Mathf.SmoothDampAngle(m_yaw, m_vehicle.eulerAngles.y, ref m_yawVel, settings.vehicleYawSmoothTime);
                        m_pitch = Mathf.MoveTowards(m_pitch, settings.vehicleDefaultPitch, settings.pitchReturnSpeed * dt);
                    }
                    break;
            }
            m_pitch = Mathf.Clamp(m_pitch, settings.minPitch, settings.maxPitch);

            UpdatePivot(dt);
            UpdateLenses(mode, prefs.fieldOfViewOffset);
            SetPriorities(mode);
        }

        void UpdateAim(float lookX, float lookY, bool freeAim, float dt)
        {
            float scale = settings.aimSensitivityScale;
            if (!freeAim)
            {
                if (LockTarget == null || !LockTarget.IsValid || GameInput.Pressed(GameInput.CycleTarget))
                    LockTarget = FindLockTarget(LockTarget);
            }
            else LockTarget = null;

            if (LockTarget != null)
            {
                Vector3 from = m_player.position + Vector3.up * settings.aimPivotHeight;
                Vector3 to = LockTarget.AimPosition - from;
                var desired = Quaternion.LookRotation(to).eulerAngles;
                float desiredPitch = desired.x > 180f ? desired.x - 360f : desired.x;
                float k = 1f - Mathf.Exp(-settings.lockOnTurnSpeed * dt);
                m_yaw = Mathf.LerpAngle(m_yaw, desired.y, k) + lookX * scale * 0.3f;
                m_pitch = Mathf.Lerp(m_pitch, desiredPitch, k) - lookY * scale * 0.3f;
            }
            else
            {
                m_yaw += lookX * scale;
                m_pitch -= lookY * scale;
            }
        }

        /// <summary>Nearest valid target in front of the camera; when cycling, the next one after current.</summary>
        LockOnTarget FindLockTarget(LockOnTarget current)
        {
            LockOnTarget best = null, firstOther = null;
            float bestScore = float.MaxValue;
            Vector3 eye = m_player.position + Vector3.up * settings.aimPivotHeight;
            Vector3 fwd = Quaternion.Euler(0, m_yaw, 0) * Vector3.forward;
            foreach (var t in LockOnTarget.All)
            {
                if (!t.IsValid || t == current) continue;
                Vector3 to = t.AimPosition - eye;
                float dist = to.magnitude;
                if (dist > settings.lockOnRange) continue;
                float angle = Vector3.Angle(fwd, new Vector3(to.x, 0, to.z));
                if (angle > settings.lockOnMaxAngle) continue;
                if (Physics.Linecast(eye, t.AimPosition, out var hit, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore)
                    && !hit.collider.transform.IsChildOf(t.transform)) continue;
                float score = angle * 2f + dist;
                if (score < bestScore) { bestScore = score; best = t; }
                if (firstOther == null) firstOther = t;
            }
            return best ?? firstOther;
        }

        void UpdatePivot(float dt)
        {
            float height = CurrentMode == Mode.Aim ? settings.aimPivotHeight : settings.pivotHeight;
            Vector3 basePos = m_player.position;
            float yaw = m_yaw;
            if (CurrentMode == Mode.Vehicle && m_vehicle != null)
            {
                basePos = m_vehicle.position;
                height = settings.vehiclePivotHeight;
                if (GameInput.Held(GameInput.LookBehind)) yaw += 180f;
                float speed = m_vehicleBody != null ? m_vehicleBody.linearVelocity.magnitude : 0f;
                float t = Mathf.InverseLerp(settings.speedZoomStart, settings.speedZoomFull, speed);
                m_tpfVehicle.CameraDistance = settings.vehicleDistance + settings.speedZoomExtraDistance * t;
            }
            m_pivot.SetPositionAndRotation(basePos + Vector3.up * height, Quaternion.Euler(m_pitch, yaw, 0f));

            if (CurrentMode == Mode.TopDown)
            {
                Vector3 target = basePos + new Vector3(0, settings.topDownHeight, -settings.topDownBack);
                var tr = m_camTopDown.transform;
                tr.position = Vector3.Lerp(tr.position, target, 1f - Mathf.Exp(-settings.topDownFollowSmoothing * dt));
                tr.rotation = Quaternion.LookRotation(basePos - tr.position);
            }
        }

        void UpdateLenses(Mode mode, float fovOffset)
        {
            SetFov(m_camOnFoot, settings.fieldOfView + fovOffset);
            SetFov(m_camAim, settings.aimFieldOfView + fovOffset);
            SetFov(m_camTopDown, settings.topDownFieldOfView);
            float speed = m_vehicleBody != null ? m_vehicleBody.linearVelocity.magnitude : 0f;
            float t = Mathf.InverseLerp(settings.speedZoomStart, settings.speedZoomFull, speed);
            SetFov(m_camVehicle, Mathf.Lerp(settings.vehicleFieldOfView, settings.vehicleMaxFieldOfView, t) + fovOffset);
        }

        static void SetFov(CinemachineCamera cam, float fov)
        {
            var lens = cam.Lens;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.1f;
            cam.Lens = lens;
        }

        void SetPriorities(Mode mode)
        {
            m_camOnFoot.Priority = mode == Mode.OnFoot ? 20 : 10;
            m_camAim.Priority = mode == Mode.Aim ? 20 : 10;
            m_camVehicle.Priority = mode == Mode.Vehicle ? 20 : 10;
            m_camTopDown.Priority = mode == Mode.TopDown ? 20 : 10;
        }
    }
}
