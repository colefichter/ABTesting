using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace ABTesting.Helpers
{
	/// <summary>
	/// Summary description for SerializationHelper.
	/// </summary>
	public class SerializationHelper
	{
		/// <summary>
		/// Given a serializable object, creates an XML file.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="fileName"></param>
		public static void SerializeToFile(object obj, string fileName)
		{
			try
			{
                XmlSerializer xs = new XmlSerializer(obj.GetType());
				using (TextWriter writer = new StreamWriter(fileName, false))
				{
					xs.Serialize(writer, obj);
					writer.Close();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("could not serialize this object:\n" + obj + "\n\n" + ex);
			}
		}
        
		/// <summary>
		/// Given the name of an XML file representing an object, returns that object.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object DeSerializeFromFile(string fileName, Type type)
		{
			try
			{
				XmlSerializer xs = new XmlSerializer(type);

				using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					object obj = xs.Deserialize(fs);
					fs.Close();
					return obj;
				}
			}
			catch (FileNotFoundException)
			{
				throw new FileNotFoundException("could not find file:" + fileName, fileName);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
				throw new Exception("could not deserialize this file:"
				                    + fileName
				                    + Environment.NewLine + e);
			}
		}
	}
}