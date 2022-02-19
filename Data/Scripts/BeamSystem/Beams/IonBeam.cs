using Sandbox.Game;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Voxels;
using VRageMath;


namespace BeamSystem.Beams
{
    using Physics;
    using Sandbox.ModAPI;
    using SessionComponents;
    using VRage.Library.Utils;

    class IonBeam : Beam
    {
        const double IonEmitLimitInv = 1.0 / Settings.IonEmitLimit;
        const float IonEmitLimitF = (float)Settings.IonEmitLimit;
        const float IonEmitLimitFInv = (float)IonEmitLimitInv;

        private static Vector4 BeamColorMin = new Vector4(0f, 0f, 1f, 0.7f);
        private static Vector4 BeamColorMid = new Vector4(0f, 0f, 5f, 1f);
        private static Vector4 BeamColorMax = new Vector4(1f, 1.52f, 5f, 2f);
        private static Vector4 BeamColorCore = BeamColorMax * 100f;
        private static MyStorageData _storageCache;


        internal override BeamType Type => BeamType.Ion;

        internal override bool IsAcceptable => true;

        internal override float ScatterFactor => 0.035f;

        internal override double Capacity => Settings.IonCapacity;

        internal override double GeneratorChargingSpeed => Settings.IonGeneratorCharge;

        internal override double GeneratorMaxCharging => Settings.IonGeneratorPower;

        internal override float RequiredElectricPower => Settings.IonElectricPower;

        internal override double AffectMinPower => Settings.IonMinPower;

        internal override double MaximumLinkRange => Settings.IonMaximumLinkRange;

        internal override float VisualEmitRateMultiplier => 1.8868f;

        private const float _SOUND_W = 0.2f;
        private const float _SOUND_INV = 1f + _SOUND_W;
        private static MySoundPair cachedNearSound = null;
        private static MySoundPair cachedFarSound = null;
        internal override void GetSound(double power, out MySoundPair soundNear, out MySoundPair soundFar, out float volume, out float crossfadingFrom, out float crossfadingTo, out float maxDistance)
        {
            float vp = (float)ScaledEmitRate(power * IonEmitLimitFInv); // power * Settings.IonEmitLimitFInv;
            vp -= 0.2f;
            vp *= 1.25f;
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
                    cachedNearSound = new MySoundPair("2cmIonBeam");
                    cachedFarSound = new MySoundPair("2cmIonBeamA");
                }
                soundNear = cachedNearSound;
                soundFar = cachedFarSound;
                float weight = (vp / (_SOUND_W + vp)) * _SOUND_INV;
                volume = weight;
                weight = weight > 0.5f ? 1f : weight + 0.5f;
                crossfadingFrom = 1200f * weight;
                crossfadingTo = 2400f * weight;
                maxDistance = 4800f * weight;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="power">0..1</param>
        /// <returns></returns>
        private double ScaledEmitRate(double power) => Math.Sin(power * Math.PI - MathHelperD.PiOver2) * 0.5 + 0.5;

        internal override void GetColors(float power, out Vector4 color1, out Vector4 color2, out Vector4 color3)
        {
            // power *= Settings.IonEmitLimitFInv;
            power = (float)ScaledEmitRate(power * IonEmitLimitFInv * 0.5f);

            // Vector4.Lerp(ref BeamColorMin, ref BeamColorMid, power * 0.25f, out color3);
            // color3.W = MathHelper.Clamp(color3.W * power * 1.25f, min: 0f, max: 0.125f);
            color3.X = 0f;
            color3.Y = 0f;
            color3.Z = 5f + power;
            color3.W = power > 0.14285714285714285714285714285714f ? 0.125f : power * 0.875f;

            // Vector4.Lerp(ref BeamColorMin, ref BeamColorMax, power, out color2);
            // MathHelper.Clamp(color2.W * power * 2.5f, min: 0f, max: 0.25f);
            color2.X = power;
            color2.Y = power * 1.52f;
            color2.Z = 5f + power * 4f;
            color2.W = power > 0.1f ? 0.25f : power * 2.5f;

            // Vector4.Lerp(ref BeamColorMin, ref BeamColorCore, power * power, out color1);
            // color1.W = 1f;
            power *= power;
            color1.X = power * 100f;
            color1.Y = power * 152f;
            color1.Z = 5f + power * 500f;
            color1.W = 1f;
        }

        internal override void GetEmissiveColor(double Power, out Color color, out float emissivity)
        {
            emissivity = (float)(Power / Settings.IonEmitLimit) * Settings.EmissionMultiplier;
            color = Color.Lerp(Colors.Black, Colors.White, Math.Min(1f, emissivity));
        }

        private const double IonBeamRangeDel = Settings.IonBeamMaxRange - Settings.IonBeamMinRange;
        internal override double GetRange(double Power)
            // Max Range: 25000
            // Min Range: 1200
            // => (Power / (Power + 5000.0) + 0.03580436373) * 33329.33035;
            => Power / Settings.IonEmitLimit * IonBeamRangeDel + Settings.IonBeamMinRange;

        private const float clamp1 = IonEmitLimitF * 0.1f;
        private const float clamp2 = IonEmitLimitF - clamp1;
        internal override float GetTickness(float power)
        {
            power = (float)ScaledEmitRate(power * IonEmitLimitFInv) * IonEmitLimitF;
            if (power > clamp1)
            {
                power -= clamp1;
                float r = (1f - clamp2 / (clamp2 + power)) * 2f;
                return Math.Max(Settings.BeamVisualTicknessMin, Settings.IonBeamTickness * r * r);
            }
            else
                return Settings.BeamVisualTicknessMin;
        }

        internal override float ImpactParticleBirthRate(float power)
        {
            return Math.Min(Settings.FxImpactBirthRate * power * 0.1f, 2f);
        }

        internal override float ImpactParticleScale(float power)
        {
            power *= 0.01f;
            power += 0.3f;
            return Math.Min(0.1f + Settings.FxImpactScale * power, 10f);
        }

        private const double quadPower = Settings.IonEmitLimit * 0.25f;
        internal override string ImpactParticleFxName(double power)
        {
            return power > quadPower ? Settings.FxExplosionName : Settings.FxSparkImpactName;
        }

        internal override double Effeciency(double power, double elapsedSeconds)
            => power * Math.Pow(Settings.IonEffeciency, elapsedSeconds);
        //{
        //    double waste = 1.0 - Math.Pow(Settings.IonEffeciency, elapsedSeconds);
        //    waste *= power;
        //    double maxWaste = Settings.IonMaxWaste * elapsedSeconds;
        //    if (waste > maxWaste)
        //        return power - maxWaste;
        //    else
        //        return power - waste;
        //}

        internal override double ReceiveEffectivity(double power, double angle, double range, bool isHardLink)
        {
            angle = (angle - 0.6) * 2.5;
            if (isHardLink && angle < 0.0)
                return power;
            if (angle < 0.0) angle = 0.0;
            else angle *= angle;
            range *= 0.0008;
            power *= Math.Pow(0.5, angle);
            power *= Math.Pow(Settings.IonReceiveEffeciency, range);
            return power;
        }

        const double ION_EMIT_T = Settings.IonEmitLimit * 2.0 / (1.0 - Settings.IonEmitLimit * 2.0 / Settings.IonMaximumCapacity);
        internal override double EmitRate(double power) => (ION_EMIT_T * power) / (ION_EMIT_T + power) + Settings.IonEmitMin;

        internal override double EmitRateForFire(double emitPower) => Math.Min(Settings.IonEmitLimit, emitPower);

        //const double ION_EMIT_T = (Settings.IonEmitLimit - Settings.IonEmitMin) / (1.0 - Settings.IonEmitLimit / Settings.IonMaximumCapacity);
        //const double ION_EMIT_R = ION_EMIT_T + Settings.IonEmitLimit;
        //internal override double EmitRate(double power) => ION_EMIT_R / (ION_EMIT_T + power);


        protected virtual float AreaEffectScale => Settings.IonImpactRadiusMultiplier;

        protected virtual float DamageScale => Settings.IonDamageModifier;

        internal override void BurnTarget(
            IHitInfo hitInfo, float damage, float tickness, Vector3 dir, ref Vector3D beamEndPoint,
            IMyEntity from, IMyEntity target, IMySlimBlock subTarget)
        {
            tickness *= AreaEffectScale;

            damage *= IonEmitLimitFInv;
            bool delayedExp = UseDelayedExp && damage > 0.5f;
            damage *= 1.1f;
            damage = Math.Min(1f, damage * damage);
            // damage *= damage;
            damage *= IonEmitLimitF;

            var impactPosition = (Vector3)hitInfo.Position;
            var force = dir * damage * (float)(Math.Sin(UpdateSessionComponent.SimulationCount * 0.01) * 0.9f + 0.1f);
            //if (target.Physics?.Enabled ?? false)
            //    target.Physics.AddForce(
            //        type: VRage.Game.Components.MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE,
            //        force: dir * damage * Settings.IonPhysicsForce * (float)Math.Sin(UpdateSessionComponent.SimulationCount * 0.01),
            //        position: impactPosition,
            //        torque: null);

            var explosionArea = new BoundingSphere()
            {
                Center = impactPosition,
                Radius = tickness * 0.25f
            };

            damage *= DamageScale;

            var hitEntity = hitInfo.HitEntity as MyEntity;
            var ownerEntity = (from as MyCubeBlock)?.CubeGrid;

            var rand = MyRandom.Instance;
            Vector3 fDir = new Vector3(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
            fDir.Normalize();
            var explosion = CreateExplosionInfo(
                ref explosionArea, damage: damage,
                hitEntity: hitEntity, ownerEntity: ownerEntity, dir: Vector3.Lerp(fDir, force, 0.95f) * Settings.IonPhysicsForce);
            explosion.ExplosionFlags |= MyExplosionFlags.APPLY_FORCE_AND_DAMAGE;

            //explosion.Velocity = explosion.Damage * Settings.LaserPhysicsForce * -dir;
            MyExplosions.AddExplosion(ref explosion);

            if (null != subTarget)
            {
                float oldDamage = subTarget.CurrentDamage;

                DelayedJobScheduler.AddDelayedJob(4L, () =>
                {
                    if (subTarget?.IsDestroyed ?? true || subTarget.CurrentDamage != oldDamage) return;

                    List<IMySlimBlock> mySlimBlocks = new List<IMySlimBlock>();
                    List<IMySlimBlock> selectedTargets = new List<IMySlimBlock>();

                    foreach (var grid in MyAPIGateway.GridGroups.GetGroup(subTarget.CubeGrid, GridLinkTypeEnum.Physical))
                    {
                        grid.GetBlocks(mySlimBlocks, b => b.FatBlock?.BlockDefinition.SubtypeName.Contains("ShieldGenerator") ?? false);
                        foreach (var sb in mySlimBlocks)
                            selectedTargets.Add(sb);
                    }

                    damage *= Settings.IonDamageModifierForShields;
                    if (selectedTargets.Count == 0)
                        subTarget.AddDamage(damage, DamageTypes.Overheating, true);
                    else
                    {
                        float damageAmount = damage / selectedTargets.Count;
                        foreach (var block in selectedTargets)
                            block.AddDamage(damageAmount, DamageTypes.Overheating, true);
                    }
                });
            }

            var castFrom = hitInfo.Position;
            if (delayedExp)
            {
                var castTo = beamEndPoint;
                var damage1 = damage * 0.25f;
                var radius1 = tickness * 0.75f;
                DelayedJobScheduler.AddDelayedJob(2L, () =>
                {
                    MyAPIGateway.Physics.CastRayParallel(
                        ref castFrom, ref castTo, CollisionLayers.CharacterCollisionLayer, hit =>
                        {
                            var impact2Position = hit?.HitEntity == null
                                ? explosionArea.Center + dir * tickness * 2f
                                : (Vector3)hit.Position;
                            explosionArea.Center = (impact2Position + impactPosition) * 0.5f;
                            explosionArea.Radius = radius1;
                            explosion = CreateExplosionInfo(
                                ref explosionArea, damage: damage1,
                                hitEntity: hitEntity, ownerEntity: ownerEntity, dir: dir);
                            MyExplosions.AddExplosion(ref explosion);

                            explosionArea.Center = impact2Position;
                            explosion = CreateExplosionInfo(
                                ref explosionArea, damage: damage1,
                                hitEntity: hitEntity, ownerEntity: ownerEntity, dir: dir);
                            MyExplosions.AddExplosion(ref explosion);
                        });
                });
                //var damage2 = damage * 0.05f;
                //var radius2 = tickness;
                //DelayedJobScheduler.AddDelayedJob(2L, () =>
                //{
                //    MyAPIGateway.Physics.CastRayParallel(ref castFrom, ref castTo, CollisionLayers.CharacterCollisionLayer, hit =>
                //    {
                //        var impact2Position = hit?.HitEntity == null
                //            ? explosionArea.Center + dir * tickness * 2f
                //            : (Vector3)hit.Position;
                //        explosionArea.Center = (impact2Position + impactPosition) * 0.5f;
                //        explosionArea.Radius = radius2;
                //        explosion = CreateExplosionInfo(
                //            ref explosionArea, damage: damage2,
                //            hitEntity: hitEntity, ownerEntity: ownerEntity, dir: dir);
                //        MyExplosions.AddExplosion(ref explosion);

                //        explosionArea.Center = impact2Position;
                //        explosion = CreateExplosionInfo(
                //            ref explosionArea, damage: damage2,
                //            hitEntity: hitEntity, ownerEntity: ownerEntity, dir: dir);
                //        MyExplosions.AddExplosion(ref explosion);
                //    });
                //});
            }
        }

        protected virtual bool UseDelayedExp => true;

    }
}
