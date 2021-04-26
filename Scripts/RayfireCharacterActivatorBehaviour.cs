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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.Rayfire
{
    [MotionGraphElement("Rayfire/RayfireCharacterActivator", "RayfireCharacterActivator")]
    public class RayfireCharacterActivatorBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("What to do with the activator on entering the state or subgraph.")]
        private What m_OnEnter = What.Enable;
        [SerializeField, Tooltip("What to do with the activator on exiting the state or subgraph.")]
        private What m_OnExit = What.Disable;
        [SerializeField, Tooltip("How far outside the character controller's capsule the activator extends.")]
        private float m_Thickness = 0.5f;
        [SerializeField, Tooltip("The time after contacting a rayfire rigid that it will be activated.")]
        private float m_Delay = 0f;

        public enum What
        {
            Enable,
            Disable,
            Ignore
        }

        private RayfireCharacterActivator m_Activator = null;

        public override void OnValidate()
        {
            base.OnValidate();

            m_Thickness = Mathf.Clamp(m_Thickness, 0.05f, 5f);
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            // Get the rayfire activator or create if not found
            m_Activator = controller.GetComponentInChildren<RayfireCharacterActivator>();
            if (m_Activator == null)
            {
                // Create activator object
                var activatorGO = new GameObject("RayfireActivator");

                // Parent
                var activatorT = activatorGO.transform;
                activatorT.SetParent(controller.transform);
                activatorT.localPosition = Vector3.zero;
                activatorT.localRotation = Quaternion.identity;
                activatorT.localScale = Vector3.one;

                // Add activator component
                m_Activator = activatorGO.AddComponent<RayfireCharacterActivator>();
            }
        }

        public override void OnEnter()
        {
            if (m_Activator != null)
            {
                switch(m_OnEnter)
                {
                    case What.Enable:
                        m_Activator.EnableActivationField(m_Thickness, m_Delay);
                        break;
                    case What.Disable:
                        m_Activator.DisableActivationField();
                        break;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Activator != null)
            {
                switch (m_OnExit)
                {
                    case What.Enable:
                        m_Activator.EnableActivationField(m_Thickness, m_Delay);
                        break;
                    case What.Disable:
                        m_Activator.DisableActivationField();
                        break;
                }
            }
        }
    }
}