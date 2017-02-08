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

using FXGuild.Karr.Debug;
using FXGuild.Karr.Pawn;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace FXGuild.Karr
{
    public class DebugOverlayUpdater : MonoBehaviour
    {
        #region Private fields

        private Vector3 m_PrevVelocity;

        [SerializeField, UsedImplicitly]
        private PawnController m_PawnController;

        #endregion

        #region Methods

        private void UpdatePanel(string a_Name, string a_UnitName, float a_Value)
        {
            // Get child panel
            var panel = transform.FindChild(a_Name);

            // Update metric value label
            var text = panel.FindChild("Value").GetComponent<Text>();
            text.text = string.Format("{0:##0.0} {1}", a_Value, a_UnitName);

            // Update metric graph
            var graph = panel.FindChild("Graph").GetComponent<DebugGraph>();
            graph.PushData(a_Value);
        }

        [UsedImplicitly]
        private void Update()
        {
            // Get the rigidbody of the monitored pawn controller
            var rb = m_PawnController.transform.GetComponent<Rigidbody>();

            // Update panels
            UpdatePanel("Speed", "km/h", rb.velocity.magnitude);
            UpdatePanel("Acceleration", "km/h²", rb.velocity.magnitude - m_PrevVelocity.magnitude);
            UpdatePanel("AngularSpeed", "rad/s", rb.angularVelocity.magnitude);

            // Save current velocity to compute acceleration next frame
            m_PrevVelocity = rb.velocity;
        }

        #endregion
    }
}
