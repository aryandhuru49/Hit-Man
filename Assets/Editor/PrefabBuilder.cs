using System.IO;
using ExitClause.AI;
using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Stealth;
using ExitClause.Systems;
using ExitClause.Vehicles;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Builds the capsule-based greybox prefabs: Player, Agent, Civilian, Car and Drone.
    /// Real characters are added automatically from Assets/ThirdParty/Characters (see CharacterArt).
    /// </summary>
    public static class PrefabBuilder
    {
        public const string Folder = "Assets/_Game/Prefabs";

        [MenuItem("Exit Clause/4. Build Prefabs", priority = 4)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(Folder);
            CharacterArt.ResetControllers();
            Save(BuildPlayer(), "Player");
            Save(BuildAgent(), "Agent");
            Save(BuildCivilian(), "Civilian");
            Save(BuildCar(), "Car");
            Save(BuildDrone(), "Drone");
            AssetDatabase.SaveAssets();
        }

        /// <summary>Rebuilds only Player.prefab (used by Exit Clause > Art), leaving the other prefabs untouched.</summary>
        public static void BuildPlayerOnly()
        {
            Directory.CreateDirectory(Folder);
            CharacterArt.ResetControllers();
            Save(BuildPlayer(), "Player");
            AssetDatabase.SaveAssets();
        }

        /// <summary>Rebuilds only Agent.prefab and Civilian.prefab (used by Exit Clause > Art).</summary>
        public static void BuildNpcsOnly()
        {
            Directory.CreateDirectory(Folder);
            CharacterArt.ResetControllers();
            Save(BuildAgent(), "Agent");
            Save(BuildCivilian(), "Civilian");
            AssetDatabase.SaveAssets();
        }

        public static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{name}.prefab");

        static void Save(GameObject go, string name)
        {
            PrefabUtility.SaveAsPrefabAsset(go, $"{Folder}/{name}.prefab");
            Object.DestroyImmediate(go);
        }

        public static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, string material, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localEulerAngles = euler;
            go.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(material);
            go.layer = parent.gameObject.layer;
            return go;
        }

        /// <summary>Humanoid stand-in: body capsule, head and arms. Returned object is the "Body" root.</summary>
        public static GameObject Humanoid(Transform parent, string suit, bool gun)
        {
            var body = new GameObject("Body");
            body.layer = parent.gameObject.layer;
            body.transform.SetParent(parent, false);
            Part(body.transform, PrimitiveType.Capsule, "Torso", new Vector3(0, 0.95f, 0), new Vector3(0.62f, 0.62f, 0.42f), suit);
            Part(body.transform, PrimitiveType.Capsule, "Legs", new Vector3(0, 0.45f, 0), new Vector3(0.5f, 0.45f, 0.36f), suit);
            Part(body.transform, PrimitiveType.Sphere, "Head", new Vector3(0, 1.68f, 0.02f), new Vector3(0.26f, 0.3f, 0.28f), "Skin");
            Part(body.transform, PrimitiveType.Capsule, "ArmL", new Vector3(-0.38f, 1.05f, 0), new Vector3(0.16f, 0.36f, 0.16f), suit);
            Part(body.transform, PrimitiveType.Capsule, "ArmR", new Vector3(0.38f, 1.05f, 0.08f), new Vector3(0.16f, 0.36f, 0.16f), suit, new Vector3(-30, 0, 0));
            if (gun) Part(body.transform, PrimitiveType.Cube, "Gun", new Vector3(0.38f, 0.95f, 0.38f), new Vector3(0.06f, 0.12f, 0.32f), "Metal");
            return body;
        }

        static GameObject BuildPlayer()
        {
            var root = new GameObject("Player") { layer = Layers.Player, tag = "Player" };
            var cc = root.AddComponent<CharacterController>();
            cc.radius = 0.35f; cc.height = 1.8f; cc.center = new Vector3(0, 0.9f, 0); cc.stepOffset = 0.4f; cc.slopeLimit = 50f;
            var h = root.AddComponent<Health>();
            h.maxHealth = 100; h.current = 100;
            root.AddComponent<PlayerMotor>();
            var combat = root.AddComponent<PlayerCombat>();
            root.AddComponent<PlayerStealth>();
            root.AddComponent<PlayerInteractor>();
            root.AddComponent<RageMode>();
            var pc = root.AddComponent<PlayerCharacter>();
            root.AddComponent<CharacterAnimatorBridge>();
            pc.bodyVisual = Humanoid(root.transform, "PlayerSuit", true);
            var model = CharacterArt.Attach(root, "Player");
            if (model != null)
            {
                Object.DestroyImmediate(pc.bodyVisual);
                pc.bodyVisual = model;
            }
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0.38f, 1.0f, 0.6f);
            combat.muzzle = muzzle.transform;
            return root;
        }

        static GameObject BuildAgent()
        {
            var root = new GameObject("Agent") { layer = Layers.Character };
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0); col.height = 1.8f; col.radius = 0.35f;
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.35f; agent.height = 1.8f; agent.speed = 1.8f; agent.angularSpeed = 360; agent.acceleration = 12; agent.stoppingDistance = 0.3f;
            var h = root.AddComponent<Health>();
            h.maxHealth = 100; h.current = 100;
            var eyes = new GameObject("Eyes");
            eyes.transform.SetParent(root.transform, false);
            eyes.transform.localPosition = new Vector3(0, 1.65f, 0.1f);
            var per = root.AddComponent<AIPerception>();
            per.eyes = eyes.transform;
            per.acceptsDisguise = "guard";
            var ai = root.AddComponent<EnemyAI>();
            ai.weapon = DataBuilder.Weapon("agency_pistol");
            var chest = new GameObject("AimPoint");
            chest.transform.SetParent(root.transform, false);
            chest.transform.localPosition = new Vector3(0, 1.3f, 0);
            root.AddComponent<LockOnTarget>().aimPoint = chest.transform;
            Humanoid(root.transform, "EnemySuit", true);
            CharacterArt.AttachNpc(root, "Agent");
            AddCone(root.transform, per);
            return root;
        }

        public static void AddCone(Transform parent, AIPerception per)
        {
            var cone = new GameObject("VisionCone", typeof(MeshFilter), typeof(MeshRenderer));
            cone.transform.SetParent(parent, false);
            cone.layer = Layers.IgnoreRaycast;
            cone.AddComponent<VisionConeVisualizer>().perception = per;
        }

        static GameObject BuildCivilian()
        {
            var root = new GameObject("Civilian") { layer = Layers.Character };
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0); col.height = 1.8f; col.radius = 0.33f;
            root.AddComponent<Rigidbody>().isKinematic = true;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.33f; agent.height = 1.8f; agent.speed = 1.4f; agent.angularSpeed = 300; agent.stoppingDistance = 0.3f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            var h = root.AddComponent<Health>();
            h.maxHealth = 40; h.current = 40;
            root.AddComponent<CivilianAI>();
            Humanoid(root.transform, "Civilian", false);
            CharacterArt.AttachNpc(root, "Civilian");
            return root;
        }

        static GameObject BuildCar()
        {
            var root = new GameObject("Car") { layer = Layers.Vehicle };
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 1200;
            var body = root.AddComponent<BoxCollider>();
            body.center = new Vector3(0, 0.15f, 0); body.size = new Vector3(1.8f, 0.8f, 4.2f);
            var cabin = root.AddComponent<BoxCollider>();
            cabin.center = new Vector3(0, 0.75f, -0.2f); cabin.size = new Vector3(1.6f, 0.55f, 2.2f);
            var h = root.AddComponent<Health>();
            h.maxHealth = 250; h.current = 250;
            var car = root.AddComponent<CarController>();
            car.halfExtents = new Vector3(0.82f, 0.25f, 1.45f);
            root.AddComponent<VehicleEntry>().car = car;
            var ai = root.AddComponent<AIDriver>();
            ai.enabled = false;

            var visual = new GameObject("Body") { layer = Layers.Vehicle };
            visual.transform.SetParent(root.transform, false);
            Part(visual.transform, PrimitiveType.Cube, "Shell", new Vector3(0, 0.15f, 0), new Vector3(1.8f, 0.6f, 4.2f), "CarBlack");
            Part(visual.transform, PrimitiveType.Cube, "Cabin", new Vector3(0, 0.72f, -0.25f), new Vector3(1.58f, 0.55f, 2.1f), "Glass");
            Part(visual.transform, PrimitiveType.Cube, "HeadlightL", new Vector3(-0.6f, 0.2f, 2.1f), new Vector3(0.3f, 0.12f, 0.05f), "LampWarm");
            Part(visual.transform, PrimitiveType.Cube, "HeadlightR", new Vector3(0.6f, 0.2f, 2.1f), new Vector3(0.3f, 0.12f, 0.05f), "LampWarm");
            var wheels = new Transform[4];
            Vector3[] wp = { new Vector3(-0.85f, -0.3f, 1.35f), new Vector3(0.85f, -0.3f, 1.35f), new Vector3(-0.85f, -0.3f, -1.35f), new Vector3(0.85f, -0.3f, -1.35f) };
            for (int i = 0; i < 4; i++)
            {
                var pivot = new GameObject("Wheel" + i);
                pivot.transform.SetParent(visual.transform, false);
                pivot.transform.localPosition = wp[i];
                Part(pivot.transform, PrimitiveType.Cylinder, "Tire", Vector3.zero, new Vector3(0.66f, 0.12f, 0.66f), "Tire", new Vector3(0, 0, 90));
                wheels[i] = pivot.transform;
            }
            car.wheelVisuals = wheels;
            return root;
        }

        static GameObject BuildDrone()
        {
            var root = new GameObject("Drone") { layer = Layers.Character };
            var col = root.AddComponent<SphereCollider>();
            col.radius = 0.45f;
            root.AddComponent<Rigidbody>().isKinematic = true;
            var h = root.AddComponent<Health>();
            h.maxHealth = 40; h.current = 40;
            var eyes = new GameObject("Eyes");
            eyes.transform.SetParent(root.transform, false);
            eyes.transform.localEulerAngles = new Vector3(40, 0, 0);
            var per = root.AddComponent<AIPerception>();
            per.eyes = eyes.transform; per.viewDistance = 16; per.viewAngle = 70; per.detectTime = 1.2f; per.closeAwareness = 0f;
            var drone = root.AddComponent<DroneAI>();
            drone.weapon = DataBuilder.Weapon("drone_gun");
            root.AddComponent<LockOnTarget>().aimPoint = root.transform;
            Part(root.transform, PrimitiveType.Cube, "Core", Vector3.zero, new Vector3(0.5f, 0.18f, 0.5f), "Metal");
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90 + 45;
                var p = Quaternion.Euler(0, a, 0) * new Vector3(0, 0.08f, 0.45f);
                Part(root.transform, PrimitiveType.Cylinder, "Rotor" + i, p, new Vector3(0.35f, 0.01f, 0.35f), "NeonCyan");
            }
            AddCone(root.transform, per);
            return root;
        }
    }
}
