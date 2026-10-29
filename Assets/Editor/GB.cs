using System.Collections.Generic;
using System.IO;
using ExitClause.AI;
using ExitClause.Combat;
using ExitClause.Hacking;
using ExitClause.Stealth;
using ExitClause.Story;
using ExitClause.Systems;
using ExitClause.Vehicles;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Greybox kit: small, readable helpers that scene builders call to lay out a level
    /// (blocks, rooms, roads, lamps, NPCs, triggers, cars, cutscenes), then light it,
    /// bake its NavMesh and save it. Everything is reproducible by re-running the builder.
    /// </summary>
    public static class GB
    {
        public static Transform Env, Actors, Logic, Cams;
        public static Camera MainCam;
        static string s_scene;
        /// <summary>True for night moods; facades light some windows.</summary>
        public static bool Night;
        static StaticEditorFlags StaticFlags => StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic |
                                               StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;

        // ---------- scene lifecycle ----------

        public static void NewScene(string name, MoodPreset mood)
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Clause", "Stop Play mode first (click the blue Play button), then rebuild the scene.", "OK");
                throw new System.InvalidOperationException("Scenes cannot be rebuilt while the game is playing.");
            }
            s_scene = name;
            Night =mood != null && mood.sunLux < 10f;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Env = new GameObject("Environment").transform;
            Actors = new GameObject("Actors").transform;
            Logic = new GameObject("Logic").transform;
            Cams = new GameObject("CutsceneCameras").transform;

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            MainCam = camGo.AddComponent<Camera>();
            MainCam.nearClipPlane = 0.1f;
            MainCam.farClipPlane = 1500f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<HDAdditionalCameraData>();
            var brain = camGo.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.6f);
            camGo.transform.position = new Vector3(0, 10, -10);

            if (mood != null) LightingBuilder.Apply(mood, MainCam);
        }

        public static SceneMissionDirector Finish(MissionDefinition mission, bool navmesh = true)
        {
            var dirGo = new GameObject("SceneMissionDirector");
            dirGo.transform.SetParent(Logic);
            var director = dirGo.AddComponent<SceneMissionDirector>();
            director.mission = mission;
            if (navmesh) BakeNavMesh();
            return director;
        }

        public static void Save()
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            string path = $"Assets/_Game/Scenes/{s_scene}.unity";
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            Debug.Log("[ExitClause] Saved " + path);
        }

        static void BakeNavMesh()
        {
            var go = new GameObject("NavMesh");
            go.transform.SetParent(Logic);
            var surface = go.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << Layers.Default;
            surface.BuildNavMesh();
            if (surface.navMeshData != null)
            {
                string path = $"Assets/_Game/Scenes/{s_scene}_NavMesh.asset";
                if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(surface.navMeshData, path);
            }
            go.AddComponent<RuntimeNavMeshBaker>();
        }

        // ---------- geometry ----------

        public static GameObject Box(string name, Vector3 center, Vector3 size, string mat, Transform parent = null, float yaw = 0f, int layer = Layers.Default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent != null ? parent : Env, false);
            go.transform.position = center;
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(mat);
            go.layer = layer;
            GameObjectUtility.SetStaticEditorFlags(go, StaticFlags);
            return go;
        }

        public static GameObject Ground(Vector3 center, Vector2 size, string mat) =>
            Box("Ground", center - Vector3.up * 0.5f, new Vector3(size.x, 1f, size.y), mat);

        /// <summary>Solid building block with a darker plinth and roof cap, sitting on the ground at its centre XZ.</summary>
        public static GameObject Building(string name, Vector3 groundCenter, Vector3 size, string mat, float yaw = 0f)
        {
            var b = Box(name, groundCenter + Vector3.up * size.y * 0.5f, size, mat, null, yaw);
            Box(name + "_Roof", groundCenter + Vector3.up * (size.y + 0.15f), new Vector3(size.x + 0.4f, 0.3f, size.z + 0.4f), "ConcreteDark", b.transform.parent, yaw);
            FacadeBuilder.Add(name, groundCenter, size, yaw, b.transform.parent, Night);
            return b;
        }

        /// <summary>Wall from a to b on the ground, with optional doorway gap centred at gapT (0..1) of width gapWidth.</summary>
        public static void Wall(Vector3 a, Vector3 b, float height, string mat, float thickness = 0.3f, float gapT = -1f, float gapWidth = 1.6f, float gapHeight = 2.4f)
        {
            Vector3 dir = b - a;
            float len = dir.magnitude;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Vector3 n = dir.normalized;
            if (gapT < 0f)
            {
                Box("Wall", (a + b) * 0.5f + Vector3.up * height * 0.5f, new Vector3(thickness, height, len), mat, null, yaw);
                return;
            }
            float gapCenter = gapT * len;
            float l1 = Mathf.Max(0, gapCenter - gapWidth * 0.5f);
            float l2 = Mathf.Max(0, len - (gapCenter + gapWidth * 0.5f));
            if (l1 > 0.01f) Box("Wall", a + n * l1 * 0.5f + Vector3.up * height * 0.5f, new Vector3(thickness, height, l1), mat, null, yaw);
            if (l2 > 0.01f) Box("Wall", b - n * l2 * 0.5f + Vector3.up * height * 0.5f, new Vector3(thickness, height, l2), mat, null, yaw);
            if (height > gapHeight)
                Box("Lintel", a + n * gapCenter + Vector3.up * (gapHeight + (height - gapHeight) * 0.5f), new Vector3(thickness, height - gapHeight, gapWidth), mat, null, yaw);
        }

        /// <summary>
        /// Rectangular room: floor, four walls (doorways given per side as t positions: N,E,S,W), optional ceiling.
        /// Returns the floor object.
        /// </summary>
        public static GameObject Room(string name, Vector3 floorCenter, Vector2 size, float height, string wallMat, string floorMat,
            float doorN = -1, float doorE = -1, float doorS = -1, float doorW = -1, bool ceiling = true)
        {
            float hx = size.x * 0.5f, hz = size.y * 0.5f;
            var floor = Box(name + "_Floor", floorCenter - Vector3.up * 0.1f, new Vector3(size.x, 0.2f, size.y), floorMat);
            Vector3 c = floorCenter;
            Wall(c + new Vector3(-hx, 0, hz), c + new Vector3(hx, 0, hz), height, wallMat, 0.3f, doorN);
            Wall(c + new Vector3(hx, 0, hz), c + new Vector3(hx, 0, -hz), height, wallMat, 0.3f, doorE);
            Wall(c + new Vector3(hx, 0, -hz), c + new Vector3(-hx, 0, -hz), height, wallMat, 0.3f, doorS);
            Wall(c + new Vector3(-hx, 0, -hz), c + new Vector3(-hx, 0, hz), height, wallMat, 0.3f, doorW);
            if (ceiling) Box(name + "_Ceiling", floorCenter + Vector3.up * (height + 0.1f), new Vector3(size.x + 0.3f, 0.2f, size.y + 0.3f), wallMat);
            return floor;
        }

        /// <summary>Staircase climbing from 'from' toward 'dir' by 'rise' metres.</summary>
        public static void Stairs(Vector3 from, Vector3 dir, float rise, float width, string mat)
        {
            int steps = Mathf.CeilToInt(rise / 0.25f);
            float stepH = rise / steps;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            for (int i = 0; i < steps; i++)
            {
                float top = stepH * (i + 1);
                Vector3 p = from + dir.normalized * (0.35f * i + 0.175f);
                Box("Step", new Vector3(p.x, from.y + top * 0.5f, p.z), new Vector3(width, top, 0.35f), mat, null, yaw);
            }
        }

        public static GameObject Lamp(Vector3 groundPos, Color color, float lumens = 3000f, float range = 18f, float height = 4.5f)
        {
            Box("LampPost", groundPos + Vector3.up * height * 0.5f, new Vector3(0.15f, height, 0.15f), "Metal");
            Box("LampHead", groundPos + Vector3.up * height, new Vector3(0.5f, 0.15f, 0.5f), "LampWarm");
            var l = LightingBuilder.PointLight(Env, groundPos + Vector3.up * (height - 0.3f), color, lumens, range);
            return l.gameObject;
        }

        public static Light Light(Vector3 pos, Color color, float lumens, float range) => LightingBuilder.PointLight(Env, pos, color, lumens, range);

        public static void Road(Vector3 a, Vector3 b, float width, string mat = "Asphalt")
        {
            Vector3 d = b - a;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            Box("Road", (a + b) * 0.5f + Vector3.up * 0.02f, new Vector3(width, 0.06f, d.magnitude + width * 0.5f), mat, null, yaw);
        }

        public static void Crate(Vector3 groundPos, float size = 1f, string mat = "Wood") =>
            Box("Crate", groundPos + Vector3.up * size * 0.5f, Vector3.one * size, mat);

        // ---------- actors ----------

        public static GameObject Player(Vector3 pos, float yaw)
        {
            Spawn("default", pos, yaw);
            var p = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Player"));
            p.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            p.transform.SetParent(Actors, true);
            return p;
        }

        public static SpawnPoint Spawn(string id, Vector3 pos, float yaw)
        {
            var go = new GameObject("Spawn_" + id);
            go.transform.SetParent(Logic);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var sp = go.AddComponent<SpawnPoint>();
            sp.id = id;
            return sp;
        }

        public static PatrolRoute Route(string name, bool loop, params Vector3[] points)
        {
            var go = new GameObject("Route_" + name);
            go.transform.SetParent(Logic);
            foreach (var p in points)
            {
                var wp = new GameObject("WP");
                wp.transform.SetParent(go.transform);
                wp.transform.position = p;
            }
            var r = go.AddComponent<PatrolRoute>();
            r.loop = loop;
            return r;
        }

        public static EnemyAI Enemy(string name, Vector3 pos, float yaw, PatrolRoute route = null, string weapon = "agency_pistol",
            string disguiseAccepted = "", string suit = "EnemySuit", bool active = true)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Agent"));
            go.name = name;
            go.transform.SetParent(Actors, true);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var ai = go.GetComponent<EnemyAI>();
            ai.displayName = name;
            ai.route = route;
            ai.stationary = route == null;
            ai.weapon = DataBuilder.Weapon(weapon);
            go.GetComponent<AIPerception>().acceptsDisguise = disguiseAccepted;
            if (suit != "EnemySuit")
                foreach (var r in go.transform.Find("Body").GetComponentsInChildren<Renderer>())
                    if (r.name != "Head" && r.name != "Gun") r.sharedMaterial = MaterialLibrary.Get(suit);
            go.SetActive(active);
            return ai;
        }

        public static CivilianAI Civilian(string name, Vector3 pos, PatrolRoute route, string mat = "Civilian")
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Civilian"));
            go.name = name;
            go.transform.SetParent(Actors, true);
            go.transform.position = pos;
            var c = go.GetComponent<CivilianAI>();
            c.route = route;
            if (mat != "Civilian") foreach (var r in go.transform.Find("Body").GetComponentsInChildren<Renderer>()) if (r.name != "Head") r.sharedMaterial = MaterialLibrary.Get(mat);
            return c;
        }

        /// <summary>Static story character (no AI) with a collider so they can be talked to or shot.</summary>
        public static GameObject Npc(string name, Vector3 pos, float yaw, string suit = "Civilian")
        {
            var root = new GameObject(name) { layer = Layers.Character };
            root.transform.SetParent(Actors, true);
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0); col.height = 1.8f; col.radius = 0.33f;
            PrefabBuilder.Humanoid(root.transform, suit, false);
            CharacterArt.AttachNpc(root, "Civilian");
            return root;
        }

        public static CarController Car(string name, Vector3 pos, float yaw, string mat = "CarBlack", bool playerUsable = true)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Car"));
            go.name = name;
            go.transform.SetParent(Actors, true);
            go.transform.SetPositionAndRotation(pos + Vector3.up * 0.7f, Quaternion.Euler(0, yaw, 0));
            go.transform.Find("Body/Shell").GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(mat);
            go.GetComponent<VehicleEntry>().playerCanUse = playerUsable;
            return go.GetComponent<CarController>();
        }

        public static DroneAI Drone(string name, Vector3 pos, PatrolRoute route, bool armed)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Drone"));
            go.name = name;
            go.transform.SetParent(Actors, true);
            go.transform.position = pos;
            var d = go.GetComponent<DroneAI>();
            d.route = route;
            if (!armed) d.weapon = null;
            return d;
        }

        // ---------- logic ----------

        public static T Trigger<T>(string name, Vector3 pos, Vector3 size) where T : Component
        {
            var go = new GameObject(name) { layer = Layers.Trigger };
            go.transform.SetParent(Logic);
            go.transform.position = pos;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
            bc.isTrigger = true;
            return go.AddComponent<T>();
        }

        public static ObjectiveTrigger ObjTrigger(string objectiveId, Vector3 pos, Vector3 size, bool marker = true, bool vehicle = false)
        {
            var t = Trigger<ObjectiveTrigger>("Trigger_" + objectiveId, pos, size);
            t.objectiveId = objectiveId;
            t.requireVehicle = vehicle;
            if (marker) Marker(objectiveId, pos - Vector3.up * size.y * 0.5f);
            return t;
        }

        public static ObjectiveMarker Marker(string objectiveId, Vector3 pos, string label = "")
        {
            var go = new GameObject("Marker_" + objectiveId);
            go.transform.SetParent(Logic);
            go.transform.position = pos;
            var m = go.AddComponent<ObjectiveMarker>();
            m.objectiveId = objectiveId;
            m.label = label;
            return m;
        }

        /// <summary>Visible prop with an interactable component (laptop, document, mailbox...).</summary>
        public static T Prop<T>(string name, Vector3 pos, Vector3 size, string mat, string markerObjective = null) where T : Component
        {
            var go = Box(name, pos, size, mat, Logic, 0f, Layers.Props);
            GameObjectUtility.SetStaticEditorFlags(go, 0);
            if (markerObjective != null) Marker(markerObjective, pos);
            return go.AddComponent<T>();
        }

        public static Door Door(Vector3 hingePos, float yaw, float width, bool locked, string objectiveOnOpen = null, string mat = "Wood")
        {
            var hinge = new GameObject("Door") { layer = Layers.Props };
            hinge.transform.SetParent(Env);
            hinge.transform.SetPositionAndRotation(hingePos, Quaternion.Euler(0, yaw, 0));
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.name = "Leaf";
            leaf.layer = Layers.Props;
            leaf.transform.SetParent(hinge.transform, false);
            leaf.transform.localPosition = new Vector3(width * 0.5f, 1.15f, 0);
            leaf.transform.localScale = new Vector3(width, 2.3f, 0.08f);
            leaf.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(mat);
            var obstacle = leaf.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            var d = hinge.AddComponent<Door>();
            d.locked = locked;
            d.objectiveOnOpen = objectiveOnOpen;
            var focus = new GameObject("Focus");
            focus.transform.SetParent(hinge.transform, false);
            focus.transform.localPosition = new Vector3(width * 0.5f, 1f, 0);
            d.focusPoint = focus.transform;
            return d;
        }

        public static HidingSpot Hide(Vector3 groundPos, float yaw, string mat = "Wood")
        {
            var go = Box("HidingSpot", groundPos + Vector3.up * 1.1f, new Vector3(1.2f, 2.2f, 0.8f), mat, Logic, yaw, Layers.Props);
            var h = go.AddComponent<HidingSpot>();
            var exit = new GameObject("Exit");
            exit.transform.SetParent(go.transform, false);
            exit.transform.localPosition = new Vector3(0, -0.5f, 1.2f);
            h.exitPoint = exit.transform;
            h.range = 1.8f;
            return h;
        }

        public static void Activate(string objectiveId, ObjectiveActivator.Mode mode, DialogueSequence dialogue, params GameObject[] targets)
        {
            var go = new GameObject("Activator_" + objectiveId + "_" + mode);
            go.transform.SetParent(Logic);
            var a = go.AddComponent<ObjectiveActivator>();
            a.objectiveId = objectiveId;
            a.mode = mode;
            a.targets = targets;
            a.dialogue = dialogue;
        }

        public static ObjectiveReaction Reaction(string objectiveId, bool onComplete)
        {
            var go = new GameObject("Reaction_" + objectiveId);
            go.transform.SetParent(Logic);
            var r = go.AddComponent<ObjectiveReaction>();
            r.objectiveId = objectiveId;
            r.onComplete = onComplete;
            return r;
        }

        public static ObjectiveOnDeath OnDeath(Component target, string objectiveId, string flag = null)
        {
            var o = target.gameObject.AddComponent<ObjectiveOnDeath>();
            o.objectiveId = objectiveId;
            o.setFlag = flag;
            return o;
        }

        public static FlagActivator Flag(string name, string requiredFlag, string forbiddenFlag, GameObject[] whenTrue, GameObject[] whenFalse = null,
            string counter = null, int atLeast = 0)
        {
            var go = new GameObject("Flag_" + name);
            go.transform.SetParent(Logic);
            var f = go.AddComponent<FlagActivator>();
            f.requiredFlag = requiredFlag;
            f.forbiddenFlag = forbiddenFlag;
            f.targets = whenTrue ?? new GameObject[0];
            f.targetsWhenFalse = whenFalse ?? new GameObject[0];
            f.counterKey = counter;
            f.counterAtLeast = atLeast;
            return f;
        }

        public static HackingTerminal Terminal(string name, Vector3 pos, DeviceDefinition device, string markerObjective = null)
        {
            var t = Prop<HackingTerminal>(name, pos, new Vector3(0.4f, 0.05f, 0.25f), "ScreenGreen", markerObjective);
            t.device = device;
            t.range = 2f;
            return t;
        }

        // ---------- cutscenes ----------

        public static CinemachineCamera Shot(string name, Vector3 pos, Vector3 lookAt, float fov = 40f)
        {
            var go = new GameObject("Shot_" + name);
            go.transform.SetParent(Cams);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(lookAt - pos);
            var cam = go.AddComponent<CinemachineCamera>();
            cam.Priority = 0;
            var lens = cam.Lens;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.1f;
            cam.Lens = lens;
            var noise = go.AddComponent<CinemachineBasicMultiChannelPerlin>();
            noise.NoiseProfile = AssetDatabase.LoadAssetAtPath<NoiseSettings>("Packages/com.unity.cinemachine/Presets/Noise/Handheld_normal_mild.asset");
            noise.AmplitudeGain = 0.4f;
            noise.FrequencyGain = 0.5f;
            return cam;
        }

        /// <summary>Creates a cutscene: PlayableDirector + Timeline with one Cinemachine shot and one subtitle clip per dialogue line.</summary>
        public static CutsceneDirector Cutscene(string name, DialogueSequence dialogue, params CinemachineCamera[] shots)
        {
            var go = new GameObject("Cutscene_" + name);
            go.transform.SetParent(Logic);
            var director = go.AddComponent<PlayableDirector>();
            director.playOnAwake = false;
            var cs = go.AddComponent<CutsceneDirector>();
            cs.director = director;
            cs.dialogue = dialogue;
            cs.shots = shots;
            TimelineBuilder.Build($"{s_scene}_{name}", director, dialogue, shots, MainCam.GetComponent<CinemachineBrain>());
            return cs;
        }

        public static CutsceneOnObjective CutsceneOn(string objectiveId, bool onComplete, CutsceneDirector cs, string progressWhenDone = null)
        {
            var c = cs.gameObject.AddComponent<CutsceneOnObjective>();
            c.objectiveId = objectiveId;
            c.onComplete = onComplete;
            c.cutscene = cs;
            c.progressWhenDone = progressWhenDone;
            return c;
        }

        public static List<GameObject> List(params Component[] cs)
        {
            var l = new List<GameObject>();
            foreach (var c in cs) if (c != null) l.Add(c.gameObject);
            return l;
        }
    }
}
