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

namespace FXGuild.Karr
{
   public class Character : MonoBehaviour
   {
      private const float VEHICULE_SPEED = 1500f;
      private const float ROTATION_POWER = 350f;
      private const float MAX_SPEED = 20f;
      private const float MAX_ANGULAR_VELOCITY = 3f;

      #region Methods

      [UsedImplicitly]
      private void Update()
      {
         var rb = GetComponent<Rigidbody>();

         if (rb.velocity.magnitude < MAX_SPEED)
         {
            float vert = Input.GetAxis("Vertical");
            rb.AddForce(vert * transform.forward * Time.deltaTime * VEHICULE_SPEED);
         }

         if (rb.angularVelocity.magnitude < MAX_ANGULAR_VELOCITY)
         {
            float hori = Input.GetAxis("Horizontal");
            rb.AddTorque(hori * Vector3.up * Time.deltaTime * ROTATION_POWER);
         }
      }

      #endregion
   }
}
