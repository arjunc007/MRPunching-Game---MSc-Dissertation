// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.XR.WSA.Input;
using System.Collections.Generic;

/// <summary>
/// HandsDetected determines if the hand is currently detected or not.
/// </summary>
public class HandsManager : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;

    /// <summary>
    /// HandDetected tracks the hand detected state.
    /// Returns true if the list of tracked hands is not empty.
    /// </summary>
    public bool HandDetected
    {
        get { return trackedHands.Count > 0; }
    }

    private List<uint> trackedHands = new List<uint>();

    void Awake()
    {
        //Disable all hands
        leftHand.SetActive(false);
        rightHand.SetActive(false);

        //Set up event functions
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
    {
        // Check to see that the source is a hand.
        if (args.state.source.kind != InteractionSourceKind.Hand)
        {
            return;
        }

        trackedHands.Add(args.state.source.id);

        //Check which hand to activate
        Vector3 headDir = args.state.headPose.forward;

        Vector3 handPos = new Vector3();
        Vector3 handDir = new Vector3();

        if (args.state.sourcePose.TryGetPosition(out handPos))
        {
            handDir = Vector3.Normalize(handPos - args.state.headPose.position);
        }

        //Activate the correct hand hand and assign it an id
        if (Vector3.Dot(Vector3.Cross(headDir, handDir), Vector3.up) < 0 && !leftHand.activeSelf)
        {
            leftHand.SetActive(true);
            leftHand.GetComponent<Hand>().id = args.state.source.id;
        }
        else if (Vector3.Dot(Vector3.Cross(headDir, handDir), Vector3.up) > 0 && !rightHand.activeSelf)
        {
            rightHand.SetActive(true);
            rightHand.GetComponent<Hand>().id = args.state.source.id;
        }
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        // Check to see that the source is a hand.
        if (args.state.source.kind != InteractionSourceKind.Hand)
        {
            return;
        }

        if (trackedHands.Contains(args.state.source.id))
        {
            trackedHands.Remove(args.state.source.id);
            if (args.state.source.id == leftHand.GetComponent<Hand>().id)
            {
                leftHand.SetActive(false);
                leftHand.GetComponent<Hand>().id = 0;
            }
            else if (args.state.source.id == rightHand.GetComponent<Hand>().id)
            {
                rightHand.SetActive(false);
                rightHand.GetComponent<Hand>().id = 0;
            }
        }
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        //Check to see that the source is a hand
        if (args.state.source.kind != InteractionSourceKind.Hand)
        {
            return;
        }

        //Change the position to the hand position
        if (trackedHands.Contains(args.state.source.id))
        {
            Vector3 position;
            if (args.state.sourcePose.TryGetPosition(out position))
            {
                if (args.state.source.id == leftHand.GetComponent<Hand>().id)
                    leftHand.transform.position = position;
                else if (args.state.source.id == rightHand.GetComponent<Hand>().id)
                    rightHand.transform.position = position;
            }
        }
    }

    void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
    }
}