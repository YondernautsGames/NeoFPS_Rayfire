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
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections.Generic;
using RayFire;

namespace NeoFPS.Rayfire
{
    public class RayfirePooledExplosion : PooledExplosion
    {
        [Header("Rayfire")]

        [SerializeField, Tooltip("Should the explosion affect kinematic rayfire rigidbodies? If so, then they will be switched to dynamic if the rayfire rigid allows it")]
        public bool m_AffectKinematic = false;

        [SerializeField, Tooltip("The amount of random rotation to add to the rayfire rigidbodies affected")]
        public float m_Lift = 2f;

        [SerializeField, Range(0f, 1f), Tooltip("The amount of random rotation to add to the rayfire rigidbodies affected")]
        public float m_Chaos = 0.5f;

        private List<RayfireRigid> m_RayfireRigidbodies = new List<RayfireRigid>();

        protected override bool raycastCheck
        {
            get { return false; }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_Lift = Mathf.Clamp(m_Lift, -10f, 10f);
        }

        public override void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            base.Explode(maxDamage, maxForce, source, ignoreRoot);

            // Clear rayfire rigid list
            m_RayfireRigidbodies.Clear();
        }

        protected override void ApplyExplosionForceEffect(ImpactHandlerInfo info, Vector3 explosionCenter)
        {
            Debug.Log("Adding explosion force");

            // Check for rayfire rigid
            var rigidbody = info.collider.attachedRigidbody;
            RayfireRigid rayfireRB = rigidbody == null
                ? info.collider.GetComponent<RayfireRigid>()
                : rigidbody.transform.GetComponent<RayfireRigid>();

            if (rayfireRB != null)
            {
                m_RayfireRigidbodies.Add(rayfireRB);

                if (rayfireRB.activation.byImpact)
                    rayfireRB.Activate();

                // Apply damage
                if (rayfireRB.damage.enable == true)
                    rayfireRB.ApplyDamage(maxDamage * info.falloff, explosionCenter);

                // Apply force
                if (rigidbody != null)
                {
                    // Affect Kinematic
                    SetKinematic(rayfireRB, rigidbody);

                    // Add explosion force
                    rigidbody.AddExplosionForce(maxForce, explosionCenter, radius, m_Lift, ForceMode.Impulse);

                    // Get local rotation strength 
                    if (m_Chaos > 0.01f)
                    {
                        float chaos = info.falloff * m_Chaos * 90f;
                        Vector3 rot = new Vector3(Random.Range(-chaos, chaos), Random.Range(-chaos, chaos), Random.Range(-chaos, chaos));
                        rigidbody.angularVelocity = rot;
                    }
                }
            }
            else
                base.ApplyExplosionForceEffect(info, explosionCenter);
        }

        // Explode kinematic objects
        void SetKinematic(RayfireRigid rigid, Rigidbody rb)
        {
            if (m_AffectKinematic == true && rb.isKinematic == true)
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
        }
    }
}
