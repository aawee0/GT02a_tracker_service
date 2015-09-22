using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    /// <summary>
    /// Форматтер логов
    /// </summary>
    public class ObjectLogFormatter
    {
        private static Regex re_password = new Regex("(password|passwd|pwd)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Отформатировать массив элементов в строку 
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        [Obsolete("use PrepareCollectionToLog instead")]
        public static string PrepareArrayToLog(object[] arr)
        {
            if (arr != null)
            {
                StringBuilder res = new StringBuilder(string.Format("Array type:{0}", arr.GetType()));
                foreach (object obj in arr)
                {
                    if (obj is string)
                        res.AppendFormat("({0})", obj);
                    else
                        res.Append(ToString(obj));
                }
                return res.ToString();
            }
            else
                return "Empty array";
        }
        /// <summary>
        /// Отформатировать коллекцию элементов в строку 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string PrepareCollectionToLog(IEnumerable collection)
        {
            if (collection != null)
            {
                StringBuilder res = new StringBuilder(string.Format("Collection type:{0}", collection.GetType()));
                foreach (object obj in collection)
                {
                    if (obj is string || obj is ValueType)
                        res.AppendFormat("({0})", obj);
                    else
                        res.Append(ToString(obj));
                }
                return res.ToString();
            }
            else
                return "Empty collection";
        }

        /// <summary>
        /// Представляет объект в виде 
        /// [ObjectType: (propertyName=propertyValue)(propertyName=propertyValue)...]
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToString(object o)
        {
            return ToString(o, false);
        }

        /// <summary>
        /// Представляет объект в виде 
        /// [ObjectType: (propertyName=propertyValue)(propertyName=propertyValue)...]
        /// </summary>
        /// <param name="o"></param>
        /// <param name="expandCollections"></param>
        /// <returns></returns>
        public static string ToString(object o, bool expandCollections)
        {
            try
            {
                if (null == o)
                    return "null";

                StringBuilder builder = new StringBuilder();
                Type type = o.GetType();
                builder.Append("[");
                builder.Append(type.Name);
                builder.Append(": ");

                MemberInfo[] members = type.GetMembers();
                for (int i = 0; i < members.Length; i++)
                {
                    MemberInfo mi = members[i];
                    string name = mi.Name;
                    if (re_password.IsMatch(name)) // не логируем пароль
                        continue;

                    string value = string.Empty;
                    FieldInfo fi = mi as FieldInfo;
                    bool printValue = false;
                    if (fi != null)
                    {
                        object v = fi.GetValue(o);
                        /*if (v == null || v.GetType().IsDefined(typeof(LogableAttribute), false))
                            value = ToString(v);
                        else */if (v is IXPathNavigable)
                        {
                            XPathNavigator nav = ((IXPathNavigable)v).CreateNavigator();
                            value = nav.OuterXml;
                        }
                        else if (expandCollections && v is IEnumerable)
                        {
                            value = PrepareCollectionToLog((IEnumerable)v);
                        }
                        else
                            value = Convert.ToString(v);

                        printValue = true;
                    }
                    
                    PropertyInfo pi = mi as PropertyInfo;
                    if (pi != null)
                    {
                        object v = pi.GetValue(o, null);
                        /*if (v == null || v.GetType().IsDefined(typeof(LogableAttribute), false))
                            value = ToString(v);
                        else*/ if (v is IXPathNavigable)
                        {
                            XPathNavigator nav = ((IXPathNavigable)v).CreateNavigator();
                            value = nav.OuterXml;
                        }
                        else if (expandCollections && v is IEnumerable)
                        {
                            value = PrepareCollectionToLog((IEnumerable)v);
                        }
                        else
                            value = Convert.ToString(v);

                        printValue = true;
                    }
                    if (printValue)
                    {
                        builder.Append("(");
                        builder.Append(name);
                        builder.Append("=");
                        builder.Append(value);
                        builder.Append(")");
                    }
                }
                builder.Append("]");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return string.Format("ObjectFormatException : " + ex.Message);
            }
        }
    }
}
