using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Web;
using EnsightenGTMConverter.Models;
using Cql.Common.Logging;
using EnsightenGTMConverter.Core;
using Newtonsoft.Json.Linq;

namespace EnsightenGTMConverter.FileActions
{
    public class XlsReader
    {
        private static readonly ILogger Logger = LogContextManager.GetLogger<FileManager>();
        private static ContainerVersion gtmObject = new ContainerVersion();
        private static Container container = new Container();

        public bool ReadXlsData(string file, ContainerVersion cv, ConfigContainer config)
        {
            string fileName = Util.GetFileName(file).Split('.')[0];
            string connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; Extended Properties=Excel 8.0;", file);

            //get all rows
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    OleDbCommand command = new OleDbCommand(String.Format("SELECT * FROM [{0}$]", fileName), connection);
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        switch (fileName)
                        {
                            case "Spaces":
                                ReadSpaces(dr, cv, config);
                                break;
                            case "Tags":
                                ReadTags(dr, cv);
                                break;
                            case "Conditions":
                                ReadConditions(dr, cv);
                                break;
                            case "DataDefinitions":
                                ReadDataDefinitions(dr, cv);
                                break;
                            default:
                                break;
                        }
                    }
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                Logger.Error(string.Format("Exception occurred while reading file {0}.", file), ex);
            }

            return false;
        }
        private void ReadSpaces(OleDbDataReader reader, ContainerVersion cv, ConfigContainer config)
        {
            while(reader.Read())
            {
                string[] spaces = config.SpaceID;
                var match = spaces.FirstOrDefault(s => s.Contains(reader[0].ToString()));
                if(match != null)
                {
                    cv.container.spaceName = reader[1].ToString();
                }
            }
        }
        private void ReadTags(OleDbDataReader reader, ContainerVersion cv)
        {
            cv.tag = new List<Tag>();

            while (reader.Read())
            {
                var space = reader[2].ToString();
                if (space.Length > 0 && cv.container.spaceName != reader[2].ToString())
                {
                    continue;
                }

                Tag tag = new Tag();
                tag.accountId = cv.container.accountId;
                tag.containerId = cv.container.containerId;
                
                tag.parameter = new List<Parameter>();
                tag.tagId = reader[0].ToString();
                tag.name = CleanName(reader[1].ToString());
                tag.type = "html";

                tag.firingTriggerId = new List<string>();
                var triggers = reader[7].ToString().Split(' ');
                if(triggers.Length > 1)
                {
                    foreach(var trigger in triggers)
                    {
                        if(trigger != "")
                        {
                            tag.firingTriggerId.Add(trigger);
                        }
                    }
                } else if(triggers[0] != "")
                {
                    tag.firingTriggerId.Add(reader[7].ToString());
                }
                
                tag.tagFiringOption = "ONCE_PER_EVENT";

                Parameter htmlParam = new Parameter();
                htmlParam.type = "TEMPLATE";
                htmlParam.key = "html";
                htmlParam.value = String.Format("<script type='text/javascript'>{0}</script>", reader[11].ToString());
                tag.parameter.Add(htmlParam);

                Parameter dwParam = new Parameter();
                dwParam.type = "BOOLEAN";
                dwParam.key = "supportDocumentWrite";
                dwParam.value = "false";
                tag.parameter.Add(dwParam);

                cv.tag.Add(tag);
            }
        }
        private void ReadConditions(OleDbDataReader reader, ContainerVersion cv)
        {
            cv.trigger = new List<Trigger>();

            while (reader.Read())
            {
                Trigger trigger = new Trigger();
                trigger.accountId = cv.container.accountId;
                trigger.containerId = cv.container.containerId;

                trigger.triggerId = reader[1].ToString();

                trigger.name = CleanName(reader[0].ToString());
                trigger.type = "WINDOW_LOADED";

                if(reader[4].ToString().Length > 0 || reader[13].ToString().Length > 0)
                {
                    trigger.filter = new List<Filter>();
                }

                if (reader[4].ToString().Length > 0)
                {
                    Filter pathFilter = new Filter();
                    var splitPath = reader[4].ToString().Split('(');
                    pathFilter.type = LookupComparison(splitPath[0].Replace("[ignore case]", ""));
                    pathFilter.parameter = new List<Parameter>();
                    Parameter pathParam1 = new Parameter();
                    Parameter pathParam2 = new Parameter();

                    pathParam1.key = "arg0";
                    pathParam2.key = "arg1";

                    pathParam1.type = "TEMPLATE";
                    pathParam2.type = "TEMPLATE";

                    pathParam1.value = "{{Page Path}}";
                    pathParam2.value = splitPath[1].Replace("/", "").Replace(")", "");
                    pathFilter.parameter.Add(pathParam1);
                    pathFilter.parameter.Add(pathParam2);
                    trigger.filter.Add(pathFilter);
                }

                if(reader[13].ToString().Length > 0)
                {
                    Filter dataFilter = new Filter();
                    if (reader[13].ToString().Contains("&"))
                    {
                        var splitData = reader[13].ToString().Split('&');
                        foreach (var data in splitData)
                        {
                            dataFilter = GetTriggerFilter(data);
                            trigger.filter.Add(dataFilter);
                        }
                    }
                    else
                    {
                        dataFilter = GetTriggerFilter(reader[13].ToString());
                        trigger.filter.Add(dataFilter);
                    }
                }
                
                cv.trigger.Add(trigger);
            }
        }
        private void ReadDataDefinitions(OleDbDataReader reader, ContainerVersion cv)
        {
            cv.variable = new List<Variable>();

            while (reader.Read())
            {
                Variable variable = new Variable();
                variable.accountId = cv.container.accountId;
                variable.containerId = cv.container.containerId;

                variable.name = CleanName(reader[0].ToString());
                variable.variableId = reader[1].ToString();
                variable.type = "jsm";
                variable.parameter = new List<Parameter>();

                Parameter param = new Parameter();
                param.value = reader[9].ToString();
                param.type = "TEMPLATE";
                param.key = "javascript";

                variable.parameter.Add(param);
                cv.variable.Add(variable);
            }
        }

        private string LookupComparison(string comparison)
        {
            comparison = comparison.Replace(" ", "");
            string parsedString = "";
            switch(comparison.ToLower())
            {
                case "equals":
                    parsedString = "EQUALS";
                    break;
                case "contains":
                    parsedString = "CONTAINS";
                    break;
                case "begins with":
                    parsedString = "STARTS_WITH";
                    break;
                case "ends with":
                    parsedString = "ENDS_WITH";
                    break;
                case "matches regex":
                    parsedString = "REGEX";
                    break;
                case "does not equal":
                    parsedString = "EQUALS";
                    break;
                case "not":
                    parsedString = "EQUALS";
                    break;
                case "does not contain":
                    parsedString = "CONTAINS";
                    break;
                case "does not begin with":
                    parsedString = "STARTS_WITH";
                    break;
                case "does not end with":
                    parsedString = "ENDS_WITH";
                    break;
                case "does not match RegEx":
                    parsedString = "REGEX";
                    break;
                default:
                    break;
            }
            return parsedString;
        }

        private string[] ParseTriggerData(string def)
        {
            def = Regex.Replace(def, @"\[[0-9a-zA-Z ]+\]", "");
            def = Regex.Replace(def, @"[^0-9a-zA-Z ]+", "");
            var splitDef = def.Split(new string[] { " is " }, StringSplitOptions.None);
            if (splitDef.Length == 1)
            {
                splitDef = def.Split(new string[] { " contains " }, StringSplitOptions.None);
            }
            return new string[] { splitDef[0].Trim(), splitDef[1].Trim() };
        }

        private Filter GetTriggerFilter(string def)
        {
            var filter = new Filter();
            filter.parameter = new List<Parameter>();
            def = def.Replace("[ignore case]", "");

            string condition = "EQUALS";

            int pFrom = def.IndexOf("[") + "[".Length;
            int pTo = def.LastIndexOf("]");

            if (pFrom > 0 && pTo > 0)
            {
                if (def.Substring(pFrom, pTo - pFrom).Contains("not"))
                {
                    var negate = new Parameter();
                    negate.type = "BOOLEAN";
                    negate.key = "negate";
                    negate.value = "true";
                    filter.parameter.Add(negate);
                }

                condition = LookupComparison(def.Substring(pFrom, pTo - pFrom));
            }

            filter.type = condition;

            string[] parsed = ParseTriggerData(def);

            var var = new Parameter();
            var param2 = new Parameter();

            var.type = "TEMPLATE";
            var.key = "arg0";
            var.value = "{{" + parsed[0] + "}}";

            param2.type = "TEMPLATE";
            param2.key = "arg1";
            param2.value = parsed[1];

            filter.parameter.Add(var);
            filter.parameter.Add(param2);

            return filter;
        }

        private string CleanName(string name)
        {
            var result = Regex.Matches(name, @"[[0-9a-zA-Z \, \. \' \- \~]+").Cast<Match>()
                  .Aggregate(" ", (s, e) => s + e.Value, s => s);
            return result;
        }
    }
}
