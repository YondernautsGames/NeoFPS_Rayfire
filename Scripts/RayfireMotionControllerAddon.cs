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
using NeoCC;
using NeoFPS;
using RayFire;
using UnityEngine;

namespace NeoFPS.Rayfire
{
    public class RayfireMotionControllerAddon : MonoBehaviour, INeoCharacterControllerHitHandler
    {
        [SerializeField, Tooltip("The valid collision layers the explosion will affect")]
        private LayerMask m_RayfireLayers = 1 << 31;


        [SerializeField, Range(0f, 1f), Tooltip("The amount of random rotation to add to the rayfire rigidbodies affected")]
        public float m_Chaos = 0.5f;

        const int k_MaxHits = 256;

        private static Collider[] s_HitColliders = new Collider[k_MaxHits];

        private NeoCharacterController m_CharacterController = null;
        private List<RayfireRigid> m_RayfireRigidbodies = new List<RayfireRigid>();
        private bool m_Collides = true; // TODO: Remove
        private bool m_WreckingBall = false;
        private float m_Radius = 4f;
        private float m_MaxForce = 10f;
        private float m_Offset = 0.1f;
        private float m_RelativeSpeedThreshold = 10f;

        public void EnableRayfireCollisions()
        {
            m_CharacterController.collisionMask |= (1 << 31);
            //Physics.IgnoreLayerCollision(31, PhysicsFilter.LayerIndex.CharacterControllers, false);
            m_Collides = true;
        }

        public void DisableRayfireCollisions()
        {
            m_CharacterController.collisionMask ^= (1 << 31);
            //Physics.IgnoreLayerCollision(31, PhysicsFilter.LayerIndex.CharacterControllers, true);
            m_Collides = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (m_Collides)
                    DisableRayfireCollisions();
                else
                    EnableRayfireCollisions();
            }

            //m_WreckingBall = true;
        }

        public void EnableWreckingBall(float relativeSpeedThreshold, float maxForce, float radius, float offset = 0f)
        {
            m_WreckingBall = true;
            m_Radius = radius;
            m_Offset = offset;
            m_MaxForce = maxForce;
            m_RelativeSpeedThreshold = relativeSpeedThreshold;
        }

        public void DisableWreckingBall()
        {
            m_WreckingBall = false;
        }

        public void OnNeoCharacterControllerHit(NeoCharacterControllerHit hit)
        {
            if (m_WreckingBall)
            {
                // Get relative velocity along contact normal
                Vector3 relativeVelocity = Vector3.Project(hit.controller.velocity, hit.normal);
                if (hit.rigidbody != null)
                    relativeVelocity += Vector3.Project(hit.rigidbody.velocity, hit.normal);

                // Check if relative velocity is above threshold
                if (relativeVelocity.sqrMagnitude > m_RelativeSpeedThreshold * m_RelativeSpeedThreshold)
                {
                    Vector3 position = hit.point + hit.normal * m_Offset;

                    int hitCount = Physics.OverlapSphereNonAlloc(position, m_Radius, s_HitColliders, m_RayfireLayers);
                    if (hitCount > 0)
                    {

                        Debug.DrawRay(position, -hit.normal, Color.red, 30f);

                        for (int i = 0; i < hitCount; ++i)
                        {

                            //Vector3 targetPosition = s_HitColliders[i].transform.position;
                            //Vector3 direction = targetPosition - position;
                            //float distance = direction.magnitude;

                            ApplyExplosionEffect(s_HitColliders[i], position, m_MaxForce);
                        }
                        m_WreckingBall = false;
                    }

                    m_RayfireRigidbodies.Clear();
                }
            }
        }


        protected void ApplyExplosionEffect(Collider c, Vector3 center, float maxForce)
        {
            // Check for rayfire rigid
            RayfireRigid rigid = c.attachedRigidbody == null
                    ? c.GetComponent<RayfireRigid>()
                    : c.attachedRigidbody.transform.GetComponent<RayfireRigid>();

            if (rigid != null)
            {
                m_RayfireRigidbodies.Add(rigid);

                float falloff = 1f;// - Mathf.Clamp01(hit.distance / m_Radius); // TODO: Calculate distance for falloff?

                if (rigid.activation.imp)
                    rigid.Activate();

                // Apply force
                var rb = c.attachedRigidbody;
                if (rb != null)
                {
                    // Affect Kinematic
                    if (rb.isKinematic == true)
                    {
                        // Convert kinematic to dynamic via rigid script
                        if (rigid != null)
                        {
                            rigid.SetSimulationType(SimType.Dynamic);

                            // Set convex shape
                            if (rigid.physics.meshCollider is MeshCollider == true)
                                ((MeshCollider)rigid.physics.meshCollider).convex = true;
                        }
                    }

                    // Add explosion force
                    rb.AddExplosionForce(maxForce, center, m_Radius, 0f, ForceMode.Impulse);

                    // Get local rotation strength 
                    if (m_Chaos > 0.01f)
                    {
                        float chaos = falloff * m_Chaos * 90f;
                        Vector3 rot = new Vector3(Random.Range(-chaos, chaos), Random.Range(-chaos, chaos), Random.Range(-chaos, chaos));
                        rb.angularVelocity = rot;
                    }
                }
            }
        }

        void Awake()
        {
            m_CharacterController = GetComponent<NeoCharacterController>();
        }
    }
}