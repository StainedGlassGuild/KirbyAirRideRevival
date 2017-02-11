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

using System.Linq;

using FXGuild.Karr.Debug;
using FXGuild.Karr.VehiculeSystem;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace FXGuild.Karr
{
    public class DebugOverlayUpdater : MonoBehaviour
    {
        #region Compile-time constants

        private const float ANGULAR_SPEED_READABILITY_SCALE_FACTOR = 10f;

        // Used to smooth acceleration computation
        private const int VELOCIY_HISTORY_SIZE = 10;

        #endregion

        #region Private fields

        [SerializeField, UsedImplicitly]
        private Vehicle m_Vehicle;

        private Vector3[] m_AccelerationHistory;
        private int m_AccelerationHistoryIdx;
        private Vector3 m_PrevVelocity;

        #endregion

        #region Methods

        [UsedImplicitly]
        private void Start()
        {
            m_AccelerationHistory = new Vector3[VELOCIY_HISTORY_SIZE];
            m_AccelerationHistoryIdx = 0;
            m_PrevVelocity = Vector3.zero;
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
            {
                return;
            }

            graph = panel.FindChild("Graph (absolute speed)").GetComponent<DebugGraph>();
            graph.PushData(a_Value[1]);
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            // Get the rigidbody of the monitored pawn controller
            var rb = m_Vehicle.transform.GetComponent<Rigidbody>();

            // Update speed panel
            float forwardSpeed = Vector3.Dot(rb.velocity, m_Vehicle.transform.forward);
            UpdatePanel("Speed", "km/h", forwardSpeed, rb.velocity.magnitude);

            // Update acceleration panel
            var delta = rb.velocity - m_PrevVelocity;
            m_PrevVelocity = rb.velocity;
            m_AccelerationHistory[m_AccelerationHistoryIdx] = delta / Time.fixedDeltaTime;
            m_AccelerationHistoryIdx = (m_AccelerationHistoryIdx + 1) % VELOCIY_HISTORY_SIZE;
            var acceleration = m_AccelerationHistory.Aggregate(Vector3.zero,
                (a_Current, a_Velocity) => a_Current + a_Velocity) / VELOCIY_HISTORY_SIZE;
            float forwardAccel = Vector3.Dot(acceleration, m_Vehicle.transform.forward);
            UpdatePanel("Acceleration", "km/h²", forwardAccel, acceleration.magnitude);

            // Update angular speed panel
            // TODO do not use arbitrary unit
            float angSpeed = rb.angularVelocity.y * ANGULAR_SPEED_READABILITY_SCALE_FACTOR;
            UpdatePanel("AngularSpeed", "", angSpeed);
        }

        #endregion
    }
}
