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

    [Header("Socket Contents")]
    [SerializeField] private GameObject leftSocketContent;
    [SerializeField] private GameObject rightSocketContent;
    [SerializeField] private GameObject frontSocketContent;
    [SerializeField] private GameObject backSocketContent;

    public bool destroyOriginalPart = true;

    // Dictionary for easy access to socket contents
    private Dictionary<string, GameObject> socketContents;

    private void Start()
    {
        leftModule.enabled = false;
        rightModule.enabled = false;
        frontModule.enabled = false;
        backModule.enabled = false;

        // Initialize the dictionary
        InitializeSocketTracking();
    }

    private void InitializeSocketTracking()
    {
        socketContents = new Dictionary<string, GameObject>
        {
            ["Left"] = null,
            ["Right"] = null,
            ["Front"] = null,
            ["Back"] = null
        };

        // Check if sockets already have parts at start (for persistence)
        CheckExistingSocketContents();
    }

    private void CheckExistingSocketContents()
    {
        leftSocketContent = GetSocketContent(leftSocket);
        rightSocketContent = GetSocketContent(rightSocket);
        frontSocketContent = GetSocketContent(frontSocket);
        backSocketContent = GetSocketContent(backSocket);

        // Update dictionary
        socketContents["Left"] = leftSocketContent;
        socketContents["Right"] = rightSocketContent;
        socketContents["Front"] = frontSocketContent;
        socketContents["Back"] = backSocketContent;

        // Update UI accordingly
        leftModule.enabled = leftSocketContent != null;
        rightModule.enabled = rightSocketContent != null;
        frontModule.enabled = frontSocketContent != null;
        backModule.enabled = backSocketContent != null;
    }

    private GameObject GetSocketContent(GameObject socket)
    {
        if (socket != null && socket.transform.childCount > 0)
        {
            return socket.transform.GetChild(0).gameObject;
        }
        return null;
    }

    private void Update()
    {
        if (hasPartWaiting && selectedPart != null)
        {
            CheckForSlotInput();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPartWaiting && other.gameObject.CompareTag("Part"))
        {
            selectedPart = other.gameObject;
            hasPartWaiting = true;

            Debug.Log($"Got Part: {selectedPart.name}! Press 1 for Left, 2 for Right, 3 for Front, 4 for Back socket.");

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
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SlotPart(rightSocket, "Right");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SlotPart(frontSocket, "Front");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SlotPart(backSocket, "Back");
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

        // Check if socket already has a part and remove it
        if (socketContents[socketName] != null)
        {
            Debug.Log($"{socketName} socket already occupied! Replacing part.");
            GameObject oldPart = socketContents[socketName];

            // Remove from socket tracking
            UpdateSocketContent(socketName, null);

            // Destroy the old part
            DestroyImmediate(oldPart);
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

        // Update tracking
        UpdateSocketContent(socketName, newPart);

        Debug.Log($"Part '{selectedPart.name}' slotted into {socketName} socket!");

        // Clean up
        if (destroyOriginalPart && selectedPart != null)
        {
            Destroy(selectedPart);
        }

        ResetPartSelection();
    }

    void UpdateSocketContent(string socketName, GameObject newContent)
    {
        // Update dictionary
        socketContents[socketName] = newContent;

        // Update serialized fields for inspector visibility
        switch (socketName)
        {
            case "Left":
                leftSocketContent = newContent;
                leftModule.enabled = newContent != null;
                break;
            case "Right":
                rightSocketContent = newContent;
                rightModule.enabled = newContent != null;
                break;
            case "Front":
                frontSocketContent = newContent;
                frontModule.enabled = newContent != null;
                break;
            case "Back":
                backSocketContent = newContent;
                backModule.enabled = newContent != null;
                break;
        }
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

    // Public methods to access socket contents
    public GameObject GetSocketContent(string socketName)
    {
        if (socketContents.ContainsKey(socketName))
        {
            return socketContents[socketName];
        }
        return null;
    }

    public bool IsSocketOccupied(string socketName)
    {
        return GetSocketContent(socketName) != null;
    }

    public List<GameObject> GetAllSocketContents()
    {
        List<GameObject> contents = new List<GameObject>();
        foreach (var content in socketContents.Values)
        {
            if (content != null)
            {
                contents.Add(content);
            }
        }
        return contents;
    }

    public Dictionary<string, GameObject> GetSocketContentsDictionary()
    {
        return new Dictionary<string, GameObject>(socketContents);
    }

    // Method to manually remove a part from a socket
    public void RemovePartFromSocket(string socketName)
    {
        if (socketContents.ContainsKey(socketName) && socketContents[socketName] != null)
        {
            GameObject partToRemove = socketContents[socketName];
            UpdateSocketContent(socketName, null);
            Destroy(partToRemove);
            Debug.Log($"Removed part from {socketName} socket.");
        }
    }
}
