﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using Infrastructure.Common;
using Xunit;

public class WSHttpTransportWithMessageCredentialSecurityTests : ConditionalWcfTest
{
    //[WcfFact]
    [Condition(nameof(Root_Certificate_Installed),
           nameof(Client_Certificate_Installed),
           nameof(SSL_Available))]
    //[OuterLoop]
    public static void Https_SecModeTransWithMessCred_CertClientCredential_Succeeds()
    {
        string clientCertThumb = null;
        EndpointAddress endpointAddress = null;
        string testString = "Hello";
        ChannelFactory<IWcfService> factory = null;
        IWcfService serviceProxy = null;

        try
        {
            // *** SETUP *** \\
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            endpointAddress = new EndpointAddress(new Uri(Endpoints.Https_SecModeTransWithMessCred_ClientCredTypeCert));
            clientCertThumb = ServiceUtilHelper.ClientCertificate.Thumbprint;

            factory = new ChannelFactory<IWcfService>(binding, endpointAddress);
            factory.Credentials.ClientCertificate.SetCertificate(
                StoreLocation.CurrentUser,
                StoreName.My,
                X509FindType.FindByThumbprint,
                clientCertThumb);

            serviceProxy = factory.CreateChannel();

            // *** EXECUTE *** \\
            string result = serviceProxy.Echo(testString);

            // *** VALIDATE *** \\
            Assert.Equal(testString, result);

            // *** CLEANUP *** \\
            ((ICommunicationObject)serviceProxy).Close();
            factory.Close();
        }
        finally
        {
            // *** ENSURE CLEANUP *** \\
            ScenarioTestHelpers.CloseCommunicationObjects((ICommunicationObject)serviceProxy, factory);
        }
    }

    //[WcfFact]
    [Condition(nameof(Root_Certificate_Installed),
               nameof(SSL_Available))]
    //[OuterLoop]
    public static void Https_SecModeTransWithMessCred_UserNameClientCredential_Succeeds()
    {
        string testString = "Hello";
        string result = null;
        string username = null;
        string password = null;
        EndpointAddress endpointAddress = null;
        ChannelFactory<IWcfService> factory = null;
        IWcfService serviceProxy = null;

        try
        {
            // *** SETUP *** \\
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            endpointAddress = new EndpointAddress(new Uri(Endpoints.Https_SecModeTransWithMessCred_ClientCredTypeUserName));

            factory = new ChannelFactory<IWcfService>(binding, endpointAddress);
            username = Guid.NewGuid().ToString("n").Substring(0, 8);
            char[] usernameArr = username.ToCharArray();
            Array.Reverse(usernameArr);
            password = new string(usernameArr);
            factory.Credentials.UserName.UserName = username;
            factory.Credentials.UserName.Password = password;

            serviceProxy = factory.CreateChannel();

            // *** EXECUTE *** \\
            result = serviceProxy.Echo(testString);

            // *** VALIDATE *** \\
            Assert.Equal(testString, result);

            // *** CLEANUP *** \\
            ((ICommunicationObject)serviceProxy).Close();
            factory.Close();
        }
        finally
        {
            // *** ENSURE CLEANUP *** \\
            ScenarioTestHelpers.CloseCommunicationObjects((ICommunicationObject)serviceProxy, factory);
        }
    }
}
