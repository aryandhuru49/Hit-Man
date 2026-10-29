using System.Collections;
using System.Collections.Generic;
using ExitClause.AI;
using ExitClause.CameraSystem;
using ExitClause.Combat;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// Player firearms (aim, fire with recoil and spread, reload), melee, stealth takedowns and
    /// throwable gadgets. Ammo lives as plain ints in GameState (INSECURE: freeze to get infinite ammo).
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        public Transform muzzle;
        public float meleeRange = 1.7f;
        public float meleeDamage = 22f;
        public float takedownRange = 1.8f;

        public readonly List<string> Gadgets = new List<string> { "coin", "noisemaker" };
        public int GadgetIndex { get; private set; }

        public WeaponDefinition Equipped { get; private set; }
        public bool IsReloading { get; private set; }
        public bool IsAiming { get; private set; }
        public bool WeaponsAllowed = true;

        /// <summary>Rage and other buffs.</summary>
        public float DamageMultiplier = 1f;
        public float SpreadMultiplier = 1f;

        PlayerCharacter m_player;
        float m_nextFire;

        void Awake() { m_player = GetComponent<PlayerCharacter>(); }

        void Start()
        {
            var id = GameSession.Instance.State.player.equippedWeaponId;
            Equip(GameDatabase.Instance.GetWeapon(id) ?? FirstOwned());
        }

        public AmmoEntry Ammo(WeaponDefinition w)
        {
            if (w == null) return null;
            var list = GameSession.Instance.State.player.ammo;
            var a = list.Find(x => x.weaponId == w.id);
            if (a == null) { a = new AmmoEntry { weaponId = w.id }; list.Add(a); }
            return a;
        }

        public bool Owns(WeaponDefinition w) => w != null && InventoryManager.Instance.Has(w.id);

        WeaponDefinition FirstOwned()
        {
            foreach (var w in GameDatabase.Instance.weapons) if (Owns(w)) return w;
            return null;
        }

        public void Equip(WeaponDefinition w)
        {
            if (w == null || !Owns(w)) return;
            Equipped = w;
            GameSession.Instance.State.player.equippedWeaponId = w.id;
            IsReloading = false;
        }

        void Update()
        {
            var rig = GameCameraRig.Instance;
            bool locked = m_player.ControlsLocked || m_player.Motor.IsBusy;
            IsAiming = !locked && WeaponsAllowed && Equipped != null && GameInput.Held(GameInput.Aim);
            if (rig != null) rig.AimRequested = IsAiming;
            if (locked) return;

            HandleWeaponSwitch();
            if (GameInput.Pressed(GameInput.CycleGadget)) GadgetIndex = (GadgetIndex + 1) % Gadgets.Count;
            if (GameInput.Pressed(GameInput.Gadget)) ThrowGadget(rig);
            if (GameInput.Pressed(GameInput.Melee)) MeleeOrTakedown();
            if (GameInput.Pressed(GameInput.Reload)) TryReload();

            if (WeaponsAllowed && Equipped != null && !IsReloading)
            {
                bool trigger = Equipped.automatic ? GameInput.Held(GameInput.Fire) : GameInput.Pressed(GameInput.Fire);
                if (trigger && Time.time >= m_nextFire) Fire(rig);
            }
        }

        void HandleWeaponSwitch()
        {
            var ws = GameDatabase.Instance.weapons;
            if (GameInput.Pressed(GameInput.Weapon1) && ws.Count > 0) Equip(ws[0]);
            if (GameInput.Pressed(GameInput.Weapon2) && ws.Count > 1) Equip(ws[1]);
            if (GameInput.Pressed(GameInput.Weapon3) && ws.Count > 2) Equip(ws[2]);
            if (GameInput.Pressed(GameInput.NextWeapon) && ws.Count > 0)
            {
                int start = Mathf.Max(0, ws.IndexOf(Equipped));
                for (int i = 1; i <= ws.Count; i++)
                {
                    var w = ws[(start + i) % ws.Count];
                    if (Owns(w)) { Equip(w); break; }
                }
            }
        }

        void Fire(GameCameraRig rig)
        {
            var ammo = Ammo(Equipped);
            if (ammo.inMagazine <= 0) { TryReload(); return; }
            ammo.inMagazine--;
            m_nextFire = Time.time + 1f / Mathf.Max(0.1f, Equipped.fireRate);

            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.4f + transform.forward * 0.5f;
            Vector3 dir;
            float spread = (IsAiming ? Equipped.spread : Equipped.hipSpread) * SpreadMultiplier;
            if (rig != null && rig.LockTarget != null)
            {
                dir = rig.LockTarget.AimPosition - origin;
            }
            else if (rig != null)
            {
                var ray = rig.CenterRay();
                Vector3 aimPoint = Physics.Raycast(ray, out var hit, Equipped.range, Hitscan.ShootMask, QueryTriggerInteraction.Ignore)
                    && !hit.collider.transform.IsChildOf(transform) ? hit.point : ray.GetPoint(Equipped.range);
                dir = aimPoint - origin;
                if (!IsAiming) transform.rotation = Quaternion.Euler(0, rig.Yaw, 0);
            }
            else dir = transform.forward;

            Hitscan.Fire(origin, Hitscan.ApplySpread(dir, spread), Equipped, gameObject, DamageMultiplier);
            rig?.AddRecoil(Equipped.recoilPitch, Equipped.recoilYaw);
            if (ammo.inMagazine <= 0) TryReload();
        }

        public void TryReload()
        {
            if (IsReloading || Equipped == null) return;
            var a = Ammo(Equipped);
            if (a.inMagazine >= Equipped.magazineSize || a.reserve <= 0) return;
            StartCoroutine(ReloadRoutine(a));
        }

        IEnumerator ReloadRoutine(AmmoEntry a)
        {
            IsReloading = true;
            yield return new WaitForSeconds(Equipped.reloadTime);
            if (!IsReloading) yield break;
            int need = Equipped.magazineSize - a.inMagazine;
            int take = Mathf.Min(need, a.reserve);
            a.reserve -= take;
            a.inMagazine += take;
            IsReloading = false;
        }

        void MeleeOrTakedown()
        {
            EnemyAI best = null;
            float bestDist = float.MaxValue;
            foreach (var e in EnemyAI.All)
            {
                if (e == null || e.IsDead) continue;
                Vector3 to = e.transform.position - transform.position;
                float d = to.magnitude;
                if (d > Mathf.Max(meleeRange, takedownRange) || d > bestDist) continue;
                if (Vector3.Dot(transform.forward, to.normalized) < 0.3f) continue;
                best = e; bestDist = d;
            }

            if (best != null && bestDist <= takedownRange && best.CanBeTakenDown(transform))
            {
                StartCoroutine(m_player.Motor.Busy(1.1f));
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(best.transform.position - transform.position, Vector3.up));
                best.ReceiveTakedown(gameObject);
                NoiseSystem.Emit(transform.position, 3f, NoiseType.Takedown, gameObject);
                return;
            }

            NoiseSystem.Emit(transform.position, 6f, NoiseType.Impact, gameObject);
            if (best != null && bestDist <= meleeRange)
            {
                var h = best.GetComponent<Health>();
                h?.TakeDamage(new DamageInfo(meleeDamage * DamageMultiplier * (m_player.Rage != null && m_player.Rage.IsActive ? 1.5f : 1f),
                    DamageType.Melee, gameObject, best.transform.position + Vector3.up, transform.forward));
            }
            StartCoroutine(m_player.Motor.Busy(0.35f));
        }

        void ThrowGadget(GameCameraRig rig)
        {
            string id = Gadgets[GadgetIndex];
            if (!InventoryManager.Instance.Remove(id)) { UIRoot.Instance.HUD.Notify("No " + id + " left."); return; }
            Vector3 fwd = rig != null ? rig.MainCamera.transform.forward : transform.forward;
            Vector3 origin = transform.position + Vector3.up * 1.6f + transform.forward * 0.4f;
            ThrownDistraction.Throw(origin, fwd * 13f + Vector3.up * 3.5f, gameObject, id == "noisemaker" ? 24f : 12f);
        }
    }
}
