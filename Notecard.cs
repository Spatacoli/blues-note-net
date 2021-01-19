using System;
using System.Device.I2c;
using System.Text;
using System.Threading;


namespace blues_note_net
{
    public class Notecard
    {
        public const int NOTECARD_I2C_ADDRESS = 0x17;
        public const int CARD_REQUEST_SEGMENT_MAX_LEN = 250;
        public const int CARD_REQUEST_SEGMENT_DELAY_MS = 250;

        private I2cDevice card;
        private byte max;

        public Notecard(byte max_transfer = 0)
        {
            card = I2cDevice.Create(new I2cConnectionSettings(1, NOTECARD_I2C_ADDRESS));
            max = max_transfer == 0 ? 255 : max_transfer;
            this.Reset();
        }

        public void Reset()
        {
            byte chunk_len = 0;
            var isAvailable = false;
            while (!isAvailable)
            {
                Thread.Sleep(1);
                var reg = new byte[2];
                reg[0] = 0;
                reg[1] = chunk_len;
                var readlen = chunk_len + 2;
                var buf = new byte[readlen];
                card.WriteRead(reg, buf);
                var available = buf[0];
                if (available == 0)
                {
                    isAvailable = true;
                }
                chunk_len = Math.Min(available, max);
            }
        }

        public string Transaction(string req)
        {
            Encoding utf8 = Encoding.UTF8;
            var reqJson = utf8.GetBytes((req + "\n").ToCharArray()).AsSpan<byte>();
            var rspJson = string.Empty;

            int chunkOffset = 0;
            int jsonLeft = reqJson.Length;
            int sentInSeg = 0;
            var chunkLength = Math.Min(jsonLeft, max);
            while (jsonLeft > 0)
            {
                Thread.Sleep(1);
                chunkLength = Math.Min(jsonLeft, max);
                var reg = new byte[1];
                reg[0] = (byte)chunkLength;
                var writeData = reqJson.Slice(chunkOffset, chunkLength);

                card.Write(writeData);
                chunkOffset += chunkLength;
                jsonLeft -= chunkLength;
                sentInSeg += chunkLength;
                if (sentInSeg > CARD_REQUEST_SEGMENT_MAX_LEN)
                {
                    sentInSeg -= CARD_REQUEST_SEGMENT_MAX_LEN;
                }
                Thread.Sleep(CARD_REQUEST_SEGMENT_DELAY_MS);
            }

            chunkLength = 0;
            var receivedNewline = false;
            var startTime = DateTime.Now;
            var timeoutTimeInSec = 10;

            while (true)
            {
                Thread.Sleep(1);
                var reg = new byte[2];
                reg[0] = 0;
                reg[1] = (byte)chunkLength;
                var readLength = chunkLength + 2;
                var buf = new byte[readLength];
                card.WriteRead(reg, buf);
                var available = buf[0];
                var good = buf[1];
                if (readLength > 2) 
                {
                    var data = buf.AsSpan<byte>(2, 2 + good);
                    rspJson += data.ToString();
                    receivedNewline = rspJson.EndsWith('\n');
                }
                chunkLength = Math.Min(available, max);
                if (chunkLength > 0)
                {
                    continue;
                } 
                if (DateTime.Compare(startTime.AddSeconds(timeoutTimeInSec), DateTime.Now) < 0)
                {
                    throw new TimeoutException("Notecard request or response was lost");
                }
                if (receivedNewline)
                {
                    break;
                }
                Thread.Sleep(50);
            }

            return rspJson;
        }
    }
}