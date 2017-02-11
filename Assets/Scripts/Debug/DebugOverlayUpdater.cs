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

using System.Collections.Generic;

using FXGuild.Karr.Debug;
using FXGuild.Karr.Vehicles;
using FXGuild.Karr.Vehicles.Model;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace FXGuild.Karr
{
   public class DebugOverlayUpdater : MonoBehaviour
   {
      #region Compile-time constants

      private const float ANGULAR_SPEED_READABILITY_SCALE_FACTOR = 10f;

      #endregion

      #region Runtime constants

      public static readonly Dictionary<Vehicle.PhysicsState, Color> PHYSICS_STATE_COLORS =
         new Dictionary<Vehicle.PhysicsState, Color>
         {
            {Vehicle.PhysicsState.Idle, Color.black},
            {Vehicle.PhysicsState.Accelerating_forward, Color.magenta},
            {Vehicle.PhysicsState.Accelerating_backward, Color.cyan},
            {Vehicle.PhysicsState.Loose_forward, Color.magenta * 0.6f},
            {Vehicle.PhysicsState.Loose_backward, Color.cyan * 0.75f},
            {Vehicle.PhysicsState.Braking, Color.red},
            {Vehicle.PhysicsState.Falling, Color.white},
            {Vehicle.PhysicsState.Flying, Color.yellow}
         };

      #endregion

      #region Private fields

      [SerializeField, UsedImplicitly]
      private Vehicle m_Vehicle;

      #endregion

      #region Methods

      private void UpdateGraphs()
      {
         // Get the rigidbody of the monitored vehicle
         var rb = m_Vehicle.transform.GetComponent<Rigidbody>();

         // Update speed panel
         UpdatePanel("Speed", "km/h", m_Vehicle.ForwardSpeed, rb.velocity.magnitude);

         // Update acceleration panel
         UpdatePanel("Acceleration", "km/h²", m_Vehicle.SmoothedForwardAcceleration,
            m_Vehicle.SmoothedAcceleration.magnitude);

         // Update angular speed panel
         // TODO do not use arbitrary unit
         float angSpeed = rb.angularVelocity.y * ANGULAR_SPEED_READABILITY_SCALE_FACTOR;
         UpdatePanel("AngularSpeed", "", angSpeed);
      }

      private void UpdatePanel(string a_Name, string a_UnitName, params float[] a_Value)
      {
         // Get child panel
         var panel = transform.FindChild(a_Name);

         // Update metric value label
         var text = panel.FindChild("Value").GetComponent<Text>();
         text.text = string.Format("{0:##0.0} {1}", a_Value[0], a_UnitName);

         // Update metric graph
         var graph = panel.FindChild("Graph").GetComponent<DebugGraph>();
         graph.PushData(a_Value[0]);

         // Some graphs may have an absolute speed secondary graph curve
         if (a_Value.Length != 2)
            return;

         graph = panel.FindChild("Graph (absolute speed)").GetComponent<DebugGraph>();
         graph.PushData(a_Value[1]);
      }

      [UsedImplicitly]
      private void FixedUpdate()
      {
         UpdateGraphs();

         // Update Physics state panel
         var vehicule = m_Vehicle.GetComponentInChildren<AVehicleModel>();
         var value = transform.FindChild("Physics State").FindChild("Value");
         string state = m_Vehicle.CurrentPhysicsState.ToString().Replace("_", " ");
         value.GetComponent<Text>().text = state;
         value.GetComponent<Text>().color = PHYSICS_STATE_COLORS[m_Vehicle.CurrentPhysicsState];

         // Update Vehicle stats panel
         var values = transform.FindChild("Vehicle Stats").FindChild("Values");
         values.GetComponent<Text>().text = vehicule.name.Replace(" Model", "");
      }

      #endregion
   }
}
