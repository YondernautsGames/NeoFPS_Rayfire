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

using NeoFPS.ModularFirearms;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Rayfire
{
	public class RayfireBulletAmmoEffect : BaseAmmoEffect
	{
		[SerializeField, Tooltip("The damage the bullet does.")]
		private float m_Damage = 25f;

		[SerializeField, Tooltip("The size of the bullet. Used to size decals.")]
		private float m_BulletDecalSize = 1f;

        [SerializeField, Tooltip("The size of the impact area where bullets will wake rayfire rigids.")]
        private float m_RayfireImpactSize = 0.1f;

        [SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler.")]
		private float m_ImpactForce = 15f;

        [SerializeField, Tooltip("")]
        private bool m_AffectInactive = true;

        [SerializeField, Tooltip("")]
        private bool m_AffectKinematic = true;

        [SerializeField, Tooltip("")]
        private bool m_DemolishCluster = true;

        [SerializeField, Tooltip("")]
        private bool m_AffectRigidBodies = true;

        [SerializeField, Tooltip("")]
        private bool m_Debris = true;

        [SerializeField, Tooltip("")]
        private bool m_Dust = true;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);
        Collider[] impactColliders;

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Damage < 0f)
				m_Damage = 0f;
            if (m_BulletDecalSize < 0.1f)
				m_BulletDecalSize = 0.1f;
            if (m_ImpactForce < 0f)
                m_ImpactForce = 0f;
            m_RayfireImpactSize = Mathf.Clamp(m_RayfireImpactSize, 0f, 0.5f);
        }
		#endif

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Show effect
            SurfaceManager.ShowBulletHit(hit, rayDirection, m_BulletDecalSize, hit.rigidbody != null);

            // Get rigid from collider or rigid body
            RayfireRigid rigid = hit.collider.GetComponentInParent<RayfireRigid>();
            //RayfireRigid rigid = hit.collider.attachedRigidbody == null
            //    ? hit.collider.GetComponent<RayfireRigid>()
            //    : hit.collider.attachedRigidbody.transform.GetComponent<RayfireRigid>();

            if (rigid != null)
            {
                // Impact Debris and dust
                ImpactDebris(rigid, hit.point, hit.normal);

                // Impact Dust
                ImpactDust(rigid, hit.point, hit.normal);

                // Apply damage and return new demolished rigid fragment 
                var shootVector = rayDirection * (hit.distance + 0.01f);
                rigid = ImpactDamage(rigid, hit, hit.point + rayDirection * -hit.distance, shootVector, hit.point);
                if (rigid != null)
                {
                    // Impact hit to rigid bodies. Activated inactive, detach clusters
                    ImpactHit(rigid, hit, hit.point, shootVector);
                }
            }
            else
            {
                // Apply damage
                if (m_Damage > 0f)
                {
                    // Apply damage
                    hit.collider.GetComponents(s_DamageHandlers);
                    for (int i = 0; i < s_DamageHandlers.Count; ++i)
                        s_DamageHandlers[i].AddDamage(m_Damage, hit, damageSource);
                    s_DamageHandlers.Clear();
                }

                // Apply force (nb check collider in case the damage resulted in the object being destroyed)
                if (hit.collider != null && m_ImpactForce > 0f)
                {
                    IImpactHandler impactHandler = hit.collider.GetComponent<IImpactHandler>();
                    if (impactHandler != null)
                        impactHandler.HandlePointImpact(hit.point, rayDirection * m_ImpactForce);
                    else
                    {
                        if (hit.rigidbody != null)
                            hit.rigidbody.AddForceAtPosition(rayDirection * m_ImpactForce, hit.point, ForceMode.Impulse);
                    }
                }
            }
        }

        // Impact hit to rigid bodies. Activated inactive, detach clusters
        void ImpactHit(RayfireRigid rigid, RaycastHit hit, Vector3 impactPoint, Vector3 shootVector)
        {
            int mask = 1 << 31;

            // Prepare impact list
            List<Rigidbody> impactRbList = new List<Rigidbody>();

            // Hit object Impact activation and detach before impact force
            if (m_RayfireImpactSize == 0)
            {
                // Inactive Activation
                if (rigid.objectType == ObjectType.Mesh)
                    if (rigid.simulationType == SimType.Inactive || rigid.simulationType == SimType.Kinematic)
                    {
                        if (m_AffectKinematic && rigid.simulationType == SimType.Kinematic)
                        {
                            rigid.SetSimulationType (SimType.Dynamic);

                            // Set convex shape
                            if (rigid.physics.meshCollider is MeshCollider == true)
                                ((MeshCollider)rigid.physics.meshCollider).convex = true;
                        }

                        if (rigid.activation.byImpact == true)
                            rigid.Activate();
                    }

                // Connected cluster one fragment detach
                if (rigid.objectType == ObjectType.ConnectedCluster)
                    if (m_DemolishCluster == true)
                        RFDemolitionCluster.DemolishConnectedCluster(rigid, new[] { hit.collider });

                // Collect for impact
                impactRbList.Add(hit.collider.attachedRigidbody);
            }

            // Group by radius Impact activation and detach before impact force
            if (m_RayfireImpactSize > 0)
            {
                // Get all colliders
                impactColliders = null;
                impactColliders = Physics.OverlapSphere(impactPoint, m_RayfireImpactSize, mask);

                // No colliders. Stop
                if (impactColliders == null)
                    return;

                // Connected cluster group detach first, check for rigids in range next
                if (rigid.objectType == ObjectType.ConnectedCluster)
                    if (m_DemolishCluster == true)
                        RFDemolitionCluster.DemolishConnectedCluster(rigid, impactColliders);

                // Collect all rigid bodies in range
                RayfireRigid scr;
                List<RayfireRigid> impactRigidList = new List<RayfireRigid>();
                for (int i = 0; i < impactColliders.Length; i++)
                {
                    // Get rigid from collider or rigid body
                    scr = impactColliders[i].attachedRigidbody == null
                        ? impactColliders[i].GetComponent<RayfireRigid>()
                        : impactColliders[i].attachedRigidbody.transform.GetComponent<RayfireRigid>();

                    // Collect uniq rigids in radius
                    if (scr != null)
                    {
                        if (impactRigidList.Contains(scr) == false)
                            impactRigidList.Add(scr);
                    }
                    // Collect RigidBodies without rigid script
                    else
                    {
                        if (m_AffectRigidBodies == true)
                            if (impactColliders[i].attachedRigidbody == null)
                                if (impactRbList.Contains(impactColliders[i].attachedRigidbody) == false)
                                    impactRbList.Add(impactColliders[i].attachedRigidbody);
                    }
                }

                // Group Activation first
                for (int i = 0; i < impactRigidList.Count; i++)
                    if (impactRigidList[i].activation.byImpact == true)
                        if (impactRigidList[i].simulationType == SimType.Inactive || impactRigidList[i].simulationType == SimType.Kinematic)
                            impactRigidList[i].Activate();

                // Collect rigid body from rigid components
                //if (strength > 0)
                //{
                for (int i = 0; i < impactRigidList.Count; i++)
                {
                    // Skip inactive objects
                    if (impactRigidList[i].simulationType == SimType.Inactive && m_AffectInactive == false)
                        continue;

                    if (m_AffectKinematic && impactRigidList[i].simulationType == SimType.Kinematic)
                    {
                        impactRigidList[i].SetSimulationType(SimType.Dynamic);

                        // Set convex shape
                        if (impactRigidList[i].physics.meshCollider is MeshCollider == true)
                            ((MeshCollider)impactRigidList[i].physics.meshCollider).convex = true;
                    }

                    // Collect
                    impactRbList.Add(impactRigidList[i].physics.rigidBody);
                }
                //}
            }

            // NO Strength
            //if (strength == 0)
            //    return;

            // No rigid bodies
            if (impactRbList.Count == 0)
                return;

            // Apply force
            for (int i = 0; i < impactRbList.Count; i++)
            {
                // Skip static and kinematik objects
                if (impactRbList[i] == null || impactRbList[i].isKinematic == true)
                    continue;

                // Add force
                impactRbList[i].AddForceAtPosition(shootVector.normalized * m_ImpactForce, impactPoint, ForceMode.VelocityChange);
            }
        }

        // Apply damage. Return new rigid
        RayfireRigid ImpactDamage(RayfireRigid scrRigid, RaycastHit hit, Vector3 shootPos, Vector3 shootVector, Vector3 impactPoint)
        {
            // No damage or damage disabled
            if (scrRigid.damage.enable == false)
                return scrRigid;

            // Check for demolition TODO input collision collider if radius is 0
            bool damageDemolition = scrRigid.ApplyDamage(m_Damage, impactPoint, m_BulletDecalSize);

            // object was not demolished
            if (damageDemolition == false)
                return scrRigid;

            // Target was demolished
            if (scrRigid.HasFragments == true)
            {
                // Get new fragment target
                bool dmlHitState = Physics.Raycast(shootPos, shootVector, out hit, shootVector.magnitude + 0.05f, 1 << 31, QueryTriggerInteraction.Ignore); ;

                // Get new hit rigid
                if (dmlHitState == true)
                    return hit.collider.attachedRigidbody.transform.GetComponent<RayfireRigid>();
            }

            return null;
        }

        // Impact Debris
        void ImpactDebris(RayfireRigid source, Vector3 impactPos, Vector3 impactNormal)
        {
            if (m_Debris == true && source.HasDebris == true)
                for (int i = 0; i < source.debrisList.Count; i++)
                    if (source.debrisList[i].onImpact == true)
                        RFParticles.CreateDebrisImpact(source.debrisList[i], impactPos, impactNormal);
        }

        // Impact Dust
        void ImpactDust(RayfireRigid source, Vector3 impactPos, Vector3 impactNormal)
        {
            if (m_Dust == true && source.HasDust == true)
                for (int i = 0; i < source.dustList.Count; i++)
                    if (source.dustList[i].onImpact == true)
                        RFParticles.CreateDustImpact(source.dustList[i], impactPos, impactNormal);
        }
    }
}