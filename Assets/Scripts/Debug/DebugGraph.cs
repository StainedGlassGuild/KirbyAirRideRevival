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

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Debug
{
    public sealed class DebugGraph : MonoBehaviour
    {
        #region Private fields

        [SerializeField, UsedImplicitly]
        private Vector2 m_VerticalAxisRange;

        [SerializeField, UsedImplicitly]
        private int m_NumHorizontalEntries;

        [SerializeField, UsedImplicitly]
        private Material m_LineMaterial;

        private List<float> m_Data;

        #endregion

        #region Methods

        [UsedImplicitly]
        private void Start()
        {
            m_Data = new List<float>();
        }

        public void PushData(float a_Value)
        {
            m_Data.Insert(0, a_Value);
            while (m_Data.Count > m_NumHorizontalEntries)
            {
                m_Data.RemoveAt(m_Data.Count - 1);
            }
        }

        private void PushPoint(int a_J, float a_Value)
        {
            var rectTrans = GetComponent<RectTransform>();
            var pos = rectTrans.position;
            var scale = rectTrans.sizeDelta;

            // Compute x coordinate
            float horizStep = scale.x / (m_NumHorizontalEntries - 1);
            float x = pos.x + scale.x / 2f - a_J * horizStep;

            // Compute y coordinate
            float clamped = Mathf.Clamp(a_Value, m_VerticalAxisRange.x, m_VerticalAxisRange.y);
            float yRange = m_VerticalAxisRange.y - m_VerticalAxisRange.x;
            float norm = (clamped - m_VerticalAxisRange.x) / yRange;
            float y = -scale.y / 2f - pos.y + scale.y - norm * scale.y;

            GL.Vertex3(x, -y, 0.0f);
        }

        /// <summary>
        /// Must be called by a GameObject with a Camera component
        /// </summary>
        public void Draw()
        {
            // Setup rendering
            m_LineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);

            // Adjust number of horizontal entries
            float width = GetComponent<RectTransform>().sizeDelta.x;
            m_NumHorizontalEntries = Mathf.Clamp(
                m_NumHorizontalEntries, 1, Mathf.FloorToInt(width));

            // Draw graph
            for (int i = 0; i < m_Data.Count - 1; ++i)
            {
                PushPoint(i, m_Data[i]);
                PushPoint(i + 1, m_Data[i + 1]);
            }

            // End rendering
            GL.End();
            GL.PopMatrix();
        }

        #endregion
    }
}
