// ----------------------------------------------------------------------------
// <copyright file="EidasLightProtocolSerializerExtension.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Abc.IdentityModel.Protocols.EidasLight
{
    internal static class EidasLightProtocolSerializerExtension
    {
        private const string EidasMessageKey = "EidasLightMessage";

        public static EidasLightMessage ReadMessageString(this EidasLightProtocolSerializer protocolSerializer, string value)
        {
            if (protocolSerializer is null)
            {
                throw new ArgumentNullException(nameof(protocolSerializer));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
            };

            using var stringReader = new StringReader(value);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            try
            {
                return protocolSerializer.ReadMessage(xmlReader);
            }
            catch (XmlException exception)
            {
                throw new EidasSerializationException("The data retrieved must contain valid XML for eIDAS light message.", exception);
            }
        }

        public static string WriteMessageString(this EidasLightProtocolSerializer protocolSerializer, EidasLightMessage message)
        {
            if (protocolSerializer is null)
            {
                throw new ArgumentNullException(nameof(protocolSerializer));
            }

            if (message is null)
            {
                return null;
            }

            using var stringWriter = new Utf8StringWriter();
            using var xmlTextWriter = new XmlTextWriter(stringWriter);

            protocolSerializer.WriteMessage(xmlTextWriter, message);
            xmlTextWriter.Flush();
            return stringWriter.ToString();
        }

        public static IDictionary<string, string[]> WriteMessageDictionary(this EidasLightProtocolSerializer protocolSerializer, EidasLightMessage message)
        {
            if (protocolSerializer is null)
            {
                throw new ArgumentNullException(nameof(protocolSerializer));
            }

            var data = (string[])null;
            if (message != null)
            {
                data = new string[] { protocolSerializer.WriteMessageString(message) };
            }

            return new Dictionary<string, string[]>()
            {
                { EidasMessageKey, data },
            };
        }

        public static EidasLightMessage ReadMessageDictionary(this EidasLightProtocolSerializer protocolSerializer, IDictionary<string, string[]> data)
        {
            if (protocolSerializer is null)
            {
                throw new ArgumentNullException(nameof(protocolSerializer));
            }

            if (data is null
                || !data.TryGetValue(EidasMessageKey, out var item)
                || item.Length != 1)
            {
                return null;
            }

            return protocolSerializer.ReadMessageString(item[0]);
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}