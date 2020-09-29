using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedisClient
{
    class JsonToDictionaryConverter
    {
        private string _aPIUrl;
        public string APIUrl
        {
            get => _aPIUrl;
            set => _aPIUrl = value;
        }

        private string _rawStringToConsume;
        public string RawStringToConsume
        {
            get => _rawStringToConsume;
            set => _rawStringToConsume = value;
        }
        private WebClient _webClient = new WebClient();
        //private List<string> _allDataTypes;

        public JsonToDictionaryConverter(string aPIUrl)
        {
            _aPIUrl = aPIUrl;
        }

        public JsonToDictionaryConverter()
        {

        }

        public async Task<Dictionary<string, object>> ProvideAPIDataFromJSON()
        {
            return await Task.Run(() => {
                _webClient.Headers["Content-Type"] = "application/json";

                if (_aPIUrl != null)
                    RawStringToConsume = _webClient.DownloadString(_aPIUrl);

                if (RawStringToConsume == null)
                    throw new Exception("You have no JSN data to convert!");

                JToken jObjectData = JToken.Parse(RawStringToConsume);


                if (jObjectData is JObject)
                    return JsonToDictioanary(jObjectData);

                Dictionary<string, object> superDict = new Dictionary<string, object>();
                if (jObjectData is JArray)
                {
                    //$"{name}Array__{_iterationsCount}"
                    superDict.Add($"Class1_Property1Array__0", JsonToDictioanary(jObjectData));
                }
                return superDict;
            });

        }

        private string _providePrettyStringOutput = string.Empty;
        public async Task<string> ProvidePrettyString(string indent, Dictionary<string, object> dataDict)
        {
            return await Task.Run(async() => {
                indent += indent;
                foreach (var s in dataDict)
                {
                    if (s.Value is String)
                    {
                        _providePrettyStringOutput += $"{indent}[ {s.Key.Substring(0, s.Key.IndexOf("_"))}: {s.Value} ]" + Environment.NewLine;
                    }
                    else if (s.Value is Dictionary<string, object>)
                    {
                        _providePrettyStringOutput += "\n";
                        await ProvidePrettyString(indent, s.Value as Dictionary<string, object>);
                    }
                }

                return _providePrettyStringOutput;
            });

        }


        private delegate void LikeActionButWithParameter(JToken jobject);
        private int _iterationsCount = 0;
        private int _upperLevelClassesCount = 1;
        private Dictionary<string, object> JsonToDictioanary(JToken jtoken)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            LikeActionButWithParameter processJObject = (JToken jobject) =>
            {
                foreach (var s in JObject.Parse(jobject.ToString()))
                {
                    string name = s.Key;
                    JToken value = s.Value;

                    try
                    {

                        if(value.Type.ToString() == "String" && value.ToString()[0] == '[' && value.ToString().Last() == ']')
                        {
                            value = JArray.Parse(value.ToString());
                        }

                        if (value.HasValues)
                        {


                            if (value.Type is Newtonsoft.Json.Linq.JTokenType.Array)
                            {
                                if (!value.Children().FirstOrDefault().HasValues)
                                {
                                    if (ReplaceJsonTypeByCsharpType(value.Children().FirstOrDefault().Type.ToString(), out string CsharpType))
                                        dictionary.Add($"{name}_{CsharpType}Array__{_iterationsCount}", name);
                                    //else dictionary.Add($"{name}_{value.Children().FirstOrDefault().Type}Array__{_iterationsCount}", name);
                                    else dictionary.Add($"{name}_{value.Children().FirstOrDefault().Type}Array__{_iterationsCount}", name);
                                }
                                else
                                {
                                    JToken[] childrenArray = value.Children().ToArray();
                                    for(int i = 0; i < childrenArray.Length; i++)
                                    {
                                        
                                        if (ReplaceJsonTypeByCsharpType(childrenArray[i].Type.ToString(), out string CsharpType))
                                            dictionary.Add($"{name}_{CsharpType}Array__{_iterationsCount}_{i}", JsonToDictioanary(childrenArray[i]));
                                        else dictionary.Add($"{name}Array__{_iterationsCount}_{i}", JsonToDictioanary(childrenArray[i]));       
                                    }
                                    
                                }

                            }
                            else dictionary.Add($"{name}__{_iterationsCount}", JsonToDictioanary(value));
                        }

                        else
                        {
                            if (ReplaceJsonTypeByCsharpType(value.Type.ToString(), out string CsharpType))
                                dictionary.Add($"{name}_{CsharpType}__{_iterationsCount}", value.ToString());
                            else dictionary.Add($"{name}_{value.Type}__{_iterationsCount}", value.ToString());
                        }
                        //_iterationsCount++;


                        _upperLevelClassesCount++;
                    }
                    catch(Exception ex) 
                    {
                        MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
                    }
                }
            };


            if (jtoken is JObject) processJObject(jtoken);
            if (jtoken is JArray)
            {
                foreach (JObject child in jtoken.Children<JObject>())
                {
                    processJObject(child);
                }
            }
            return dictionary;
        }


        private bool ReplaceJsonTypeByCsharpType(string jsonType, out string CsharpType)
        {
            bool toReturn = false;
            string CsharpTypeToOut = "no_such_a_type";

            Dictionary<string, string> jsonAndCsharpDataTypeCorrelation = new Dictionary<string, string>();
            jsonAndCsharpDataTypeCorrelation.Add("Date", "DateTime");
            jsonAndCsharpDataTypeCorrelation.Add("Integer", "int?");
            jsonAndCsharpDataTypeCorrelation.Add("Float", "float?");
            //jsonAndCsharpDataTypeCorrelation.Add("Null", "string");




            foreach (var s in jsonAndCsharpDataTypeCorrelation)
            {
                if (s.Key.Equals(jsonType))
                {
                    toReturn = true;
                    CsharpTypeToOut = jsonAndCsharpDataTypeCorrelation[jsonType];
                }
            }
            CsharpType = CsharpTypeToOut;
            return toReturn;
        }












    }
}
