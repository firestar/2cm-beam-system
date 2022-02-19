using Sandbox.Game;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;


namespace BeamSystem.Beams
{
    abstract class Beam
    {
        internal readonly static Beam Laser = new LaserBeam();
        internal readonly static Beam Ion = new IonBeam();
        internal readonly static Beam LongRangeIonBeam = new LongRangeIonBeam();

        internal abstract BeamType Type { get; }

        internal abstract bool IsAcceptable { get; }

        internal abstract float ScatterFactor { get; }

        internal abstract double AffectMinPower { get; }

        internal abstract double Capacity { get; }

        internal abstract float GetTickness(float power);

        internal abstract float RequiredElectricPower { get; }

        internal abstract void GetColors(float power, out Vector4 color1, out Vector4 color2, out Vector4 color3);

        internal abstract void GetEmissiveColor(double Power, out Color color, out float emissivity);

        internal abstract double GetRange(double Power);

        internal abstract float ImpactParticleScale(float power);

        internal abstract float ImpactParticleBirthRate(float power);

        internal abstract string ImpactParticleFxName(double power);

        internal abstract double Effeciency(double power, double elapsedSeconds);

        internal abstract double EmitRate(double power);

        internal abstract double EmitRateForFire(double emitPower);

        internal virtual float VisualEmitRateMultiplier => 1f;

        internal abstract void GetSound(double power, out MySoundPair soundNear, out MySoundPair soundFar, out float volume, out float crossfadingFrom, out float crossfadingTo, out float maxDistance);

        internal abstract double MaximumLinkRange { get; }

        /// <summary>
        /// Apply Damage
        /// </summary>
        /// <param name="hitInfo">from raycasting</param>
        /// <param name="damage">beam energy</param>
        /// <param name="tickness">beam tickness</param>
        /// <param name="dir">fire direction</param>
        /// <param name="from">where fire</param>
        /// <param name="target"></param>
        /// <param name="subTarget">if target is cubeGrid this is sub target like block</param>
        /// <param name="tempList">for use temporary</param>
        internal abstract void BurnTarget(
            IHitInfo hitInfo, float damage, float tickness, Vector3 dir, ref Vector3D beamEndPoint,
            IMyEntity from, IMyEntity target, IMySlimBlock subTarget);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="power"></param>
        /// <param name="angle">0 = equal, 1 = 180 degree</param>
        /// <returns></returns>
        internal abstract double ReceiveEffectivity(double power, double angle, double range, bool isHardLink);

        internal abstract double GeneratorChargingSpeed { get; }

        internal abstract double GeneratorMaxCharging { get; }










        #region Utility

        protected static MyExplosionInfo CreateExplosionInfo(ref BoundingSphere explosionArea, float damage,
              MyEntity hitEntity, MyEntity ownerEntity, Vector3? dir)
              => new MyExplosionInfo()
              {
                  PlayerDamage = damage * Settings.PlayerDamageMultiplier,
                  Damage = damage,
                  ExplosionType = MyExplosionTypeEnum.CUSTOM,
                  ExplosionSphere = explosionArea,
                  LifespanMiliseconds = MyExplosionsConstants.EXPLOSION_LIFESPAN,
                  HitEntity = hitEntity,
                  ParticleScale = 1,
                  OwnerEntity = ownerEntity,
                  Direction = dir,
                  VoxelExplosionCenter = explosionArea.Center,
                  ExplosionFlags = MyExplosionFlags.AFFECT_VOXELS
                      | MyExplosionFlags.CREATE_DECALS
                      | MyExplosionFlags.APPLY_DEFORMATION
                      // | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE
                      // | MyExplosionFlags.CREATE_DEBRIS
                      // | MyExplosionFlags.CREATE_PARTICLE_EFFECT
                      // | MyExplosionFlags.CREATE_SHRAPNELS
                      ,
                  VoxelCutoutScale = Settings.VoxelCutoffScale,
                  PlaySound = false,
                  // ApplyForceAndDamage = true,
                  ObjectsRemoveDelayInMiliseconds = 40,
                  CheckIntersections = true
              };

        #endregion
    }


    static class BeamHelper
    {
        internal static bool AddDamage(
            this IMySlimBlock block, float damage, MyStringHash damageSource, bool sync,
            MyHitInfo? hitInfo = null, long attackerId = 0)
        {
            if (null != block)
                try
                {
                    return block.DoDamage(damage, damageSource, sync, hitInfo, attackerId);
                }
                catch (System.NullReferenceException) { }
            return false;
        }
    }

    enum BeamType
    {
        Laser, Ion
    }

}
