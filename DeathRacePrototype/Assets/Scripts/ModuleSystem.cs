using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleSystem : MonoBehaviour
{
    [Header("Sockets")]
    public GameObject leftSocket;
    public GameObject rightSocket;
    public GameObject frontSocket;
    public GameObject backSocket;

    [Header("UI")]
    public Image leftModule;
    public Image rightModule;
    public Image frontModule;
    public Image backModule;

    [Header("Current State")]
    public GameObject selectedPart;
    public bool hasPartWaiting = false;

    public bool destroyOriginalPart = true;

    private void Start()
    {
        leftModule.enabled = false;
        rightModule.enabled = false;
        frontModule.enabled = false;
        backModule.enabled = false;
    }
    private void Update()
    {
        if (hasPartWaiting && selectedPart != null)
        {
            CheckForSlotInput();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasPartWaiting && collision.gameObject.CompareTag("Part"))
        {
            selectedPart = collision.gameObject;
            hasPartWaiting = true;

            Debug.Log($"Got Part: {selectedPart.name}! Press 1 for Left, 2 for Right, 3 for Light socket.");

            if (destroyOriginalPart)
            {
                selectedPart.SetActive(false); 
            }
        }
    }

    void CheckForSlotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SlotPart(leftSocket, "Left");

            leftModule.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SlotPart(rightSocket, "Right");

            rightModule.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SlotPart(frontSocket, "Front");

            frontModule.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SlotPart(backSocket, "Back");

            backModule.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Cancel part selection
            CancelPartSelection();
        }
    }

    void SlotPart(GameObject socket, string socketName)
    {
        if (socket == null)
        {
            Debug.LogWarning($"{socketName} socket is not assigned!");
            return;
        }

        if (selectedPart == null)
        {
            Debug.LogWarning("No part selected to slot!");
            return;
        }

        // Check if socket already has a part
        if (socket.transform.childCount > 0)
        {
            Debug.Log($"{socketName} socket already occupied! Replacing part.");
            // Destroy existing part in socket
            for (int i = socket.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(socket.transform.GetChild(i).gameObject);
            }
        }

        // Create the part in the socket
        GameObject newPart = Instantiate(selectedPart, socket.transform.position, socket.transform.rotation);

        // Parent it to the socket for organization
        newPart.transform.SetParent(socket.transform);

        // Make sure it's active
        newPart.SetActive(true);

        // Remove any rigidbody/collider from the slotted part so it doesn't interfere
        Rigidbody rb = newPart.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = newPart.GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // Keep collider but make it trigger

        Debug.Log($"Part '{selectedPart.name}' slotted into {socketName} socket!");

        // Clean up
        if (destroyOriginalPart && selectedPart != null)
        {
            Destroy(selectedPart);
        }

        ResetPartSelection();
    }

    void CancelPartSelection()
    {
        Debug.Log("Part selection cancelled.");

        // Re-enable the original part if it was hidden
        if (!destroyOriginalPart && selectedPart != null)
        {
            selectedPart.SetActive(true);
        }

        ResetPartSelection();
    }

    void ResetPartSelection()
    {
        selectedPart = null;
        hasPartWaiting = false;
    }
}
