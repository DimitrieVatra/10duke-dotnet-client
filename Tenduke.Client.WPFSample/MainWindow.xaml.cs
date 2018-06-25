﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tenduke.Client.Config;
using Tenduke.Client.Util;
using Tenduke.Client.WPF;

namespace Tenduke.Client.WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Registry key for stored authorization.
        /// </summary>
        private readonly string REGISTRY_KEY_STORED_AUTHORIZATION = "Software\\10Duke\\Tenduke.EntitlementClient\\SampleApp";

        /// <summary>
        /// Public key to use for verifying 10Duke Entitlement Service signatures.
        /// </summary>
        public static readonly RSA EntServerPublicKey = CryptoUtil.ReadRsaPublicKey(Properties.Settings.Default.SignerKey);

        /// <summary>
        /// OAuth 2.0 configuration for connecting this sample application to the 10Duke Entitlement service.
        /// </summary>
        public readonly AuthorizationCodeGrantConfig OAuthConfig = new AuthorizationCodeGrantConfig()
        {
            AuthzUri = Properties.Settings.Default.AuthzUri,
            TokenUri = Properties.Settings.Default.TokenUri,
            UserInfoUri = Properties.Settings.Default.UserInfoUri,
            ClientID = Properties.Settings.Default.ClientID,
            ClientSecret = Properties.Settings.Default.ClientSecret,
            RedirectUri = Properties.Settings.Default.RedirectUri,
            Scope = Properties.Settings.Default.Scope,
            SignerKey = EntServerPublicKey
        };

        /// <summary>
        /// <para>The <see cref="Tenduke.EntitlementClient.EntClient"/> instance used by this sample application.</para>
        /// <para>Please note that </para>
        /// </summary>
        protected EntClient EntClient { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            EntClient = new EntClient() { OAuthConfig = OAuthConfig };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // This sample application always requires sign-on / authorization against the 10Duke entitlement service.
            EnsureAuthorization();
            if (EntClient.IsAuthorized())
            {
                ShowWelcomeMessage();
                ShowComputerId();
                StoreAuthorization();
            }
            else
            {
                // If the authorization process was cancelled, close this form. This will cause the whole application
                // to be closed.
                Close();
            }
        }

        /// <summary>
        /// Checks that either a previously stored valid authorization against the 10Duke Entitlement Service exists,
        /// or launches embedded browser for signing on and getting the authorization.
        /// </summary>
        private void EnsureAuthorization()
        {
            if (!UseStoredAuthorization())
            {
                EntClient.AuthorizeSync();
            }
        }

        /// <summary>
        /// Checks value of the <c>StoreAuthorization</c> setting to determine if earlier authorization stored in the registry
        /// should be read and used. If stored authorization is used and a stored authorization value is found in the registry,
        /// initializes <see cref="EntClient"/> to use the stored authorization.
        /// </summary>
        /// <returns><c>true</c> if stored authorization is used and a stored authorization info is found in the registry,
        /// <c>false</c> otherwise.</returns>
        private bool UseStoredAuthorization()
        {
            bool retValue = false;
            if (Properties.Settings.Default.StoreAuthorization)
            {
                var storedAuthorization = ReadAuthorizationInfoFromRegistry();
                if (storedAuthorization != null)
                {
                    EntClient.AuthorizationSerializer.WriteAuthorization(storedAuthorization);
                    retValue = true;
                }
            }

            return retValue;
        }

        /// <summary>
        /// Reads stored authorization info from registry.
        /// </summary>
        /// <returns>Authorization info serialized as a byte array, or <c>null</c> if no stored authorization information found.</returns>
        private byte[] ReadAuthorizationInfoFromRegistry()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_STORED_AUTHORIZATION))
            {
                return key == null ? null : key.GetValue("StoredAuthorization") as byte[];
            }
        }

        /// <summary>
        /// Checks value of the <c>StoreAuthorization</c> setting to determine if authorization from the 10Duke Entitlement
        /// Service should be stored in the registry. If value of the setting is <c>true</c>, stored current <see cref="EntClient.Authorization"/>
        /// to the registry.
        /// </summary>
        /// <returns><c>true</c> if authorization was stored, <c>false</c> otherwise.</returns>
        private bool StoreAuthorization()
        {
            bool retValue = false;
            if (Properties.Settings.Default.StoreAuthorization)
            {
                StoreAuthorizationInfoToRegistry(EntClient.AuthorizationSerializer.ReadAuthorization());
                retValue = true;
            }

            return retValue;
        }

        /// <summary>
        /// Stores authorization info to registry.
        /// </summary>
        /// <param name="authorizationInfo">Serialized authorization info.</param>
        private void StoreAuthorizationInfoToRegistry(byte[] authorizationInfo)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_STORED_AUTHORIZATION))
            {
                key.SetValue("StoredAuthorization", authorizationInfo);
            }
        }

        /// <summary>
        /// Populates welcome message shown by this form using user attributes received from the 10Duke entitlement
        /// service in the received OpenID Connect ID token.
        /// </summary>
        private void ShowWelcomeMessage()
        {
            var name = (string)EntClient.Authorization.AccessTokenResponse.IDToken["name"];
            if (string.IsNullOrEmpty(name))
            {
                var givenName = (string)EntClient.Authorization.AccessTokenResponse.IDToken["given_name"];
                var familyName = (string)EntClient.Authorization.AccessTokenResponse.IDToken["family_name"];

                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(givenName))
                {
                    builder.Append(givenName);
                }
                if (!string.IsNullOrEmpty(familyName))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(familyName);
                }

                name = builder.Length == 0 ? null : builder.ToString();
            }

            name = name ?? "anonymous";
            labelWelcome.Content = string.Format("Welcome {0}", name);
        }

        /// <summary>
        /// Computes a computer id (identifier for this system) and displays it on the form.
        /// </summary>
        private void ShowComputerId()
        {
            textBoxComputerId.Text = EntClient.ComputerId;
        }
    }
}