using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.Rayfire
{
    [MotionGraphElement("Rayfire/RayfireCharacterWreckingBall", "RayfireCharacterWreckingBall")]
    public class RayfireCharacterWreckingBallBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("What to do with the character wrecking-ball on entering the state or subgraph.")]
        private What m_OnEnter = What.Enable;
        [SerializeField, Tooltip("What to do with the character wrecking-ball on exiting the state or subgraph.")]
        private What m_OnExit = What.Disable;
        [SerializeField, Tooltip("The maximum force to apply to the rayfire rigid objects the character hits.")]
        private float m_MaxForce = 10f;
        [SerializeField, Tooltip("The impact radius of the wrecking ball.")]
        private float m_Radius = 2f;
        [SerializeField, Tooltip("The offset from the point of impact along the impact normal for the center of the wrecking ball explosion (negative values will push objects towards you).")]
        private float m_Offset = 0.1f;
        [SerializeField, Tooltip("The minium relative speed of the character and impacted object for the wrecking ball to shatter it.")]
        private float m_RelativeSpeedThreshold = 10f;

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
                        m_RayfireMotion.EnableWreckingBall(m_RelativeSpeedThreshold, m_MaxForce, m_Radius, m_Offset);
                        break;
                    case What.Disable:
                        m_RayfireMotion.DisableWreckingBall();
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
                        m_RayfireMotion.EnableWreckingBall(m_RelativeSpeedThreshold, m_MaxForce, m_Radius, m_Offset);
                        break;
                    case What.Disable:
                        m_RayfireMotion.DisableWreckingBall();
                        break;
                }
            }
        }
    }
}