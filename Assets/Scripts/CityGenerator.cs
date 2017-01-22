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
   public class CityGenerator : MonoBehaviour
   {
      #region Compile-time constants

      private const float CITY_DIMENSIONS = 100;

      private const uint NUM_BUILDINGS = 30;

      private const float MIN_BUILDING_LATERAL_SIZE = 2f;
      private const float MAX_BUILDING_LATERAL_SIZE = 6f;
      private const float MIN_BUILDING_HEIGHT = 3f;
      private const float MAX_BUILDING_HEIGHT = 5f;

      #endregion

      #region Methods

      [UsedImplicitly]
      private void Start()
      {
         // Create City ground
         var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
         plane.name = "City Ground";
         plane.transform.localScale = new Vector3(CITY_DIMENSIONS / 10f, 1, CITY_DIMENSIONS / 10f);
         plane.transform.position = Vector3.zero;
         plane.GetComponent<MeshRenderer>().material = MaterialRepository.Instance.Tarmac;

         // Randomly create some buildings
         for (uint i = 0; i < NUM_BUILDINGS; ++i)
            AddRandomBuilding();
      }

      private void AddRandomBuilding()
      {
         var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         cube.name = "Building";
         var trans = cube.transform;
         trans.parent = transform;

         // Randomly pick dimensions
         trans.localScale = new Vector3(
            Random.Range(MIN_BUILDING_LATERAL_SIZE, MAX_BUILDING_LATERAL_SIZE),
            Random.Range(MIN_BUILDING_LATERAL_SIZE, MAX_BUILDING_LATERAL_SIZE),
            Random.Range(MIN_BUILDING_HEIGHT, MAX_BUILDING_HEIGHT));

         // Random pick position
         trans.position = new Vector3(
            Random.Range(0, CITY_DIMENSIONS) - CITY_DIMENSIONS / 2f,
            trans.localScale.y / 2f,
            Random.Range(0, CITY_DIMENSIONS) - CITY_DIMENSIONS / 2f);

         // Set material
         cube.GetComponent<MeshRenderer>().material = MaterialRepository.Instance.Building;
      }

      #endregion
   }
}
