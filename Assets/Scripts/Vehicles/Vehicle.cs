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

using System;

using FXGuild.Karr.Vehicles.Input;

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Vehicles
{
   public sealed class Vehicle : MonoBehaviour
   {
      #region Private fields

      [SerializeField, UsedImplicitly]
      private APawnInputSource m_PawnInputSrc;

      [SerializeField, UsedImplicitly]
      private VehicleProperties m_VehicleProperties;

      #endregion

      #region Properties

      /// <summary>
      /// Current velocity
      /// </summary>
      public Vector3 Velocity
      {
         get { return GetComponent<Rigidbody>().velocity; }
      }

      /// <summary>
      /// Current speed on the local forward axis (can be negative)
      /// </summary>
      public float ForwardSpeed
      {
         get { return Vector3.Dot(Velocity, transform.forward); }
      }

      /// <summary>
      /// Value between 0 and 1 that tells how close is the current speed from the top speed.
      /// Works for both propulsion directions.
      /// </summary>
      public float TopSpeedProgression
      {
         get
         {
            // Choose the right propulsion direction
            var propulsion = ForwardSpeed > 0
               ? m_VehicleProperties.Engine.ForwardPropulsion
               : m_VehicleProperties.Engine.BackwardPropulsion;

            return Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / propulsion.MaxVelocity);
         }
      }

      #endregion

      #region Methods

      [UsedImplicitly]
      private void FixedUpdate()
      {
         var rb = GetComponent<Rigidbody>();

         #region Engine propulsion

         {
            // Force is in the local z axis
            var force = transform.forward;

            // Adjust acceleration according to framerate
            force *= Time.fixedDeltaTime;

            // Apply input acceleration
            float acceleration = m_PawnInputSrc.ForwardAcceleration;
            force *= acceleration;

            // Check if we are accelerating in the same direction as we're going
            if (Math.Sign(acceleration) == Math.Sign(ForwardSpeed))
            {
               // Choose the right propulsion direction
               var propulsion = acceleration > 0
                  ? m_VehicleProperties.Engine.ForwardPropulsion
                  : m_VehicleProperties.Engine.BackwardPropulsion;

               // Set engine power according to direction
               force *= propulsion.Power;

               // Reduce engine power the closer we are to the max speed in this direction
               // TODO: add a velocity bias to TopSpeedProgression to reduce wobble in speed
               force *= Mathf.Pow(1 - TopSpeedProgression, propulsion.DecayFactor);
            }
            else
            // We're braking
               force *= m_VehicleProperties.BrakesPower;

            // Add final force to rigidbody
            rb.AddForce(force);
         }

         #endregion

         #region Side grip

         {
            // Force is in the local x axis
            var gripForce = transform.right;

            // Adjust force according to framerate
            gripForce *= Time.fixedDeltaTime;

            // Compute current sideways speed
            float currSidewaysSpeed = Vector3.Dot(rb.velocity, transform.right);

            // Grip allows to stop some of the sideway force
            gripForce *= -currSidewaysSpeed;
            gripForce *= m_VehicleProperties.SideGrip;

            // Add final force to rigidbody
            rb.AddForce(gripForce);
         }

         #endregion

         #region Rotation

         {
            // Torque is around y axis
            var torque = Vector3.up;

            // Adjust rotation according to framerate
            torque *= Time.fixedDeltaTime;

            // Apply input rotation
            torque *= m_PawnInputSrc.RotationAcceleration;

            // Apply engine rotation power
            var propulsion = m_VehicleProperties.Engine.RotationalPropulsion;
            torque *= propulsion.Power;

            // Reduce rotation power the closer we are to the max angular speed
            float absVelocity = Mathf.Abs(rb.angularVelocity.y);
            float accelProgression = Mathf.Clamp01(absVelocity / propulsion.MaxVelocity);
            torque *= Mathf.Pow(1 - accelProgression, propulsion.DecayFactor);

            // Add final torque to rigidbody
            rb.AddTorque(torque);
         }

         #endregion
      }

      #endregion
   }
}
