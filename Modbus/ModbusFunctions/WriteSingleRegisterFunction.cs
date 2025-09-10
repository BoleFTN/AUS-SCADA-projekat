using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] request = new byte[12];

            short transactionId = IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId);
            byte[] BytesTransactionId = BitConverter.GetBytes(transactionId);

            short ProtocolId = IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId);
            byte[] BytesProtocolId = BitConverter.GetBytes(ProtocolId);

            short length = IPAddress.HostToNetworkOrder((short)CommandParameters.Length);
            byte[] BytesLength = BitConverter.GetBytes(length);

            //ovde se razlikuje kreiranje requesta
            ModbusWriteCommandParameters writeCP = (ModbusWriteCommandParameters)this.CommandParameters;
            short RegisterAddress = IPAddress.HostToNetworkOrder((short)writeCP.OutputAddress);
            byte[] BytesRegistertAddres = BitConverter.GetBytes(RegisterAddress);

            short RegisterValue = IPAddress.HostToNetworkOrder((short)writeCP.Value);
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
            Dictionary<Tuple<PointType, ushort>, ushort> values = new Dictionary<Tuple<PointType, ushort>, ushort>();

            byte[] tempAdress = new byte[2];
            byte[] tempValue = new byte[2];

            tempAdress[0] = response[8];
            tempAdress[1] = response[9];

            ushort adress = BitConverter.ToUInt16(new byte[2] { tempAdress[1], tempAdress[0] }, 0);

            tempValue[0] = response[10];
            tempValue[1] = response[1];

            ushort value = BitConverter.ToUInt16(new byte[2] { tempValue[1], tempValue[0] }, 0);

            values.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, adress),value);

            return values;
        }
    }
}