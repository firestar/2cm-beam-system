using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    partial class BeamEmitter
    {
        private MyEntity3DSoundEmitter soundEmitterNear, soundEmitterFar;
        private MyEntity3DSoundEmitter fireSoundEmitterNear, fireSoundEmitterFar;
        private MySoundPair currentSoundNear = null;
        private MySoundPair currentSoundFar = null;
        float volume, crossfadingFrom, crossfadingTo, crossfadingRangeInv, soundMaxDistance;

        void UpdateSound(double emitPower)
        {
            if (BaseComponent.IsDedicated) return;

            if (soundEmitterNear == null)
            {
                soundEmitterNear = new MyEntity3DSoundEmitter(Entity as MyEntity, true, dopplerScaler: Settings.BeamSoundDopplerScale);
                soundEmitterNear.CanPlayLoopSounds = true;
                soundEmitterFar = new MyEntity3DSoundEmitter(Entity as MyEntity, true, dopplerScaler: Settings.BeamSoundDopplerScale);
                soundEmitterFar.CanPlayLoopSounds = true;
            }
            MySoundPair soundNear = null;
            MySoundPair soundFar = null;
            if (null == LinkedReceiver)
            {
                Beam.GetSound(emitPower, out soundNear, out soundFar, out volume, out crossfadingFrom, out crossfadingTo, out soundMaxDistance);

                crossfadingRangeInv = 1f / (crossfadingTo - crossfadingFrom);
                //if (soundNear != null)
                //{
                //    soundEmitterNear.VolumeMultiplier = volume;
                //    soundEmitterFar.VolumeMultiplier = volume;
                //}
                soundEmitterNear.CustomMaxDistance = crossfadingTo;
                soundEmitterFar.CustomMaxDistance = soundMaxDistance;
            }
            if (currentSoundNear != soundNear)
            {
                currentSoundNear = soundNear;
                currentSoundFar = soundFar;
                if (null == soundNear)
                {
                    soundEmitterNear.StopSound(forced: true);
                    soundEmitterFar.StopSound(forced: true);
                    soundEmitterNear.VolumeMultiplier = 0f;
                    soundEmitterFar.VolumeMultiplier = 0f;
                }
                else
                {
                    soundEmitterNear.PlaySingleSound(currentSoundNear, true);
                    soundEmitterFar.PlaySingleSound(currentSoundFar, true);
                }
            }
        }

        Vector3 oldCloset;
        void UpdateSoundSim()
        {
            if (soundEmitterFar?.IsPlaying ?? false)
            {
                if (MyTransparentGeometry.HasCamera && vBeamLength > 0D)
                {
                    var pnt = MyTransparentGeometry.Camera.Translation;
                    var mat = Entity.WorldMatrix;
                    var start = mat.Translation;
                    var dir = mat.Forward;
                    var end = start + dir * vBeamLength;

                    var line = (end - start);
                    var len = line.Normalize();

                    var v = pnt - start;
                    var d = Vector3D.Dot(v, line);
                    d = MathHelperD.Clamp(d, 0D, len);


                    var closet = start + line * d;
                    // soundEmitter.VolumeMultiplier = System.Math.Max(0f, 1f - (float)(closet - pnt).LengthSquared() / soundEmitter.CustomMaxDistance.Value);

                    soundEmitterNear.SetPosition(closet);
                    soundEmitterFar.SetPosition(closet);

                    var velocity = me.LinearVelocity + ((Vector3)closet - oldCloset);
                    oldCloset = closet;
                    soundEmitterNear.SetVelocity(velocity);
                    soundEmitterFar.SetVelocity(velocity);

                    var distance = (float)(pnt - closet).Length();
                    if (distance < crossfadingFrom)
                    {
                        soundEmitterNear.VolumeMultiplier = volume;
                        soundEmitterFar.VolumeMultiplier = 0f;
                    }
                    else if (distance < crossfadingTo)
                    {
                        distance -= crossfadingFrom;
                        distance *= crossfadingRangeInv;

                        soundEmitterNear.VolumeMultiplier = (1f - distance) * volume;
                        soundEmitterFar.VolumeMultiplier = distance * volume;
                    }
                    else
                    {
                        soundEmitterNear.VolumeMultiplier = 0f;
                        soundEmitterFar.VolumeMultiplier = volume;
                    }
                }
                else
                {
                    soundEmitterNear.SetPosition(null);
                    soundEmitterFar.SetPosition(null);

                    soundEmitterNear.SetVelocity(null);
                    soundEmitterFar.SetVelocity(null);

                    soundEmitterNear.VolumeMultiplier = volume;
                    soundEmitterFar.VolumeMultiplier = 0f;
                }
            }
        }

        private void DisposeSounds()
        {
            if (null != soundEmitterNear)
            {
                soundEmitterNear.StopSound(forced: true, cleanUp: true);
                soundEmitterFar.StopSound(forced: true, cleanUp: true);
            }
        }

        public void UpdateBeforeSimulation100()
        {
            soundEmitterFar?.Update();
            soundEmitterNear?.Update();
        }
    }
}