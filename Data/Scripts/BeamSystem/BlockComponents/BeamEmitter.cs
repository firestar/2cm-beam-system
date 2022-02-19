namespace BeamSystem.BlockComponents
{
    using Beams;
    using BeamSystem.SessionComponents;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRageMath;


    partial class BeamEmitter
    {
        readonly IBeamEmitterOwner me;
        readonly bool isForward;
        readonly double dirScale;
        readonly Beam Beam;
        readonly double gridSize;

        IMySlimBlock ForwardBlock => isForward ? me.ForwardBlock : me.BackwardBlock;
        IMyEntity Entity => me.Entity;
        Vector3D From => me.From;
        Vector3D EmitterDirection => me.EmitterDirection * dirScale;

        private bool isHardLink;
        private MyParticleEffect fxImpactParticle = null;
        private IMyEntity hitEntity = null;
        private IMySlimBlock hitBlock = null;
        private double currentBeamLength;
        public double EmitPower; // { get; private set; }

        private BeamComponent __linkedReceiver = null;
        internal BeamComponent LinkedReceiver
        {
            get
            {
                //if (null != __linkedReceiver && !__linkedReceiver.IsFunctional)
                //{
                //    LinkedReceiver = null;
                //}
                return __linkedReceiver;
            }
            private set
            {
                if (__linkedReceiver != value)
                {
                    var old = __linkedReceiver;
                    if (null != __linkedReceiver)
                    {
                        __linkedReceiver.LinkSenders.Remove(me.GetBeamComponent());
                    }
                    __linkedReceiver = value;
                    if (null == __linkedReceiver)
                    {
                        isHardLink = false;
                    }
                    else
                    {
                        __linkedReceiver.LinkSenders.Add(me.GetBeamComponent());
                        UpdateSound(0.0); // stop beam sound
                    }
                }
            }
        }

        public bool HasDrawing => !isHardLink || EmitPower > 0.0
            || emitVisual > Settings.MinimumEmitVisual;

        public BeamEmitter(IBeamEmitterOwner me, Beam beam, bool isForward = true)
        {
            this.me = me;
            this.isForward = isForward;
            this.dirScale = isForward ? 1.0 : -1.0;
            this.Beam = beam;
            this.gridSize = me.GridSize;
        }

        public void OnRemovedFromScene()
        {
            LinkedReceiver = null;
            EmitPower = 0.0;
            emitVisual = 0.0;
            DisposeSounds();
            RemoveFxEffect();
            RemoveLight();
        }

        public void OnBeforeSave()
        {
            RemoveFxEffect();
            RemoveLight();
        }

        private bool beamEmitted = false;
        public void DoEmit(double elapsedSeconds, double realEmitPower, double additionalEmitPower, ulong receiveFromGenerator)
        {
            if (realEmitPower > 0.0)
            {
                // calculate visible emit power
                this.EmitPower = Beam.EmitRateForFire(realEmitPower + additionalEmitPower);
                IHitInfo hitInfo = null;
                castBeamLength = Beam.GetRange(this.EmitPower) + 0.04 * gridSize;

                if (!isHardLink || !LinkedReceiver.IsFunctional)
                {
                    var receiver = BeamBaseComponent.GetComponentOf(ForwardBlock);
                    if (null != receiver &&
                        receiver.IsFunctional &&
                        receiver.IsBeamReceivable(Beam)) // is hard link in functional ?
                    {
                        LinkedReceiver = receiver;
                        isHardLink = true;
                        hitBlock = null;
                        this.hitEntity = null;
                        currentBeamLength = 0D;
                    }
                    else // need ray cast
                    {
                        isHardLink = false;

                        Raycasting(beamSerial, allowParallel: !hitBlock?.IsDestroyed ?? true); // tickness * 0.1);

                        if (raycastResult?.beamSerial != this.beamSerial)
                        {
                            return;
                        }

                        if (raycastResult.impact)
                        {
                            BeamComponent linkedReceiver = LinkedReceiver;
                            if (this.hitBlock != raycastResult.hitBlock)
                            {
                                this.hitBlock = raycastResult.hitBlock;
                                receiver = BeamBaseComponent.GetComponentOf(hitBlock);

                                if (receiver?.IsBeamReceivable(Beam) ?? false)
                                {
                                    linkedReceiver = receiver;
                                    isHardLink = me.SlimBlock.Neighbours.Contains(hitBlock);
                                    this.hitBlock = null;
                                    this.hitEntity = null;
                                }
                                else
                                    linkedReceiver = null;
                            }

                            if (isHardLink)
                            {
                                currentBeamLength = 0.0;
                                if (beamEmitted)
                                {
                                    beamEmitted = false;
                                    ++beamSerial;
                                }
                            }
                            else
                            {
                                hitInfo = raycastResult.hitInfo;
                                currentBeamLength = (hitInfo.Position - From).Length();
                                this.hitEntity = hitInfo.HitEntity;
                                if (currentBeamLength > Beam.MaximumLinkRange)
                                    linkedReceiver = null;
                            }
                            LinkedReceiver = linkedReceiver;
                        }
                        else
                        {
                            currentBeamLength = castBeamLength;
                            hitBlock = null;
                            hitEntity = null;
                            LinkedReceiver = null;
                        }
                    }
                } // raycasting

                if (currentBeamLength < vBeamLength)
                    vBeamLength = currentBeamLength;

                bool impact;
                if (LinkedReceiver?.CubeBlock?.IsFunctional ?? false)
                {
                    if (UpdateSessionComponent.DoUpdate && realEmitPower > 0.0)
                        LinkedReceiver.ReceivePowerFrom(realEmitPower, additionalEmitPower,
                            receiveFromGenerator, EmitterDirection, currentBeamLength, isHardLink);
                    impact = false;
                }
                else
                    impact = hitEntity != null && realEmitPower > Beam.AffectMinPower * Settings.UpdateTick;

                if (currentBeamLength > 0.0)
                {
                    UpdateSound(this.EmitPower);
                }

                float tickness;

                float fEmitPower = (float)this.EmitPower;
                if (impact)
                {
                    tickness = Beam.GetTickness(fEmitPower);
                    var fxName = Beam.ImpactParticleFxName(this.EmitPower);
                    var to = hitInfo.Position;
                    Vector3 fDir = EmitterDirection;
                    if (null != fxImpactParticle && !fxImpactParticle.GetName().Equals(fxName))
                    {
                        fxImpactParticle.StopEmitting();
                        MyParticlesManager.RemoveParticleEffect(fxImpactParticle);
                        fxImpactParticle = null;
                    }


                    //var fwd = Vector3.Cross(fDir, new Vector3(fDir.Y, fDir.Z, fDir.X));
                    //Vector3.GetNormalized(ref fwd);
                    //var pMat = MatrixD.CreateWorld(to, fwd, fDir);

                    var fwd = -hitInfo.Normal;
                    Vector3 up;
                    Vector3.Cross(ref fwd, ref fDir, out up);
                    Vector3.Add(ref fwd, ref fDir, out fwd);
                    Vector3.GetNormalized(ref fwd);
                    var pMat = MatrixD.CreateWorld(to, fwd, up);
                    

                    if (!BaseComponent.IsDedicated && (fxImpactParticle != null ||
                        MyParticlesManager.TryCreateParticleEffect(
                            effectName: fxName,
                            effect: out fxImpactParticle)))
                    {
                        fxImpactParticle.WorldMatrix = pMat;
                        fxImpactParticle.UserScale = Beam.ImpactParticleScale(fEmitPower);
                        fxImpactParticle.Velocity = hitInfo?.HitEntity?.Physics?.GetVelocityAtPoint(to) ?? Vector3.Zero;
                        fxImpactParticle.UserBirthMultiplier = Beam.ImpactParticleBirthRate(fEmitPower);
                    }

                    if (BaseComponent.IsServer && UpdateSessionComponent.DoUpdate)
                        try
                        {
                            Beam.BurnTarget(hitInfo, fEmitPower, tickness, fDir,
                                beamEndPoint: ref castTo, from: Entity, target: hitEntity, subTarget: hitBlock);
                        }
                        catch (System.NullReferenceException) { }
                }
                else
                {
                    tickness = Beam.GetTickness(fEmitPower);
                    RemoveFxEffect();
                }

                if (null != LinkedReceiver || tickness < 0.5f
                    || (MyTransparentGeometry.Camera.Translation - From).LengthSquared() > Settings.BeamLightDistanceSquared)
                    RemoveLight();
                else if (null == light)
                    light = AddLight();
            }
            else
            {
                this.EmitPower = 0.0;
                UpdateSound(0.0);
                RemoveFxEffect();
                RemoveLight();
                if (beamEmitted)
                {
                    beamEmitted = false;
                    ++beamSerial;
                }
            }
        }
    }
}