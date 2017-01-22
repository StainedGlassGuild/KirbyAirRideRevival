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

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace FXGuild.Karr
{
   public class DebugOverlay : MonoBehaviour
   {
      #region Private fields

      private Vector3 m_PrevVelocity;

      #endregion

      #region Public fields

      public Character Character;

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Update()
      {
         // Update physics info panel
         var panel = transform.FindChild("Physics info panel");
         var rb = Character.GetComponent<Rigidbody>();
         var velocity = rb.velocity;
         SetText(panel, "Speed", "{0:##0.0}", velocity.magnitude);
         SetText(panel, "Acceleration", "{0:##0.0}", (velocity - m_PrevVelocity).magnitude);
         SetText(panel, "Angular velocity", "{0:##0.0}", rb.angularVelocity.magnitude);
         m_PrevVelocity = velocity;
      }

      private void SetText(Transform a_Transform, string a_Property, string a_Format, params object[] a_Args)
      {
         a_Transform.FindChild(a_Property).GetComponent<Text>().text = string.Format(a_Format, a_Args);
      }

      #endregion
   }
}
