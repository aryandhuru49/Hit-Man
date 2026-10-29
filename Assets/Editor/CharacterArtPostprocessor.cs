using UnityEditor;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Sets Mixamo FBX import options automatically for files dropped under CharacterArt.Root:
    /// Humanoid rig, and looping in-place clips. Runs only on the first import, so later Inspector tweaks stick.
    /// </summary>
    public class CharacterArtPostprocessor : AssetPostprocessor
    {
        bool IsCharacterFbx => assetPath.StartsWith(CharacterArt.Root) && assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);

        void OnPreprocessModel()
        {
            if (!IsCharacterFbx) return;
            var mi = (ModelImporter)assetImporter;
            if (!mi.importSettingsMissing) return;
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }

        void OnPreprocessAnimation()
        {
            if (!IsCharacterFbx) return;
            var mi = (ModelImporter)assetImporter;
            if (!mi.importSettingsMissing) return;
            bool loop = !assetPath.EndsWith("/Model.fbx");
            var clips = mi.defaultClipAnimations;
            foreach (var c in clips)
            {
                c.loopTime = loop;
                c.loopPose = loop;
                // Bake root motion into the pose so in-place loops never drift away from the CharacterController.
                c.lockRootRotation = true; c.keepOriginalOrientation = true;
                c.lockRootHeightY = true; c.keepOriginalPositionY = true;
                c.lockRootPositionXZ = true; c.keepOriginalPositionXZ = true;
            }
            mi.clipAnimations = clips;
        }
    }
}
