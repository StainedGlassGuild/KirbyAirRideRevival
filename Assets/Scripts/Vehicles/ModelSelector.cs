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

using FXGuild.Karr.Vehicles.Model;

using JetBrains.Annotations;

using UnityEngine;

namespace FXGuild.Karr.Vehicles
{
   public sealed class ModelSelector : MonoBehaviour
   {
      #region Private fields

      private int m_CurrModel;

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Start()
      {
         m_CurrModel = 1;
         ChangeModel();
      }

      [UsedImplicitly]
      private void Update()
      {
         int numModels = VehicleModelRepository.Instance.Models.Count;

         // Check for input to switch vehicle
         if (UnityEngine.Input.GetButtonDown("Next Vehicle"))
         {
            m_CurrModel = (m_CurrModel + 1) % numModels;
            ChangeModel();
         }
         else if (UnityEngine.Input.GetButtonDown("Previous Vehicle"))
         {
            m_CurrModel = (m_CurrModel + numModels - 1) % numModels;
            ChangeModel();
         }

         // TODO remove me one day
         if (UnityEngine.Input.GetButtonDown("Respawn"))
         {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
            transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
         }
      }

      private void ChangeModel()
      {
         // Remove current model if there is one
         var component = GetComponentInChildren<AVehicleModel>();
         if (component != null)
         {
            Destroy(component.gameObject);
         }

         // Get model from repository
         var model = VehicleModelRepository.Instance.Models[m_CurrModel];

         // Create model instance
         var obj = Instantiate(model.gameObject);
         obj.name = model.name;

         // Set model as child of character
         obj.transform.parent = transform;
         obj.transform.localPosition = Vector3.zero;
         obj.transform.localRotation = Quaternion.identity;

         // Put Kirby at the correct position on the vehicle model
         transform.Find("Kirby").localPosition =
            obj.GetComponent<AVehicleModel>().KirbySittingPosition;
      }

      #endregion
   }
}
