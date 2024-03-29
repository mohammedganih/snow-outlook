using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Configuration;


namespace Mail
{
    class Program
    {
        static bool IsValid(string value)
        {
            return Regex.IsMatch(value, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
        }

        static void Main(string[] args)
        {
            Application outlookApplication = null;
            NameSpace outlookNamespace = null;
            MAPIFolder inboxFolder = null;
            Items mailItems = null;
            
            try
            {
                outlookApplication = new Application();
                outlookNamespace = outlookApplication.GetNamespace("MAPI");
                outlookNamespace.Logon(Missing.Value, Missing.Value, false, true);
                inboxFolder = outlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                mailItems = inboxFolder.Folders["PI Notifications"].Items;

                //Console.WriteLine(mailItems.Count);
                int mail_count = 1;

                List<string> subject_list = new List<string>();
               
                foreach (MailItem item in mailItems)
                {

                    string forwarded_mail_subject = item.Subject.ToString().ToLower();
                    int subject_index = forwarded_mail_subject.IndexOf("piserverhealthmonitoring");
                    string subject = forwarded_mail_subject.Substring(subject_index);

                    string subject_line1 = "piserverhealthmonitoring";
                    string subject_line2 = "new notification event";

                    if (mail_count == 1)
                    {
                        subject_list.Add(subject);
                    }
                    else if (subject_list.Contains(subject))
                    {
                        continue;
                    }
                    
                   
                        if (subject.Contains(subject_line1) && subject.Contains(subject_line2))
                        {
                        
                            DateTime mail_received_time = item.ReceivedTime;
                            DateTime current_time = DateTime.Now;
                            TimeSpan time_difference = current_time.Subtract(mail_received_time);

                            if (time_difference.TotalMinutes <= 30)
                            {
                               
                                var stringBuilder = new StringBuilder();

                                var mail_full_body = item.Body.ToString();
                                int issue_index = mail_full_body.IndexOf("Issue:");
                                var mail_body  = mail_full_body.Substring(issue_index);
                                //Console.WriteLine(text);
                                stringBuilder.AppendLine("");

                                var stringReader_forwarded = new StringReader(mail_full_body);

                                var stringReader = new StringReader(mail_body);
                            
                                string line1, forwarded_line;

                            string issue_line = null;
                            string imp_server = null;
                            string action_line = null;
                            string severity_line = null;
                            string start_time = null;
                            //string link_line = null;
                            //string kb_line;
                            string reason_line = null;
                            string caller_before;
                            string notes_line = null;
                            string caller = null;
                            string sub = null;
                            int name_index, names_index;
                            string issue = null;
                            string reason = null;
                            string impacted = null;
                            string start = null;
                            string severity = null;
                            string action = null;
                            string notes = null;

                           forwarded_line = stringReader_forwarded.ReadLine();

                            while (forwarded_line != null)
                            {
                                if (forwarded_line.Contains("Subject:"))
                                {
                                    sub = forwarded_line;
                                    //stringBuilder.AppendLine("Subject : " + sub);
                                }

                                if (forwarded_line.Contains("To:"))
                                {
                                    name_index = forwarded_line.IndexOf("<");
                                    if (name_index > 0)
                                    {
                                        //name = fwd_line.IndexOf("<");
                                        caller_before = forwarded_line.Substring(3, name_index - 3);
                                        Console.WriteLine(caller_before);
                                        caller = caller_before.Trim();
                                        Console.WriteLine(caller);
                                    }
                                    else
                                    {
                                        names_index = forwarded_line.IndexOf(":");
                                        caller_before = forwarded_line.Substring(3);
                                        Console.WriteLine(caller_before);
                                        caller = caller_before.Trim();
                                        Console.WriteLine(caller);
                                    }
                                    //stringBuilder.AppendLine("Caller : " + caller);
                                }
                                forwarded_line = stringReader_forwarded.ReadLine();
                            }
                            
								line1 = stringReader.ReadLine();

                                while (line1 != null)
                                {
                                                                    
                                    if (line1.Contains("Issue:"))
                                    {
                                        stringReader.ReadLine();
                                        issue_line = stringReader.ReadLine();
                                        issue = " (1) " + "Issue : " + issue_line;
                                        //stringBuilder.AppendLine("Issue : " + issue_line);
                                    }

                                    if (line1.Contains("Probable Reason:"))
                                    {
                                        stringReader.ReadLine();
                                        reason_line = stringReader.ReadLine();
                                        reason = " (2) " + "Probable Reason : " + reason_line;
                                        //stringBuilder.AppendLine("Probable Reason : " + reason_line);
                                    }

                                    if (line1.Contains("Impacted Server:"))
                                    {
                                        stringReader.ReadLine();
                                        imp_server = stringReader.ReadLine();
                                        impacted = " (3) " + "Imapacted Server : " + imp_server;
                                        //stringBuilder.AppendLine("Imapacted Server : " + imp_server);
                                    }

                                    if (line1.Contains("Start Time:"))
                                    {
                                        stringReader.ReadLine();
                                        start_time = stringReader.ReadLine();
                                        start = " (2) " + "Start Time : " + start_time;
                                        //stringBuilder.AppendLine("Start Time : " + start_time);
                                    }
                                    if (line1.Contains("Severity:"))
                                    {
                                        stringReader.ReadLine();
                                        severity_line = stringReader.ReadLine();
                                        severity = " (3) " + "Severity : " + severity_line;
                                        //stringBuilder.AppendLine("Severity : " + severity_line);
                                    }

                               

                                if (line1.Contains("Action Needed:"))
                                {
                                   // stringBuilder.AppendLine("Action needed : ");
                                    action_line = stringReader.ReadLine();

                                    do
                                    {
                                        if (!string.IsNullOrWhiteSpace(action_line) && (IsValid(action_line) == false))
                                        {
                                            stringBuilder.Append(action_line);                                           
                                        }
                                        action_line = stringReader.ReadLine();
                                       

                                    } while (!action_line.Contains("Notes:"));
                                    //stringBuilder.AppendLine("Notes: " + action_line);
                                    stringReader.ReadLine();
                                    notes_line = stringReader.ReadLine();
                                    notes = " (4) " + "Notes: " + notes_line;
                                    //stringBuilder.AppendLine("Notes: " + notes_line);
                                }
                                //foreach (Match vals in Regex.Matches(mail_body, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                                //{

                                //    if (vals.ToString().Contains("ind01.safelinks.protection.outlook.com/?"))
                                //    {
                                //        stringBuilder.AppendLine("Related KB Articles:");
                                //        stringBuilder.AppendLine(vals.Value);
                                //    }
                                //}
                                //Console.WriteLine(action_line);
                                line1 = stringReader.ReadLine();

                                }

                                string action_space = stringBuilder.ToString();
                                action = action_space.Trim();
                                Console.WriteLine(mail_count);
                                //Console.WriteLine(action);
                                string sdesc = reason_line + " in " + imp_server;
                                string desc = issue + start + severity + notes;
                                //Console.WriteLine(desc);
                                Incidents.IncCreate(sdesc, caller, desc, action);
                                Marshal.ReleaseComObject(item);
                                mail_count = mail_count + 1;


                            }
                           
                        }
                    
                }
                subject_list.Clear();
            }

            //Error handler.
            catch (System.Exception e)
            {
                Console.WriteLine("{0} Exception caught: ", e);
            }
            finally
            {
                ReleaseComObject(mailItems);
                ReleaseComObject(inboxFolder);
                ReleaseComObject(outlookNamespace);
                ReleaseComObject(outlookApplication);
            }
           
             
            Console.WriteLine("No more notifications pending");
           

            Console.ReadKey();
        }


        private static void ReleaseComObject(object obj)
        {
            if (obj != null)
            {
                Marshal.ReleaseComObject(obj);
                obj = null;
            }
        }
    }
    public class Incidents
    {

        public static void IncCreate(string shortdesc, string call, string desc, string worknotes)
        {
            try
            {
                var shortdescription = shortdesc;
                var caller = call;
                var description = desc;
                var work_notes = worknotes;
                var assignment_group = "PI Server Health Monitoring";
                
                //var worknotes = notes;
                
                string username = ConfigurationManager.AppSettings["ServiceNowUserName"];
                string password = ConfigurationManager.AppSettings["ServiceNowPassword"];
                string url = ConfigurationManager.AppSettings["ServiceNowUrl"];

                var auth = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Headers.Add("Authorization", auth);
                request.Method = "Post";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    //string json = JsonConvert.SerializeObject(new
                    //{
                    //    u_description = description,
                    //    U_short_description = shortdescription;
                    var contents = "{ \"caller_id\" : \"" + caller + "\", \"work_notes\" : \"" + work_notes + "\",\"assignment_group\" : \"" + assignment_group + "\", \"assigned_to\" : \"" + caller + "\", \"description\" : \"" + description + "\", \"short_description\" : \"" + shortdescription + "\"}";
                    streamWriter.Write(contents);
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    var res = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Console.WriteLine(res);

                    JObject joResponse = JObject.Parse(res.ToString());
                    JObject ojObject = (JObject)joResponse["result"];
                    //string incNumber = ((JValue)ojObject.SelectToken("number")).Value.ToString();

                    //return ;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("{0} Exception caught: ", ex);
            }
        }
    }
}          
