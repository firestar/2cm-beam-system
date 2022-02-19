using Sandbox.Game.Lights;
using System;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    partial class BeamEmitter
    {
        private MyLight light = null;

        private double emitVisual;
        private float vBeamTickness;
        private Vector3D vBeamTo;
        private Vector3D vBeamFrom;
        private Vector4 vBeamColor2, vBeamColor3, vBeamColor1;
        private Color emissionColor = Colors.Black;
        private float emissivity;
        private double vBeamLength;

        private Vector3D VisualMuzzlePosition => From + EmitterDirection * me.VisualMuzzlePosition;
        protected float VisualEmitRate => (float)(emitVisual / Beam.Capacity) * Beam.VisualEmitRateMultiplier;

        public void UpdateAfterSimulation()
        {
            if (HasDrawing)
            {
                if (isHardLink)
                {
                    emitVisual = 0D;
                    vBeamLength = 0D;
                }
                else if (raycastResult?.beamSerial == beamSerial)
                {
                    emitVisual = MathHelper.Lerp(emitVisual, Beam.EmitRateForFire(EmitPower), Settings.VisualBeamEmitLerpSpeed);
                    vBeamLength = MathHelper.Lerp(vBeamLength, currentBeamLength, Settings.VisualBeamLengthLerpSpeed);
                    if (emitVisual > Settings.MinimumEmitVisual)
                    {
                        var dir = EmitterDirection;
                        vBeamFrom = From + dir * me.BeamStartOffset;
                        vBeamTo = vBeamFrom + dir * vBeamLength;
                        float fEmitVisual = (float)emitVisual;
                        vBeamTickness = Beam.GetTickness(fEmitVisual);
                        Beam.GetColors(fEmitVisual, out vBeamColor1, out vBeamColor2, out vBeamColor3);
                        Vector3 hsv;
                        if ((hsv = me.SlimBlock.ColorMaskHSV).Y > 0f)
                        {
                            // H: 0 ~ 1
                            // S: -0.8 ~ 0.2
                            if (hsv.Y > 0f)
                            {
                                float saturation;
                                float value;
                                // hsv.Y += 0.8f;
                                vBeamColor1.GetSV(out saturation, out value);
                                vBeamColor1 = ColorExtensions.HSVtoColor(hsv.X, saturation, value, vBeamColor1.W);
                                vBeamColor2.GetSV(out saturation, out value);
                                vBeamColor2 = ColorExtensions.HSVtoColor(hsv.X, saturation, value, vBeamColor2.W);
                                vBeamColor3.GetSV(out saturation, out value);
                                vBeamColor3 = ColorExtensions.HSVtoColor(hsv.X, saturation, value, vBeamColor3.W);
                            }
                        }
                    }
                }
                else
                    emitVisual = 0D;
                Draw();
                UpdateSoundSim();
            }
        }


        private static void DrawLine(ref Vector3 dir, ref Vector3D start, ref Vector3D end, ref Vector4 color, float tickness,
            MyStringId matLine, MyStringId matTerminal)
        {
            Vector3D delta = dir * tickness;
            Vector3D s1 = start - delta;
            Vector3D e1 = end + delta;
            MySimpleObjectDraw.DrawLine(s1, start, matTerminal, ref color, tickness);
            MySimpleObjectDraw.DrawLine(start, end, matLine, ref color, tickness);
            MySimpleObjectDraw.DrawLine(e1, end, matTerminal, ref color, tickness);
        }

        private static void DrawLine(ref Vector3 dir, ref Vector3D start, ref Vector3D end, ref Vector4 color, float tickness)
            => DrawLine(ref dir, ref start, ref end, ref color, tickness, Materials.Beam, Materials.BeamStart);

        private static void DrawGlareLine(ref Vector3 dir, ref Vector3D start, ref Vector3D end, ref Vector4 color, float tickness)
            => DrawLine(ref dir, ref start, ref end, ref color, tickness, Materials.BeamGlare, Materials.BeamGlareStart);

        private int drawCall = 0;
        private float oldLightIntensity;
        private Vector3 oldLightPos;

        private void Draw()
        {
            if (emitVisual > Settings.MinimumEmitVisual)
            {
                var mat = Entity.WorldMatrix;
                var camTransform = MyTransparentGeometry.Camera;
                var camPos = (Vector3)camTransform.Translation;
                var toCamDir = camPos - (Vector3)vBeamFrom;
                var distance = toCamDir.Normalize();
                Vector3 dir = mat.Forward;
                var dot = Vector3.Dot(toCamDir, dir);

                float coreTickness = vBeamTickness * 0.1f;
                //bool drawGlare = (vBeamColor1.X + vBeamColor1.Y + vBeamColor1.Z) > 3f;
                //if (drawGlare)
                //{
                //    var invDot = dot * dot;
                //    invDot *= invDot;
                //    invDot = 1f - invDot;
                //    var vColor = vBeamColor3 * invDot;
                //    DrawGlareLine(ref dir, ref vBeamFrom, ref vBeamTo, ref vColor, vBeamTickness);
                //    vColor = vBeamColor2 * invDot;
                //    DrawGlareLine(ref dir, ref vBeamFrom, ref vBeamTo, ref vColor, vBeamTickness * 0.5f);
                //}
                float ticknessR = 1f + (MyRandom.Instance.NextFloat() - 0.5f) * Beam.ScatterFactor;
                DrawLine(ref dir, ref vBeamFrom, ref vBeamTo, ref vBeamColor1, coreTickness * ticknessR);

                var visualEmitRate = this.VisualEmitRate;
                if (visualEmitRate > 0.25f)
                {
                    if (dot > 0f)
                    {
                        dot = 1f - dot;
                        dot *= dot;
                        dot = 1f - dot;
                        toCamDir.Normalize();
                        Vector3 camForward = camTransform.Forward;

                        float vWeight = vBeamTickness * 10f * dot;
                        vWeight = vWeight / (distance + vWeight);
                        var flareRadius = vBeamTickness * 2f;
                        if (null == LinkedReceiver && vBeamLength > vBeamTickness && vWeight > 0.1f && flareRadius < distance)
                        {
                            var rdist = distance - flareRadius;
                            if (flareRadius > rdist)
                            {
                                vWeight *= rdist / flareRadius;
                            }
                            var v = (float)(vBeamLength / (vBeamLength + vBeamTickness));
                            v = 2f - v * 2f;
                            v *= v;
                            v *= v;
                            v = 1f - v;
                            vWeight *= v;
                            var vColor = vBeamColor2 * vWeight;
                            MyTransparentGeometry.AddBillboardOriented(
                                material: Materials.BeamMuzzleFlare,
                                color: vColor,
                                origin: vBeamFrom + toCamDir * vBeamTickness,
                                leftVector: camTransform.Left,
                                upVector: camTransform.Up,
                                radius: flareRadius);
                        }
                        if (distance < vBeamTickness * 10f)
                        {
                            var from = VisualMuzzlePosition;
                            var right = (Vector3)mat.Right;
                            var up = (Vector3)mat.Up;
                            //if (vBeamLength > coreTickness * 2f)
                            //    MyTransparentGeometry.AddBillboardOriented(
                            //        material: Materials.BeamMuzzleGlare,
                            //        color: vBeamColor3 * dot,
                            //        origin: from,
                            //        leftVector: right,
                            //        upVector: up,
                            //        radius: coreTickness);
                            var delta = dir * me.GetVisualMuzzleDepth(visualEmitRate);
                            var cColor = vBeamColor1 * Math.Max(dot, Math.Min(1f, vBeamTickness / distance));
                            MyTransparentGeometry.AddBillboardOriented(
                                material: Materials.BeamMuzzle,
                                color: cColor,
                                origin: from + delta,
                                leftVector: right,
                                upVector: up,
                                radius: coreTickness * 1.05f * ticknessR);
                        }
                        else
                        if (distance < vBeamTickness * 100f)
                        {
                            MyTransparentGeometry.AddBillboardOriented(
                                material: Materials.BeamMuzzle,
                                color: vBeamColor1 * dot,
                                origin: vBeamFrom,
                                leftVector: mat.Right,
                                upVector: mat.Up,
                                radius: coreTickness * ticknessR);
                        }
                    }

                    toCamDir = (Vector3)vBeamTo - camPos;
                    var distance2 = toCamDir.Normalize();
                    dot = Vector3.Dot(toCamDir, dir);
                    if (dot > 0f && distance2 < vBeamTickness * 100f)
                    {
                        dot = 1f - dot;
                        dot *= dot;
                        dot = 1f - dot;

                        MyTransparentGeometry.AddBillboardOriented(
                            material: Materials.BeamMuzzle,
                            color: dot * Math.Min(1f, vBeamTickness * 10f / distance2) * vBeamColor1,
                            origin: vBeamTo + dir * (coreTickness * -0.05f),
                            leftVector: mat.Right,
                            upVector: mat.Down,
                            radius: coreTickness * ticknessR);
                    }
                }

                if (null != light && drawCall == 0)
                {
                    var r = distance * Settings.BeamLightdistanceFInv;
                    r *= r;
                    r = 1f - r;
                    var intensity = (float)emitVisual * r;
                    var pos = vBeamFrom + (dir * (vBeamTickness * 0.5f));
                    light.Intensity = intensity;
                    light.LightOn = true;
                    light.Range = vBeamTickness * r;
                    light.Position = pos;
                    light.Color = vBeamColor1;

                    var diff = oldLightIntensity - intensity;
                    if (diff * diff > 3f || (oldLightPos - pos).LengthSquared() > 3.0)
                    {
                        oldLightIntensity = intensity;
                        oldLightPos = pos;
                        light.UpdateLight();
                    }
                }

                drawCall = (drawCall + 1) % 10;
            }
            else
            {
                emitVisual = 0.0;
                drawCall = 0;
                if (light?.LightOn ?? false)
                {
                    light.LightOn = false;
                    light.Intensity = 0f;
                    light.UpdateLight();
                }
            }
        }

        internal void RemoveFxEffect()
        {
            if (null != fxImpactParticle)
            {
                fxImpactParticle.StopEmitting();
                MyParticlesManager.RemoveParticleEffect(fxImpactParticle);
                fxImpactParticle = null;
            }
        }

        internal void RemoveLight()
        {
            if (null != light)
            {
                MyLights.RemoveLight(light);
                light = null;
            }
        }

        private static MyLight AddLight()
        {
            var light = MyLights.AddLight();
            light.CastShadows = false;
            light.LightType = MyLightType.DEFAULT;
            light.PointLightOffset = 10f;
            light.Falloff = 1f;
            light.DiffuseFactor = 0f;
            light.GlareOn = false;
            light.GlossFactor = 1f;
            light.ReflectorOn = false;
            light.Start("BeamEmitLight");
            return light;
        }
    }
}