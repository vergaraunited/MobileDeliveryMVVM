using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Net.Mail;
using MobileDeliveryMVVM.Models;

namespace M2O_Order_Integration
{
    public class classEmail
    {
        public void CreateEmailConfigFile(string sFilePath)
        {
            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                "<configuration>\r\n" +
                "<SMTPServer>smtp.office365.com</SMTPServer>\r\n" +
                "</configuration>";
            doc.LoadXml(xml); //Your string here

            //Create Directory
            if (!Directory.Exists(sFilePath))
            {
                Directory.CreateDirectory(sFilePath);
            }

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(sFilePath + "SMTPServer.config", null);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
            writer.Close();
        }

        public string GetSMTPServer()
        {

            XmlDocument xmlfile = new XmlDocument();
            //xmlfile.Load(frmMain.sConfigPath + "SMTPServer.config");
            xmlfile.Load("SMTPServer.config");
            XmlNode SMTPServernode = xmlfile.SelectSingleNode("configuration/SMTPServer");
            return SMTPServernode.InnerText;
        }

        public bool OrderAckEmail()
        {
            bool bRetVal = false;

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>Order Acknowledgement</b><br /><br />"
                            + "Thank you for your order. We appreciate your business.<br /><br />"
                            + "We have receive your WTS order. <br />"
                            + "If you have any questions please contact Customer Service.<br /><br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("WTSSupport@unitedwindowmfg.com");
            newEmail.To.Add("mzaragoza@unitedwindowmfg.com");


            newEmail.Subject = "United Order Acknowledgement - WTS Quote: "; //+ frmMain.sOrderNo;
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;

            }

            return bRetVal;
        }

        public bool InternalEmail(List<OrderDetailModel> items)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>M2O Order Notification</b><br /><br />"
                            //+ "M2O PO: " + frmMain.sOrderNo + "<br />"
                            //+ "Dealer: " + frmMain.sDealerName + "<br />"
                            + "Order Date: " + sDate + "<br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            //newEmail.ReplyToList.Add("WTSSupport@unitedwindowmfg.com");
            //newEmail.To.Add("mzaragoza@unitedwindowmfg.com");
           // newEmail.To.Add("nwilson@unitedwindowmfg.com");
            newEmail.To.Add("evergar@unitedwindowmfg.com");


            newEmail.Subject = "Stop Complete " ;
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;

            }

            return bRetVal;
        }

        public bool UndefinedOption(List<string> OptionDetails)
        {
            bool bRetVal = false;
            string OptionsDetailsBody = "";

            foreach (string Option in OptionDetails)
            {
                if (OptionsDetailsBody == "")
                {
                    OptionsDetailsBody = Option.Trim();
                }
                else
                {
                    OptionsDetailsBody += "<br />" + Option.Trim();
                }
            }

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>Invalid Option Mapping</b><br /><br />"
                           // + "WTS Quote: " + frmMain.sOrderNo + "<br />"
                            + OptionsDetailsBody + "<br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("WTSSupport@unitedwindowmfg.com");
            newEmail.To.Add("mzaragoza@unitedwindowmfg.com");
            newEmail.To.Add("nwilson@unitedwindowmfg.com");
            newEmail.Subject = "Invalid Option Mapping - "; //+ frmMain.sOrderNo;
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;

            }

            return bRetVal;
        }
        public bool POCancellationEmail(string PO, string OrderNo)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>Lowes Order Cancellation = PO " + PO + "</b><br /><br />"
                            + "WinSys Order Number: " + OrderNo + "<br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("homedepotorders@unitedwindowmfg.com");
            newEmail.To.Add("homedepotorders@unitedwindowmfg.com");
            //newEmail.To.Add("mzaragoza@unitedwindowmfg.com");

            newEmail.Subject = "Lowes Order Cancelled";
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;
            }

            return bRetVal;
        }

        public bool UpdateOrderCreatedError(string PO)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>The order for Lowes PO " + PO + " was created successfullly. However, an error occured when trying to update the OrderCreated record in the database.</b><br /><br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("homedepotorders@unitedwindowmfg.com");
            newEmail.To.Add("mzaragoza@unitedwindowmfg.com");

            newEmail.Subject = "Lowes Order Status Update Error";
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;
            }

            return bRetVal;
        }

        public bool UnknownDealer(string PO)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>The order for Lowes PO " + PO + " was not processed because the dealer account could not be found. Verify the account is setup.</b><br /><br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("homedepotorders@unitedwindowmfg.com");
            newEmail.To.Add("dnigro@unitedwindowmfg.com");
            newEmail.To.Add("dlugo@unitedwindowmfg.com");
            newEmail.To.Add("vtorello@unitedwindowmfg.com");

            newEmail.Subject = "Lowes Order Unknown Dealer";
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;
            }

            return bRetVal;
        }

        public bool UnknownStockItem(string PO)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>The order for Lowes PO " + PO + " contains a Stock/ECat Item but was unable to map to a WinSys stock item.</b><br /><br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("homedepotorders@unitedwindowmfg.com");
            newEmail.To.Add("mzaragoza@unitedwindowmfg.com");

            newEmail.Subject = "Lowes Order Unknown Stock/ECat Item";
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;
            }

            return bRetVal;
        }

        public bool LineItemCountMisMatch(string PO, string OrderNo)
        {
            bool bRetVal = false;
            string sDate = DateTime.Now.ToString("MMM dd,yyyy h:mt");

            //Format Email
            string emailBody = "<html>"
                            + "<body>"
                            + "<b>Lowes 870CC Shipping Notication Issue</b><br /><br />"
                            + "<b>The line item count on Lowes PO " + PO + " did not match the line item count on the WinSys Order Number: " + OrderNo + ". Please review immediately.<br />"
                            + "</body></html>";

            MailMessage newEmail = new MailMessage();
            newEmail.From = new MailAddress("WTSSupportalert@unitedwindowmfg.com");
            newEmail.ReplyToList.Add("mzaragoza@unitedwindowmfg.com");
            newEmail.To.Add("mzaragoza@unitedwindowmfg.com");
            //newEmail.To.Add("mzaragoza@unitedwindowmfg.com");

            newEmail.Subject = "Lowes PO/Order Line Count Mismatch";
            newEmail.Body = emailBody;
            newEmail.IsBodyHtml = true;
            SmtpClient mailServer = new SmtpClient(GetSMTPServer());
            mailServer.Port = 587;
            mailServer.UseDefaultCredentials = false;
            mailServer.EnableSsl = true;
            //gmail settings
            mailServer.Credentials = new System.Net.NetworkCredential("wtssupportalert@unitedwindowmfg.com", "Lifeline@90");


            // mailServer.Timeout = 5000;
            try
            {
                mailServer.Send(newEmail);
            }
            catch (SmtpFailedRecipientException NoMailBox)
            {
                string msg = NoMailBox.Message;
                string st = NoMailBox.StackTrace;
                if (msg.Contains("Mailbox unavailable"))
                {
                    bRetVal = true;
                }
                else
                {
                    bRetVal = false;
                }


            }
            catch (Exception ee)
            {
                bRetVal = false;
                string msg = ee.Message;
                string st = ee.StackTrace;
            }

            return bRetVal;
        }
    }
}
