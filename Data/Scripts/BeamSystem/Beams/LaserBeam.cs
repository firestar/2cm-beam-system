using Sandbox.Game;
using Sandbox.Game.Entities;
using System;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Voxels;
using VRageMath;


namespace BeamSystem.Beams
{
    using SessionComponents;


    class LaserBeam : Beam
    {
        private static Vector4 BeamColorMin = new Vector4(10f, 0f, 0f, 0.7f);
        private static Vector4 BeamColorMax = new Vector4(9f, 1.8f, 1.7f, 2f); // new Vector4(2f, 1.52f, 1f, 2f);
        private static Vector4 BeamColorCore = BeamColorMax * 5f;
        private static Color EmissionColorMin = BeamColorMin;
        private static Color EmissionColorMax = BeamColorMax;
        private static MyStorageData _storageCache;


        internal override BeamType Type => BeamType.Laser;

        internal override bool IsAcceptable => true;

        internal override float ScatterFactor => 0.1f;

        internal override double Capacity => Settings.LaserCapacity;

        internal override double GeneratorChargingSpeed => Settings.LaserGeneratorCharge;

        internal override double GeneratorMaxCharging => Settings.LaserGeneratorPower;

        internal override float RequiredElectricPower => Settings.LaserElectricPower;

        internal override double AffectMinPower => Settings.LaserMinPower;

        internal override double MaximumLinkRange => Settings.LaserMaximumLinkRange;

        private const float _SOUND_W = 0.1f;
        private const float _SOUND_INV = 1f + _SOUND_W;
        private static MySoundPair cachedNearSound = null;
        private static MySoundPair cachedFarSound = null;

        internal override void GetSound(double power, out MySoundPair soundNear, out MySoundPair soundFar, out float volume, out float crossfadingFrom, out float crossfadingTo, out float maxDistance)
        {
            float vp = (float)power * Settings.LaserEmitLimitFInv;
            vp -= 0.1f;
            vp *= 1.1111111111111111111111111111111f;
            if (vp <= 0f)
            {
                soundNear = null;
                soundFar = null;
                volume = crossfadingFrom = crossfadingTo = maxDistance = 0f;
            }
            else
            {
                if (cachedNearSound == null)
                {
                    cachedNearSound = new MySoundPair("2cmLaserBeam");
                    cachedFarSound = new MySoundPair("2cmLaserBeamA");
                }
                soundNear = cachedNearSound;
                soundFar = cachedFarSound;
                float weight = (vp / (_SOUND_W + vp)) * _SOUND_INV;
                volume = weight;
                weight = weight > 0.5f ? 1f : weight + 0.5f;
                crossfadingFrom = 300f * weight;
                crossfadingTo = 600f * weight;
                maxDistance = 1200f * weight;
            }
        }

        internal override void GetColors(float power, out Vector4 color1, out Vector4 color2, out Vector4 color3)
        {
            power *= Settings.LaserEmitLimitFInv;
            Vector4.Lerp(ref BeamColorMin, ref BeamColorMax, power * 0.5f, out color3);
            color3.W = MathHelper.Clamp(color3.W * power * 2.5f, min: 0f, max: 0.125f);
            Vector4.Lerp(ref BeamColorMin, ref BeamColorMax, power, out color2);
            color2.W = MathHelper.Clamp(color2.W * power * 2.5f, min: 0f, max: 0.25f);
            Vector4.Lerp(ref BeamColorMin, ref BeamColorCore, power * power, out color1);
            color1.W = 1f;
        }

        internal override void GetEmissiveColor(double Power, out Color color, out float emissivity)
        {
            emissivity = (float)(Power / Capacity) * Settings.EmissionMultiplier;
            if (emissivity > 1f)
            {
                color = Color.Lerp(EmissionColorMax, Color.White, Math.Min(1f, emissivity - 1f));
            }
            else
                color = Color.Lerp(Colors.Black, EmissionColorMax, emissivity);
        }

        internal override double GetRange(double Power)
        {
            // Max Range: 4000
            // Min Range: 800
            // return (Power / (Power + 10.0) + 1.0) * 800.0;
            return (Power / (Power + 100.0) + 0.1959570957) * 3886.172345;
        }

        internal override float GetTickness(float power)
        {
            return power * 0.01f;
        }

        internal override float ImpactParticleBirthRate(float power)
        {
            return Math.Min(Settings.FxImpactBirthRate * power * 0.1f, 2f);
        }

        internal override float ImpactParticleScale(float power)
        {
            power *= 0.01f;
            power += 0.3f;
            return Math.Min(Settings.FxImpactScale * power, 10f);
        }

        internal override string ImpactParticleFxName(double power)
        {
            return Settings.FxSparkImpactName;
        }

        internal override double Effeciency(double power, double elapsedSeconds)
            => power * Math.Pow(Settings.LaserEffeciency, elapsedSeconds);

        internal override double ReceiveEffectivity(double power, double angle, double range, bool isHardLink)
        {
            angle = (angle - 0.6) * 2.5;
            if (isHardLink && angle < 0.0)
                return power;
            if (angle < 0.0) angle = 0.0;
            else angle *= angle;
            range *= 0.004;
            power *= Math.Pow(0.5, angle);
            power *= Math.Pow(Settings.LaserEffeciency, range);
            return power;
        }

        internal override double EmitRate(double power) => Math.Min(Capacity, power);
        internal override double EmitRateForFire(double emitPower) => Math.Min(Capacity, emitPower);

        internal override void BurnTarget(
            IHitInfo hitInfo, float damage, float tickness, Vector3 dir, ref Vector3D beamEndPoint,
            IMyEntity from, IMyEntity target, IMySlimBlock subTarget)
        {
            var explosionArea = new BoundingSphere()
            {
                Center = hitInfo.Position,
                Radius = Math.Max(0.5f, tickness * Settings.LaserImpactRadiusMultiplier)
            };
            damage *= Settings.LaserDamageModifier;

            var explosion = CreateExplosionInfo(
                ref explosionArea,
                damage: damage,
                hitEntity: hitInfo.HitEntity as MyEntity,
                ownerEntity: (from as MyCubeBlock)?.CubeGrid,
                dir: dir);
            //explosion.Velocity = explosion.Damage * Settings.LaserPhysicsForce * -dir;
            explosion.StrengthImpulse = 0.01f; // explosion.Damage * (float)Settings.LaserPhysicsForce;
            MyExplosions.AddExplosion(ref explosion);
            if (target.Physics?.Enabled ?? false)
                target.Physics.AddForce(
                    type: VRage.Game.Components.MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE,
                    force: dir * damage * Settings.LaserPhysicsForce,
                    position: explosionArea.Center,
                    torque: null);

            if (null != subTarget)
            {
                float oldDamage = subTarget.CurrentDamage;
                DelayedJobScheduler.AddDelayedJob(4L, () =>
                {
                    if (subTarget?.IsDestroyed ?? true || subTarget.CurrentDamage != oldDamage) return;

                    float damageAmount = damage * Settings.LaserDamageDirectScale;
                    BoundingSphereD area = explosionArea;
                    // damageAmount *= 0.5f;
                    subTarget.AddDamage(damageAmount, DamageTypes.Overheating, true);
                    //var subTargets = subTarget.CubeGrid.GetBlocksInsideSphere(ref area);
                    //subTargets.Remove(subTarget);
                    //damageAmount /= subTargets.Count;
                    //foreach (var block in subTarget.CubeGrid.GetBlocksInsideSphere(ref area))
                    //    block.AddDamage(damageAmount, DamageTypes.Overheating, true);
                });
            }
        }

    }
}
