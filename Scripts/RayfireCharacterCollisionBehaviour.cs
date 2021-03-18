﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.Rayfire
{
    [MotionGraphElement("Rayfire/RayfireCharacterCollision", "RayfireCharacterCollision")]
    public class RayfireCharacterCollisionBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("What to do with the activator on entering the state or subgraph.")]
        private What m_OnEnter = What.Enable;
        [SerializeField, Tooltip("What to do with the activator on exiting the state or subgraph.")]
        private What m_OnExit = What.Disable;

        public enum What
        {
            Enable,
            Disable,
            Ignore
        }

        private RayfireMotionControllerAddon m_RayfireMotion = null;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            // Get the rayfire motion controller add-on
            m_RayfireMotion = controller.GetComponentInChildren<RayfireMotionControllerAddon>();
        }

        public override void OnEnter()
        {
            if (m_RayfireMotion != null)
            {
                switch(m_OnEnter)
                {
                    case What.Enable:
                        m_RayfireMotion.EnableRayfireCollisions();
                        break;
                    case What.Disable:
                        m_RayfireMotion.DisableRayfireCollisions();
                        break;
                }
            }
        }

        public override void OnExit()
        {
            if (m_RayfireMotion != null)
            {
                switch (m_OnExit)
                {
                    case What.Enable:
                        m_RayfireMotion.EnableRayfireCollisions();
                        break;
                    case What.Disable:
                        m_RayfireMotion.DisableRayfireCollisions();
                        break;
                }
            }
        }
    }
}