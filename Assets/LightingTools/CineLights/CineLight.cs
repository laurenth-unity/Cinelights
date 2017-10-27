﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using LightUtilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CineLight : MonoBehaviour
{
    public string displayName = "Light";
    public bool drawGizmo = true;
    public bool linkToCameraRotation;
    [Range(-180, 180)]
    public float Yaw = 0f;
    [Range(-90, 90)]
    public float Pitch = 0f;
    [Range(-180f, 180f)]
    public float Roll = 0f;
    public Vector3 offset;
    public float distance = 2.0f;
    [SerializeField][HideInInspector]
    public GameObject light;
    [SerializeField][HideInInspector]
    public GameObject LightParentPitch;
    [SerializeField][HideInInspector]
    public GameObject LightParentYaw;

    [SerializeField]
    public bool useShadowCaster;
    public Vector2 shadowsCasterSize = new Vector2(1,1);
    public float shadowsCasterDistance = 1;
    public Vector2 shadowsCasterOffset;
    [SerializeField]
    public GameObject shadowCasterGO;

    public bool showEntities = true;
    [SerializeField]
    private GameObject m_attachmentTransform;
    [SerializeField]
    private bool m_attach = false;

    public bool timelineSelected = false;

    private void OnEnable()
    {
        if (LightParentYaw == null || LightParentYaw.transform.parent != gameObject.transform) { CreateLightParentYaw(); }
        if (LightParentPitch == null || LightParentPitch.transform.parent != LightParentYaw.transform) { CreateLightParentPitch(); }
        if (light == null || light.transform.parent != LightParentPitch.transform) { CreateLight(); }
        if(useShadowCaster && gameObject.GetComponentInChildren<MeshRenderer>() == null) { CreateShadowCaster(shadowsCasterSize, shadowsCasterDistance); }
        //Enable if it has been disabled
        if (light != null)
            light.GetComponent<Light>().enabled = true;
        if (shadowCasterGO != null)
            shadowCasterGO.GetComponent<MeshRenderer>().enabled = true;
    }

    public void SetAttachmentTransform(GameObject attachmentTransform,bool attach)
    {
        m_attachmentTransform = attachmentTransform;
        m_attach = attach;
    }

    private void LateUpdate()
    {
        if (m_attach == true)
        {
            gameObject.transform.position = m_attachmentTransform.transform.position;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(timelineSelected)
            Gizmos.color = Handles.color  = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
        else
            Gizmos.color = Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.1f);
        SharedGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
        SharedGizmos();
    }

    private void SharedGizmos()
    {
        if (LightParentYaw != null && drawGizmo)
        {
            EditorLightingUtilities.DrawCross(LightParentYaw.transform);
            EditorLightingUtilities.DrawSpotlightGizmo(light.GetComponent<Light>());
            if (light.GetComponent<Light>().type != LightType.Spot)
                Debug.Log("light is not a spotlight");
        }
    }
#endif
    void CreateLightParentYaw()
    {
        LightParentYaw = new GameObject("LightParentYaw");
        LightParentYaw.transform.parent = gameObject.transform;

    }

    void CreateLightParentPitch()
    {
        LightParentPitch = new GameObject("LightParentPitch");
        LightParentPitch.transform.parent = LightParentYaw.transform;
        LightParentYaw.transform.localPosition = Vector3.zero;
    }

    void SetLightRotation()
    {
        light.transform.localRotation = Quaternion.Euler(0, 180, Roll);
    }

    void CreateLight()
    {
        light = new GameObject("TargetSpot");
        light.transform.parent = LightParentPitch.transform;
        var targetedLightSpot = light.AddComponent<Light>();
        targetedLightSpot.type = LightType.Spot;
        SetLightRotation();
        light.transform.localPosition = new Vector3(0, 0, distance);
    }

    public void ApplyShadowCaster()
    {
        if (useShadowCaster && shadowCasterGO == null && light.GetComponentInChildren<MeshRenderer>() == null)
            CreateShadowCaster(shadowsCasterSize, shadowsCasterDistance);
        else
        {
            shadowCasterGO = light.GetComponentInChildren<MeshRenderer>().gameObject;
            if (!useShadowCaster && shadowCasterGO != null)
                DestroyImmediate(shadowCasterGO.gameObject);
        }
    }

    void CreateShadowCaster(Vector2 size, float distance)
    {
        shadowCasterGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadowCasterGO.transform.parent = light.transform;
        shadowCasterGO.transform.localPosition = new Vector3(0,0,-distance);
        shadowCasterGO.transform.localRotation = Quaternion.identity;
        shadowCasterGO.transform.localScale = new Vector3(size.x, size.y, 0);
        DestroyImmediate(shadowCasterGO.GetComponent<MeshCollider>());
    }

    void SetLightParentTransform()
    {
		if (LightParentPitch != null && LightParentYaw!=null )
        {
			if ( linkToCameraRotation)
            {
                var cameraRotation = FindObjectOfType<Camera>().transform.rotation;
                gameObject.transform.rotation = cameraRotation;
            }
        }
    }

    private void OnDisable()
    {
        light.GetComponent<Light>().enabled = false;
        if (shadowCasterGO != null)
            shadowCasterGO.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ApplyShowFlags(bool show)
    {
        if (light != null)
        {
            if (!show) { light.hideFlags = HideFlags.HideInHierarchy; }
            if (show)
            {
                light.hideFlags = HideFlags.None;
            }
        }
        if (LightParentPitch != null)
        {
            if (!show) { LightParentPitch.hideFlags = HideFlags.HideInHierarchy; }
            if (show)
            {
                LightParentPitch.hideFlags = HideFlags.None;
            }
        }
        if (LightParentYaw != null)
        {
            if (!show) { LightParentYaw.hideFlags = HideFlags.HideInHierarchy; }
            if (show)
            {
                LightParentYaw.hideFlags = HideFlags.None;
            }
        }
    }

}
