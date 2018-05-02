﻿using System;
using Tenduke.Client.Authorization;
using Tenduke.Client.Config;
using Tenduke.Client.EntApi.Authz;
using Tenduke.Client.Util;

namespace Tenduke.Client
{
    /// <summary>
    /// Basic class for clients for working against the 10Duke Entitlement service.
    /// </summary>
    public class BaseClient<C, A> where A : IOAuthConfig where C : BaseClient<C, A>
    {
        #region Private fields

        /// <summary>
        /// Configuration for communicating with the <c>/authz/</c> API of the 10Duke Entitlement service.
        /// </summary>
        private IAuthzApiConfig authzApiConfig;

        #endregion

        #region Properties

        /// <summary>
        /// OAuth 2.0 configuration to use for communicating with the 10Duke Entitlement service.
        /// </summary>
        public A OAuthConfig { get; set; }

        /// <summary>
        /// Configuration for communicating with the <c>/authz/</c> API of the 10Duke Entitlement service.
        /// </summary>
        public IAuthzApiConfig AuthzApiConfig { get; set; }

        /// <summary>
        /// Authorization process result information received from the 10Duke Entitlement service.
        /// </summary>
        public AuthorizationInfo Authorization { get; set; }

        /// <summary>
        /// Gets an <see cref="AuthzApi"/> object for accessing the <c>/authz/</c> API of the 10Duke Entitlement service.
        /// Please note that the OAuth authentication / authorization process must be successfully completed before
        /// getting the <see cref="AuthzApi"/> object.
        /// </summary>
        public AuthzApi AuthzApi
        {
            get
            {
                var authzApiConfig = AuthzApiConfig;
                if (authzApiConfig == null)
                {
                    throw new InvalidOperationException("Configuration for AuthzApi missing, please specify either AuthzApiConfig or OAuthConfig");
                }

                if (Authorization == null)
                {
                    throw new InvalidOperationException("OAuth authorization must be negotiated with the server before accessing the AuthzApi");
                }

                if (Authorization.Error != null)
                {
                    throw new InvalidOperationException(
                        string.Format("OAuth authorization has not been completed successfully (error code {0}, error message \"{1}\")",
                        Authorization.Error,
                        Authorization.ErrorDescription ?? ""));
                }

                return new AuthzApi()
                {
                    AuthzApiConfig = authzApiConfig,
                    AccessToken = Authorization.AccessTokenResponse
                };
            }
        }

        /// <summary>
        /// Gets an <see cref="Util.AuthorizationSerializer"/> for reading and writing <see cref="Authorization"/>
        /// of this object by binary serialization.
        /// </summary>
        public AuthorizationSerializer<C, A> AuthorizationSerializer
        {
            get
            {
                return new AuthorizationSerializer<C, A>() { TendukeClient = (C) this };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if this client object currently contains a valid access token in <see cref="Authorization"/>.
        /// Access token is used for 10Duke Entitlement Service API requests.
        /// </summary>
        /// <returns><c>true</c> if authorized, <c>false</c> otherwise.</returns>
        public bool IsAuthorized()
        {
            return Authorization != null && Authorization.AccessTokenResponse != null;
        }

        /// <summary>
        /// Discards authorization information received from the server by setting <see cref="Authorization"/> to <c>null</c>.
        /// </summary>
        public void ClearAuthorization()
        {
            Authorization = null;
        }

        #endregion
    }
}