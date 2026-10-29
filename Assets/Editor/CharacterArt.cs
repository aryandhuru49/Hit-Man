using ExitClause.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Swaps the greybox capsule for a real (Mixamo) character, by folder convention, so it survives rebuilds.
    /// Put the files in Assets/ThirdParty/Characters/&lt;Role&gt;/ (Role = Player, Agent or Civilian):
    ///   Model.fbx (the skinned character) and any of Idle, Walk, Run, CrouchIdle, CrouchWalk, Aim, Fall (.fbx).
    /// Agent and Civilian reuse the Player animations when their own folder has none (Humanoid clips retarget).
    /// The Animator Controller is generated from whichever clips exist. With no Model.fbx the greybox is kept.
    /// </summary>
    public static class CharacterArt
    {
        public const string Root = "Assets/ThirdParty/Characters";

        static string Dir(string role) => $"{Root}/{role}";

        [MenuItem("Exit Clause/Art/Apply Player Character (rebuilds Player prefab only)", priority = 40)]
        public static void ApplyPlayer()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir("Player")}/Model.fbx") == null)
            {
                EditorUtility.DisplayDialog("Exit Clause", $"No character found.\nPut your Mixamo character at:\n{Dir("Player")}/Model.fbx", "OK");
                return;
            }
            PrefabBuilder.BuildPlayerOnly();
            Debug.Log("[CharacterArt] Player prefab rebuilt with the imported character. Every scene uses it on the next Play.");
        }

        [MenuItem("Exit Clause/Art/Apply Agent and Civilian Characters (rebuilds those prefabs)", priority = 41)]
        public static void ApplyNpcs()
        {
            PrefabBuilder.BuildNpcsOnly();
            Debug.Log("[CharacterArt] Agent and Civilian prefabs rebuilt. Rebuild a scene (Exit Clause > Scenes) to also swap its scripted story NPCs.");
        }

        [MenuItem("Exit Clause/Art/Open Characters Folder", priority = 43)]
        public static void OpenFolder()
        {
            foreach (var role in new[] { "Player", "Agent", "Civilian" }) System.IO.Directory.CreateDirectory(Dir(role));
            System.IO.Directory.CreateDirectory(MaterialLibrary.TextureRoot);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(Dir("Player"));
        }

        public static bool HasModel(string role) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir(role)}/Model.fbx") != null;

        /// <summary>
        /// NPC version: adds the model and hides (but keeps) the greybox "Body", because the scene builders
        /// still recolour Body's renderers by name. Adds the NPC animator bridge when it is not there yet.
        /// </summary>
        public static GameObject AttachNpc(GameObject root, string role)
        {
            var model = Attach(root, role);
            if (model == null) return null;
            var body = root.transform.Find("Body");
            if (body != null) body.gameObject.SetActive(false);
            var bridge = root.GetComponent<ExitClause.AI.NpcAnimatorBridge>();
            if (bridge == null) bridge = root.AddComponent<ExitClause.AI.NpcAnimatorBridge>();
            bridge.animator = model.GetComponent<Animator>();
            return model;
        }

        /// <summary>Adds the role's model under <paramref name="root"/>. Returns it, or null when no art is imported.</summary>
        public static GameObject Attach(GameObject root, string role)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir(role)}/Model.fbx");
            if (src == null) return null;
            var model = (GameObject)PrefabUtility.InstantiatePrefab(src, root.transform);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            foreach (var t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = root.layer;
            var anim = model.GetComponent<Animator>();
            if (anim == null) anim = model.AddComponent<Animator>();
            anim.applyRootMotion = false; // PlayerMotor moves the character, not the animation
            anim.runtimeAnimatorController = BuildController(role);
            var bridge = root.GetComponent<CharacterAnimatorBridge>();
            if (bridge != null) bridge.animator = anim;
            return model;
        }

        static AnimationClip Clip(string role, string name)
        {
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath($"{Dir(role)}/{name}.fbx"))
                if (a is AnimationClip c && !c.name.StartsWith("__preview__")) return c;
            return role != "Player" ? Clip("Player", name) : null;
        }

        static readonly System.Collections.Generic.Dictionary<string, AnimatorController> s_built = new System.Collections.Generic.Dictionary<string, AnimatorController>();
        static bool s_regenerate;

        /// <summary>Called by the prefab builds: the next Attach per role regenerates its controller from the clips.</summary>
        public static void ResetControllers() { s_built.Clear(); s_regenerate = true; }

        /// <summary>
        /// Scene builds reuse the existing &lt;Role&gt;.controller (the prefabs reference it), and only the
        /// prefab builds regenerate it, once per role.
        /// </summary>
        static AnimatorController BuildController(string role)
        {
            if (s_built.TryGetValue(role, out var c) && c != null) return c;
            if (!s_regenerate) c = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{Dir(role)}/{role}.controller");
            if (c == null) c = CreateController(role);
            return s_built[role] = c;
        }

        /// <summary>Generates &lt;Role&gt;.controller with the parameters CharacterAnimatorBridge sends.</summary>
        static AnimatorController CreateController(string role)
        {
            var idle = Clip(role, "Idle");
            if (idle == null) { Debug.LogWarning($"[CharacterArt] No {Dir(role)}/Idle.fbx, so the character will stand in T-pose."); return null; }
            var walk = Clip(role, "Walk");
            var run = Clip(role, "Run");
            var crouchIdle = Clip(role, "CrouchIdle");
            var crouchWalk = Clip(role, "CrouchWalk");
            var aim = Clip(role, "Aim");
            var fall = Clip(role, "Fall");

            string path = $"{Dir(role)}/{role}.controller";
            AssetDatabase.DeleteAsset(path);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Crouch", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Aim", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Cover", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter(new AnimatorControllerParameter { name = "Grounded", type = AnimatorControllerParameterType.Bool, defaultBool = true });

            var loco = ctrl.CreateBlendTreeInController("Locomotion", out var tree, 0);
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle, 0f);
            if (walk != null) tree.AddChild(walk, 2f);
            if (run != null) tree.AddChild(run, 5.5f);

            if (crouchIdle != null || crouchWalk != null)
            {
                var crouch = ctrl.CreateBlendTreeInController("CrouchLocomotion", out var ctree, 0);
                ctree.blendParameter = "Speed";
                ctree.useAutomaticThresholds = false;
                ctree.AddChild(crouchIdle != null ? crouchIdle : crouchWalk, 0f);
                if (crouchWalk != null) ctree.AddChild(crouchWalk, 2f);
                Link(loco, crouch, AnimatorConditionMode.If, "Crouch");
                Link(crouch, loco, AnimatorConditionMode.IfNot, "Crouch");
            }

            var sm = ctrl.layers[0].stateMachine;
            if (aim != null)
            {
                var s = sm.AddState("Aim");
                s.motion = aim;
                Link(loco, s, AnimatorConditionMode.If, "Aim");
                Link(s, loco, AnimatorConditionMode.IfNot, "Aim");
            }
            if (fall != null)
            {
                var s = sm.AddState("Airborne");
                s.motion = fall;
                Link(loco, s, AnimatorConditionMode.IfNot, "Grounded");
                Link(s, loco, AnimatorConditionMode.If, "Grounded");
            }
            sm.defaultState = loco;
            AssetDatabase.SaveAssets();
            return ctrl;
        }

        static void Link(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string param)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = 0.15f;
            t.AddCondition(mode, 0f, param);
        }
    }
}
