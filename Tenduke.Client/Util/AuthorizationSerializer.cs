﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Tenduke.Client.Authorization;
using Tenduke.Client.Config;

namespace Tenduke.Client.Util
{
    /// <summary>
    /// Object that works with an <see cref="EntClient"/> instance for reading and writing <see cref="EntClient.Authorization"/>
    /// by binary serialization.
    /// </summary>
    public class AuthorizationSerializer<C, A> where A : IOAuthConfig where C : BaseClient<C, A>
    {
        #region Properties

        /// <summary>
        /// The <see cref="BaseDesktopClient{C}"/> for which <see cref="AuthorizationInfo"/> is serialized or deserialized.
        /// </summary>
        public C EntClient { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes <see cref="AuthorizationInfo"/> object into a binary stream.
        /// </summary>
        /// <param name="authorization">The <see cref="AuthorizationInfo"/> to serialize.</param>
        /// <param name="stream">The stream for writing the binary serialized authorization.
        /// Caller of this method is responsible for closing the stream after calling this method.</param>
        public static void SerializeAuthorization(AuthorizationInfo authorization, Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, authorization);
        }

        /// <summary>
        /// Serializes <see cref="AuthorizationInfo"/> object into a byte array.
        /// </summary>
        /// <param name="authorization">The <see cref="AuthorizationInfo"/> to serialize.</param>
        /// <returns>Byte array representing the serialized authorization object.</returns>
        public static byte[] SerializeAuthorization(AuthorizationInfo authorization)
        {
            using (var stream = new MemoryStream())
            {
                SerializeAuthorization(authorization, stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes <see cref="AuthorizationInfo"/> object into a Base 64 encoded string.
        /// </summary>
        /// <param name="authorization">The <see cref="AuthorizationInfo"/> to serialize.</param>
        /// <returns>Base 64 encoded string representing the serialized authorization object.</returns>
        public static string SerializeAuthorizationToBase64(AuthorizationInfo authorization)
        {
            byte[] serialized = SerializeAuthorization(authorization);
            return Convert.ToBase64String(serialized);
        }

        /// <summary>
        /// Deserializes <see cref="AuthorizationInfo"/>.
        /// </summary>
        /// <param name="stream">Stream for reading the serialized <see cref="AuthorizationInfo"/> object.
        /// Caller of this method is responsible for closing the stream after calling this method.</param>
        /// <returns>The deserialized <see cref="AuthorizationInfo"/>.</returns>
        public static AuthorizationInfo DeserializeAuthorization(Stream stream)
        {
            var formatter = new BinaryFormatter();
            var deserialized = formatter.Deserialize(stream);
            if (!(deserialized is AuthorizationInfo))
            {
                throw new InvalidOperationException("The serialized object does not represent a valid AuthorizationInfo");
            }

            return deserialized as AuthorizationInfo;
        }

        /// <summary>
        /// Deserializes <see cref="AuthorizationInfo"/>.
        /// </summary>
        /// <param name="serialized">Byte array representing the serialized <see cref="AuthorizationInfo"/> object.</param>
        /// <returns>The deserialized <see cref="AuthorizationInfo"/>.</returns>
        public static AuthorizationInfo DeserializeAuthorization(byte[] serialized)
        {
            using (var stream = new MemoryStream(serialized))
            {
                return DeserializeAuthorization(stream);
            }
        }

        /// <summary>
        /// Deserializes <see cref="AuthorizationInfo"/>.
        /// </summary>
        /// <param name="serialized">Base64 encoded string representing the serialized <see cref="AuthorizationInfo"/> object.</param>
        /// <returns>The deserialized <see cref="AuthorizationInfo"/>.</returns>
        public static AuthorizationInfo DeserializeAuthorizationFromBase64(string serialized)
        {
            var serializedBytes = Convert.FromBase64String(serialized);
            return DeserializeAuthorization(serializedBytes);
        }

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if <see cref="EntClient"/> is not set.
        /// </summary>
        /// <returns>Returns the <see cref="EntClient"/> property value that is verified to be not <c>null</c>.</returns>
        protected C AssertEntClient()
        {
            if (EntClient == null)
            {
                throw new InvalidOperationException("EntClient must be set");
            }

            return EntClient;
        }

        #endregion
    }
}
