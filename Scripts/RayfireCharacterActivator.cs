using System;
using System.Collections;
using System.Collections.Generic;
using NeoCC;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using RayFire;
using UnityEngine;

namespace NeoFPS.Rayfire
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class RayfireCharacterActivator : MonoBehaviour, INeoSerializableComponent
    {
        [SerializeField, Tooltip("Should the activation zone be enabled immediately on start.")]
        private bool m_EnableOnStart = false;
        [SerializeField, Tooltip("How far beyond the character controller capsule should the activation zone extend.")]
        private float m_Thickness = 0.25f;
        [SerializeField, Tooltip("The delay between contacting a rayfire rigid and it being activated.")]
        private float m_Delay = 0f;
        [SerializeField, Tooltip("Should the rayfire rigid cluster be demolished on contact.")]
        private bool m_DemolishCluster;

        private CapsuleCollider m_Collider = null;
        private NeoCharacterController m_CharacterController = null;

        public float delay
        {
            get { return m_Delay; }
            set { m_Delay = Mathf.Clamp(value, 0f, 60f); }
        }

        public float thickness
        {
            get { return m_Thickness; }
            set
            {
                m_Thickness = Mathf.Clamp(value, 0.05f, 5f);
                if (m_Collider.enabled)
                    OnControllerHeightChanged(m_CharacterController.height, 0f);
            }
        }

        private void OnValidate()
        {
            m_Thickness = Mathf.Clamp(m_Thickness, 0.05f, 5f);
            m_Delay = Mathf.Clamp(m_Delay, 0f, 60f);
        }

        private void Awake()
        {
            // Get the capsule collider
            m_Collider = GetComponent<CapsuleCollider>();
            m_Collider.isTrigger = true;
            m_Collider.enabled = false;

            m_CharacterController = GetComponentInParent<NeoCharacterController>();
            if (m_CharacterController == null)
                gameObject.SetActive(false);
        }

        private void Start()
        {
            if (m_EnableOnStart)
                EnableActivationField(m_Thickness, m_Delay);
        }

        private void OnControllerHeightChanged(float newHeight, float rootOffset)
        {
            m_Collider.height = newHeight + m_Thickness * 2f;
            m_Collider.radius = m_CharacterController.radius + m_Thickness;
            m_Collider.center = new Vector3(0f, newHeight * 0.5f, 0f);
        }

        public void EnableActivationField()
        {
            EnableActivationField(m_Thickness, 0f);
        }

        public void EnableActivationField(float thickness)
        {
            EnableActivationField(thickness, 0f);
        }

        public void EnableActivationField(float thickness, float delay)
        {
            this.thickness = thickness;
            this.delay = delay;

            m_CharacterController.onHeightChanged += OnControllerHeightChanged;
            OnControllerHeightChanged(m_CharacterController.height, 0f);

            m_Collider.enabled = true;
        }

        public void DisableActivationField()
        {
            m_CharacterController.onHeightChanged -= OnControllerHeightChanged;
            m_Collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Get rigid from collider or rigid body
            RayfireRigid rigid = other.attachedRigidbody == null
                ? other.GetComponent<RayfireRigid>()
                : other.attachedRigidbody.GetComponent<RayfireRigid>();

            // Has no rigid
            if (rigid == null)
                return;

            //Activation
            if (rigid.activation.byActivator && rigid.simulationType == SimType.Inactive || rigid.simulationType == SimType.Kinematic)
            {
                if (m_Delay <= 0)
                    rigid.Activate();
                else
                    StartCoroutine(DelayedActivationCor(rigid));
            }

            // Connected cluster one fragment detach
            if (m_DemolishCluster && rigid.objectType == ObjectType.ConnectedCluster)
            {
                if (m_Delay <= 0)
                    RFDemolitionCluster.DemolishConnectedCluster(rigid, new[] { other });
                else
                    StartCoroutine(DelayedClusterCor(rigid, other));
            }
        }

        // Exclude from simulation and keep object in scene
        IEnumerator DelayedActivationCor(RayfireRigid rigid)
        {
            // Wait life time
            yield return new WaitForSeconds(m_Delay);

            // Activate
            if (rigid != null)
                rigid.Activate();
        }

        // Demolish cluster
        IEnumerator DelayedClusterCor(RayfireRigid rigid, Collider coll)
        {
            // Wait life time
            yield return new WaitForSeconds(m_Delay);

            // Activate
            if (rigid != null && coll != null)
                RFDemolitionCluster.DemolishConnectedCluster(rigid, new[] { coll });
        }

        #region SAVE GAMES

        private static readonly NeoSerializationKey k_ThicknessKey = new NeoSerializationKey("thickness");
        private static readonly NeoSerializationKey k_DelayKey = new NeoSerializationKey("delay");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_Collider.enabled)
            {
                writer.WriteValue(k_ThicknessKey, m_Thickness);
                writer.WriteValue(k_DelayKey, delay);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Enable if thickness and delay settings found
            float t, d;
            if (reader.TryReadValue(k_ThicknessKey, out t, 0f) && reader.TryReadValue(k_DelayKey, out d, 0f))
                EnableActivationField(t, d);
            else
                DisableActivationField();

            // Disable auto enable to prevent clashing
            m_EnableOnStart = false;
        }

        #endregion
    }
}