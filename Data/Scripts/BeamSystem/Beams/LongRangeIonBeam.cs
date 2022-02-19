namespace BeamSystem.Beams
{
    class LongRangeIonBeam : IonBeam
    {
        internal override double GetRange(double Power) => base.GetRange(Power) * 2.0;

        internal override float GetTickness(float power) => base.GetTickness(power) * 0.5f;
        
        internal override float ImpactParticleScale(float power) => base.ImpactParticleScale(power) * 0.5f;

        // protected override float DamageScale => base.DamageScale * 0.5f;

        protected override bool UseDelayedExp => false;
    }
}
