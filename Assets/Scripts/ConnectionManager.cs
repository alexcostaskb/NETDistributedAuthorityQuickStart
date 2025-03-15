using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

/// <summary>
/// The connection manager for creating or joining a session.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    private string _profileName;
    private string _sessionName;
    private int _maxPlayers = 10;
    private ConnectionState _state = ConnectionState.Disconnected;
    private ISession _session;
    private NetworkManager m_NetworkManager;

    // define the connection state
    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    private async void Awake()
    {
        // Initialize the NetworkManager
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

        // Initialize Unity Services
        await UnityServices.InitializeAsync();
    }

    // callback for when the session owner is promoted
    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        // The local client is the session owner
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
        }
    }

    // callback for when a client is connected
    private void OnClientConnectedCallback(ulong clientId)
    {
        // The local client is connected
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

    // GUI for creating or joining a session
    private void OnGUI()
    {
        // If the client is already connected, return
        if (_state == ConnectionState.Connected)
        {
            return;
        }

        // Enable the GUI if the client is not connecting
        GUI.enabled = _state != ConnectionState.Connecting;

        // Display the connection manager GUI
        using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        {
            GUILayout.Label("Profile Name", GUILayout.Width(100));
            _profileName = GUILayout.TextField(_profileName);
        }

        // Display the session name GUI
        using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        {
            GUILayout.Label("Session Name", GUILayout.Width(100));
            _sessionName = GUILayout.TextField(_sessionName);
        }

        // Display the max players GUI
        GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(_profileName) && !string.IsNullOrEmpty(_sessionName);

        // Display the max players GUI
        if (GUILayout.Button("Create or Join Session"))
        {
            CreateOrJoinSessionAsync();
        }
    }

    private void OnDestroy()
    {
        // Leave the session when the client is destroyed
        _session?.LeaveAsync();
    }

    // Create or join a session
    private async Task CreateOrJoinSessionAsync()
    {
        _state = ConnectionState.Connecting;

        try
        {
            // Switch the profile and sign in anonymously
            AuthenticationService.Instance.SwitchProfile(_profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // options for the session
            var options = new SessionOptions()
            {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            // create or join the session
            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

            // state is connected
            _state = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            // state is disconnected
            _state = ConnectionState.Disconnected;

            Debug.LogException(e);
        }
    }
}