using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    partial class IonBeamCombiner : BeamCombiner
    {
        private const double _MIN_ = Settings.ION_CAPACITY_SOUND_MIN_RANGE * Settings.ION_CAPACITY_SOUND_MIN_RANGE;
        private const double _MAX_ = Settings.ION_CAPACITY_SOUND_MAX_RANGE * Settings.ION_CAPACITY_SOUND_MAX_RANGE;

        // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/master/Sources/SpaceEngineers.Game/Entities/Blocks/MyShipWelder.cs
        // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/master/Sources/Sandbox.Game/Game/Audio/MyEntity3DSoundEmitter.cs
        private static readonly MySoundPair[] _Sounds = new MySoundPair[60];
        private static readonly float SOUND_INDX_MULT = _Sounds.Length - 0.001f;


        struct SoundEmitter
        {
            MySoundPair currentSound;
            MyEntity3DSoundEmitter emitter;

            public SoundEmitter(MyEntity entity)
            {
                emitter = new MyEntity3DSoundEmitter(entity, useStaticList: true);
                emitter.CanPlayLoopSounds = true;
                currentSound = null;
            }

            public void Play(MySoundPair sound)
            {
                if (!emitter.IsPlaying || currentSound != sound)
                {
                    currentSound = sound;
                    if (null == sound)
                        emitter.StopSound(forced: true);
                    else
                        emitter.PlaySingleSound(sound, stopPrevious: true);
                }
            }

            public float VolumeMultiplier
            {
                get { return emitter.VolumeMultiplier; }
                set { emitter.VolumeMultiplier = value; }
            }

            public void Stop()
            {
                if (emitter.IsPlaying)
                {
                    emitter.StopSound(forced: true);
                    currentSound = null;
                }
            }

            public void CleanUp() => emitter.StopSound(forced: true, cleanUp: true);

            public bool FastUpdate(bool silenced) => emitter.FastUpdate(silenced);

            public bool IsPlaying => emitter.IsPlaying;
        }

        static MySoundPair GetSoundPair(int n)
        {
            n = System.Math.Min(_Sounds.Length - 1, n - 1);
            if (_Sounds[n] == null)
                return _Sounds[n] = new MySoundPair("2cmIonCapacitor" + (n + 1));
            else
                return _Sounds[n];
        }



        private SoundEmitter soundEmitter1, soundEmitter2;
        private float currentSoundLevel = 0f;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            if (!IsDedicated)
            {
                soundEmitter1 = new SoundEmitter(Entity as MyEntity);
                soundEmitter2 = new SoundEmitter(Entity as MyEntity);
            }
        }

        public override void OnRemovedFromScene()
        {
            if (!IsDedicated)
            {
                soundEmitter1.CleanUp();
                soundEmitter2.CleanUp();
            }
            base.OnRemovedFromScene();
        }

        /// <summary>
        /// Update Sound
        /// </summary>
        /// <param name="soundLevel">0..1</param>
        void UpdateSound(float soundLevel)
        {
            if (IsDedicated) return;

            int capcitorsCount = capacitors.Count;
            if (capcitorsCount == 0)
            {
                soundEmitter1.Stop();
                soundEmitter2.Stop();
                return;
            }
            if (soundLevel > currentSoundLevel)
                currentSoundLevel = soundLevel;
            else if (currentSoundLevel < 0.02f)
                currentSoundLevel = soundLevel;
            else
                currentSoundLevel += (soundLevel - currentSoundLevel) * 0.015f;

            if (currentSoundLevel < 0.001f)
            {
                soundEmitter1.Stop();
                soundEmitter2.Stop();
            }
            else if (Entity.GetDistanceSquaredFromCamera() < _MIN_)
            {
                float volumeMultiplier = capcitorsCount;
                int cue = MathHelper.Floor(currentSoundLevel * SOUND_INDX_MULT);
                if (cue == 0)
                {
                    soundEmitter1.VolumeMultiplier = volumeMultiplier * soundLevel;
                    soundEmitter1.Play(GetSoundPair(1));
                    soundEmitter2.Stop();
                }
                else if (cue % 2 == 1)
                {
                    soundEmitter1.Play(GetSoundPair(cue));
                    soundEmitter2.Play(GetSoundPair(cue + 1));

                    float v = soundLevel % 1f;
                    soundEmitter1.VolumeMultiplier = volumeMultiplier * (1f - v);
                    soundEmitter2.VolumeMultiplier = volumeMultiplier * v;
                }
                else
                {
                    soundEmitter2.Play(GetSoundPair(cue));
                    soundEmitter1.Play(GetSoundPair(cue + 1));

                    float v = soundLevel % 1f;
                    soundEmitter2.VolumeMultiplier = volumeMultiplier * (1f - v);
                    soundEmitter1.VolumeMultiplier = volumeMultiplier * v;
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (!IsDedicated && (soundEmitter1.IsPlaying || soundEmitter2.IsPlaying) && Entity.GetDistanceSquaredFromCamera() > _MAX_)
            {
                soundEmitter1.Stop();
                soundEmitter2.Stop();
                currentSoundLevel = 0f;
            }
        }

    }
}
