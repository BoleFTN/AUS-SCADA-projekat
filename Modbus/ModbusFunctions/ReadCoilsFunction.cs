using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            //HostToNetwork
            byte[] request = new byte[12];

            short transactionId = IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId);
            byte[] BytesTransactionId = BitConverter.GetBytes(transactionId);

            short ProtocolId = IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId);
            byte[] BytesProtocolId = BitConverter.GetBytes(ProtocolId);

            short length  = IPAddress.HostToNetworkOrder((short)CommandParameters.Length);
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
            ModbusReadCommandParameters paramCom = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> d = new Dictionary<Tuple<PointType, ushort>, ushort>();

            int q = response[8];

            for (int i = 0; i < q; i++) {
                for (int j = 0; j < 8; j++) {
                    if (paramCom.Quantity < (j + i * 8)) { break;}

                    ushort v = (ushort)(response[9 + i] & (byte)0x1);
                    response[9 + i] /= 2;

                    d.Add(new Tuple<PointType,ushort>(PointType.DIGITAL_OUTPUT,(ushort)(paramCom.StartAddress + (j+i*80))),v);
                }
            }

            return d;
        }
    }
}