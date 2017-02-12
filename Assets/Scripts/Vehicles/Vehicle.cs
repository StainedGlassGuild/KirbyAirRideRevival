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
using System.Linq;

using FXGuild.Karr.Vehicles.Input;

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Vehicles
{
   public sealed class Vehicle : MonoBehaviour
   {
      #region Nested types

      // ReSharper disable InconsistentNaming
      public enum PhysicsState
      {
         Idle,
         Accelerating_forward,
         Accelerating_backward,
         Loose_forward,
         Loose_backward,
         Braking,
         Falling,
         Flying
      }

      #endregion

      #region Compile-time constants

      private const int VELOCIY_HISTORY_SIZE = 10;

      #endregion

      #region Private fields

      [SerializeField, UsedImplicitly]
      private APawnInputSource m_InputSrc;

      [SerializeField, UsedImplicitly]
      private VehicleProperties m_VehicleProperties;

      private int m_AccelerationHistoryIdx;
      private Vector3 m_PrevVelocity;
      private Vector3[] m_AccelerationHistory;

      private bool m_IsTouchingGround;

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

      public float SidewaysSpeed
      {
         get { return Vector3.Dot(Velocity, transform.right); }
      }

      public Vector3 SmoothedAcceleration
      {
         get
         {
            return m_AccelerationHistory.Aggregate(Vector3.zero,
               (a_Current, a_Velocity) => a_Current + a_Velocity) / VELOCIY_HISTORY_SIZE;
         }
      }

      public float SmoothedForwardAcceleration
      {
         get { return Vector3.Dot(SmoothedAcceleration, transform.forward); }
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

      public PhysicsState CurrentPhysicsState { get; private set; }

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Start()
      {
         m_AccelerationHistory = new Vector3[VELOCIY_HISTORY_SIZE];
         m_AccelerationHistoryIdx = 0;
         m_PrevVelocity = Vector3.zero;

         m_IsTouchingGround = false;
      }

      [UsedImplicitly]
      private void OnCollisionStay(Collision a_CollisionInfo)
      {
         m_IsTouchingGround = true;
      }

      [UsedImplicitly]
      private void FixedUpdate()
      {
         if (m_IsTouchingGround)
         {
            UpdateSmoothedAcceleration();
            UpdateEnginePropulsion();
            UpdateSideGrip();
         }
         else
            CurrentPhysicsState = PhysicsState.Falling;

         // Rotation is updated even in mid-air
         UpdateRotation();

         // Let OnCollisionStay be called next frame to tell when vehicle is touching ground
         m_IsTouchingGround = false;
      }

      private void UpdateSmoothedAcceleration()
      {
         var velocity = GetComponent<Rigidbody>().velocity;
         var delta = velocity - m_PrevVelocity;
         m_PrevVelocity = velocity;
         m_AccelerationHistory[m_AccelerationHistoryIdx] = delta / Time.fixedDeltaTime;
         m_AccelerationHistoryIdx = (m_AccelerationHistoryIdx + 1) % VELOCIY_HISTORY_SIZE;
      }

      private void UpdateEnginePropulsion()
      {
         // Obtain acceleration from input source
         float inputAcceleration = m_InputSrc.ForwardAcceleration;

         // Check there is some input
         if (Mathf.Abs(inputAcceleration) < 1e-6)
         {
            CurrentPhysicsState = Mathf.Abs(ForwardSpeed) < 0.1f
               ? PhysicsState.Idle
               : ForwardSpeed > 0
                  ? PhysicsState.Loose_forward
                  : PhysicsState.Loose_backward;

            // No propulsion needed
            return;
         }

         // Force is in the local z axis
         var force = transform.forward;

         // Adjust acceleration according to framerate
         force *= Time.fixedDeltaTime;

         // Apply input acceleration
         force *= inputAcceleration;

         // Check if we are accelerating in the same direction as we're going
         if (Math.Sign(inputAcceleration) == Math.Sign(ForwardSpeed))
         {
            // Choose the right propulsion direction
            VehicleProperties.PropulsionProperties propulsion;
            if (inputAcceleration > 0)
            {
               CurrentPhysicsState = PhysicsState.Accelerating_forward;
               propulsion = m_VehicleProperties.Engine.ForwardPropulsion;
            }
            else
            {
               CurrentPhysicsState = PhysicsState.Accelerating_backward;
               propulsion = m_VehicleProperties.Engine.BackwardPropulsion;
            }

            // Set engine power according to direction
            force *= propulsion.Power;

            // Reduce engine power the closer we are to the max speed in this direction
            // TODO: add a velocity bias to TopSpeedProgression to reduce wobble in speed
            force *= Mathf.Pow(1 - TopSpeedProgression, propulsion.DecayFactor);
         }
         else
         {
            CurrentPhysicsState = PhysicsState.Braking;
            force *= m_VehicleProperties.BrakesPower;
         }

         // Add final force to rigidbody
         GetComponent<Rigidbody>().AddForce(force);
      }

      private void UpdateSideGrip()
      {
         // Force is in the local x axis
         var gripForce = transform.right;

         // Adjust force according to framerate
         gripForce *= Time.fixedDeltaTime;

         // Grip allows to stop some of the sideway force
         gripForce *= -SidewaysSpeed;
         gripForce *= m_VehicleProperties.SideGrip;

         // Add final force to rigidbody
         GetComponent<Rigidbody>().AddForce(gripForce);
      }

      private void UpdateRotation()
      {
         // Torque is around y axis
         var torque = transform.up;

         // Adjust rotation according to framerate
         torque *= Time.fixedDeltaTime;

         // Apply input rotation
         torque *= m_InputSrc.RotationAcceleration;

         // Select appropriate rotational propulsion
         var propulsion = CurrentPhysicsState == PhysicsState.Falling 
            ? m_VehicleProperties.Engine.MidAirRotationalPropulsion 
            : m_VehicleProperties.Engine.GroundRotationalPropulsion;

         // Apply engine rotation power
         torque *= propulsion.Power;

         // Reduce rotation power the closer we are to the max angular speed
         float absVelocity = Mathf.Abs(GetComponent<Rigidbody>().angularVelocity.y);
         float accelProgression = Mathf.Clamp01(absVelocity / propulsion.MaxVelocity);
         torque *= Mathf.Pow(1 - accelProgression, propulsion.DecayFactor);

         // Add final torque to rigidbody
         GetComponent<Rigidbody>().AddTorque(torque);
      }

      #endregion

      // Used to smooth acceleration computation
   }
}
