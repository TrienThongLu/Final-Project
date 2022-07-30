using System.IO.Ports;

namespace Final_Project.Utils.Services
{
    public class SendSMSService
    {
        public async Task<bool> SendSMS(string phonenumber, string OTPMes)
        {
            try
            {
                var GSM = "GSM";

                SerialPort sp = new SerialPort();
                sp.PortName = "COM3";
                sp.Open();
                sp.WriteLine("AT" + Environment.NewLine);
                Thread.Sleep(500);

                sp.WriteLine("AT+CMGF=1" + Environment.NewLine);
                Thread.Sleep(500);

                sp.WriteLine("AT+CSCS=\"" + GSM + "\"" + Environment.NewLine);
                Thread.Sleep(500);

                sp.WriteLine("AT+CMGS=\"" + phonenumber + "\"" + Environment.NewLine);
                Thread.Sleep(500);

                sp.WriteLine(OTPMes + Environment.NewLine);
                Thread.Sleep(500);

                sp.Write(new byte[] { 26 }, 0, 1);
                Thread.Sleep(500);

                var response = sp.ReadExisting();
                if (response.Contains("ERROR"))
                {
                    return false;
                }
                else
                {
                    sp.Close();
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Cannot connect to module to send sms message");
                return false;
            }
        }
    }
}
