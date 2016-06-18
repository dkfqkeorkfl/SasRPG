using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[RequireComponent(typeof(Camera))]
public class SasCamera : MonoBehaviour
{
    public bool mIsShowGizmo = false;

    private Camera mCam = null;
    private Transform mCachedTm = null;

    private Camera cam
    {
        get
        {
            if (mCam == null)
                mCam = GetComponent<Camera>();
            return mCam;
        }
    }

    private Transform cachedTm
    {
        get
        {
            if (mCachedTm == null)
                mCachedTm = transform;
            return mCachedTm;
        }
    }

    public bool IsObjectInFrustum(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public bool IsObjectInFrustum(Vector3 point, out Vector3 posViewport)
    {
        Bounds b = new Bounds(point, Vector3.zero);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        bool isVisible = GeometryUtility.TestPlanesAABB(planes, b);
        posViewport = Vector3.zero;
        if (isVisible)
            posViewport = cam.WorldToViewportPoint(point);
        return isVisible;
    }

    [Obsolete("Need check, please. it doesn't complate yet", true)]
    public bool IsFront(Transform tm)
    {
        Vector3 forward = cachedTm.forward.normalized;
        Vector3 toObject = (tm.position - cachedTm.position).normalized;

        float dotProduct = Vector3.Dot(forward, toObject);
        float angle = dotProduct * 180;
        return angle > 180f - 30f;
    }

    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying || !mIsShowGizmo)
            return;

        Vector2 v = SasCamera.GetGameViewSize();
        float aspectGame = v.x / v.y;
        float aspectFinal = aspectGame / cam.aspect;

        Matrix4x4 mtLocalToWorld = cachedTm.localToWorldMatrix;
        Matrix4x4 mtScale = Matrix4x4.Scale(
            new Vector3(cam.aspect * (cam.rect.width / cam.rect.height),
                aspectFinal, 1));

        Gizmos.matrix = mtLocalToWorld * mtScale;
        Gizmos.DrawFrustum(cachedTm.position, cam.fieldOfView, cam.nearClipPlane, cam.farClipPlane, aspectFinal);
        Gizmos.matrix = Matrix4x4.identity;
    }

    public static Vector2 GetGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetsizeOfMainGameView
            = T.GetMethod("GetSizeOfMainGameView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (Vector2)GetsizeOfMainGameView.Invoke(null, null);
    }
}
