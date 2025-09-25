using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {

            ModbusReadCommandParameters readParams = CommandParameters as ModbusReadCommandParameters;

            byte[] request = new byte[12];

            // Transaction ID 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.TransactionId)), 0, request, 0, 2);
            // Protocol ID 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.ProtocolId)), 0, request, 2, 2);
            // Length 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.Length)), 0, request, 4, 2);
            // Unit ID 
            request[6] = readParams.UnitId;
            // Function Code 
            request[7] = readParams.FunctionCode;
            // Start Address 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.StartAddress)), 0, request, 8, 2);
            // Quantity 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.Quantity)), 0, request, 10, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters paramCom = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> d = new Dictionary<Tuple<PointType, ushort>, ushort>();

            int q = response[8];

            for (int i = 0; i < q; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (paramCom.Quantity < (j + i * 8)) { break; }

                    ushort v = (ushort)(response[9 + i] & (byte)0x01);
                    response[9 + i] /= 2;

                    d.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, (ushort)(paramCom.StartAddress + (j + i * 8))), v);
                }
            }

            return d;
        }
    }
}