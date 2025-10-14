using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityCoreCustomization.Models;
using Microsoft.Extensions.Configuration;
using PARSGREEN.CORE.RESTful.SMS;

namespace IdentityCoreCustomization.Services
{
    public interface ISmsService
    {
        Task SendSms(string SmsText, List<string> ReceipentPhones);
        Task SendSms(string SmsText, string ReceipentPhone);
    }

    public class SmsService : ISmsService
    {
        private ParsGreenConfig parsGreenConfig = new ParsGreenConfig();
        private IConfiguration Configuration { get; }

        public SmsService(IConfiguration configuration)
        {
            Configuration = configuration;
            parsGreenConfig.SendFromNumber = Configuration.GetValue<string>("ParsGreen:SendFromNumber");
            parsGreenConfig.ApiToken = Configuration.GetValue<string>("ParsGreen:ApiToken");
        }
        public async Task SendSms(string SmsText, List<string> ReceipentPhones)
        {
            await Task.Run(() =>
            {
                Message msg = new Message(parsGreenConfig.ApiToken);
                msg.SendSms(SmsText, ReceipentPhones.ToArray(), parsGreenConfig.SendFromNumber);
            }).ConfigureAwait(false);
            
        }

        public async Task SendSms(string SmsText, string ReceipentPhone)
        {
            await Task.Run(() =>
            {
                Message msg = new Message(parsGreenConfig.ApiToken);
                msg.SendSms(SmsText, new string[] {ReceipentPhone}, parsGreenConfig.SendFromNumber);
            }).ConfigureAwait(false);
        }
    }
}
