/*
* Copyright 2020 Yondernauts Game Studios Ltd
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*       http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using UnityEngine;
using RayFire;

namespace NeoFPS.Rayfire
{
    public static class RayfireHelpers
    {
        // Extension method for RayfireRigid to set simulation type
        public static void SetSimulationType(this RayfireRigid rigid, SimType simType)
        {
            rigid.simulationType = SimType.Dynamic;
            RFPhysic.SetSimulationType(rigid.physics.rigidBody, rigid.simulationType, rigid.objectType, rigid.physics.gr, rigid.physics.si);
        }
    }
}
