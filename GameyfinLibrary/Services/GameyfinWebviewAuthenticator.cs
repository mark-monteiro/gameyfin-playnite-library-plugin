using System;
using System.Collections.Generic;
using System.Linq;

using Playnite.SDK;

using GameyfinLibrary.Views;
using Playnite.SDK.Events;
using System.Runtime;
using System.Windows;

namespace GameyfinLibrary.Services
{
    internal class GameyfinWebviewAuthenticator : ObservableObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether authentication is in progress.
        /// </summary>
        public bool AuthenticationInProgress
        {
            get => _authInProgress;
            set => SetValue(ref _authInProgress, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether authentication succeeded. Is null while authentication hasn't started or is in processes.
        /// </summary>
        public bool? AuthenticateSuccess
        {
            get => _authSuccess;
            set => SetValue(ref _authSuccess, value);
        }

        /// <summary>
        /// Gets or sets the authentication error message to display, if any
        /// </summary>
        public string AuthenticationErrorMessage
        {
            get => _authErrorMessage;
            set => SetValue(ref _authErrorMessage, value);
        }

        /// <summary>
        /// Gets or sets the authentication cookie value retrieved as part of a successful authentication.
        /// </summary>
        public string AuthCookieValue
        {
            get => _authCookie;
            set => SetValue(ref _authCookie, value);
        }

        private readonly GameyfinLibrarySettingsViewModel _settingsVm;
        private readonly IPlayniteAPI _playniteApi;

        private IWebView _webView = null;
        private bool _authInProgress = false;
        private bool? _authSuccess = null;
        private string _authErrorMessage = null;
        private string _authCookie = null;

        public GameyfinWebviewAuthenticator(GameyfinLibrarySettingsViewModel settingsVm, IPlayniteAPI playniteApi)
        {
            _settingsVm = settingsVm;
            _playniteApi = playniteApi;
        }

        /// <summary>
        /// Open a web view dialog to initiate the authentication UX flow. This method returns immediately, but the web
        /// view will remain open until the user successfully logs in and navigates to the Gameyfin home page, at which
        /// point the auth cookie value will be extracted and the web view closed.
        /// </summary>
        public void StartForwardAuthLogin()
        {
            try
            {
                // Reset authentication status
                AuthenticationInProgress = true;
                AuthenticateSuccess = null;
                AuthenticationErrorMessage = null;

                // Fail if another auth web view is already open
                if (_webView != null)
                    throw new InvalidOperationException("Another authentication dialog is already open");

                // Validate settings before initiating authentication
                var settings = _settingsVm.Settings;
                if (settings.AuthMethod != GameyfinAuthMethod.ForwardAuth)
                    throw new InvalidOperationException("Interactive authentication is only required for ForwardAuth");
                if (String.IsNullOrWhiteSpace(settings.AuthCookieName))
                    throw new InvalidOperationException("An authentication cookie name is required");

                // Open a new web view; note it is not disposed when this method returns, but when authentication finishes
                _webView = _playniteApi.WebViews.CreateView(400, 445);

                // Add an event handler for when a new page is loaded in the web view
                // The handler will be responsible for making sure the web view is eventually closed and disposed
                _webView.LoadingChanged += WebViewOnLoadingChanged;

                // Dispose the web view when the web view window is closed. The window can either be closed automatically
                // when authentication completes (i.e. _webView.close()), or manually by the user
                _webView.WindowHost.Closed += (s, e) => DisposeWebView();

                // Start navigation to the target address and show the webview as a modal dialog
                _webView.Navigate(settings.GameyfinUrl);
                _webView.OpenDialog();
            }
            catch (Exception ex)
            {
                HandleAuthFailure(ex);
            }
        }

        // NOTE: Accessing methods on the web view in this event handler causes deadlocks. In order to work around this
        // issue this event handler returns immediately, and the actual handler is invoked asynchronously using the
        // dispatcher on the web view window
        private void WebViewOnLoadingChanged(object sender, WebViewLoadingChangedEventArgs e)
        {
            _webView.WindowHost.Dispatcher.InvokeAsync(() => WebViewOnLoadingChanged(e));
        }

        private void WebViewOnLoadingChanged(WebViewLoadingChangedEventArgs e)
        {
            try
            {
                // Do nothing when loading starts
                if (e.IsLoading) return;

                // Do nothing if the current address does not match the target (i.e. user was re-directed to a login page)
                // The user will log in then be re-directed back to gameyfin, which will trigger this event handler a second time
                var settings = _settingsVm.Settings;
                var serverUrl = settings.GameyfinUrl;
                if (!_webView.GetCurrentAddress().StartsWith(serverUrl)) return;

                // We have reached the target page; get the authentication cookie
                var authCookieName = settings.AuthCookieName;
                var authCookie = _webView.GetCookies().FirstOrDefault(x => x.Name == authCookieName);

                // Validate the auth cookie value
                if (authCookie is null) {
                    HandleAuthFailure($"Did not find an authentication cookie with name '{authCookieName}'");
                    return;
                } else if (String.IsNullOrWhiteSpace(authCookie.Value)) {
                    HandleAuthFailure($"Authentication cookie was found but has an empty value");
                    return;
                }

                // Auth cookie retrieved successfully
                HandleAuthSuccess(authCookie.Value);
            }
            catch (Exception ex)
            {
                HandleAuthFailure(ex);
            }
        }

        private void HandleAuthSuccess(string cookieValue)
        {
            AuthCookieValue = cookieValue;
            AuthenticateSuccess = true;
            _webView.Close();
        }

        private void HandleAuthFailure(Exception ex)
            => HandleAuthFailure(ex.Message);

        private void HandleAuthFailure(string message)
        {
            AuthenticationErrorMessage = message;
            AuthenticateSuccess = false;
            AuthenticationInProgress = false;
            _webView?.Close();
        }

        private void DisposeWebView()
        {
            AuthenticationInProgress = false;

            if (_webView == null) return;
            _webView.LoadingChanged -= WebViewOnLoadingChanged;
            _webView.Dispose();
            _webView = null;
        }
    }
}
