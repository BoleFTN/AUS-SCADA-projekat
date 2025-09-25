using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {

            ModbusWriteCommandParameters writeParams = CommandParameters as ModbusWriteCommandParameters;

            byte[] request = new byte[12];

            // Transaction ID 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.TransactionId)), 0, request, 0, 2);
            // Protocol ID 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.ProtocolId)), 0, request, 2, 2);
            // Length 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.Length)), 0, request, 4, 2);
            // Unit ID 
            request[6] = writeParams.UnitId;
            // Function Code 
            request[7] = writeParams.FunctionCode;
            // Register Address 
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.OutputAddress)), 0, request, 8, 2);
            // Register Value
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.Value)), 0, request, 10, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
           Dictionary<Tuple<PointType, ushort>, ushort> values = new Dictionary<Tuple<PointType, ushort>, ushort>();

            byte[] tempAdress = new byte[2];
            byte[] tempValue = new byte[2];

            tempAdress[0] = response[8];
            tempAdress[1] = response[9];

            ushort adress = BitConverter.ToUInt16(new byte[2] { tempAdress[0], tempAdress[1] }, 0);

            tempValue[0] = response[10];
            tempValue[1] = response[11];

            ushort value = BitConverter.ToUInt16(new byte[2] { tempValue[0], tempValue[1] }, 0);

            values.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, adress),value);

            return values;
        }
    }
}