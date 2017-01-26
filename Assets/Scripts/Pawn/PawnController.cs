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
      private float m_EnginePower;

      [SerializeField, UsedImplicitly]
      private float m_AngularPower;

      [SerializeField, UsedImplicitly]
      private float m_MaxAngularVelocity;

      [SerializeField, UsedImplicitly]
      private float m_MaxSpeed;

      [SerializeField, UsedImplicitly]
      private APawnInputSource m_PawnInputSrc;

      [SerializeField, UsedImplicitly]
      private float m_AccelerationStabilisationFactor;

      [SerializeField, UsedImplicitly]
      private float m_SpeedBias;

      [SerializeField, UsedImplicitly]
      private TimeGraph m_Graph;

      private Quaternion m_PrevRotation;

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Start()
      {
         m_Graph.m_dataSets = new[] {new TimeGraphDataSet {m_displayColor = Color.cyan}};
         m_Graph.m_dataSets[0].SetMaxEntries(100);
         m_PrevRotation = transform.rotation;
      }

      [UsedImplicitly]
      private void Update()
      {
         var rb = GetComponent<Rigidbody>();
         float speed = rb.velocity.magnitude;
         m_Graph.m_dataSets[0].PushData(speed);
         {
            var engineForce = transform.forward;
            engineForce *= Time.deltaTime;
            engineForce *= m_EnginePower;
            engineForce *= m_PawnInputSrc.ForwardAcceleration;
            float speedGap = m_MaxSpeed - speed + m_SpeedBias;
            engineForce *= 1 -
                           Mathf.Exp(-(speedGap * speedGap) / m_AccelerationStabilisationFactor);
            rb.AddForce(engineForce);
         }

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

         if (Quaternion.Angle(transform.rotation, m_PrevRotation) < 0.01f)
            transform.rotation = m_PrevRotation;
         m_PrevRotation = transform.rotation;

         #endregion

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
