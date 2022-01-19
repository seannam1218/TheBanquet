using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
	private float yOffset = -0.5f;
	private Transform target;
	public int smoothValue;
	public float pullFactor;
	public float zoomFactor;
	private Vector3 mousePos;
	private Vector3 curCamPos;
	private float distMouseTarget;
	private float defaultCameraZ = -10;
	private float targetViewSize;
	private float defaultCameraViewSize = 4.5f;

	// Use this for initialization
	public Coroutine my_co;

	void Start()
	{
		target = transform.parent;
	}
	void Update()
	{
		mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10);
		distMouseTarget = (float)Math.Sqrt(Math.Pow(mousePos.x / Screen.width - 0.5f, 2) + Math.Pow(mousePos.y / Screen.height - 0.5f, 2));

		Vector3 targetPos = new Vector3(
			target.position.x + (mousePos.x / Screen.width - 0.5f) * pullFactor * 100 * Time.deltaTime,
			target.position.y + yOffset + (mousePos.y / Screen.height - 0.5f) * pullFactor * 100 * Time.deltaTime / 1.8f,
			defaultCameraZ * 100 * Time.deltaTime / 2 + (defaultCameraZ * distMouseTarget));

		targetViewSize = defaultCameraViewSize + (float)Math.Pow(distMouseTarget, 2) * zoomFactor * 100 * Time.deltaTime;

		transform.position += (targetPos - transform.position) * 100 * Time.deltaTime / smoothValue;
		GetComponent<Camera>().orthographicSize += (targetViewSize - GetComponent<Camera>().orthographicSize) * 100 * Time.deltaTime / smoothValue;
		//transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime*smoothValue);
	}



}