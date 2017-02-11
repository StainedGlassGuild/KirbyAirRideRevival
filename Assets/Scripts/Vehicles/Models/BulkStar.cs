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

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Vehicles.Model
{
   public sealed class BulkStar : AVehiculeModel
   {
      #region Nested types

      [Serializable]
      private struct EngineRumbling
      {
         [UsedImplicitly]
         public float BaseAmplitude;

         [UsedImplicitly]
         public float AmplitudeFactor;

         [UsedImplicitly]
         public float BaseFrequency;

         [UsedImplicitly]
         public float FrequencyFactor;
      }

      #endregion

      #region Private fields

      [SerializeField, UsedImplicitly]
      private EngineRumbling m_EngineRumbling;

      private float m_CurrRumblePhase;

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Start()
      {
         m_CurrRumblePhase = 0;
      }

      protected override void UpdateAnimation()
      {
         // Engine rumbling animation
         float amplitude = m_EngineRumbling.BaseAmplitude +
                           m_EngineRumbling.AmplitudeFactor * ParentVehicle.TopSpeedProgression;
         float frequency = m_EngineRumbling.BaseFrequency +
                           m_EngineRumbling.FrequencyFactor * ParentVehicle.TopSpeedProgression;
         m_CurrRumblePhase += frequency * Time.deltaTime;
         float rumble = amplitude * Mathf.Cos(m_CurrRumblePhase);
         var engine = transform.Find("Engine");
         engine.transform.localScale = Vector3.one * (1 + rumble);
      }

      #endregion
   }
}
