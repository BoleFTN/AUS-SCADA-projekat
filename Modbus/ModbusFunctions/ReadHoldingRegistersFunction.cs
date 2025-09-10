using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] request = new byte[12];

            short transactionId = IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId);
            byte[] BytesTransactionId = BitConverter.GetBytes(transactionId);

            short ProtocolId = IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId);
            byte[] BytesProtocolId = BitConverter.GetBytes(ProtocolId);

            short length = IPAddress.HostToNetworkOrder((short)CommandParameters.Length);
            byte[] BytesLength = BitConverter.GetBytes(length);

            ModbusReadCommandParameters readCP = (ModbusReadCommandParameters)this.CommandParameters;
            short RegisterAddress = IPAddress.HostToNetworkOrder((short)readCP.StartAddress);
            byte[] BytesRegistertAddres = BitConverter.GetBytes(RegisterAddress);

            short RegisterValue = IPAddress.HostToNetworkOrder((short)readCP.Quantity);
            byte[] BytesRegisterValue = BitConverter.GetBytes(RegisterValue);

            request[0] = BytesTransactionId[0];
            request[1] = BytesTransactionId[1];
            request[2] = BytesProtocolId[0];
            request[3] = BytesProtocolId[1];
            request[4] = BytesLength[0];
            request[5] = BytesLength[1];
            request[6] = (byte)CommandParameters.UnitId;
            request[7] = (byte)CommandParameters.FunctionCode;
            request[8] = BytesRegistertAddres[0];
            request[9] = BytesRegistertAddres[1];
            request[10] = BytesRegisterValue[0];
            request[11] = BytesRegisterValue[1];

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters parmCont = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> d = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort adress = ((ModbusReadCommandParameters)CommandParameters).StartAddress;

            for (int i = 0; i < response[8]/2; i++) {
                byte byte1 = response[8+1+i*2];
                byte byte2 = response[8+2+i*2];

                ushort value = BitConverter.ToUInt16(new byte[2] {byte1,byte2 },0);

                d.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, adress), value);
            }
            return d;
        }
    }
}