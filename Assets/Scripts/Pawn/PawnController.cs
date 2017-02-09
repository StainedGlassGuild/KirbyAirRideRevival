// MIT License
// 
// Copyright (c) 2017 FXGuild
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using FXGuild.Karr.Pawn.Input;

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Pawn
{
    public class PawnController : MonoBehaviour
    {
        #region Private fields

        [SerializeField, UsedImplicitly]
        private float m_EnginePowerForward;

        [SerializeField, UsedImplicitly]
        private float m_EnginePowerBrakes;

        [SerializeField, UsedImplicitly]
        private float m_EnginePowerBackward;

        [SerializeField, UsedImplicitly]
        private float m_AngularPower;

        [SerializeField, UsedImplicitly]
        private float m_MaxAngularVelocity;

        [SerializeField, UsedImplicitly]
        private float m_MaxSpeedForward;

        [SerializeField, UsedImplicitly]
        private float m_MaxSpeedBackward;

        [SerializeField, UsedImplicitly]
        private APawnInputSource m_PawnInputSrc;

        [SerializeField, UsedImplicitly]
        private float m_AccelerationStabilisation;

        [SerializeField, UsedImplicitly]
        private float m_SpeedBias;

        private Quaternion m_PrevRotation;

        #endregion

        #region Methods

        [UsedImplicitly]
        private void Start()
        {
            m_PrevRotation = transform.rotation;
        }

        [UsedImplicitly]
        private void Update()
        {
            var rb = GetComponent<Rigidbody>();

            #region Handle engine force

            var engineForce = transform.forward;
            engineForce *= Time.deltaTime;
            engineForce *= m_PawnInputSrc.ForwardAcceleration > 0
                ? m_EnginePowerForward
                : Vector3.Dot(rb.velocity, transform.forward) > 0
                    ? m_EnginePowerBrakes
                    : m_EnginePowerBackward;
            engineForce *= m_PawnInputSrc.ForwardAcceleration;
            float speedGap = m_MaxSpeedForward - rb.velocity.magnitude + m_SpeedBias;
            engineForce *= 1 - Mathf.Exp(-(speedGap * speedGap) / m_AccelerationStabilisation);
            if (rb.velocity.magnitude < (m_PawnInputSrc.ForwardAcceleration > 0
                ? m_MaxSpeedForward
                : m_MaxSpeedBackward))
            {
                rb.AddForce(engineForce);
            }

            #endregion

            #region Adjust rotation

            // Limit angular velocity to a maximum value
            if (rb.angularVelocity.magnitude < m_MaxAngularVelocity)
            {
                // Add torque force according to pawn inputs
                var torque = Vector3.up;
                torque *= Time.deltaTime;
                torque *= m_AngularPower;
                torque *= m_PawnInputSrc.RotationAcceleration;
                rb.AddTorque(torque);
            }

            // Limit rotation to axis Y
            var rotation = transform.rotation;
            rotation.eulerAngles = Vector3.up * rotation.eulerAngles.y;
            transform.rotation = rotation;

            // TODO: check why there's a small constant rotation force to ignore
            if (Quaternion.Angle(transform.rotation, m_PrevRotation) < 0.01f)
            {
                transform.rotation = m_PrevRotation;
            }
            m_PrevRotation = transform.rotation;

            #endregion

            // TODO remove this hack
            var pos = transform.position;
            if (pos.z > 300)
            {
                pos.z -= 300;
                transform.position = pos;
            }
        }

        #endregion
    }
}
