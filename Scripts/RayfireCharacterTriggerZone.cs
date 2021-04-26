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

namespace NeoFPS.Rayfire
{
    public class RayfireCharacterTriggerZone : CharacterTriggerZone
    {
        [Header("Rayfire Activation Field")]

        [SerializeField, Tooltip("")]
        private RayfireAction m_ActivatorAction = RayfireAction.Ignore;
        [SerializeField, Tooltip("")]
        private float m_ActivatorThickness = 0.5f;
        [SerializeField, Tooltip("")]
        private float m_ActivatorDelay = 0f;
        [SerializeField, Tooltip("")]
        private bool m_DisableActivatorOnExit = true;

        [Header("Character Collisions")]

        [SerializeField, Tooltip("")]
        private RayfireAction m_CollisionsAction = RayfireAction.Ignore;
        [SerializeField, Tooltip("")]
        private bool m_FlipCollisionsOnExit = true;

        [Header("Wrecking Ball")]

        [SerializeField, Tooltip("")]
        private RayfireAction m_WreckingBallAction = RayfireAction.Ignore;
        [SerializeField, Tooltip("The minium relative speed of the character and impacted object for the wrecking ball to shatter it.")]
        private float m_RelativeSpeedThreshold = 10f;
        [SerializeField, Tooltip("")]
        private float m_WreckingBallForce = 20f;
        [SerializeField, Tooltip("")]
        private float m_WreckingBallRadius = 4f;
        [SerializeField, Tooltip("")]
        private float m_WreckingBallOffset = 0.1f;
        [SerializeField, Tooltip("")]
        private bool m_DisableWreckingBallOnExit = true;


        public enum RayfireAction
        {
            Enable,
            Disable,
            Ignore
        }

        protected override void OnCharacterEntered(ICharacter c)
        {
            switch (m_ActivatorAction)
            {
                case RayfireAction.Enable:
                    {
                        var activator = c.GetComponentInChildren<RayfireCharacterActivator>();
                        if (activator != null)
                            activator.EnableActivationField(m_ActivatorThickness, m_ActivatorDelay);
                    }
                    break;
                case RayfireAction.Disable:
                    {
                        var activator = c.GetComponentInChildren<RayfireCharacterActivator>();
                        if (activator != null)
                            activator.DisableActivationField();
                    }
                    break;
            }

            switch (m_CollisionsAction)
            {
                case RayfireAction.Enable:
                    {
                        var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                        if (rayfireMotion != null)
                            rayfireMotion.EnableRayfireCollisions();
                    }
                    break;
                case RayfireAction.Disable:
                    {
                        var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                        if (rayfireMotion != null)
                            rayfireMotion.DisableRayfireCollisions();
                    }
                    break;
            }

            switch (m_WreckingBallAction)
            {
                case RayfireAction.Enable:
                    {
                        var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                        if (rayfireMotion != null)
                            rayfireMotion.EnableWreckingBall(m_RelativeSpeedThreshold, m_WreckingBallForce, m_WreckingBallRadius, m_WreckingBallOffset);
                    }
                    break;
                case RayfireAction.Disable:
                    {
                        var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                        if (rayfireMotion != null)
                            rayfireMotion.DisableWreckingBall();
                    }
                    break;
            }

            base.OnCharacterEntered(c);
        }

        protected override void OnCharacterExited(ICharacter c)
        {
            if (m_ActivatorAction == RayfireAction.Enable && m_DisableActivatorOnExit)
            {
                var activator = c.GetComponentInChildren<RayfireCharacterActivator>();
                if (activator != null)
                    activator.DisableActivationField();
            }

            if (m_FlipCollisionsOnExit)
            {
                switch (m_CollisionsAction)
                {
                    case RayfireAction.Enable:
                        {
                            var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                            if (rayfireMotion != null)
                                rayfireMotion.DisableRayfireCollisions();
                        }
                        break;
                    case RayfireAction.Disable:
                        {
                            var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                            if (rayfireMotion != null)
                                rayfireMotion.EnableRayfireCollisions();
                        }
                        break;
                }
            }

            if (m_WreckingBallAction == RayfireAction.Enable && m_DisableWreckingBallOnExit)
            {
                var rayfireMotion = c.GetComponent<RayfireMotionControllerAddon>();
                if (rayfireMotion != null)
                    rayfireMotion.DisableWreckingBall();
            }

            base.OnCharacterExited(c);
        }
    }
}